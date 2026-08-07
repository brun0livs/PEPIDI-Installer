using Microsoft.Data.SqlClient;

namespace AgentePEPIDI
{
    /// <summary>
    /// Ponto central de acesso à base de dados do AgentePEPIDI.
    /// A string de ligação é injetada no arranque após ser desencriptada
    /// do ficheiro conn.bin; a partir daí, <see cref="GetConnection"/> é
    /// chamado sempre que é necessário abrir uma ligação SQL.
    /// </summary>
    public static class CONN
    {
        /// <summary>String de ligação ao SQL Server, preenchida no arranque.</summary>
        public static string ConnectionString { get; set; }

        /// <summary>Cria e devolve uma nova <see cref="SqlConnection"/> com a string configurada.</summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
