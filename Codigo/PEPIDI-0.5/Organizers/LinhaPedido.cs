using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Guna.UI2.WinForms;
using PEPIDI.Models;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// UserControl que representa uma linha de pedido de EPI no FormPedidos.
    /// Tem um stepper de quantidade (0-5), combo de tamanho e combo de modelo.
    /// Quando o utilizador muda o tamanho, pede confirmação antes de gravar —
    /// se cancelar, reverte para o tamanho anterior sem perguntar novamente.
    /// </summary>
    public partial class LinhaPedido : UserControl
    {
        private readonly EfeitoUI M = new EfeitoUI();
        private int _quantidade = 0;
        // _tamanhoAnterior guarda o tamanho antes de qualquer alteração do utilizador,
        // para poder reverter caso ele cancele a confirmação de mudança.
        private string _tamanhoAnterior = null;
        // _isReverting impede que o SelectedIndexChanged dispare recursivamente
        // quando programaticamente revertemos o tamanho para o anterior
        private bool _isReverting = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Quantidade
        {
            get => _quantidade;
            set { _quantidade = Math.Max(0, value); lblQuantidade.Text = _quantidade.ToString(); }
        }

        // Texto do item
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        // Texto do item
        public string DescricaoEPI
        {
            get => label1.Text;
            set => label1.Text = value;
        }

        // Acede ao combo real de tamanhos
        // O nome depois da seta (=>) TEM de ser o nome que deste no Designer (modern...)
        public Guna.UI2.WinForms.Guna2ComboBox ComboTamanho => ModerComboBoxTamanho;
        public Guna.UI2.WinForms.Guna2ComboBox ComboModelo => moderComboBoxModelo;


        // Tamanho selecionado *no momento*
        public string TamanhoSelecionado => ComboTamanho.SelectedItem?.ToString() ?? string.Empty;

        // Evento personalizado para avisar o FormPedidos
        public event EventHandler<TamanhoAlteradoEventArgs> TamanhoAlteradoPeloUtilizador;

        // Classe auxiliar para passar os dados para o FormPedidos
        public class TamanhoAlteradoEventArgs : EventArgs
        {
            public string CodigoEpi { get; set; }
            public string NovoTamanho { get; set; }
        }

        public LinhaPedido()
        {
            InitializeComponent();
            Quantidade = 0;

            btnMais.Click += (s, e) => { if (Quantidade < 5) Quantidade++; };
            btnMenos.Click += (s, e) => { if (Quantidade > 0) Quantidade--; };

            ComboTamanho.SelectedIndexChanged += ComboTamanho_SelectedIndexChanged;

            this.Load += (s, e) => {
                ArredondarCantos(this, 20);
                ArredondarCantos(tlpQuant, 20);
                // Subscrevemos Resize UMA vez aqui — nunca dentro do método,
                // para não acumular handlers a cada chamada.
                this.Resize    += (_, _) => ArredondarCantos(this, 20);
                tlpQuant.Resize += (_, _) => ArredondarCantos(tlpQuant, 20);
                _tamanhoAnterior = ComboTamanho.SelectedItem?.ToString();
            };
        }

        /// <summary>
        /// Aplica uma região com cantos arredondados ao controlo recebido.
        /// Chamado no Load e sempre que o Resize dispara (subscrito uma vez no Load).
        /// </summary>
        private void ArredondarCantos(Control ctrl, int raio)
        {
            if (ctrl.Width <= 0 || ctrl.Height <= 0) return;

            int d = raio * 2;
            var rect = new Rectangle(0, 0, ctrl.Width, ctrl.Height);

            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();

                // evita leak da Region anterior
                ctrl.Region?.Dispose();
                ctrl.Region = new Region(path);
            }
        }

        /// <summary>
        /// Define o tamanho no combo sem disparar o diálogo de confirmação.
        /// Usado no carregamento inicial e quando o FormPedidos precisa de restaurar
        /// o tamanho do histórico sem que o utilizador tenha interagido.
        /// </summary>
        public void DefinirTamanhoSemConfirmar(string tamanho)
        {
            _isReverting = true;
            if (!string.IsNullOrEmpty(tamanho) && ComboTamanho.Items.Contains(tamanho))
            {
                ComboTamanho.SelectedItem = tamanho;
                _tamanhoAnterior = tamanho;
            }
            _isReverting = false;
        }

        private void ComboTamanho_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isReverting) return; // Se estivermos a carregar ou a reverter, ignora

            string novoTamanho = ComboTamanho.SelectedItem?.ToString();

            // Se o tamanho mudou e já tínhamos um tamanho definido anteriormente
            if (!string.IsNullOrEmpty(_tamanhoAnterior) && novoTamanho != _tamanhoAnterior)
            {
                DialogResult dr = M.AbrirMensagem(
                    $"Deseja mesmo alterar o tamanho de '{_tamanhoAnterior}' para '{novoTamanho}'?",
                    "Confirmar Alteração");

                if (dr == DialogResult.Yes)
                {
                    _tamanhoAnterior = novoTamanho; // Atualiza o histórico

                    // Vai buscar o Info que guardaste na Tag ao criar a linha
                    var info = this.Tag as PEPIDI.Models.LinhaPedidoInfo;
                    string codigoEpiAtual = info?.CodigoEpi ?? "";

                    // Dispara o evento para o FormPedidos apanhar!
                    TamanhoAlteradoPeloUtilizador?.Invoke(this, new TamanhoAlteradoEventArgs
                    {
                        CodigoEpi = codigoEpiAtual,
                        NovoTamanho = novoTamanho
                    });
                }
                else
                {
                    _isReverting = true; // Bloqueia para não perguntar de novo ao reverter
                    ComboTamanho.SelectedItem = _tamanhoAnterior; // Volta atrás
                    _isReverting = false;
                }
            }
        }

        public void AtualizarModelos(List<string> modelos)
        {
            ComboModelo.Items.Clear();

            if (modelos == null || modelos.Count <= 1)
            {
                // Esconde a label de "Modelo" e a Combo caso só haja 1 ou nenhum
                // Nota: Precisas de dar nomes às labels no Designer para as esconderes (ex: lblModeloTexto)
                label4.Visible = false; // "Modelo:"
                ComboModelo.Visible = false;

                if (modelos != null && modelos.Count == 1)
                    ComboModelo.Items.Add(modelos[0]);
            }
            else
            {
                // Se houver vários (ex: Würth, Bellota, etc), mostra as opções
                label4.Visible = true;
                ComboModelo.Visible = true;
                foreach (var m in modelos) ComboModelo.Items.Add(m);
                ComboModelo.SelectedIndex = 0;
            }
        }

        // Dentro da classe LinhaPedido.cs


        /// <summary>
        /// Configura o layout visual da linha consoante se a família tem um ou vários modelos.
        /// Quando há apenas um modelo (ex: Polo Manga Curta), as colunas do modelo
        /// são escondidas e a label de nome expande-se por SetColumnSpan para aproveitar o espaço.
        /// Quando há vários (ex: Sapatos — Bellota, Würth, etc), mostra o combo de modelo.
        /// </summary>
        public void ConfigurarLayout(bool temVariosModelos, string familia, string nomeVista, string modelo)
        {
            label1.Text = temVariosModelos ? nomeVista : modelo;

            if (!temVariosModelos)
            {
                label4.Visible = false;
                ComboModelo.Visible = false;

                // Expande a label de nome para ocupar as três primeiras colunas do tlpOP
                // (onde estariam a label "Modelo:" e o combo de modelo)
                tlpOP.SetColumnSpan(label1, 3);
            }
            else
            {
                label4.Visible = true;
                ComboModelo.Visible = true;

                // Volta ao span normal (só a coluna 0)
                tlpOP.SetColumnSpan(label1, 1);

                // Ajusta proporções: descrição 15%, label modelo 8.5%, combo modelo 31.5%
                // Os restantes ~45% ficam para as colunas de tamanho e quantidade (definidas no Designer)
                tlpOP.ColumnStyles[0].SizeType = SizeType.Percent;
                tlpOP.ColumnStyles[0].Width = 15F;

                tlpOP.ColumnStyles[1].SizeType = SizeType.Percent;
                tlpOP.ColumnStyles[1].Width = 8.5F;

                tlpOP.ColumnStyles[2].SizeType = SizeType.Percent;
                tlpOP.ColumnStyles[2].Width = 31.5F;
            }
        }

    }
}
