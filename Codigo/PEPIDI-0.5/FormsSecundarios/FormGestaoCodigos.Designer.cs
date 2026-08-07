namespace PEPIDI.FormsSecundarios
{
    partial class FormGestaoCodigos
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGestaoCodigos));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(components);
            tlpRoot = new TableLayoutPanel();
            tlpHeader = new TableLayoutPanel();
            lblFechar = new Label();
            lblTitulo = new Label();
            tlpBody = new TableLayoutPanel();
            lblFamiliasTitulo = new Label();
            dgvFamilias = new PEPIDI.Models.PEPIDIDataGridView();
            tlpNova = new TableLayoutPanel();
            txtVistaNova = new Guna.UI2.WinForms.Guna2TextBox();
            cmbTipoNova = new Guna.UI2.WinForms.Guna2ComboBox();
            btnAdicionar = new Guna.UI2.WinForms.Guna2Button();
            txtPrefixoNova = new Guna.UI2.WinForms.Guna2TextBox();
            txtNomeNova = new Guna.UI2.WinForms.Guna2TextBox();
            tlpFooter = new TableLayoutPanel();
            btnEliminar = new Guna.UI2.WinForms.Guna2Button();
            btnGuardar = new Guna.UI2.WinForms.Guna2Button();
            btnCancelar = new Guna.UI2.WinForms.Guna2Button();
            tlpRoot.SuspendLayout();
            tlpHeader.SuspendLayout();
            tlpBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFamilias).BeginInit();
            tlpNova.SuspendLayout();
            tlpFooter.SuspendLayout();
            SuspendLayout();
            // 
            // guna2BorderlessForm1
            // 
            guna2BorderlessForm1.BorderRadius = 15;
            guna2BorderlessForm1.ContainerControl = this;
            guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            guna2BorderlessForm1.TransparentWhileDrag = true;
            // 
            // tlpRoot
            // 
            tlpRoot.BackColor = Color.White;
            tlpRoot.ColumnCount = 1;
            tlpRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpRoot.Controls.Add(tlpHeader, 0, 0);
            tlpRoot.Controls.Add(tlpBody, 0, 1);
            tlpRoot.Controls.Add(tlpFooter, 0, 2);
            tlpRoot.Dock = DockStyle.Fill;
            tlpRoot.Location = new Point(0, 0);
            tlpRoot.Name = "tlpRoot";
            tlpRoot.RowCount = 3;
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));
            tlpRoot.Size = new Size(800, 676);
            tlpRoot.TabIndex = 0;
            // 
            // tlpHeader
            // 
            tlpHeader.BackColor = Color.FromArgb(254, 107, 0);
            tlpHeader.ColumnCount = 2;
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpHeader.Controls.Add(lblFechar, 1, 0);
            tlpHeader.Controls.Add(lblTitulo, 0, 0);
            tlpHeader.Dock = DockStyle.Fill;
            tlpHeader.Location = new Point(0, 0);
            tlpHeader.Margin = new Padding(0);
            tlpHeader.Name = "tlpHeader";
            tlpHeader.RowCount = 1;
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpHeader.Size = new Size(800, 70);
            tlpHeader.TabIndex = 0;
            // 
            // lblFechar
            // 
            lblFechar.Cursor = Cursors.Hand;
            lblFechar.Dock = DockStyle.Right;
            lblFechar.Font = new Font("Roboto", 18.75F);
            lblFechar.ForeColor = Color.White;
            lblFechar.Image = (Image)resources.GetObject("lblFechar.Image");
            lblFechar.Location = new Point(697, 0);
            lblFechar.Margin = new Padding(10, 0, 0, 0);
            lblFechar.Name = "lblFechar";
            lblFechar.Size = new Size(103, 70);
            lblFechar.TabIndex = 10;
            lblFechar.TextAlign = ContentAlignment.MiddleLeft;
            lblFechar.Click += lblFechar_Click;
            // 
            // lblTitulo
            // 
            lblTitulo.Dock = DockStyle.Fill;
            lblTitulo.Font = new Font("Roboto Medium", 18.75F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(20, 0);
            lblTitulo.Margin = new Padding(20, 0, 0, 0);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(380, 70);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "GESTÃO DE CÓDIGOS EPI";
            lblTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tlpBody
            // 
            tlpBody.ColumnCount = 1;
            tlpBody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpBody.Controls.Add(lblFamiliasTitulo, 0, 0);
            tlpBody.Controls.Add(dgvFamilias, 0, 1);
            tlpBody.Controls.Add(tlpNova, 0, 2);
            tlpBody.Dock = DockStyle.Fill;
            tlpBody.Location = new Point(15, 85);
            tlpBody.Margin = new Padding(15, 15, 15, 0);
            tlpBody.Name = "tlpBody";
            tlpBody.RowCount = 3;
            tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 7.5F));
            tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tlpBody.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5F));
            tlpBody.Size = new Size(770, 511);
            tlpBody.TabIndex = 1;
            // 
            // lblFamiliasTitulo
            // 
            lblFamiliasTitulo.Dock = DockStyle.Fill;
            lblFamiliasTitulo.Font = new Font("Roboto Medium", 12F, FontStyle.Bold);
            lblFamiliasTitulo.ForeColor = Color.FromArgb(64, 64, 64);
            lblFamiliasTitulo.Location = new Point(3, 0);
            lblFamiliasTitulo.Name = "lblFamiliasTitulo";
            lblFamiliasTitulo.Size = new Size(764, 38);
            lblFamiliasTitulo.TabIndex = 0;
            lblFamiliasTitulo.Text = "Famílias de EPI (edite Prefixo / Nome Visível / Tipo / Ativo)";
            lblFamiliasTitulo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // dgvFamilias
            // 
            dgvFamilias.AllowUserToAddRows = false;
            dgvFamilias.AllowUserToDeleteRows = false;
            dgvFamilias.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.Transparent;
            dgvFamilias.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvFamilias.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFamilias.BackgroundColor = Color.White;
            dgvFamilias.BadgeColumnName = "";
            dgvFamilias.BorderStyle = BorderStyle.None;
            dgvFamilias.CardRowHeight = 48;
            dgvFamilias.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvFamilias.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.White;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.Black;
            dataGridViewCellStyle2.Padding = new Padding(0, 8, 0, 8);
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvFamilias.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvFamilias.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.Transparent;
            dataGridViewCellStyle3.Font = new Font("Roboto", 11F);
            dataGridViewCellStyle3.ForeColor = Color.Black;
            dataGridViewCellStyle3.Padding = new Padding(18, 10, 18, 10);
            dataGridViewCellStyle3.SelectionBackColor = Color.Transparent;
            dataGridViewCellStyle3.SelectionForeColor = Color.Black;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvFamilias.DefaultCellStyle = dataGridViewCellStyle3;
            dgvFamilias.Dock = DockStyle.Fill;
            dgvFamilias.EnableHeadersVisualStyles = false;
            dgvFamilias.Font = new Font("Roboto", 11F);
            dgvFamilias.GridColor = SystemColors.ControlDark;
            dgvFamilias.HeaderFontSize = 15F;
            dgvFamilias.Location = new Point(0, 42);
            dgvFamilias.Margin = new Padding(0, 4, 0, 4);
            dgvFamilias.MultiSelect = false;
            dgvFamilias.Name = "dgvFamilias";
            dgvFamilias.RowHeadersVisible = false;
            dgvFamilias.RowHeadersWidth = 92;
            dataGridViewCellStyle4.BackColor = Color.Transparent;
            dgvFamilias.RowsDefaultCellStyle = dataGridViewCellStyle4;
            dgvFamilias.ScrollBars = ScrollBars.None;
            dgvFamilias.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFamilias.Size = new Size(770, 400);
            dgvFamilias.TabIndex = 1;
            // 
            // tlpNova
            // 
            tlpNova.ColumnCount = 5;
            tlpNova.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpNova.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpNova.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpNova.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpNova.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tlpNova.Controls.Add(txtVistaNova, 2, 0);
            tlpNova.Controls.Add(cmbTipoNova, 3, 0);
            tlpNova.Controls.Add(btnAdicionar, 4, 0);
            tlpNova.Controls.Add(txtPrefixoNova, 0, 0);
            tlpNova.Controls.Add(txtNomeNova, 1, 0);
            tlpNova.Dock = DockStyle.Fill;
            tlpNova.Location = new Point(0, 450);
            tlpNova.Margin = new Padding(0, 4, 0, 4);
            tlpNova.Name = "tlpNova";
            tlpNova.RowCount = 1;
            tlpNova.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpNova.Size = new Size(770, 57);
            tlpNova.TabIndex = 2;
            // 
            // txtVistaNova
            // 
            txtVistaNova.AutoRoundedCorners = true;
            txtVistaNova.CustomizableEdges = customizableEdges1;
            txtVistaNova.DefaultText = "";
            txtVistaNova.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtVistaNova.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtVistaNova.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtVistaNova.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtVistaNova.Dock = DockStyle.Fill;
            txtVistaNova.FocusedState.BorderColor = Color.FromArgb(243, 108, 33);
            txtVistaNova.Font = new Font("Roboto", 11.25F);
            txtVistaNova.ForeColor = Color.Black;
            txtVistaNova.HoverState.BorderColor = Color.Gray;
            txtVistaNova.Location = new Point(314, 8);
            txtVistaNova.Margin = new Padding(6, 8, 6, 8);
            txtVistaNova.MaxLength = 64;
            txtVistaNova.Name = "txtVistaNova";
            txtVistaNova.PlaceholderForeColor = Color.Silver;
            txtVistaNova.PlaceholderText = "Nome";
            txtVistaNova.SelectedText = "";
            txtVistaNova.ShadowDecoration.CustomizableEdges = customizableEdges2;
            txtVistaNova.Size = new Size(142, 41);
            txtVistaNova.TabIndex = 4;
            txtVistaNova.TextOffset = new Point(10, 0);
            // 
            // cmbTipoNova
            // 
            cmbTipoNova.BackColor = Color.Transparent;
            cmbTipoNova.BorderRadius = 15;
            cmbTipoNova.CustomizableEdges = customizableEdges3;
            cmbTipoNova.Dock = DockStyle.Fill;
            cmbTipoNova.DrawMode = DrawMode.OwnerDrawFixed;
            cmbTipoNova.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoNova.FocusedColor = Color.Empty;
            cmbTipoNova.Font = new Font("Roboto", 11.25F);
            cmbTipoNova.ForeColor = Color.FromArgb(64, 64, 64);
            cmbTipoNova.ItemHeight = 30;
            cmbTipoNova.Items.AddRange(new object[] { "Letra", "Numero" });
            cmbTipoNova.Location = new Point(467, 7);
            cmbTipoNova.Margin = new Padding(5, 7, 5, 7);
            cmbTipoNova.Name = "cmbTipoNova";
            cmbTipoNova.ShadowDecoration.CustomizableEdges = customizableEdges4;
            cmbTipoNova.Size = new Size(144, 36);
            cmbTipoNova.TabIndex = 5;
            // 
            // btnAdicionar
            // 
            btnAdicionar.BorderRadius = 10;
            btnAdicionar.Cursor = Cursors.Hand;
            btnAdicionar.CustomizableEdges = customizableEdges5;
            btnAdicionar.Dock = DockStyle.Fill;
            btnAdicionar.FillColor = Color.FromArgb(67, 160, 71);
            btnAdicionar.Font = new Font("Roboto", 11F, FontStyle.Bold);
            btnAdicionar.ForeColor = Color.White;
            btnAdicionar.Location = new Point(621, 7);
            btnAdicionar.Margin = new Padding(5, 7, 5, 7);
            btnAdicionar.Name = "btnAdicionar";
            btnAdicionar.ShadowDecoration.CustomizableEdges = customizableEdges6;
            btnAdicionar.Size = new Size(144, 43);
            btnAdicionar.TabIndex = 6;
            btnAdicionar.Text = "+ Adicionar";
            btnAdicionar.Click += btnAdicionar_Click;
            // 
            // txtPrefixoNova
            // 
            txtPrefixoNova.AutoRoundedCorners = true;
            txtPrefixoNova.CustomizableEdges = customizableEdges7;
            txtPrefixoNova.DefaultText = "";
            txtPrefixoNova.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtPrefixoNova.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtPrefixoNova.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtPrefixoNova.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtPrefixoNova.Dock = DockStyle.Fill;
            txtPrefixoNova.FocusedState.BorderColor = Color.FromArgb(243, 108, 33);
            txtPrefixoNova.Font = new Font("Roboto", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPrefixoNova.ForeColor = Color.Black;
            txtPrefixoNova.HoverState.BorderColor = Color.Gray;
            txtPrefixoNova.Location = new Point(6, 8);
            txtPrefixoNova.Margin = new Padding(6, 8, 6, 8);
            txtPrefixoNova.MaxLength = 64;
            txtPrefixoNova.Name = "txtPrefixoNova";
            txtPrefixoNova.PlaceholderForeColor = Color.Silver;
            txtPrefixoNova.PlaceholderText = "Prefixo";
            txtPrefixoNova.SelectedText = "";
            txtPrefixoNova.ShadowDecoration.CustomizableEdges = customizableEdges8;
            txtPrefixoNova.Size = new Size(142, 41);
            txtPrefixoNova.TabIndex = 2;
            txtPrefixoNova.TextOffset = new Point(10, 0);
            // 
            // txtNomeNova
            // 
            txtNomeNova.AutoRoundedCorners = true;
            txtNomeNova.CustomizableEdges = customizableEdges9;
            txtNomeNova.DefaultText = "";
            txtNomeNova.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtNomeNova.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtNomeNova.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtNomeNova.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtNomeNova.Dock = DockStyle.Fill;
            txtNomeNova.FocusedState.BorderColor = Color.FromArgb(243, 108, 33);
            txtNomeNova.Font = new Font("Roboto", 11.25F);
            txtNomeNova.ForeColor = Color.Black;
            txtNomeNova.HoverState.BorderColor = Color.Gray;
            txtNomeNova.Location = new Point(160, 8);
            txtNomeNova.Margin = new Padding(6, 8, 6, 8);
            txtNomeNova.MaxLength = 64;
            txtNomeNova.Name = "txtNomeNova";
            txtNomeNova.PlaceholderForeColor = Color.Silver;
            txtNomeNova.PlaceholderText = "Código";
            txtNomeNova.SelectedText = "";
            txtNomeNova.ShadowDecoration.CustomizableEdges = customizableEdges10;
            txtNomeNova.Size = new Size(142, 41);
            txtNomeNova.TabIndex = 3;
            txtNomeNova.TextOffset = new Point(10, 0);
            // 
            // tlpFooter
            // 
            tlpFooter.ColumnCount = 4;
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tlpFooter.Controls.Add(btnEliminar, 0, 0);
            tlpFooter.Controls.Add(btnGuardar, 3, 0);
            tlpFooter.Controls.Add(btnCancelar, 2, 0);
            tlpFooter.Dock = DockStyle.Fill;
            tlpFooter.Location = new Point(10, 606);
            tlpFooter.Margin = new Padding(10);
            tlpFooter.Name = "tlpFooter";
            tlpFooter.RowCount = 1;
            tlpFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpFooter.Size = new Size(780, 60);
            tlpFooter.TabIndex = 2;
            // 
            // btnEliminar
            // 
            btnEliminar.BorderRadius = 10;
            btnEliminar.CustomizableEdges = customizableEdges11;
            btnEliminar.DisabledState.BorderColor = Color.DarkGray;
            btnEliminar.DisabledState.CustomBorderColor = Color.DarkGray;
            btnEliminar.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnEliminar.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnEliminar.Dock = DockStyle.Fill;
            btnEliminar.FillColor = Color.Red;
            btnEliminar.Font = new Font("Roboto", 18.75F);
            btnEliminar.ForeColor = Color.White;
            btnEliminar.Location = new Point(10, 10);
            btnEliminar.Margin = new Padding(10);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.ShadowDecoration.CustomizableEdges = customizableEdges12;
            btnEliminar.Size = new Size(175, 40);
            btnEliminar.TabIndex = 7;
            btnEliminar.Text = "Eliminar";
            btnEliminar.Click += btnEliminar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.BorderRadius = 10;
            btnGuardar.CustomizableEdges = customizableEdges13;
            btnGuardar.DisabledState.BorderColor = Color.DarkGray;
            btnGuardar.DisabledState.CustomBorderColor = Color.DarkGray;
            btnGuardar.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnGuardar.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnGuardar.Dock = DockStyle.Fill;
            btnGuardar.FillColor = Color.FromArgb(243, 108, 33);
            btnGuardar.Font = new Font("Roboto", 18.75F);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.Location = new Point(595, 10);
            btnGuardar.Margin = new Padding(10);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.ShadowDecoration.CustomizableEdges = customizableEdges14;
            btnGuardar.Size = new Size(175, 40);
            btnGuardar.TabIndex = 9;
            btnGuardar.Text = "Guardar";
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.BorderRadius = 10;
            btnCancelar.CustomizableEdges = customizableEdges15;
            btnCancelar.DisabledState.BorderColor = Color.DarkGray;
            btnCancelar.DisabledState.CustomBorderColor = Color.DarkGray;
            btnCancelar.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnCancelar.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnCancelar.Dock = DockStyle.Fill;
            btnCancelar.FillColor = Color.Silver;
            btnCancelar.Font = new Font("Roboto", 18.75F);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.Location = new Point(400, 10);
            btnCancelar.Margin = new Padding(10);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.ShadowDecoration.CustomizableEdges = customizableEdges16;
            btnCancelar.Size = new Size(175, 40);
            btnCancelar.TabIndex = 8;
            btnCancelar.Text = "Cancelar";
            btnCancelar.Click += btnCancelar_Click;
            // 
            // FormGestaoCodigos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 676);
            Controls.Add(tlpRoot);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormGestaoCodigos";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestão de Códigos EPI";
            Load += FormGestaoCodigos_Load;
            tlpRoot.ResumeLayout(false);
            tlpHeader.ResumeLayout(false);
            tlpBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvFamilias).EndInit();
            tlpNova.ResumeLayout(false);
            tlpFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void ConfigurarTextBox(Guna.UI2.WinForms.Guna2TextBox txt, string placeholder, int maxLen,
                                       Guna.UI2.WinForms.Suite.CustomizableEdges ceMain,
                                       Guna.UI2.WinForms.Suite.CustomizableEdges ceShadow)
        {
            txt.AutoRoundedCorners = true;
            txt.CustomizableEdges = ceMain;
            txt.DefaultText = "";
            txt.DisabledState.BorderColor = System.Drawing.Color.FromArgb(208, 208, 208);
            txt.DisabledState.FillColor   = System.Drawing.Color.FromArgb(226, 226, 226);
            txt.DisabledState.ForeColor   = System.Drawing.Color.FromArgb(138, 138, 138);
            txt.Dock = System.Windows.Forms.DockStyle.Fill;
            txt.FocusedState.BorderColor = System.Drawing.Color.FromArgb(243, 108, 33);
            txt.Font = new System.Drawing.Font("Roboto", 11F);
            txt.ForeColor = System.Drawing.Color.Black;
            txt.HoverState.BorderColor = System.Drawing.Color.Gray;
            txt.Margin = new System.Windows.Forms.Padding(5, 12, 5, 12);
            txt.MaxLength = maxLen;
            txt.PlaceholderForeColor = System.Drawing.Color.Silver;
            txt.PlaceholderText = placeholder;
            txt.SelectedText = "";
            txt.ShadowDecoration.CustomizableEdges = ceShadow;
            txt.TextOffset = new System.Drawing.Point(10, 0);
        }

        private void ConfigurarBotao(Guna.UI2.WinForms.Guna2Button btn, string texto, System.Drawing.Color fill,
                                     Guna.UI2.WinForms.Suite.CustomizableEdges ce, System.Drawing.Color? fore = null)
        {
            btn.BorderRadius = 10;
            btn.Cursor = System.Windows.Forms.Cursors.Hand;
            btn.CustomizableEdges = ce;
            btn.Dock = System.Windows.Forms.DockStyle.Fill;
            btn.FillColor = fill;
            btn.Font = new System.Drawing.Font("Roboto", 11.25F, System.Drawing.FontStyle.Bold);
            btn.ForeColor = fore ?? System.Drawing.Color.White;
            btn.Margin = new System.Windows.Forms.Padding(10);
            btn.Text = texto;
        }

        #endregion

        private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
        private System.Windows.Forms.TableLayoutPanel tlpRoot;
        private System.Windows.Forms.TableLayoutPanel tlpHeader;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.TableLayoutPanel tlpBody;
        private System.Windows.Forms.Label lblFamiliasTitulo;
        private PEPIDI.Models.PEPIDIDataGridView dgvFamilias;
        private System.Windows.Forms.TableLayoutPanel tlpNova;
        private Guna.UI2.WinForms.Guna2ComboBox cmbTipoNova;
        private Guna.UI2.WinForms.Guna2Button btnAdicionar;
        private System.Windows.Forms.TableLayoutPanel tlpFooter;
        private Label lblFechar;
        private Guna.UI2.WinForms.Guna2Button btnGuardar;
        private Guna.UI2.WinForms.Guna2TextBox txtNomeNova;
        private Guna.UI2.WinForms.Guna2TextBox txtVistaNova;
        private Guna.UI2.WinForms.Guna2TextBox txtPrefixoNova;
        private Guna.UI2.WinForms.Guna2Button btnEliminar;
        private Guna.UI2.WinForms.Guna2Button btnCancelar;
    }
}
