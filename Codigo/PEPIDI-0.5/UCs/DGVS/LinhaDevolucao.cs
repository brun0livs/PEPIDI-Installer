using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PEPIDI.UCs.DGVS
{
    /// <summary>
    /// UserControl que representa uma linha de devolução no painel de detalhes de pedido.
    /// Mostra o artigo, tamanho e quantidade a devolver, e um combo para o gestor indicar
    /// o estado em que o artigo regressa ao armazém (Novo / Usado / Gasto).
    /// O evento EstadoAlterado notifica PedidosDetalhes quando o estado muda,
    /// para que as notas automáticas sejam geradas em tempo real.
    /// </summary>
    public partial class LinhaDevolucao : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IDEPI { get; set; }

        public string DescricaoArtigo => lblModelo.Text;
        public string TamanhoSelecionado => lblTamanho.Text;
        public int QuantidadeDevolvida => Convert.ToInt32(lblQuantDevolvida.Text);

        /// <summary>Estado selecionado no combo; "Usado" como fallback se o combo não existir.</summary>
        public string EstadoSelecionado
        {
            get { return cmbEstado != null ? cmbEstado.Text : "Usado"; }
        }

        /// <summary>Dispara quando o utilizador altera o estado no combo (Novo/Usado/Gasto).</summary>
        public event EventHandler EstadoAlterado;

        public LinhaDevolucao(int idEpi, string modelo, string tamanho, string estado, int quantDevolvida)
        {
            InitializeComponent();

            this.IDEPI = idEpi;
            lblModelo.Text = modelo;
            lblTamanho.Text = tamanho;
            lblQuantDevolvida.Text = quantDevolvida.ToString();

            if (cmbEstado != null)
            {
                cmbEstado.Items.Clear();
                cmbEstado.Items.Add("Novo");
                cmbEstado.Items.Add("Usado");
                cmbEstado.Items.Add("Gasto");

                // O estado vem da BD; se não houver, assume "Usado" (caso mais comum)
                cmbEstado.Text = string.IsNullOrEmpty(estado) ? "Usado" : estado;

                cmbEstado.SelectedIndexChanged += CmbEstado_SelectedIndexChanged;
            }
        }

        private void CmbEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            EstadoAlterado?.Invoke(this, EventArgs.Empty);
        }
    }
}