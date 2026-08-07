using PEPIDI.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace PEPIDI.Organizers
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            EfeitoUI M = new();

            // Prioridade alta para evitar lag perceptível em máquinas com carga elevada
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Restaura o modo de ecrã que o utilizador escolheu na última sessão.
            // Se o valor guardado não corresponder ao enum (ex: instalação nova), usa FullHD como base segura.
            string memoriaEcra = Properties.Settings.Default.ModoEcraGuardado;
            if (Enum.TryParse(memoriaEcra, out TipoEcra modoLido))
            {
                GestorTema.ModoAtual = modoLido;
            }
            else
            {
                GestorTema.ModoAtual = TipoEcra.MonitorFullHD;
            }

            // O ficheiro conn.bin vive em %AppData%\PEPIDI\ — fora da pasta da app,
            // para sobreviver a atualizações do executável sem perder a configuração.
            string pastaApp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PEPIDI");
            string caminhoArquivoBin = Path.Combine(pastaApp, "conn.bin");

            if (!Directory.Exists(pastaApp))
                Directory.CreateDirectory(pastaApp);

            string connString = "";

            if (File.Exists(caminhoArquivoBin))
            {
                // Instalação já configurada: desencripta e usa diretamente
                connString = Criptografia.DesencriptarDeFicheiro(caminhoArquivoBin);
            }
            else
            {
                // Primeira vez (ou depois de apagar o ficheiro): pede os dados da BD ao utilizador
                using FormConfigDB frm = new();
                frm.Text = "Configurar Conexão com a Base de Dados";

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    connString = frm.ConnectionStringFinal;
                    // Guarda encriptado para a próxima sessão e para o AgentePEPIDI usar
                    Criptografia.EncriptarParaFicheiro(connString, caminhoArquivoBin);
                }
                else
                {
                    M.AbrirMensagem("Configuração da Base de Dados cancelada.\nA encerrar...", "Erro");
                    return;
                }
            }

            // Injeta a string numa propriedade estática — o resto da app usa GetConn.ConnectionString
            // sem precisar de saber como ela foi obtida
            GetConn.ConnectionString = connString;

            Application.Run(new FrmLogIn());
        }
    }
}