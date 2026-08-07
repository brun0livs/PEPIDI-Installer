using System;
using System.Linq;
using System.Windows.Forms;
using PEPIDI.Utils;

namespace PEPIDI.FormsSecundarios
{
    /// <summary>
    /// Modal de resolução de conflito de Prefixo na gestão de Famílias.
    /// É chamado pelo <see cref="FormGestaoCodigos"/> quando o utilizador tenta atribuir
    /// um prefixo X a uma família mas X já pertence a outra família. Aqui o utilizador
    /// escolhe o novo prefixo para a família deslocada — sugerido como o próximo livre.
    /// </summary>
    public partial class FormConflitoPrefixo : Form
    {
        private readonly EfeitoUI M = new();

        /// <summary>Novo prefixo aceite pelo utilizador (apenas válido se DialogResult == OK).</summary>
        public string NovoPrefixo { get; private set; }

        /// <param name="prefixoColidido">Prefixo que está em conflito (ex: "02").</param>
        /// <param name="familiaIntruso">Família que está a tomar o prefixo.</param>
        /// <param name="familiaDeslocada">Família que tem de mudar de prefixo.</param>
        /// <param name="sugestao">Próximo prefixo livre sugerido (ex: "09").</param>
        public FormConflitoPrefixo(string prefixoColidido, string familiaIntruso, string familiaDeslocada, string sugestao)
        {
            InitializeComponent();

            lblMensagem.Text =
                $"O prefixo '{prefixoColidido}' está a ser atribuído à família " +
                $"'{familiaIntruso}', mas já pertence a '{familiaDeslocada}'.\n\n" +
                $"Indica o novo prefixo para '{familiaDeslocada}':";

            txtNovoPref.Text = sugestao;
            lblSugestao.Text = $"(sugestão: '{sugestao}' — próximo prefixo livre)";

            GestorTema.AplicarEstilos(this);
        }

        private void lblFechar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnAceitar_Click(object sender, EventArgs e)
        {
            string novo = (txtNovoPref.Text ?? "").Trim();
            if (novo.Length != 2 || !novo.All(char.IsDigit))
            {
                M.AbrirMensagem("O novo prefixo tem de ter exactamente 2 dígitos numéricos (ex: 09).", "Prefixo Inválido");
                txtNovoPref.Focus();
                return;
            }

            NovoPrefixo = novo;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
