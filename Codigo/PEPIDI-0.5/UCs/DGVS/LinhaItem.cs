using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace PEPIDI.UCs.DGVS
{
    /// <summary>
    /// UserControl que representa uma linha de artigo EPI no painel de detalhes de pedido.
    /// Tem dois modos visuais: modo edição (estado Pendente — mostra combo de quantidade e
    /// combo de estado Novo/Usado) e modo aprovação (estado Aprovado — mostra checkbox de entrega).
    /// ConfigurarSugestaoStock() preenche os combos de estado consoante o stock disponível:
    /// se só há Usado, bloqueia o combo e gera uma nota automática "sem stock de novo".
    /// </summary>
    public partial class LinhaItem : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int IDEPI { get; set; }

        // M = Modelo, T = Tamanho — nomes curtos herdados da versão inicial, mantidos por compatibilidade
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string M { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string T { get; set; }

        // QD = Quantidade Disponível em stock, QA = Quantidade Aprovada/pedida
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int QD { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int QA { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EstadoLinha { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int QuantidadeOriginal { get; set; }

        /// <summary>
        /// Texto de nota automática gerado por ConfigurarSugestaoStock quando o stock
        /// obriga a uma escolha específica (ex: " (sem stock de novo)").
        /// Lido por PedidosDetalhes.Linha_EstadoAlterado para incluir nas observações.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ObservacoesAutomaticas { get; set; } = "";

        /// <summary>
        /// Código EPI do modelo substituto escolhido pelo gestor; null enquanto não houver troca.
        /// Usado em PedidosDetalhes.btnAprovar_Click para apontar ao stock correto.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CodigoEpiSubstituto { get; set; }

        /// <summary>Argumentos do evento ModeloSubstituido.</summary>
        public class ModeloSubstituidoEventArgs : EventArgs
        {
            public string CodigoOriginal { get; set; }
            public string ModeloOriginal { get; set; }
            public string CodigoNovo { get; set; }
            public string ModeloNovo { get; set; }
        }

        /// <summary>
        /// Disparado quando o gestor troca de modelo na combo de alternativas.
        /// Fired com CodigoEpiSubstituto == null quando reverte para o original.
        /// </summary>
        public event EventHandler<ModeloSubstituidoEventArgs> ModeloSubstituido;

        // Combo e lista de alternativas criados dinamicamente por ConfigurarAlternativas
        private Guna2ComboBox _cmbModelo;
        private List<(string Codigo, string ModeloNome)> _alternativas;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string EstadoSelecionado
        {
            get { return cmbEstado.Text; }
            set
            {
                if (cmbEstado.Items.Contains(value)) cmbEstado.SelectedItem = value;
                cmbEstado.Text = value;
            }
        }

        public bool EstadoForcadoPeloSistema
        {
            get { return cmbEstado != null && !cmbEstado.Enabled; }
        }

        public event EventHandler QuantidadeAlterada;
        public event EventHandler EstadoAlterado;

        public string DescricaoArtigo { get { return this.M; } }
        public string TamanhoSelecionado { get { return this.T; } }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool Selecionado
        {
            get { return chkEntregar.Checked; }
            set { chkEntregar.Checked = value; }
        }

        public int QuantidadeSelecionada
        {
            get
            {
                if (EstadoLinha == "Aprovado")
                {
                    if (!chkEntregar.Checked) return 0;
                    return (this.QA > 0) ? this.QA : 1;
                }
                if (int.TryParse(cmbQuant.Text, out int result)) return result;
                return 0;
            }
        }

        public LinhaItem(string _M, string _T, int _QD, int _QA, string _estado)
        {
            InitializeComponent();
            M = _M;
            T = _T;
            QD = _QD;
            QA = _QA;
            EstadoLinha = _estado;
            QuantidadeOriginal = _QA;

            // MÁGICA DO SCROLL: Foca no painel pai quando o rato entra aqui
            this.MouseEnter += (s, e) => { if (this.Parent != null) this.Parent.Focus(); };
        }

        private void Linha_Load(object sender, EventArgs e)
        {
            lblModelo.Text = M;
            lblTamanho.Text = T;
            lblQuantDisp.Text = (EstadoLinha == "Aprovado") ? QA.ToString() : QD.ToString();

            if (EstadoLinha == "Aprovado")
            {
                cmbQuant.Visible = false;
                chkEntregar.Visible = true;
                chkEntregar.Dock = DockStyle.Fill;
                tableLayoutPanel1.ColumnStyles[4].Width = 0F;
                tableLayoutPanel1.ColumnStyles[5].Width = 15F;

                if (QD <= 0)
                {
                    chkEntregar.Checked = false;
                    chkEntregar.Enabled = false;
                    this.BackColor = Color.FromArgb(255, 235, 235);
                }
                else
                {
                    chkEntregar.Checked = true;
                    chkEntregar.Enabled = true;
                }
            }
            else
            {
                cmbQuant.Visible = true;
                chkEntregar.Visible = false;
                cmbQuant.Dock = DockStyle.Top;
                if (cmbEstado != null) cmbEstado.Visible = true;
                tableLayoutPanel1.ColumnStyles[4].Width = 15F;
                tableLayoutPanel1.ColumnStyles[5].Width = 0F;
                ConfigurarModoEdicao();
            }
        }

        private void ConfigurarModoEdicao()
        {
            cmbQuant.Items.Clear();
            int limiteMaximo = Math.Min(QD, 5);
            for (int i = 0; i <= limiteMaximo; i++) cmbQuant.Items.Add(i);
            int valorParaSelecionar = (QA > limiteMaximo) ? limiteMaximo : QA;
            cmbQuant.Text = valorParaSelecionar.ToString();
        }

        private void CmbQuant_SelectedIndexChanged(object sender, EventArgs e)
        {
            QuantidadeAlterada?.Invoke(this, EventArgs.Empty);
        }

        private int stockNovo;
        private int stockUsado;

        public void AtualizarLabels(int novo, int usado)
        {
            this.stockNovo = novo;
            this.stockUsado = usado;
        }

        private void ChkEntregar_CheckedChanged(object sender, EventArgs e)
        {
            this.BackColor = chkEntregar.Checked ? Color.FromArgb(224, 224, 224) : Color.MistyRose;
            QuantidadeAlterada?.Invoke(this, EventArgs.Empty);
        }

        private int memoriaStockNovo;
        private int memoriaStockUsado;

        /// <summary>
        /// Preenche o combo de estado (Novo/Usado) baseado no stock disponível.
        /// Se ambos existem → utilizador escolhe; se só um → bloqueia e seleciona automaticamente.
        /// Quando só há Usado, define ObservacoesAutomaticas para que PedidosDetalhes
        /// inclua a nota " (sem stock de novo)" nas observações do pedido.
        /// </summary>
        public void ConfigurarSugestaoStock(int qtdNovo, int qtdUsado)
        {
            if (cmbEstado == null) return;

            this.memoriaStockNovo = qtdNovo;
            this.memoriaStockUsado = qtdUsado;
            cmbEstado.Items.Clear();
            this.ObservacoesAutomaticas = "";

            bool temNovo = qtdNovo > 0;
            bool temUsado = qtdUsado > 0;

            if (temNovo && temUsado)
            {
                cmbEstado.Items.Add("Novo");
                cmbEstado.Items.Add("Usado");
                cmbEstado.Enabled = true;
                cmbEstado.SelectedIndex = 0;
            }
            else if (temNovo)
            {
                cmbEstado.Items.Add("Novo");
                cmbEstado.Enabled = false;
                cmbEstado.SelectedIndex = 0;
            }
            else if (temUsado)
            {
                cmbEstado.Items.Add("Usado");
                cmbEstado.Enabled = false;
                this.ObservacoesAutomaticas = " (sem stock de novo)";
                cmbEstado.SelectedIndex = 0;
            }

            AtualizarLabelQuantidade();
        }

        private void AtualizarLabelQuantidade()
        {
            if (cmbEstado.Text == "Novo")
            {
                lblQuantDisp.Text = memoriaStockNovo.ToString();
                this.QD = memoriaStockNovo;
            }
            else if (cmbEstado.Text == "Usado")
            {
                lblQuantDisp.Text = memoriaStockUsado.ToString();
                this.QD = memoriaStockUsado;
            }
        }

        private void cmbEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarLabelQuantidade();
            EstadoAlterado?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Substitui o lblModelo por uma Guna2ComboBox quando existem vários modelos
        /// disponíveis para a família e função deste artigo.
        /// Deve ser chamado após Tag (código EPI atual) estar definida.
        /// Se só houver uma alternativa (ou nenhuma), o label mantém-se inalterado.
        /// </summary>
        public void ConfigurarAlternativas(List<(string Codigo, string ModeloNome)> alternativas)
        {
            if (alternativas == null || alternativas.Count <= 1) return;

            _alternativas = alternativas;

            _cmbModelo = new Guna2ComboBox();
            _cmbModelo.Dock = DockStyle.Fill;
            _cmbModelo.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbModelo.DrawMode = DrawMode.OwnerDrawFixed;
            _cmbModelo.Font = lblModelo.Font;
            _cmbModelo.ForeColor = lblModelo.ForeColor;
            _cmbModelo.FillColor = Color.FromArgb(224, 224, 224);
            _cmbModelo.BorderThickness = 0;
            _cmbModelo.Margin = new Padding(2, 3, 2, 5);
            _cmbModelo.IntegralHeight = false;
            _cmbModelo.ItemHeight = 29;
            _cmbModelo.FocusedState.BorderColor = Color.FromArgb(64, 64, 64);

            foreach (var alt in alternativas)
                _cmbModelo.Items.Add(alt.ModeloNome);

            // Seleciona o modelo atual SEM disparar o evento (wire só depois)
            string codigoAtual = this.Tag?.ToString().Trim() ?? "";
            int idx = alternativas.FindIndex(a => a.Codigo == codigoAtual);
            _cmbModelo.SelectedIndex = idx >= 0 ? idx : 0;

            _cmbModelo.SelectedIndexChanged += CmbModelo_SelectedIndexChanged;

            // Remove o label e coloca o combo na mesma célula (col 0, row 0)
            tableLayoutPanel1.Controls.Remove(lblModelo);
            tableLayoutPanel1.Controls.Add(_cmbModelo, 0, 0);
        }

        private void CmbModelo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_alternativas == null || _cmbModelo == null) return;
            int idx = _cmbModelo.SelectedIndex;
            if (idx < 0 || idx >= _alternativas.Count) return;

            var selecionado = _alternativas[idx];
            string codigoOriginal = this.Tag?.ToString().Trim() ?? "";

            // Null se reverteu ao original; código do substituto caso contrário
            this.CodigoEpiSubstituto = (selecionado.Codigo == codigoOriginal) ? null : selecionado.Codigo;

            ModeloSubstituido?.Invoke(this, new ModeloSubstituidoEventArgs
            {
                CodigoOriginal = codigoOriginal,
                ModeloOriginal = this.M,
                CodigoNovo = selecionado.Codigo,
                ModeloNovo = selecionado.ModeloNome
            });
        }
    }
}