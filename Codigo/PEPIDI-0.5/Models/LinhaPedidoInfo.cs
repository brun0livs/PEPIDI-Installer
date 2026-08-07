using System.Collections.Generic;

namespace PEPIDI.Models
{
    /// <summary>
    /// DTO (objeto de transferência) com a informação de uma linha de pedido de EPI.
    /// Usado por MostraRoupa para devolver os dados ao FormPedidos, que os usa para
    /// construir os controlos LinhaPedido no FlowLayoutPanel.
    /// TamanhoAtual é o tamanho registado no perfil do funcionário (pré-selecionado).
    /// TamanhosDisponiveis são os tamanhos em stock para o modelo+cor desta linha.
    /// ModelosDisponiveis são todos os modelos da mesma família (para o combo de modelo).
    /// </summary>
    public class LinhaPedidoInfo
    {
        public string CodigoEpi { get; set; }
        public string Cor { get; set; }
        public string Modelo { get; set; }
        public string TamanhoAtual { get; set; }
        public string Familia { get; set; }
        public string NomeVista { get; set; }
        public List<string> TamanhosDisponiveis { get; set; } = new List<string>();
        public List<string> ModelosDisponiveis { get; set; } = new List<string>();
    }
}
