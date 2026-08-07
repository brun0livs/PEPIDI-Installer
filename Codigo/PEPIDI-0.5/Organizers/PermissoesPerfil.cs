using System;
using System.Data.SqlClient;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// Representa o conjunto de permissões de um utilizador da aplicação de gestão.
    /// As permissões vêm da tabela Funcoes e são carregadas uma vez no login,
    /// sendo depois passadas por parâmetro aos UCs que precisam de as consultar.
    /// Um perfil vazio (tudo false, NivelAcesso=3) é o estado mais restritivo possível.
    /// </summary>
    public class PermissoesPerfil
    {
        // NivelAcesso: 0 = super-admin, valores maiores = menos poderes
        public int NivelAcesso { get; set; }
        public bool PodeSubmeter { get; set; }
        public bool PodeVerStock { get; set; }
        public bool PodeInserirStock { get; set; }
        public bool PodeCriarStock { get; set; }
        public bool PodeVerHistorico { get; set; }
        public bool PodeEditarFunc { get; set; }
        public bool PodeAprovar { get; set; }
        public bool PodeEntregar { get; set; }
        public bool PodeCriarFuncoes { get; set; }
        public bool PodeAlterarDefinicoes { get; set; }
        public bool PodeVerUsados { get; set; }

        /// <summary>
        /// Carrega as permissões do funcionário a partir da BD (JOIN Funcionarios + Funcoes).
        /// Todos os flags de permissão tratam DBNull como false — é mais seguro do que
        /// assumir que a coluna tem valor. Se o funcionário não existir, devolve perfil vazio.
        /// </summary>
        public static PermissoesPerfil VerPermissoes(int idFuncionario)
        {
            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT
                    NivelAcesso, PodeVerStock, PodeInserirStock, PodeCriarStock, PodeVerHistorico, PodeEditarFunc,
                    PodeSubmeter, PodeAprovar, PodeEntregar, PodeCriarFuncoes, PodeAlterarDefinicoes, PodeVerUsados
                    FROM Funcionarios FU
                    JOIN Funcoes F ON FU.FuncaoID = F.ID
                    WHERE FU.Nr = @NrFunc", conn);

                cmd.Parameters.AddWithValue("@NrFunc", idFuncionario);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new PermissoesPerfil
                        {
                            // NivelAcesso 3 como fallback — nível mais restritivo por segurança
                            NivelAcesso = reader["NivelAcesso"] != DBNull.Value ? Convert.ToInt32(reader["NivelAcesso"]) : 3,

                            PodeVerStock = reader["PodeVerStock"] != DBNull.Value && Convert.ToBoolean(reader["PodeVerStock"]),
                            PodeInserirStock = reader["PodeInserirStock"] != DBNull.Value && Convert.ToBoolean(reader["PodeInserirStock"]),
                            PodeCriarStock = reader["PodeCriarStock"] != DBNull.Value && Convert.ToBoolean(reader["PodeCriarStock"]),
                            PodeVerHistorico = reader["PodeVerHistorico"] != DBNull.Value && Convert.ToBoolean(reader["PodeVerHistorico"]),
                            PodeEditarFunc = reader["PodeEditarFunc"] != DBNull.Value && Convert.ToBoolean(reader["PodeEditarFunc"]),
                            PodeSubmeter = reader["PodeSubmeter"] != DBNull.Value && Convert.ToBoolean(reader["PodeSubmeter"]),
                            PodeAprovar = reader["PodeAprovar"] != DBNull.Value && Convert.ToBoolean(reader["PodeAprovar"]),
                            PodeEntregar = reader["PodeEntregar"] != DBNull.Value && Convert.ToBoolean(reader["PodeEntregar"]),
                            PodeCriarFuncoes = reader["PodeCriarFuncoes"] != DBNull.Value && Convert.ToBoolean(reader["PodeCriarFuncoes"]),
                            PodeAlterarDefinicoes = reader["PodeAlterarDefinicoes"] != DBNull.Value && Convert.ToBoolean(reader["PodeAlterarDefinicoes"]),
                            PodeVerUsados = reader["PodeVerUsados"] != DBNull.Value && Convert.ToBoolean(reader["PodeVerUsados"])
                        };
                    }
                }
            }

            // Funcionário não encontrado → perfil totalmente restrito (fail-safe)
            return new PermissoesPerfil();
        }
    }
}