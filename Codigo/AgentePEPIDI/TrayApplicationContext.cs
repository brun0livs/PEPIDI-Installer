using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace AgentePEPIDI
{
    /// <summary>Tipo da última notificação mostrada — para distinguir o destino ao clicar no balão.</summary>
    internal enum TipoNotificacao { Nenhuma, StockBaixo, NovosPedidos }

    /// <summary>
    /// Núcleo do AgentePEPIDI a correr em segundo plano. Gere o ícone na área
    /// de notificação do Windows e o temporizador de monitorização (dispara de
    /// 5 em 5 minutos). Em cada ciclo verifica dois eventos:
    /// (1) artigos de stock abaixo do mínimo definido em Definicoes;
    /// (2) novos pedidos de EPI com Estado = 'Pendente' criados desde a última verificação.
    /// Abre o <see cref="FormDetalhesStock"/> quando o utilizador clica no aviso em balão.
    /// </summary>
    public class ContextoDoAgente : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private Timer timerMonitorizacao;
        private FormDetalhesStock janelinhaDetalhes = null;

        // Marca o momento em que o agente arrancou — só notifica pedidos criados depois disto
        private DateTime ultimaVerificacao = DateTime.Now;

        // Tipo da última notificação mostrada — lido pelo handler do balão para decidir destino
        private TipoNotificacao _ultimaNotificacao = TipoNotificacao.Nenhuma;

        // Caminho do ficheiro de comando partilhado com o PEPIDI (FileSystemWatcher do lado do PEPIDI)
        private static string CaminhoComandoPEPIDI =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "PEPIDI", "proxima_acao.txt");

        // P/Invoke para trazer a janela do PEPIDI à frente quando já está aberta
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        private const int SW_RESTORE = 9;

        public ContextoDoAgente()
        {
            // 1. Configurar o Menu de Contexto (botão direito no ícone)
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Abrir PEPIDI", null, AbrirPEPIDI_Click);
            menu.Items.Add("Abrir Detalhes de Stock", null, AbrirDetalhes_Click);
            menu.Items.Add("─"); // Separador visual
            menu.Items.Add("Sair do Agente", null, Sair_Click);

            // 2. Configurar o Ícone na área de notificação (perto do relógio)
            // Usa caminho absoluto baseado no executável do agente — Directory.Current
            // pode estar em qualquer sítio se o processo for lançado pelo PEPIDI.
            // Fallback para o ícone do sistema se logo.ico não existir.
            Icon iconeTray;
            try
            {
                string caminhoIcone = System.IO.Path.Combine(AppContext.BaseDirectory, "logo.ico");
                iconeTray = System.IO.File.Exists(caminhoIcone)
                    ? new Icon(caminhoIcone)
                    : SystemIcons.Application;
            }
            catch
            {
                iconeTray = SystemIcons.Application;
            }

            trayIcon = new NotifyIcon()
            {
                Icon = iconeTray,
                ContextMenuStrip = menu,
                Visible = true,
                Text = "Agente PEPIDI" // Texto que aparece ao passar o rato
            };

            trayIcon.BalloonTipClicked += TrayIcon_BalloonTipClicked;
            trayIcon.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) AbrirOuTrazerPEPIDI(); };

            // 3. Configurar o Timer para verificações periódicas (a cada 5 minutos)
            timerMonitorizacao = new Timer
            {
                Interval = 30000 // 30.000 milissegundos = 30 segundos
            };
            timerMonitorizacao.Tick += TimerMonitorizacao_Tick;
            timerMonitorizacao.Start();

            // Vai mostrar o balão mal o agente ligue, para provar que está vivo!
            // Tipo Nenhuma → se o utilizador clicar, faz só o fallback (FormDetalhesStock).
            MostrarAlerta("Agente Iniciado", "O Agente PEPIDI está a correr em segundo plano!",
                          TipoNotificacao.Nenhuma, ToolTipIcon.Info);
        }

        private void AbrirPEPIDI_Click(object sender, EventArgs e)
        {
            AbrirOuTrazerPEPIDI();
        }

        private void AbrirDetalhes_Click(object sender, EventArgs e)
        {
            TrayIcon_BalloonTipClicked(sender, e); // Reutiliza o mesmo método para abrir os detalhes
        }

        private void TrayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            // Decide o destino conforme a última notificação mostrada.
            // Escreve um ficheiro de comando em %AppData%\PEPIDI\proxima_acao.txt que o
            // FormGestao do PEPIDI observa via FileSystemWatcher — assim navega
            // imediatamente para a UC certa (Stock ou Pedidos Pendentes).
            string acao;
            switch (_ultimaNotificacao)
            {
                case TipoNotificacao.StockBaixo:   acao = "STOCK";   break;
                case TipoNotificacao.NovosPedidos: acao = "PEDIDOS"; break;
                default:                           acao = null;      break;
            }

            if (acao == null)
            {
                // Sem contexto (ex: balão de "Agente Iniciado") → fallback: detalhes de stock
                AbrirDetalhesStockFallback();
                return;
            }

            EscreverComandoEPEPIDI(acao);
            AbrirOuTrazerPEPIDI();
        }

        /// <summary>
        /// Escreve o comando para o PEPIDI processar — best-effort, falha silenciosa
        /// se a pasta não existir ou se outro processo tiver o ficheiro aberto.
        /// </summary>
        private void EscreverComandoEPEPIDI(string acao)
        {
            try
            {
                string dir = Path.GetDirectoryName(CaminhoComandoPEPIDI);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(CaminhoComandoPEPIDI, acao);
            }
            catch { /* o PEPIDI pode estar a ler nesse instante — ignorar */ }
        }

        /// <summary>
        /// Tenta trazer o PEPIDI à frente. Se não estiver a correr, lança o executável.
        /// O comando escrito em <see cref="EscreverComandoEPEPIDI"/> é consumido depois
        /// pelo FormGestao (via watcher) ou no arranque do PEPIDI.
        /// </summary>
        private void AbrirOuTrazerPEPIDI()
        {
            var processos = Process.GetProcessesByName("PEPIDI");
            if (processos.Length > 0)
            {
                int pid = processos[0].Id;
                IntPtr handle = processos[0].MainWindowHandle;

                // MainWindowHandle pode ser zero em janelas com estilos não-standard
                if (handle == IntPtr.Zero)
                    handle = EncontrarJanelaDoProcesso(pid);

                if (handle != IntPtr.Zero)
                {
                    ShowWindow(handle, SW_RESTORE);
                    SetForegroundWindow(handle);
                }
                return;
            }

            // PEPIDI fechado — lança o executável (dev ou produção)
            string exe = EncontrarExePEPIDI();
            if (exe != null && File.Exists(exe))
            {
                try { Process.Start(exe); }
                catch { /* falha silenciosa */ }
            }
        }

        private static IntPtr EncontrarJanelaDoProcesso(int pid)
        {
            IntPtr resultado = IntPtr.Zero;
            EnumWindows((hWnd, _) =>
            {
                GetWindowThreadProcessId(hWnd, out uint windowPid);
                if (windowPid == (uint)pid && IsWindowVisible(hWnd))
                {
                    resultado = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return resultado;
        }

        /// <summary>Procura o PEPIDI.exe nos caminhos prováveis (produção e dev).</summary>
        private static string EncontrarExePEPIDI()
        {
            const string nome = "PEPIDI.exe";

            // 1. Produção: mesma pasta do agente
            string mesmaPasta = Path.Combine(AppContext.BaseDirectory, nome);
            if (File.Exists(mesmaPasta)) return mesmaPasta;

            // 2. Desenvolvimento: sobe a árvore procurando pasta irmã PEPIDI-0.5
            string dir = AppContext.BaseDirectory;
            for (int i = 0; i < 8; i++)
            {
                dir = Path.GetDirectoryName(dir);
                if (dir == null) break;

                string debugDir = Path.Combine(dir, "PEPIDI-0.5", "bin", "Debug");
                if (!Directory.Exists(debugDir)) continue;

                // Direto em bin\Debug\ (improvável mas cobre publicações)
                string direto = Path.Combine(debugDir, nome);
                if (File.Exists(direto)) return direto;

                // Subpastas TFM: net9.0-windows*, net9.0-windows10.0.xxxxx.0, etc.
                foreach (string sub in Directory.GetDirectories(debugDir))
                {
                    string candidato = Path.Combine(sub, nome);
                    if (File.Exists(candidato)) return candidato;
                }
            }
            return null;
        }

        /// <summary>Comportamento legado — só usado se o balão não tinha contexto (ex: arranque).</summary>
        private void AbrirDetalhesStockFallback()
        {
            if (janelinhaDetalhes == null || janelinhaDetalhes.IsDisposed)
            {
                janelinhaDetalhes = new FormDetalhesStock();
                janelinhaDetalhes.Show();
            }
            else
            {
                if (janelinhaDetalhes.WindowState == FormWindowState.Minimized)
                    janelinhaDetalhes.WindowState = FormWindowState.Normal;
                janelinhaDetalhes.BringToFront();
            }
        }

        // Método que vai correr sempre que o relógio (Timer) "bater"
        private async void TimerMonitorizacao_Tick(object sender, EventArgs e)
        {
            try
            {
                timerMonitorizacao.Stop(); // Pausa para não encavalar consultas

                using (SqlConnection conn = CONN.GetConnection())
                {
                    await conn.OpenAsync();

                    // ── 1. STOCK BAIXO ────────────────────────────────────────────
                    // Lê o limite configurado em Definicoes; cai para 20 se não existir
                    int limite = 20;
                    string sqlDef = "SELECT Valor FROM Definicoes WHERE Chave = 'StockMinimo'";
                    using (SqlCommand cmdDef = new SqlCommand(sqlDef, conn))
                    {
                        var result = await cmdDef.ExecuteScalarAsync();
                        if (result != null) limite = Convert.ToInt32(result);
                    }

                    string sqlStock = "SELECT COUNT(*) FROM Stock WHERE Quantidade <= @lim AND Estado = 1";
                    using (SqlCommand cmdStock = new SqlCommand(sqlStock, conn))
                    {
                        cmdStock.Parameters.AddWithValue("@lim", limite);
                        int contagem = (int)await cmdStock.ExecuteScalarAsync();

                        if (contagem > 0)
                            MostrarAlerta("PEPIDI - Alerta de Stock",
                                $"Atenção: Há {contagem} {(contagem == 1 ? "item" : "itens")} com stock baixo ou em falta!",
                                TipoNotificacao.StockBaixo);
                    }

                    // ── 2. NOVOS PEDIDOS DE EPI PENDENTES ─────────────────────────
                    // Conta pedidos criados desde a última verificação ainda sem aprovação
                    string sqlPedidos = @"SELECT COUNT(*) FROM PedidoRegistos
                                          WHERE Estado = 'Pendente'
                                            AND CriacaoData > @desde";
                    using (SqlCommand cmdPedidos = new SqlCommand(sqlPedidos, conn))
                    {
                        cmdPedidos.Parameters.AddWithValue("@desde", ultimaVerificacao);
                        int novosPedidos = (int)await cmdPedidos.ExecuteScalarAsync();

                        if (novosPedidos > 0)
                        {
                            string msg = novosPedidos == 1
                                ? "1 novo pedido de EPI aguarda aprovação."
                                : $"{novosPedidos} novos pedidos de EPI aguardam aprovação.";
                            MostrarAlerta("PEPIDI - Novos Pedidos", msg, TipoNotificacao.NovosPedidos, ToolTipIcon.Info);
                        }
                    }
                }

                // Regista o momento desta verificação para o próximo ciclo
                ultimaVerificacao = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Se der erro (ex: BD desligada), o agente fica calado para não chatear
                Console.WriteLine(ex.Message);
            }
            finally
            {
                timerMonitorizacao.Start(); // Retoma o ciclo
            }
        }

        /// <summary>
        /// Mostra um aviso em balão na área de notificação.
        /// O ícone padrão é Warning (triângulo amarelo); passa ToolTipIcon.Info
        /// para alertas informativos (ex: novos pedidos).
        /// O parâmetro <paramref name="tipo"/> é guardado em <see cref="_ultimaNotificacao"/>
        /// para que o handler do clique saiba para onde levar o utilizador.
        /// Nota: Windows 10/11 gerem a duração real do balão pelas suas próprias regras.
        /// </summary>
        private void MostrarAlerta(string titulo, string mensagem, TipoNotificacao tipo, ToolTipIcon icone = ToolTipIcon.Warning)
        {
            _ultimaNotificacao = tipo;
            trayIcon.ShowBalloonTip(30000, titulo, mensagem, icone);
        }

        // Método para fechar o Agente de forma limpa
        private void Sair_Click(object sender, EventArgs e)
        {
            // É fundamental esconder o ícone antes de sair, senão ele fica lá "fantasma" até passares o rato
            trayIcon.Visible = false;
            Application.Exit();
        }
    }
}