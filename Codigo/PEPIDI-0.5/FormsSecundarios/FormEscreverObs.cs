using System;
using System.Windows.Forms;

namespace PEPIDI.FormsSecundarios
{
    /// <summary>
    /// Formulário simples para escrever ou editar observações de um pedido.
    /// Pré-preenche o TextBox com as notas existentes (se houver).
    /// A propriedade Notas expõe o texto final para o chamador ler após DialogResult.OK.
    /// </summary>
    public partial class FormEscreverObs : Form
    {
        /// <summary>Texto final das observações após o utilizador fechar com OK.</summary>
        public string Notas => txtNotas.Text;

        public FormEscreverObs(string notasExistentes = "")
        {
            InitializeComponent();
            txtNotas.Text = notasExistentes;
            lblFechar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            btnGuardar.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {

        }
    }
}
