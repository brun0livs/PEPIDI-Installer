using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// Gere a criação e atualização de credenciais de acesso na tabela LogIn.
    /// Usado ao importar ou criar funcionários — garante que cada um começa
    /// com a password predefinida igual ao número mecanográfico, já em hash.
    /// </summary>
    internal class GestorDeLogins
    {
        /// <summary>
        /// Regista ou atualiza o login do funcionário com o número mecanográfico indicado.
        /// A password inicial é o próprio número mecanográfico (o utilizador deve alterá-la
        /// no primeiro acesso via FormNovaPasse). O hash é gerado internamente — a BD
        /// nunca recebe a password em claro.
        /// Lança exceção em caso de erro para que o chamador decida como apresentá-la.
        /// </summary>
        public static void RegistarOuAtualizarLogin(string nrMecanografico)
        {
            try
            {
                Hash H = new();
                string passwordEmHash = H.GerarHashSenha(nrMecanografico);

                using (var conn = GetConn.GetConnection())
                {
                    conn.Open();
                    GetConn.SetContext(conn);

                    using (var ctxCmd = new SqlCommand(
                        "EXEC sys.sp_set_session_context N'PasswordAcao', @v, @readonly = 0", conn))
                    {
                        ctxCmd.Parameters.AddWithValue("@v", "reposta");
                        ctxCmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand("sp_RegistarLogin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Nr", nrMecanografico);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordEmHash);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Propaga o erro — este método é chamado em contextos variados (import em batch,
                // criação individual) e cada um trata o erro à sua maneira
                throw new Exception("Erro ao gerir as credenciais de segurança: " + ex.Message);
            }
        }
    }
}
