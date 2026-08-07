using Microsoft.Data.SqlClient;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// Ponto central de acesso à ligação SQL Server da aplicação.
    /// A ConnectionString é preenchida uma única vez no arranque (Program.cs)
    /// depois de desencriptar o ficheiro conn.bin — o resto da app nunca
    /// precisa de saber onde a string vem ou como está guardada.
    /// </summary>
    public static class GetConn
    {
        /// <summary>
        /// String de ligação ativa. Atribuída em Program.cs no arranque;
        /// null enquanto a app ainda não foi configurada.
        /// </summary>
        public static string ConnectionString { get; set; }

        /// <summary>
        /// Cria e devolve uma nova SqlConnection com a string de ligação global.
        /// Cada chamador é responsável por fechar/dispor a ligação (usar 'using').
        /// Não é uma ligação persistente — o SqlClient trata do pooling internamente.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        /// <summary>
        /// Nr do utilizador autenticado atualmente. Definido em FormLogIn após login
        /// bem-sucedido. Usado por SetContext() para identificar quem fez cada operação
        /// nos triggers de AuditLog via SESSION_CONTEXT do SQL Server.
        /// </summary>
        public static int NrFuncAtual { get; set; } = 0;

        /// <summary>
        /// Define SESSION_CONTEXT('NrFunc') na conexão SQL já aberta.
        /// Chamar logo após conn.Open() em qualquer operação que gere auditoria.
        /// Seguro de chamar múltiplas vezes na mesma sessão (readonly = false).
        /// Se NrFuncAtual não estiver definido (≤ 0), não faz nada — triggers
        /// usarão AlteradoPor = 0 (sistema/desconhecido) como fallback.
        /// </summary>
        public static void SetContext(SqlConnection conn)
        {
            if (NrFuncAtual <= 0) return;
            using var cmd = new SqlCommand(
                "EXEC sys.sp_set_session_context N'NrFunc', @nr, @readonly = 0", conn);
            cmd.Parameters.AddWithValue("@nr", NrFuncAtual);
            cmd.ExecuteNonQuery();
        }
    }
}
