using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// Gere as operações sobre pedidos: carregar listas e reprovar.
    /// O fluxo de aprovação e finalização vive em PedidosDetalhes.btnAprovar_Click.
    /// </summary>
    internal class GestorDePedidos
    {
        EfeitoUI M = new EfeitoUI();

        /// <summary>
        /// Devolve todos os pedidos no estado indicado ("Pendente", "Aprovado", "Finalizado").
        /// O resultado é mapeado pelas colunas definidas no Designer do UC Pedidos.
        /// </summary>
        public DataTable CarregarPedidosPorEstado(string estado)
        {
            using (SqlConnection conn = GetConn.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_CarregarPedidosPorEstado", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Estado", estado);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Rejeita o pedido: muda o estado para 'Rejeitado' e limpa os campos
        /// de aprovação/entrega/PDF para evitar dados residuais de um estado anterior.
        /// Não usa transação porque é uma operação atómica de uma única tabela.
        /// </summary>
        public void Reprovar(int idPedido, int idReprovador, RichTextBox txtObservacoes)
        {
            using (SqlConnection conn = GetConn.GetConnection())
            {
                conn.Open();
                GetConn.SetContext(conn);

                using (SqlCommand cmd = new SqlCommand(@"UPDATE PedidoRegistos SET Estado = 'Rejeitado', AprovadoPor = @Reprovador, EntregadoPor = NULL, CaminhoPDF = '-', Notas = @Notas WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", idPedido);
                    cmd.Parameters.AddWithValue("@Reprovador", idReprovador);
                    cmd.Parameters.AddWithValue("@Notas", txtObservacoes.Text);

                    cmd.ExecuteNonQuery();
                }

                M.AbrirMensagem("Pedido reprovado com sucesso!", "Reprovado");
            }
        }
    }
}
