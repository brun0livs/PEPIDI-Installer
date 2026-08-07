using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// Repositório de dados de funcionários. Centraliza a pesquisa e lookup
    /// de informação básica (nome e função) para evitar SQL espalhado pelos UCs.
    /// </summary>
    public class MostrarFuncionarios
    {
        EfeitoUI M = new EfeitoUI();

        /// <summary>
        /// Devolve um DataTable com os funcionários que correspondem ao termo de pesquisa.
        /// O parâmetro @Termo é passado à SP sp_ProcurarFuncionarios que trata o LIKE internamente.
        /// Se pesquisa for vazio, a SP devolve todos os funcionários ativos.
        /// </summary>
        public DataTable CarregarFuncionarios(string pesquisa = "")
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ProcurarFuncionarios", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Passa o texto da textbox para o SQL
                cmd.Parameters.AddWithValue("@Termo", pesquisa);

                SqlDataAdapter da = new SqlDataAdapter(cmd);

                try
                {
                    conn.Open();
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    M.AbrirMensagem("Erro ao pesquisar: " + ex.Message, "Erro");
                }
            }
            return dt;
        }

        /// <summary>
        /// Devolve o nome e a função de um funcionário pelo número mecanográfico.
        /// Retorna strings vazias se não existir — o chamador deve verificar antes de usar.
        /// </summary>
        public void BuscarNomeEFuncao(int nr, out string nome, out string funcao)
        {
            nome = string.Empty;
            funcao = string.Empty;

            using (SqlConnection conn = GetConn.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_BuscaInfoFuncAtivo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nr", nr);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nome = reader["Nome"].ToString();
                        funcao = reader["Funcao"].ToString();
                    }
                }
            }
        }
    }
}
