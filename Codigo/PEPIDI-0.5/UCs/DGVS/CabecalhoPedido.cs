using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PEPIDI.UCs.UcsSecundarios
{
    /// <summary>
    /// Cabeçalho visual da lista de linhas de pedido (LinhaItem) em PedidosDetalhes.
    /// Adapta os títulos das colunas consoante o estado do pedido:
    /// em modo "Aprovado" os labels mudam para "Quantidade" e "Selecionar",
    /// refletindo que a ação do gestor já não é aprovar mas sim confirmar a entrega.
    /// </summary>
    public partial class CabecalhoPedido : UserControl
    {
        public CabecalhoPedido(string estado)
        {
            InitializeComponent();
            GereEstado(estado);
        }

        private void GereEstado(string estado)
        {
            if (estado == "Aprovado")
            {
                // No modo de entrega, "QuantDisp" passa a mostrar a quantidade aprovada
                // e "Quant" torna-se a checkbox de confirmação de entrega
                lblQuantDisp.Text = "Quantidade";
                lblQuant.Text = "Selecionar";
            }
            // Para outros estados (Pendente, Finalizado) os labels do Designer ficam inalterados
        }
    }
}
