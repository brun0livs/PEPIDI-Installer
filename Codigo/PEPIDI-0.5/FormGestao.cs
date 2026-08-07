using Guna.UI2.WinForms;
using PEPIDI.Models;
using PEPIDI.Organizers;
using PEPIDI.UCs.DGVS;
using PEPIDI.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;



namespace PEPIDI
{
    public partial class FormGestao : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;
        string funcao;
        EfeitoUI M = new();

        int IDGestor;
        private readonly PermissoesPerfil permissoes;

        // Configuração do Menu
        bool menuAberto;
        int larguraMax;
        int larguraMin;
        const int velocidade = 35; // Velocidade da animação

        // Variáveis de controlo no topo do Form
        private System.Timers.Timer blinkTimer;
        private int blinkCount = 0;
        private int maxBlinks = 100; // 6 alternâncias = pisca 3 vezes
        private Color corOriginalTexto;

        // Watcher para comandos vindos do AgentePEPIDI (proxima_acao.txt em %AppData%\PEPIDI\).
        // O agente escreve "STOCK" ou "PEDIDOS" quando o utilizador clica numa notificação,
        // e este watcher dispara a navegação para a UC correspondente.
        private FileSystemWatcher _watcherAcaoAgente;
        private static string CaminhoComandoAgente =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "PEPIDI", "proxima_acao.txt");

        public FormGestao(int _idGestor, PermissoesPerfil _permissoes)
        {
            InitializeComponent();

            // Magia Anti-Tremor
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            IDGestor = _idGestor;
            permissoes = _permissoes;
        }

        // --- VACINA CONTRA TREMORES ---
        public static void SetDoubleBuffered(Control c)
        {
            if (System.Windows.Forms.SystemInformation.TerminalServerSession) return;
            System.Reflection.PropertyInfo pi = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pi.SetValue(c, true, null);
        }

        private void FormGestao_Load(object sender, EventArgs e)
        {
            timerStockBaixo.Start();
            timerStockBaixo_Tick(this, EventArgs.Empty);
            

            // Aplicar o Double Buffer ao SplitContainer (pnlMenu) e aos painéis dentro dele!
            SetDoubleBuffered(pnlMenu);
            SetDoubleBuffered(pnlMenu.Panel1);
            SetDoubleBuffered(pnlMenu.Panel2);
            SetDoubleBuffered(tlpMenu);
            SetDoubleBuffered(tableLayoutPanel3);
            SetDoubleBuffered(pnlConteudo);
            SetDoubleBuffered(tableLayoutPanel2);

            this.KeyPreview = true;

            // 1. APLICAR O TEMA CENTRALIZADO
            GestorTema.AplicarEstilos(this);

            // 2. LER AS LARGURAS CORRETAS
            this.PerformLayout();
            larguraMin = Nav1.Height; // Fica quadrado perfeito
            larguraMax = GestorTema.ModoAtual == TipoEcra.Surface ? 400 : 220;

            // 3. CARREGAR UTILIZADOR E PERMISSÕES
            var info = Details.GetInfoGestor(IDGestor);
            funcao = info.Funcao;
            lblNome.Text = info.Nome + " · " + info.Funcao;
            AplicarPermissoes();

            // Garante que as Tags estão preenchidas no arranque para os nomes não se perderem
            foreach (Control c in tlpMenu.Controls)
            {
                if (c is Guna2Button btn && btn.Tag == null)
                {
                    btn.Tag = btn.Text;
                }
            }

            AbrirPrimeiraPermissao();

            // 4. LER A SETTING E FORÇAR O TAMANHO INICIAL DO SPLITCONTAINER
            menuAberto = Properties.Settings.Default.MenuExpandido;

            if (menuAberto == true)
            {
                pnlMenu.SplitterDistance = larguraMax;
            }
            else
            {
                pnlMenu.SplitterDistance = larguraMin;
            }

            // 5. AJUSTAR OS TEXTOS/ÍCONES LOGO NO ARRANQUE
            ConfigurarBotoes(menuAberto);

            // 6. INICIAR O WATCHER DE COMANDOS DO AGENTE (silencioso se falhar)
            IniciarWatcherAcaoAgente();
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Integração com o AgentePEPIDI: quando o utilizador clica numa notificação do
        // agente, este escreve %AppData%\PEPIDI\proxima_acao.txt com "STOCK" ou "PEDIDOS".
        // O FileSystemWatcher dispara imediatamente, lê o conteúdo, apaga o ficheiro
        // (consome o comando) e chama Navegar() para abrir a UC correspondente.
        // Se o ficheiro já existe ao arrancar, processa-o de imediato (caso em que o
        // PEPIDI foi lançado pelo próprio agente).
        // ────────────────────────────────────────────────────────────────────────────────
        private void IniciarWatcherAcaoAgente()
        {
            try
            {
                string dir = Path.GetDirectoryName(CaminhoComandoAgente);
                if (string.IsNullOrEmpty(dir)) return;
                Directory.CreateDirectory(dir);

                _watcherAcaoAgente = new FileSystemWatcher(dir, Path.GetFileName(CaminhoComandoAgente))
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
                };
                _watcherAcaoAgente.Created += (s, ev) => BeginInvoke(new Action(ProcessarAcaoPendente));
                _watcherAcaoAgente.Changed += (s, ev) => BeginInvoke(new Action(ProcessarAcaoPendente));
                _watcherAcaoAgente.EnableRaisingEvents = true;

                // Caso o ficheiro já exista (PEPIDI lançado pelo agente), processa logo
                if (File.Exists(CaminhoComandoAgente))
                    BeginInvoke(new Action(ProcessarAcaoPendente));
            }
            catch
            {
                // Watcher é nice-to-have — falha silenciosamente para não bloquear o arranque
            }
        }

        private void ProcessarAcaoPendente()
        {
            try
            {
                if (!File.Exists(CaminhoComandoAgente)) return;

                // O watcher pode disparar antes do conteúdo estar totalmente escrito —
                // pequena espera para garantir que o ficheiro está pronto a ler.
                System.Threading.Thread.Sleep(50);

                string acao = File.ReadAllText(CaminhoComandoAgente).Trim().ToUpperInvariant();
                try { File.Delete(CaminhoComandoAgente); } catch { /* outro processo a escrever */ }

                string chave = acao switch
                {
                    "STOCK"    => "stock",
                    "PEDIDOS"  => "pedidos pendentes",
                    _          => null
                };

                if (chave != null && PodeAceder(chave))
                {
                    Navegar(chave);

                    // Traz a janela do PEPIDI à frente (caso esteja minimizada)
                    if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
                    Activate();
                    BringToFront();
                }
            }
            catch { /* ignorar erros — o comando é best-effort */ }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                _watcherAcaoAgente?.Dispose();
                _watcherAcaoAgente = null;
            }
            catch { }
            base.OnFormClosed(e);
        }

        private void AbrirPrimeiraPermissao()
        {
            Guna2Button[] botoesNavegacao = { Nav1, Nav2, Nav3, Nav4, Nav5, Nav6, Nav7, Nav9 };

            foreach (var btn in botoesNavegacao)
            {
                if (btn.Visible)
                {
                    Nav_Clicked(btn, EventArgs.Empty);
                    return;
                }
            }

            EfeitoUI M = new EfeitoUI();
            M.AbrirMensagem("Não tem permissões para aceder a nenhum módulo.", "Aviso de Acesso");
        }

        public bool PodeAceder(string key)
        {
            switch (key.ToLowerInvariant())
            {
                case "stock": return permissoes.PodeVerStock || permissoes.PodeCriarStock;
                case "funcionários":
                case "funcionarios": return permissoes.PodeEditarFunc;
                case "pedidos pendentes": return permissoes.PodeAprovar || permissoes.PodeEntregar;
                case "pedidos aprovados": return permissoes.PodeEntregar || permissoes.PodeAprovar;
                case "histórico":
                case "historico": return permissoes.PodeVerHistorico;
                case "gestão":
                case "gestao": return permissoes.PodeInserirStock || permissoes.PodeCriarStock;
                case "funções":
                case "funcoes": return permissoes.PodeCriarFuncoes;
                case "definições":
                case "definicoes": return permissoes.PodeAlterarDefinicoes;
                case "sair": return true;
                default: return false;
            }
        }

        private void AplicarPermissoes()
        {
            Nav1.Visible = Nav1.Enabled = false;
            Nav2.Visible = Nav2.Enabled = false;
            Nav3.Visible = Nav3.Enabled = false;
            Nav4.Visible = Nav4.Enabled = false;
            Nav5.Visible = Nav5.Enabled = false;
            Nav6.Visible = Nav6.Enabled = false;
            Nav7.Visible = Nav7.Enabled = false;
            Nav9.Visible = Nav9.Enabled = false;

            Nav10.Visible = true; Nav10.Enabled = true;

            if (PodeAceder("Stock")) { Nav1.Visible = true; Nav1.Enabled = true; }
            if (PodeAceder("Funcionários")) { Nav2.Visible = true; Nav2.Enabled = true; }
            if (PodeAceder("Pedidos Pendentes")) { Nav3.Visible = true; Nav3.Enabled = true; }
            if (PodeAceder("Pedidos Aprovados")) { Nav4.Visible = true; Nav4.Enabled = true; }
            if (PodeAceder("Histórico")) { Nav5.Visible = true; Nav5.Enabled = true; }
            if (PodeAceder("Gestão")) { Nav6.Visible = true; Nav6.Enabled = true; }
            if (PodeAceder("Funções")) { Nav7.Visible = true; Nav7.Enabled = true; }
            if (PodeAceder("Definições")) { Nav9.Visible = true; Nav9.Enabled = true; }
        }

        private void Nav_Clicked(object sender, EventArgs e)
        {
            var clicked = (Guna2Button)sender;
            string chaveNavegacao = clicked.Tag != null ? clicked.Tag.ToString() : clicked.Text;

            if (chaveNavegacao.Equals("Sair", StringComparison.OrdinalIgnoreCase))
            {
                Close();
                return;
            }

            if (chaveNavegacao.Equals("Expandir", StringComparison.OrdinalIgnoreCase))
            {
                menuAberto = !menuAberto;

                // Guarda a definição do utilizador
                Properties.Settings.Default.MenuExpandido = menuAberto;
                Properties.Settings.Default.Save();

                // ESCONDE LOGO OS TEXTOS ANTES DE ANIMAR! (Isto evita o efeito esmagado)
                ConfigurarBotoes(menuAberto);

                timerMenu.Start();
                return;
            }

            if (!clicked.Checked)
            {
                foreach (Control c in tlpMenu.Controls)
                {
                    if (c is Guna2Button nb && nb.Checked)
                    {
                        nb.Checked = false;
                        nb.Image = ImageHelper.InverterCores(nb.Image);
                    }
                }

                clicked.Checked = true;
                clicked.Image = ImageHelper.InverterCores(clicked.Image);
            }

            Navegar(chaveNavegacao);
            //calcular percentagens de cada modulo

        }

        private async void Navegar(string key)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                await Task.Yield();
                UserControl controlParaAbrir = key.ToLowerInvariant() switch
                {
                    "stock" => new UCs.Stock(permissoes, funcao),
                    "definições" => new UCs.Definicoes(IDGestor),
                    "pedidos pendentes" => new UCs.Pedidos(IDGestor, "Pendente", funcao),
                    "pedidos aprovados" => new UCs.Pedidos(IDGestor, "Aprovado", funcao),
                    "gestao" or "gestão" => new UCs.CriarStock(IDGestor, permissoes.PodeCriarStock, funcao),
                    "funções" or "funcoes" => new UCs.Funcoes(IDGestor, funcao),
                    "historico" or "histórico" => new UCs.Pedidos(IDGestor, "Finalizado", funcao),
                    "funcionarios" or "funcionários" => new UCs.Funcionarios(IDGestor),
                    _ => null
                };
                if (controlParaAbrir != null)
                {
                    AbrirControl(controlParaAbrir);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao navegar: {ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public void AbrirControl(UserControl control)
        {
            SendMessage(pnlConteudo.Handle, WM_SETREDRAW, false, 0);
            pnlConteudo.Controls.Clear();
            control.Dock = DockStyle.Fill;
            pnlConteudo.Controls.Add(control);
            SendMessage(pnlConteudo.Handle, WM_SETREDRAW, true, 0);
            pnlConteudo.Refresh();
        }

        // O MOTOR DA ANIMAÇÃO DO SPLITCONTAINER
        private void timerMenu_Tick(object sender, EventArgs e)
        {
            if (menuAberto) // ANIMAR PARA ABRIR
            {
                if (pnlMenu.SplitterDistance + velocidade < larguraMax)
                {
                    pnlMenu.SplitterDistance += velocidade;
                }
                else
                {
                    pnlMenu.SplitterDistance = larguraMax;
                    timerMenu.Stop();
                }
            }
            else // ANIMAR PARA FECHAR
            {
                if (pnlMenu.SplitterDistance - velocidade > larguraMin)
                {
                    pnlMenu.SplitterDistance -= velocidade;
                }
                else
                {
                    pnlMenu.SplitterDistance = larguraMin;
                    timerMenu.Stop();
                }
            }
        }

        // ESTA É A FUNÇÃO QUE METE OS ÍCONES BONITOS E APAGA O TEXTO
        private void ConfigurarBotoes(bool mostrarTexto)
        {
            foreach (Control c in tlpMenu.Controls)
            {
                if (c is Guna2Button btn)
                {
                    if (mostrarTexto)
                    {
                        // MODO ABERTO: Restaura texto da TAG e alinha à esquerda
                        if (btn.Tag != null) btn.Text = btn.Tag.ToString();
                        btn.ImageAlign = HorizontalAlignment.Left;
                        btn.TextAlign = HorizontalAlignment.Left;
                    }
                    else
                    {
                        // MODO FECHADO: Apaga texto e alinha ícone ao centro
                        if (btn.Tag == null || string.IsNullOrEmpty(btn.Tag.ToString()))
                        {
                            btn.Tag = btn.Text;
                        }
                        btn.Text = "";
                        btn.ImageAlign = HorizontalAlignment.Center;
                        btn.TextAlign = HorizontalAlignment.Center;
                    }
                }
            }
        }

        private void FormGestao_KeyPress(object sender, KeyPressEventArgs e) { }

        private void FormGestao_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                if (this.FormBorderStyle == FormBorderStyle.None)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.CenterToScreen();
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                }
            }
        }

        private void FormGestao_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            AdaptarDPI();
        }

        private void AdaptarDPI()
        {
            float dpiScale = this.DeviceDpi / 96f;
            if (dpiScale <= 1f) return;

            larguraMax = (int)(300 * dpiScale);
            if (menuAberto) pnlMenu.SplitterDistance = larguraMax;

            int tamanhoIcone = (int)(43 * dpiScale);
            Size novoTamanhoGuna = new Size(tamanhoIcone, tamanhoIcone);

            Nav1.ImageSize = novoTamanhoGuna;
            Nav2.ImageSize = novoTamanhoGuna;
            Nav3.ImageSize = novoTamanhoGuna;
            Nav4.ImageSize = novoTamanhoGuna;
            Nav5.ImageSize = novoTamanhoGuna;
            Nav6.ImageSize = novoTamanhoGuna;
            Nav7.ImageSize = novoTamanhoGuna;
            Nav8.ImageSize = novoTamanhoGuna;
            Nav9.ImageSize = novoTamanhoGuna;
            Nav10.ImageSize = novoTamanhoGuna;
        }

        private void btnAcessRapido_Click(object sender, EventArgs e)
        {
            // Declara a variável que vai receber o texto
            string codigoAcesso;

            // O método devolve o DialogResult E preenche a string 'codigoAcesso'
            DialogResult res = M.AbrirMensagem("Número de Funcionário:", "PEPIDI - Acesso Rápido", true, out codigoAcesso);

            // Validamos se o utilizador clicou em OK e se efetivamente escreveu algo
            if (res == DialogResult.OK && !string.IsNullOrWhiteSpace(codigoAcesso))
            {
                // Aqui já tens a string pronta a usar!
                MessageBox.Show($"O código inserido foi: {codigoAcesso}");
            }
        }

        private void timerStockBaixo_Tick(object sender, EventArgs e)
        {
            try
            {
                int count = 0;
                using (var conn = GetConn.GetConnection())
                {
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                        "SELECT COUNT(*) FROM Stock WHERE Estado = 1 AND Quantidade <= 5", conn))
                    {
                        conn.Open();
                        count = (int)cmd.ExecuteScalar();
                    }
                }

                if (count > 0)
                {
                    lblStockBaixo.Text = $"⚠ {count} artigo{(count > 1 ? "s" : "")} com stock baixo";
                    IniciarDestaque(lblStockBaixo);
                }
                else
                {
                    lblStockBaixo.Text = "";
                }
            }
            catch { /* Não deixar o timer rebentar a aplicação */ }
        }

        private void LblStockBaixo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lblStockBaixo.Text)) return;
            if (PodeAceder("Stock"))
                Nav_Clicked(Nav1, EventArgs.Empty);
        }

        private void IniciarDestaque(ToolStripStatusLabel label)
        {
            // Guarda a cor original para conseguir restaurar no fim
            corOriginalTexto = label.ForeColor;
            blinkCount = 0;

            // Se o timer já existir, para-o antes de reiniciar
            if (blinkTimer != null)
            {
                blinkTimer.Stop();
                blinkTimer.Dispose();
            }

            blinkTimer = new System.Timers.Timer();
            blinkTimer.Interval = 500; // Velocidade do pisca (500ms = meio segundo)

            blinkTimer.Elapsed += (sender, e) =>
            {
                // Alterna entre Vermelho (Destaque) e a Cor Original
                if (label.ForeColor == corOriginalTexto)
                {
                    label.ForeColor = Color.Red; // Cor de destaque (pode ser Gold, Orange, etc.)
                                                 // Se quiseres dar ainda mais destaque, podes mudar a fonte para Negrito:
                                                 // label.Font = new Font(label.Font, FontStyle.Bold);
                }
                else
                {
                    label.ForeColor = corOriginalTexto;
                    // label.Font = new Font(label.Font, FontStyle.Regular);
                }

                blinkCount++;

                // Quando atingir o limite, para de piscar e garante que volta à cor normal
                if (blinkCount >= maxBlinks)
                {
                    blinkTimer.Stop();
                    label.ForeColor = corOriginalTexto;
                    // label.Font = new Font(label.Font, FontStyle.Regular);
                }
            };

            blinkTimer.Start();
        }
    }
}