using iText.Commons.Utils;
using Microsoft.Data.SqlClient;
using PEPIDI.FormsSecundarios;
using PEPIDI.Organizers;
using PEPIDI.Utils;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PEPIDI.UCs
{
    /// <summary>
    /// UC de definições globais da aplicação.
    /// aCarregar=true durante o Load impede que os eventos de mudança de ComboBox/Switch
    /// disparem ações reais enquanto os valores estão a ser lidos da BD/settings — sem isto
    /// o simples carregamento do combo já acionaria um Application.Restart().
    /// A mudança de modo de ecrã é confirmada pelo utilizador antes de reiniciar a app;
    /// o novo modo é guardado em Properties.Settings para ser lido no próximo arranque.
    /// O switch do agente invoca StartupHelper para gerir o registo no Windows e
    /// simultaneamente inicia/mata o processo AgentePEPIDI.exe.
    /// </summary>
    public partial class Definicoes : UserControl
    {
        int IDGestor;
        bool aCarregar = true;
        EfeitoUI M = new();


        public Definicoes(int _IDGestor)
        {
            InitializeComponent();
            IDGestor = _IDGestor;
        }

        private void Definicoes_Load(object sender, EventArgs e)
        {
            if (IDGestor == 1077)
            {
                pnlDefsPrev.Visible = true;
            }
            GestorTema.AplicarEstilos(this);

            // 1. Injetar os itens por código (Assim tens a certeza absoluta que o texto bate certo)
            // Se já os tens no Designer, podes apagar de lá para não haver duplicados.
            cmbDisplay.Items.Clear();
            cmbDisplay.Items.Add("Modo Monitor (1080p)");
            cmbDisplay.Items.Add("Modo Portátil (1366x768)");
            cmbDisplay.Items.Add("Modo Tátil (Tablet/Surface)");

            // 2. Ir buscar a memória (ex: "Surface", "MonitorFullHD", etc)
            string modoGuardado = Properties.Settings.Default.ModoEcraGuardado;

            // 3. Traduzir a memória para o texto exato da ComboBox
            if (modoGuardado == "Surface")
                cmbDisplay.SelectedItem = "Modo Tátil (Tablet/Surface)";
            else if (modoGuardado == "Portatil")
                cmbDisplay.SelectedItem = "Modo Portátil (1366x768)";
            else
                cmbDisplay.SelectedItem = "Modo Monitor (1080p)"; // O defeito

            // 4. Tirar o travão de segurança agora que tudo está carregado
            aCarregar = false;

            bool modoNotif = Properties.Settings.Default.Notificacao;

            // 6. Colocar o Switch na posição correta SEM disparar o evento de mensagem
            // Desativamos o evento temporariamente para não abrir a MessageBox ao carregar o ecrã
            // Garante que o evento está ligado (o designer pode não o ter registado)
            switchAgent.CheckedChanged -= switchAgent_CheckedChanged;
            switchAgent.Checked = modoNotif;
            switchAgent.CheckedChanged += switchAgent_CheckedChanged;
            CarregarDoSQL();
        }

        private void cmbDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Se o ecrã ainda estiver a carregar, ignoramos o evento para não pedir para reiniciar à toa!
            if (aCarregar || cmbDisplay.SelectedItem == null) return;

            string escolha = cmbDisplay.SelectedItem.ToString();
            TipoEcra novoModo = TipoEcra.MonitorFullHD; // Padrão

            // 1. Descobrir qual foi a escolha
            if (escolha.Contains("Tablet") || escolha.Contains("Surface"))
                novoModo = TipoEcra.Surface;
            else if (escolha.Contains("Portátil"))
                novoModo = TipoEcra.Portatil;

            // 2. Perguntar se quer reiniciar agora
            DialogResult resposta = MessageBox.Show(
                "O programa vai reiniciar para aplicar a nova resolução. Queres continuar?",
                "Alterar Resolução",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resposta == DialogResult.Yes)
            {
                // 3. GUARDAR NAS SETTINGS
                Properties.Settings.Default.ModoEcraGuardado = novoModo.ToString();
                Properties.Settings.Default.Save();

                // 4. REINICIAR
                Application.Restart();
                Environment.Exit(0);
            }
            else
            {
                // Se ele cancelar, voltamos a meter o travão e revertemos a ComboBox para o que estava guardado
                aCarregar = true;
                Definicoes_Load(null, null); // Re-executa o Load para meter o valor antigo
            }
        }

        private void btnComp_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog { Description = "Selecione a pasta para os Comprovativos" })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtComprovativos.Text = fbd.SelectedPath;
                    GravarDefinicao("CaminhoComprovativos", fbd.SelectedPath, "String", IDGestor);
                }
            }
        }

        private void btnRel_Click(object sender, EventArgs e)
        {
            // CORREÇÃO: Mudei a descrição para "Relatórios"
            using (var fbd = new FolderBrowserDialog { Description = "Selecione a pasta para os Relatórios" })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtRelatorios.Text = fbd.SelectedPath;
                    GravarDefinicao("CaminhoRelatorios", fbd.SelectedPath, "String", IDGestor);
                }
            }
        }

        public void GravarDefinicao(string chave, string valor, string tipo, int utilizadorAtual)
        {
            using (var conn = GetConn.GetConnection())
            {
                using (var cmd = new SqlCommand("sp_UpsertDefinicao", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Chave", chave);
                    cmd.Parameters.AddWithValue("@Valor", valor);
                    cmd.Parameters.AddWithValue("@Tipo", tipo);
                    cmd.Parameters.AddWithValue("@AlteradoPor", utilizadorAtual);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void CarregarDoSQL()
        {
            try
            {
                using (var conn = GetConn.GetConnection())
                {
                    conn.Open();
                    // Boa prática: Pedir só as colunas que precisamos
                    using (var cmd = new SqlCommand("SELECT Chave, Valor FROM Definicoes", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        // MUDANÇA AQUI: Usamos WHILE para ler todas as definições guardadas
                        while (reader.Read())
                        {
                            string chave = reader["Chave"].ToString();
                            string valor = reader["Valor"].ToString();

                            // Compara a chave e preenche a textbox correspondente
                            switch (chave)
                            {
                                case "CaminhoComprovativos":
                                    txtComprovativos.Text = valor;
                                    break;

                                case "CaminhoRelatorios":
                                    txtRelatorios.Text = valor;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignorar erro se ainda não houver dados, ou se a chave mudar
            }
        }

        private void btnNovaPass_Click(object sender, EventArgs e)
        {
            using (Form overlay = new Form())
            {
                // Configurar o formulário "sombra"
                overlay.StartPosition = FormStartPosition.CenterScreen;
                overlay.WindowState = FormWindowState.Maximized;
                overlay.FormBorderStyle = FormBorderStyle.None; // Sem bordas
                overlay.Opacity = 0.50d;                        // 50% transparente
                overlay.BackColor = Color.Black;                // Cor preta
                overlay.ShowInTaskbar = false;                  // Não aparece na barra de tarefas

                // Faz o overlay cobrir exatamente o formulário atual (this)
                overlay.Location = this.Location;
                overlay.Size = this.Size;

                // Mostra a sombra
                overlay.Show(this);
                using (FormNovaPasse frm = new FormNovaPasse(IDGestor))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {

                    }
                }
            }
        }

        private void switchAgent_CheckedChanged(object sender, EventArgs e)
        {
            // Grava a preferência do utilizador
            Properties.Settings.Default.Notificacao = switchAgent.Checked;
            Properties.Settings.Default.Save();

            if (switchAgent.Checked)
            {
                // Regista no Windows para arrancar no futuro
                StartupHelper.RegistarAgenteNoArranque();

                // E arranca AGORA MESMO!
                IniciarAgenteImediatamente();
            }
            else
            {
                // Remove do arranque do Windows
                StartupHelper.RemoverAgenteDoArranque();

                // E mata o processo que está a correr AGORA MESMO!
                EncerrarAgenteEmSegundoPlano();
            }
        }

        // Método auxiliar para procurar e encerrar o processo do agente
        private void EncerrarAgenteEmSegundoPlano()
        {
            try
            {
                // Procura todos os processos a correr com o nome do teu agente (sem o ".exe")
                Process[] processosAgente = Process.GetProcessesByName("AgentePEPIDI");

                foreach (Process processo in processosAgente)
                {
                    processo.Kill(); // Força o encerramento do processo
                    processo.WaitForExit(); // Aguarda que o encerramento seja concluído com segurança
                }
            }
            catch (Exception ex)
            {
                // Em software robusto, nunca deixamos um erro passar em branco.
                MessageBox.Show("Aviso: Não foi possível encerrar o agente em segundo plano.\nErro: " + ex.Message,
                                "Gestão de Processos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void IniciarAgenteImediatamente()
        {
            try
            {
                string caminhoAgente = EncontrarExeAgente();
                if (caminhoAgente != null)
                {
                    if (Process.GetProcessesByName("AgentePEPIDI").Length == 0)
                        Process.Start(caminhoAgente);
                }
                else
                {
                    MessageBox.Show("Não foi possível encontrar o executável do agente (AgentePEPIDI.exe).\n\n" +
                                    "Certifica-te de que o projeto AgentePEPIDI está compilado.",
                                    "Agente Não Encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao tentar iniciar o agente:\n" + ex.Message,
                                "Erro de Execução", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string EncontrarExeAgente()
        {
            const string nomeAgente = "AgentePEPIDI.exe";

            // 1. Produção: mesmo diretório do PEPIDI.exe
            string mesmaPasta = Path.Combine(Application.StartupPath, nomeAgente);
            if (File.Exists(mesmaPasta)) return mesmaPasta;

            // 2. Desenvolvimento: sobe a árvore até encontrar a pasta irmã AgentePEPIDI
            string dir = Application.StartupPath;
            for (int i = 0; i < 8; i++)
            {
                dir = Path.GetDirectoryName(dir);
                if (dir == null) break;

                string[] candidatos = {
                    Path.Combine(dir, "AgentePEPIDI", "bin", "Debug", nomeAgente),
                    Path.Combine(dir, "AgentePEPIDI", "bin", "Release", nomeAgente),
                    Path.Combine(dir, "AgentePEPIDI", nomeAgente),
                };
                foreach (string c in candidatos)
                    if (File.Exists(c)) return c;
            }
            return null;
        }

        // ====================================================================================
        // GESTÃO DE CÓDIGOS DE EPI
        // ====================================================================================

        /// <summary>
        /// Abre o gestor dedicado de Famílias / Prefixos / Códigos EPI.
        /// O <see cref="PEPIDI.FormsSecundarios.FormGestaoCodigos"/> é modal e fecha
        /// devolvendo OK se houve commit em BD ou Cancel se o utilizador desistiu.
        /// </summary>
        private void btnGestaoCodigos_Click(object sender, EventArgs e)
        {
            // 1. Descobrir quem é o "Pai" (O Form principal que está aberto)
            Form pai = this.FindForm();

            // 2. Criar a Sombra (Overlay)
            using (Form overlay = new Form())
            {
                // Configuração da sombra
                overlay.StartPosition = FormStartPosition.Manual;
                overlay.FormBorderStyle = FormBorderStyle.None;
                overlay.Opacity = 0.50d;
                overlay.BackColor = Color.Black;
                overlay.ShowInTaskbar = false;

                if (pai != null)
                {
                    overlay.Location = pai.Location;
                    overlay.Size = pai.Size;
                    overlay.Show(pai);
                }
                else
                {
                    overlay.WindowState = FormWindowState.Maximized;
                    overlay.Show();
                }

                // 3. Abrir o teu FormGestaoDeFiltros POR CIMA da sombra
                using (PEPIDI.FormsSecundarios.FormGestaoCodigos frm = new PEPIDI.FormsSecundarios.FormGestaoCodigos())
                {
                    // Passamos o 'overlay' para o ShowDialog, para ele saber que tem de ficar colado à sombra
                    frm.ShowDialog(overlay);
                }

                // 4. Fechar a sombra logo a seguir ao FormGestaoDeFiltros ser fechado
                overlay.Close();
            }
        }

    }
}