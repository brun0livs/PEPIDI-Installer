using PEPIDI.Organizers;
using System;
using Microsoft.Data.SqlClient;

namespace PEPIDI.Models
{
    /// <summary>
    /// Helper estático para obter informação resumida de um funcionário (nome + função).
    /// Centraliza este lookup para não duplicar a mesma query em FormGestao, FormPedidos, etc.
    /// </summary>
    internal static class Details
    {
        /// <summary>
        /// Devolve o nome e a função do funcionário com o número indicado.
        /// Usa a SP sp_BuscaInfoFuncAtivo que só devolve funcionários ativos.
        /// Se não encontrar (ex: nr inválido ou inativo), devolve ("Desconhecido", "Desconhecido")
        /// para evitar NullReferenceException nos chamadores.
        /// </summary>
        public static (string Nome, string Funcao) GetInfoGestor(int nr)
        {
            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_BuscaInfoFuncAtivo", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nr", nr);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string nome = reader["Nome"].ToString();
                            string funcao = reader["Funcao"].ToString();
                            return (nome, funcao);
                        }
                    }
                }
            }

            return ("Desconhecido", "Desconhecido");
        }
    }
}
