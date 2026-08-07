using PEPIDI;
using PEPIDI.Organizers;
using PEPIDI.Utils;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Security.Policy;

namespace PEPIDI
{
    /// <summary>
    /// Formulário de login. Aceita o NMEC (número mecanográfico) e a password.
    /// Entrar() compara o hash SHA-256 da password introduzida com o valor guardado na tabela LogIn.
    /// Após autenticação bem-sucedida, decide o destino com base nas permissões:
    ///   - PodeSubmeter=true → FormPedidos (funcionário normal a pedir fardamento)
    ///   - PodeSubmeter=false → FormGestao (gestor/RH/programador)
    /// O ícone do olho no campo de password alterna UseSystemPasswordChar para revelar/esconder.
    /// Enter no campo de password dispara o clique no botão de login (SuppressKeyPress evita o beep).
    /// </summary>
    public partial class FrmLogIn : Form
    {
        readonly PEPIDI.Organizers.Hash hash = new();
        EfeitoUI M = new EfeitoUI();
        public FrmLogIn()
        {
            InitializeComponent();
            GestorTema.AplicarEstilos(this);
        }

        private void BtnLogIn_Click(object sender, EventArgs e)
        {
            try
            {
                var userTxt = txtUser.Text;
                var passTxt = pbPass.Text;

                // Deteção de registo de dispositivo: username com '|' ou '\\' é um comando interno
                // usado em contextos de debug/setup, não deve aparecer em uso normal
                if (!string.IsNullOrEmpty(userTxt) && userTxt.Any(c => c == '|' || c == '\\'))
                {

                    string nomePc = Environment.MachineName;
                    int idFunc = Convert.ToInt32(userTxt);
                    using SqlConnection conn = GetConn.GetConnection();
                    using SqlCommand cmd = new("sp_RegistaLoginDispositivo", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDFuncionario", idFunc);
                    cmd.Parameters.AddWithValue("@NomePC", nomePc);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    M.AbrirMensagem("Dispositivo registado com sucesso.", "Sucesso");
                }
                Entrar(Convert.ToInt32(userTxt), passTxt);
            }
            catch (Exception ex) { M.AbrirMensagem($"Valores Inválidos: {ex.Message}", "Erro"); }
        }

        private void Entrar(int user, string pass)
        {
            // Gera o hash antes de abrir a ligação para não bloquear o UI desnecessariamente
            string HPass = hash.GerarHashSenha(pass);
            Debug.WriteLine(HPass);

            using SqlConnection connection = new(GetConn.ConnectionString);
            try
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Login WHERE Nr = @username AND Password = @password";

                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@username", user);
                command.Parameters.AddWithValue("@password", HPass);

                object result = command.ExecuteScalar();

                // COUNT(*) nunca é null — o ternário é só uma salvaguarda defensiva
                int count = (result != null) ? Convert.ToInt32(result) : 0;

                if (count > 0)
                {
                    txtUser.Text = "";
                    pbPass.Text = "";

                    var permissoes = PermissoesPerfil.VerPermissoes(user);

                    // Popula a sessão global antes de abrir qualquer formulário filho
                    Sessao.IdFuncionarioAtual = user;
                    Sessao.NivelAcessoAtual = permissoes.NivelAcesso;
                    // Regista quem está autenticado para os triggers de AuditLog (SESSION_CONTEXT)
                    GetConn.NrFuncAtual = user;

                    if (permissoes.PodeSubmeter)
                    {
                        AbreFormUserPedido(user);
                    }
                    else
                    {
                        AbreFormUserGestor(user, permissoes);
                    }
                }
                else
                {
                    M.AbrirMensagem("Credenciais inválidas. Verifique o NMEC e a Password.", "Erro");
                    pbPass.Text = "";
                    pbPass.Focus();
                }
            }
            catch (SqlException ex)
            {
                M.AbrirMensagem("Erro de ligação à base de dados:\n" + ex.Message, "Erro de SQL");
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Ocorreu um erro inesperado:\n" + ex.Message, "Erro Crítico");
            }
        }


        private void AbreFormUserPedido(int NrFunc)
        {
            try
            {
                FormPedidos frm = new FormPedidos(NrFunc, this);
                frm.ShowDialog();
                // Devolve o foco ao campo de utilizador para o próximo login sem tocar no rato
                txtUser.Focus();
            }
            catch (Exception ex)
            {
                M.AbrirMensagem($"Erro ao abrir o formulário: {ex.Message}", "Erro");
            }
        }

        private void AbreFormUserGestor(int NrFunc, PermissoesPerfil perms)
        {
            try
            {
                Form frm = new FormGestao(NrFunc, perms);
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                M.AbrirMensagem($"Erro ao abrir o formulário: {ex.Message}", "Erro");
            }
        }


        private void PbPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogIn.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true; // sem beep
            }
        }

        private void PbPass_IconRightClick(object sender, EventArgs e)
        {
            // Alterna visibilidade da password; ao revelar, força a fonte Roboto
            // porque a fonte de asteriscos do sistema pode ter tamanho diferente
            if (pbPass.UseSystemPasswordChar)
            {
                pbPass.UseSystemPasswordChar = false;
                pbPass.IconRight = PEPIDI.Properties.Resources.eye_off;
                pbPass.Font = new Font("Roboto", 11, FontStyle.Regular);
            }
            else
            {
                pbPass.UseSystemPasswordChar = true;
                pbPass.IconRight = PEPIDI.Properties.Resources.eye_on;
            }
        }

        private void FrmLogIn_Load(object sender, EventArgs e)
        {

        }
    }
}
