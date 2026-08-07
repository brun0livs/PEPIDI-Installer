using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PEPIDI.FormsSecundarios
{
    /// <summary>
    /// Formulário simples para introdução de um motivo de rejeição de pedido.
    /// O campo de texto é obrigatório — não fecha com OK se estiver vazio.
    /// O motivo é exposto via propriedade Motivo para o chamador ler após DialogResult.OK.
    /// </summary>
    public partial class FormMotivo : Form
    {
        /// <summary>Texto do motivo introduzido pelo utilizador; disponível após DialogResult.OK.</summary>
        public string Motivo { get; private set; }
        EfeitoUI M = new EfeitoUI();

        public FormMotivo()
        {
            InitializeComponent();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMotivo.Text))
            {
                M.AbrirMensagem("Por favor, escreva o motivo.", "Falta de Preenchimento");
                return;
            }
            Motivo = txtMotivo.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
