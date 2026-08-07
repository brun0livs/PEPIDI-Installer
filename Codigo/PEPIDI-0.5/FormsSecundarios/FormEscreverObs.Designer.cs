namespace PEPIDI.FormsSecundarios
{
    partial class FormEscreverObs
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            tlpMain = new TableLayoutPanel();
            tlpHeader = new TableLayoutPanel();
            lblTitulo = new Label();
            lblFechar = new Label();
            tlpContent = new TableLayoutPanel();
            txtNotas = new Guna.UI2.WinForms.Guna2TextBox();
            tlpButtons = new TableLayoutPanel();
            btnCancelar = new Guna.UI2.WinForms.Guna2Button();
            btnGuardar = new Guna.UI2.WinForms.Guna2Button();
            guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(components);
            tlpMain.SuspendLayout();
            tlpHeader.SuspendLayout();
            tlpContent.SuspendLayout();
            tlpButtons.SuspendLayout();
            SuspendLayout();
            // 
            // tlpMain
            // 
            tlpMain.BackColor = Color.White;
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.Controls.Add(tlpHeader, 0, 0);
            tlpMain.Controls.Add(tlpContent, 0, 1);
            tlpMain.Controls.Add(tlpButtons, 0, 2);
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.Location = new Point(0, 0);
            tlpMain.Margin = new Padding(0);
            tlpMain.Name = "tlpMain";
            tlpMain.RowCount = 3;
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            tlpMain.Size = new Size(800, 450);
            tlpMain.TabIndex = 0;
            // 
            // tlpHeader
            // 
            tlpHeader.BackColor = Color.FromArgb(243, 108, 33);
            tlpHeader.ColumnCount = 2;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tlpHeader.Controls.Add(lblTitulo, 0, 0);
            tlpHeader.Controls.Add(lblFechar, 1, 0);
            tlpHeader.Dock = DockStyle.Fill;
            tlpHeader.Location = new Point(0, 0);
            tlpHeader.Margin = new Padding(0);
            tlpHeader.Name = "tlpHeader";
            tlpHeader.RowCount = 1;
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpHeader.Size = new Size(800, 67);
            tlpHeader.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Roboto Medium", 18.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(10, 0);
            lblTitulo.Margin = new Padding(10, 0, 0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(710, 67);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "ESCREVER OBSERVAÇÕES";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFechar
            // 
            lblFechar.Cursor = Cursors.Hand;
            lblFechar.Dock = DockStyle.Fill;
            lblFechar.Font = new Font("Roboto", 18.75F);
            lblFechar.ForeColor = Color.White;
            lblFechar.Location = new Point(720, 0);
            lblFechar.Margin = new Padding(0);
            lblFechar.Name = "lblFechar";
            lblFechar.Size = new Size(80, 67);
            lblFechar.TabIndex = 1;
            lblFechar.Text = "✕";
            lblFechar.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tlpContent
            // 
            tlpContent.ColumnCount = 1;
            tlpContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpContent.Controls.Add(txtNotas, 0, 0);
            tlpContent.Dock = DockStyle.Fill;
            tlpContent.Location = new Point(0, 67);
            tlpContent.Margin = new Padding(0);
            tlpContent.Name = "tlpContent";
            tlpContent.RowCount = 1;
            tlpContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpContent.Size = new Size(800, 315);
            tlpContent.TabIndex = 1;
            // 
            // txtNotas
            // 
            txtNotas.BorderColor = Color.FromArgb(224, 224, 224);
            txtNotas.BorderRadius = 10;
            txtNotas.CustomizableEdges = customizableEdges1;
            txtNotas.DefaultText = "";
            txtNotas.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtNotas.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtNotas.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtNotas.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtNotas.Dock = DockStyle.Fill;
            txtNotas.FocusedState.BorderColor = Color.FromArgb(243, 108, 33);
            txtNotas.Font = new Font("Roboto", 12F);
            txtNotas.HoverState.BorderColor = Color.FromArgb(255, 192, 128);
            txtNotas.Location = new Point(20, 20);
            txtNotas.Margin = new Padding(20);
            txtNotas.Multiline = true;
            txtNotas.Name = "txtNotas";
            txtNotas.PlaceholderForeColor = Color.Silver;
            txtNotas.PlaceholderText = "Escreve aqui as tuas observações...";
            txtNotas.SelectedText = "";
            txtNotas.ShadowDecoration.CustomizableEdges = customizableEdges2;
            txtNotas.Size = new Size(760, 275);
            txtNotas.TabIndex = 0;
            // 
            // tlpButtons
            // 
            tlpButtons.BackColor = SystemColors.Control;
            tlpButtons.ColumnCount = 4;
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpButtons.Controls.Add(btnCancelar, 0, 0);
            tlpButtons.Controls.Add(btnGuardar, 3, 0);
            tlpButtons.Dock = DockStyle.Fill;
            tlpButtons.Location = new Point(0, 382);
            tlpButtons.Margin = new Padding(0);
            tlpButtons.Name = "tlpButtons";
            tlpButtons.RowCount = 1;
            tlpButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpButtons.Size = new Size(800, 68);
            tlpButtons.TabIndex = 2;
            // 
            // btnCancelar
            // 
            btnCancelar.BorderRadius = 10;
            btnCancelar.CustomizableEdges = customizableEdges3;
            btnCancelar.DisabledState.BorderColor = Color.DarkGray;
            btnCancelar.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnCancelar.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnCancelar.Dock = DockStyle.Fill;
            btnCancelar.FillColor = Color.Silver;
            btnCancelar.Font = new Font("Roboto", 14F);
            btnCancelar.ForeColor = Color.Black;
            btnCancelar.Location = new Point(10, 10);
            btnCancelar.Margin = new Padding(10);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnCancelar.Size = new Size(180, 48);
            btnCancelar.TabIndex = 1;
            btnCancelar.Text = "Cancelar";
            // 
            // btnGuardar
            // 
            btnGuardar.BorderRadius = 10;
            btnGuardar.CustomizableEdges = customizableEdges5;
            btnGuardar.DisabledState.BorderColor = Color.DarkGray;
            btnGuardar.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnGuardar.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnGuardar.Dock = DockStyle.Fill;
            btnGuardar.FillColor = Color.FromArgb(243, 108, 33);
            btnGuardar.Font = new Font("Roboto", 14F);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(610, 10);
            btnGuardar.Margin = new Padding(10);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.ShadowDecoration.CustomizableEdges = customizableEdges6;
            btnGuardar.Size = new Size(180, 48);
            btnGuardar.TabIndex = 2;
            btnGuardar.Text = "Guardar";
            btnGuardar.Click += btnGuardar_Click;
            // 
            // guna2BorderlessForm1
            // 
            guna2BorderlessForm1.BorderRadius = 15;
            guna2BorderlessForm1.ContainerControl = this;
            guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            guna2BorderlessForm1.TransparentWhileDrag = true;
            // 
            // FormEscreverObs
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tlpMain);
            Font = new Font("Roboto", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4);
            Name = "FormEscreverObs";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FormEscreverObs";
            TopMost = true;
            tlpMain.ResumeLayout(false);
            tlpHeader.ResumeLayout(false);
            tlpHeader.PerformLayout();
            tlpContent.ResumeLayout(false);
            tlpButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tlpMain;
        private TableLayoutPanel tlpHeader;
        private Label lblTitulo;
        private Label lblFechar;
        private TableLayoutPanel tlpContent;
        private Guna.UI2.WinForms.Guna2TextBox txtNotas;
        private TableLayoutPanel tlpButtons;
        private Guna.UI2.WinForms.Guna2Button btnCancelar;
        private Guna.UI2.WinForms.Guna2Button btnGuardar;
        private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
    }
}
