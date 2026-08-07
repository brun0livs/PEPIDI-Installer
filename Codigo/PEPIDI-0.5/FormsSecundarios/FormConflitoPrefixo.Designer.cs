namespace PEPIDI.FormsSecundarios
{
    partial class FormConflitoPrefixo
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();

            guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(components);
            tlpRoot     = new System.Windows.Forms.TableLayoutPanel();
            tlpHeader   = new System.Windows.Forms.TableLayoutPanel();
            lblTitulo   = new System.Windows.Forms.Label();
            lblFechar   = new System.Windows.Forms.Label();
            tlpBody     = new System.Windows.Forms.TableLayoutPanel();
            lblMensagem = new System.Windows.Forms.Label();
            lblNovoPrefLabel = new System.Windows.Forms.Label();
            txtNovoPref = new Guna.UI2.WinForms.Guna2TextBox();
            lblSugestao = new System.Windows.Forms.Label();
            lblNota     = new System.Windows.Forms.Label();
            tlpFooter   = new System.Windows.Forms.TableLayoutPanel();
            btnCancelar = new Guna.UI2.WinForms.Guna2Button();
            btnAceitar  = new Guna.UI2.WinForms.Guna2Button();

            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            tlpBody.SuspendLayout();
            tlpFooter.SuspendLayout();
            SuspendLayout();

            // guna2BorderlessForm1
            guna2BorderlessForm1.BorderRadius = 15;
            guna2BorderlessForm1.ContainerControl = this;
            guna2BorderlessForm1.TransparentWhileDrag = true;

            // tlpRoot
            tlpRoot.BackColor = System.Drawing.Color.WhiteSmoke;
            tlpRoot.ColumnCount = 1;
            tlpRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpHeader, 0, 0);
            tlpRoot.Controls.Add(tlpBody,   0, 1);
            tlpRoot.Controls.Add(tlpFooter, 0, 2);
            tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpRoot.Name = "tlpRoot";
            tlpRoot.RowCount = 3;
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));

            // tlpHeader
            tlpHeader.BackColor = System.Drawing.Color.FromArgb(254, 107, 0);
            tlpHeader.ColumnCount = 2;
            tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tlpHeader.Controls.Add(lblTitulo, 0, 0);
            tlpHeader.Controls.Add(lblFechar, 1, 0);
            tlpHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpHeader.Margin = new System.Windows.Forms.Padding(0);
            tlpHeader.Name = "tlpHeader";
            tlpHeader.RowCount = 1;
            tlpHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // lblTitulo
            lblTitulo.Dock = System.Windows.Forms.DockStyle.Fill;
            lblTitulo.Font = new System.Drawing.Font("Roboto Medium", 14F, System.Drawing.FontStyle.Bold);
            lblTitulo.ForeColor = System.Drawing.Color.White;
            lblTitulo.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "⚠  CONFLITO DE PREFIXO";
            lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // lblFechar
            lblFechar.Cursor = System.Windows.Forms.Cursors.Hand;
            lblFechar.Dock = System.Windows.Forms.DockStyle.Fill;
            lblFechar.Font = new System.Drawing.Font("Segoe UI Symbol", 14F, System.Drawing.FontStyle.Bold);
            lblFechar.ForeColor = System.Drawing.Color.White;
            lblFechar.Name = "lblFechar";
            lblFechar.Text = "✕";
            lblFechar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblFechar.Click += lblFechar_Click;

            // tlpBody
            tlpBody.ColumnCount = 2;
            tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            tlpBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tlpBody.Controls.Add(lblMensagem,      0, 0);
            tlpBody.SetColumnSpan(lblMensagem, 2);
            tlpBody.Controls.Add(lblNovoPrefLabel, 0, 1);
            tlpBody.Controls.Add(txtNovoPref,      1, 1);
            tlpBody.Controls.Add(lblSugestao,      0, 2);
            tlpBody.SetColumnSpan(lblSugestao, 2);
            tlpBody.Controls.Add(lblNota,          0, 3);
            tlpBody.SetColumnSpan(lblNota, 2);
            tlpBody.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpBody.Margin = new System.Windows.Forms.Padding(0);
            tlpBody.Name = "tlpBody";
            tlpBody.Padding = new System.Windows.Forms.Padding(25, 18, 25, 10);
            tlpBody.RowCount = 4;
            tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tlpBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));

            // lblMensagem
            lblMensagem.Dock = System.Windows.Forms.DockStyle.Fill;
            lblMensagem.Font = new System.Drawing.Font("Roboto", 11F);
            lblMensagem.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            lblMensagem.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblMensagem.Name = "lblMensagem";
            lblMensagem.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // lblNovoPrefLabel
            lblNovoPrefLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            lblNovoPrefLabel.Font = new System.Drawing.Font("Roboto Medium", 11F, System.Drawing.FontStyle.Bold);
            lblNovoPrefLabel.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            lblNovoPrefLabel.Name = "lblNovoPrefLabel";
            lblNovoPrefLabel.Text = "Novo Prefixo:";
            lblNovoPrefLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // txtNovoPref
            txtNovoPref.AutoRoundedCorners = true;
            txtNovoPref.CustomizableEdges = ce1;
            txtNovoPref.DefaultText = "";
            txtNovoPref.Dock = System.Windows.Forms.DockStyle.Fill;
            txtNovoPref.FocusedState.BorderColor = System.Drawing.Color.FromArgb(243, 108, 33);
            txtNovoPref.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Bold);
            txtNovoPref.ForeColor = System.Drawing.Color.Black;
            txtNovoPref.HoverState.BorderColor = System.Drawing.Color.Gray;
            txtNovoPref.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            txtNovoPref.MaxLength = 2;
            txtNovoPref.Name = "txtNovoPref";
            txtNovoPref.PlaceholderText = "";
            txtNovoPref.ShadowDecoration.CustomizableEdges = ce2;
            txtNovoPref.TextOffset = new System.Drawing.Point(10, 0);
            txtNovoPref.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // lblSugestao
            lblSugestao.Dock = System.Windows.Forms.DockStyle.Fill;
            lblSugestao.Font = new System.Drawing.Font("Roboto", 9.5F, System.Drawing.FontStyle.Italic);
            lblSugestao.ForeColor = System.Drawing.Color.FromArgb(120, 120, 120);
            lblSugestao.Name = "lblSugestao";
            lblSugestao.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // lblNota
            lblNota.Dock = System.Windows.Forms.DockStyle.Fill;
            lblNota.Font = new System.Drawing.Font("Roboto", 9F, System.Drawing.FontStyle.Italic);
            lblNota.ForeColor = System.Drawing.Color.FromArgb(110, 110, 110);
            lblNota.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            lblNota.Name = "lblNota";
            lblNota.Text = "Códigos EPI existentes mantêm o seu prefixo histórico — a alteração afecta apenas códigos futuros.";
            lblNota.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            // tlpFooter
            tlpFooter.ColumnCount = 3;
            tlpFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            tlpFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tlpFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            tlpFooter.Controls.Add(btnCancelar, 1, 0);
            tlpFooter.Controls.Add(btnAceitar,  2, 0);
            tlpFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpFooter.Margin = new System.Windows.Forms.Padding(0);
            tlpFooter.Name = "tlpFooter";
            tlpFooter.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            tlpFooter.RowCount = 1;
            tlpFooter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // btnCancelar
            btnCancelar.BorderRadius = 10;
            btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCancelar.CustomizableEdges = ce3;
            btnCancelar.Dock = System.Windows.Forms.DockStyle.Fill;
            btnCancelar.FillColor = System.Drawing.Color.LightGray;
            btnCancelar.Font = new System.Drawing.Font("Roboto", 11F, System.Drawing.FontStyle.Bold);
            btnCancelar.ForeColor = System.Drawing.Color.Black;
            btnCancelar.Margin = new System.Windows.Forms.Padding(8);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.ShadowDecoration.CustomizableEdges = ce4;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Click += btnCancelar_Click;

            // btnAceitar
            btnAceitar.BorderRadius = 10;
            btnAceitar.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAceitar.CustomizableEdges = ce5;
            btnAceitar.Dock = System.Windows.Forms.DockStyle.Fill;
            btnAceitar.FillColor = System.Drawing.Color.FromArgb(242, 103, 34);
            btnAceitar.Font = new System.Drawing.Font("Roboto", 11F, System.Drawing.FontStyle.Bold);
            btnAceitar.ForeColor = System.Drawing.Color.White;
            btnAceitar.Margin = new System.Windows.Forms.Padding(8);
            btnAceitar.Name = "btnAceitar";
            btnAceitar.ShadowDecoration.CustomizableEdges = ce6;
            btnAceitar.Text = "Aceitar e Trocar";
            btnAceitar.Click += btnAceitar_Click;

            // FormConflitoPrefixo
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(560, 320);
            Controls.Add(tlpRoot);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "FormConflitoPrefixo";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Conflito de Prefixo";

            tlpRoot.ResumeLayout(false);
            tlpHeader.ResumeLayout(false);
            tlpBody.ResumeLayout(false);
            tlpFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.TableLayoutPanel tlpHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblFechar;
        private System.Windows.Forms.TableLayoutPanel tlpBody;
        private System.Windows.Forms.Label lblMensagem;
        private System.Windows.Forms.Label lblNovoPrefLabel;
        private Guna.UI2.WinForms.Guna2TextBox txtNovoPref;
        private System.Windows.Forms.Label lblSugestao;
        private System.Windows.Forms.Label lblNota;
        private System.Windows.Forms.TableLayoutPanel tlpFooter;
        private Guna.UI2.WinForms.Guna2Button btnCancelar;
        private Guna.UI2.WinForms.Guna2Button btnAceitar;
    }
}
