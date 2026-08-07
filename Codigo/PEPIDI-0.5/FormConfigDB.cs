using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PEPIDI
{
    /// <summary>
    /// Formulário de configuração da ligação à base de dados.
    /// Suporta Windows Authentication (Integrated Security) e SQL Server Authentication.
    /// Quando se usa Windows Auth, os campos de utilizador e password ficam desativados.
    /// TrustServerCertificate=True é sempre incluído para evitar erros de certificado
    /// em ambientes internos sem CA configurada — aceitável porque a ligação é local.
    /// A connection string final fica exposta via ConnectionStringFinal para o chamador
    /// gravar no conn.bin após encriptação.
    /// </summary>
    public partial class FormConfigDB : Form
    {
        /// <summary>Connection string construída após clicar em Guardar; vazia se o form fechar sem gravar.</summary>
        public string ConnectionStringFinal { get; private set; } = string.Empty;
        EfeitoUI M = new EfeitoUI();
        public FormConfigDB()
        {
            InitializeComponent();
        }

        private void ChkWinAuth_CheckedChanged(object sender, EventArgs e)
        {
            bool usarWindowsAuth = chkWinAuth.Checked;
            // Desativa os campos de credenciais quando se usa autenticação do Windows
            txtUser.Enabled = !usarWindowsAuth;
            txtPass.Enabled = !usarWindowsAuth;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            string servidor = txtServidor.Text.Trim();
            string baseDados = txtBaseDados.Text.Trim();

            if (string.IsNullOrWhiteSpace(servidor) || string.IsNullOrWhiteSpace(baseDados))
            {
                M.AbrirMensagem("Servidor e Base de Dados são obrigatórios.", "Informação");
                return;
            }

            if (chkWinAuth.Checked)
            {
                // ADICIONADO O TrustServerCertificate=True AQUI!
                ConnectionStringFinal = $"Server={servidor};Database={baseDados};Integrated Security=True;TrustServerCertificate=True;";
            }
            else
            {
                string utilizador = txtUser.Text.Trim();
                string senha = txtPass.Text.Trim();

                if (string.IsNullOrWhiteSpace(utilizador) || string.IsNullOrWhiteSpace(senha))
                {
                    M.AbrirMensagem("Preencha o utilizador e a palavra-passe.", "Informação");
                    return;
                }

                // ADICIONADO O TrustServerCertificate=True AQUI TAMBÉM!
                ConnectionStringFinal = $"Server={servidor};Database={baseDados};User Id={utilizador};Password={senha};TrustServerCertificate=True;";
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
