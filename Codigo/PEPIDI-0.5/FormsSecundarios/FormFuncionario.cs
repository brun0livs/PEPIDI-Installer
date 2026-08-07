using Guna.UI2.WinForms;
using PEPIDI.Organizers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PEPIDI.Utils;


namespace PEPIDI.FormsSecundarios
{
    /// <summary>
    /// Formulário para criar ou editar um funcionário.
    /// NMEC=null indica modo "Criar"; NMEC com valor indica modo "Editar".
    /// ConfigurarModo() determina o modo a partir de NMEC e configura os campos e título.
    /// txtNr_Leave() verifica se o número mecanográfico já existe antes de guardar (só no modo Criar).
    /// CarregarComboFuncoes() filtra as funções disponíveis pelo NivelAcesso do gestor —
    /// um gestor com NivelAcesso=1 não pode criar funcionários com funções de NivelAcesso=0.
    /// Os tamanhos são carregados dinamicamente da tabela Familias (Ativo=1), guardados em
    /// _combosSize (Dictionary Familia→Combo) para desacoplar do número de famílias.
    /// </summary>
    public partial class FormFuncionario : Form
    {
        private int? NMEC = null;
        private int? IDGestor;
        EfeitoUI M = new();

        // Mapeamento dinâmico: Nome da família → combo de tamanho
        private readonly Dictionary<string, Guna2ComboBox> _combosSize = new();

        public FormFuncionario(int? _nr = null, int? _IDGestor = null, int _nivelAcesso = 0)
        {
            InitializeComponent();
            NMEC = _nr;
            IDGestor = _IDGestor;
            CarregarComboFuncoes(_nivelAcesso);
            CarregaComboEstabs();
            // Famílias dinâmicas são carregadas no Load, quando o layout já tem dimensões reais
            ConfigurarModo();
            GestorTema.AplicarEstilos(this);
        }

        // =====================================================================
        // FAMÍLIAS DINÂMICAS
        // =====================================================================

        /// <summary>
        /// Lê todas as famílias ativas da BD e cria, para cada uma,
        /// um painel (label + combo) dentro do flpTamanhos.
        /// Guarda cada combo em _combosSize[Nome] para uso em carregar/guardar.
        /// Chamado no Load (não no construtor) para ter dimensões reais do layout.
        /// </summary>
        private void CarregarFamiliasDinamicas()
        {
            pnlTamanhos.Controls.Clear();
            _combosSize.Clear();

            // Dimensões por painel — layout horizontal: [Label | Combo]
            const int altCmb    = 38;
            const int largLbl   = 90;  // label à esquerda (alinhado à direita)
            const int largCmb   = 85;  // combo à direita
            const int gapLC     = 6;   // espaço entre label e combo
            const int largPnl   = largLbl + gapLC + largCmb; // = 181
            const int altPainel = 52;  // altura do painel (combo centrado verticalmente)
            const int margemH   = 12;  // espaço horizontal entre painéis
            const int margemV   = 12;  // espaço vertical entre linhas
            const int paddingTop = 10;

            // 1) Lê todas as famílias para uma lista
            var familias = new List<(string nome, string nomeVista, string tipoTam)>();
            try
            {
                using var conn = GetConn.GetConnection();
                conn.Open();
                using var cmd = new SqlCommand(
                    "SELECT Nome, NomeVista, TipoTamanho FROM Familias WHERE Ativo = 1 ORDER BY NomeVista", conn);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    familias.Add((
                        rdr["Nome"].ToString(),
                        rdr["NomeVista"].ToString(),
                        rdr["TipoTamanho"].ToString()
                    ));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[FormFunc] Erro ao carregar famílias: " + ex.Message);
                M.AbrirMensagem("Erro ao carregar famílias: " + ex.Message, "Erro");
                return;
            }

            if (familias.Count == 0) return;

            // 2) Quantos painéis cabem por linha e quantas linhas
            const int maxPorLinha = 4;  // nunca mais de 4 por linha
            int largDisp = pnlTamanhos.ClientSize.Width;
            int painelComMargem = largPnl + margemH;
            // Máximo real: menor entre maxPorLinha, o que cabe na largura e o total de famílias
            int maxCabe    = Math.Max(1, Math.Min(maxPorLinha, Math.Min(familias.Count, largDisp / painelComMargem)));
            // Número de linhas necessárias para não exceder maxCabe por linha
            int totalLinhas = (int)Math.Ceiling(familias.Count / (double)maxCabe);
            // Distribuir igualmente (última linha pode ter menos, mas a diferença é ≤ 1)
            int porLinha   = (int)Math.Ceiling(familias.Count / (double)totalLinhas);

            // 3) Cria os painéis com posicionamento absoluto, centrando cada linha
            for (int i = 0; i < familias.Count; i++)
            {
                int linha = i / porLinha;
                int colNaLinha = i % porLinha;

                // Quantos itens há nesta linha (pode ser menos na última)
                int itensNestaLinha = Math.Min(porLinha, familias.Count - linha * porLinha);

                // Largura efetiva ocupada pela linha (sem margem direita do último item)
                int largLinha = itensNestaLinha * largPnl + (itensNestaLinha - 1) * margemH;
                int offsetX = Math.Max(0, (largDisp - largLinha) / 2);

                int x = offsetX + colNaLinha * painelComMargem;
                int y = paddingTop + linha * (altPainel + margemV);

                var (nome, nomeVista, tipoTam) = familias[i];

                var pnl = new Panel
                {
                    Location  = new Point(x, y),
                    Width     = largPnl,
                    Height    = altPainel,
                    BackColor = Color.White,
                };

                // Label — à esquerda, alinhado à direita para ficar colado ao combo
                var lbl = new Label
                {
                    Text      = nomeVista + " :",
                    Font      = new Font("Roboto", 11F, FontStyle.Regular),
                    Bounds    = new Rectangle(0, 0, largLbl, altPainel),
                    TextAlign = ContentAlignment.MiddleRight,
                };

                // Combo — à direita do label, centrado verticalmente no painel
                int cmbY = (altPainel - altCmb) / 2;
                var cmb = new Guna2ComboBox
                {
                    Bounds        = new Rectangle(largLbl + gapLC, cmbY, largCmb, altCmb),
                    Font          = new Font("Roboto", 12F),
                    BorderRadius  = 17,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DrawMode      = DrawMode.OwnerDrawFixed,
                    ItemHeight    = 28,
                    BorderColor   = Color.FromArgb(213, 218, 223),
                    ForeColor     = Color.Black,
                    BackColor     = Color.Transparent,
                };
                cmb.FocusedState.BorderColor = Color.FromArgb(243, 108, 33);
                cmb.HoverState.BorderColor   = Color.FromArgb(254, 107, 0);
                cmb.HoverState.FillColor     = Color.White;
                cmb.HoverState.ForeColor     = Color.Black;

                PreencherItensTamanho(cmb, nome, tipoTam);

                pnl.Controls.Add(lbl);
                pnl.Controls.Add(cmb);
                pnlTamanhos.Controls.Add(pnl);
                _combosSize[nome] = cmb;
            }
        }

        /// <summary>
        /// Preenche os itens de tamanho num combo consoante o tipo de família.
        /// Letra → XXS..XXXL; Numero+Sapato → 34..48; Numero+outro → 30,32..50.
        /// </summary>
        private static void PreencherItensTamanho(Guna2ComboBox cmb, string nomeFamilia, string tipoTamanho)
        {
            if (tipoTamanho == "Numero")
            {
                if (nomeFamilia.ToLower().Contains("sapato"))
                    for (int i = 34; i <= 48; i++) cmb.Items.Add(i.ToString());
                else
                    for (int i = 30; i <= 50; i += 2) cmb.Items.Add(i.ToString());
            }
            else // Letra
            {
                cmb.Items.AddRange(new object[] { "XXS", "XS", "S", "M", "L", "XL", "XXL", "XXXL" });
            }

            if (cmb.Items.Count > 0)
                cmb.SelectedIndex = 0;
        }

        // =====================================================================
        // MODO (CRIAR / EDITAR)
        // =====================================================================

        private void ConfigurarModo()
        {
            if (NMEC.HasValue)
            {
                this.Text = "Editar Funcionário";
                txtNr.Text = NMEC.Value.ToString();
                txtNr.Enabled = false;
                CarregarFunc(NMEC.Value);
            }
            else
            {
                this.Text = "Novo Funcionário";
                txtNr.Text = "";
                txtNr.Enabled = true;
                txtNome.Focus();
            }
        }

        /// <summary>
        /// Carrega Nome, Função e Estabelecimento do funcionário.
        /// Os tamanhos são carregados separadamente em CarregarTamanhosFuncionario,
        /// chamado no Load depois de CarregarFamiliasDinamicas criar os combos.
        /// </summary>
        private void CarregarFunc(int nr)
        {
            try
            {
                using var cn = new SqlConnection(GetConn.ConnectionString);
                cn.Open();

                using var cmd = new SqlCommand("sp_ObterFuncionarioPorNr", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nr", nr);

                using var rd = cmd.ExecuteReader();
                if (!rd.Read())
                {
                    M.AbrirMensagem($"Funcionário com Nº {nr} não encontrado.", "Erro");
                    return;
                }
                txtNome.Text = rd["Nome"]?.ToString() ?? "";
                SelectByText(cmbFuncoes, rd["NomeFuncao"]?.ToString() ?? "");
                SelectByText(cmbEstab,   rd["NomeEstab"]?.ToString()  ?? "");

                Debug.WriteLine($"[EditFunc] Funcionário {nr} carregado.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[EditFunc] Erro: " + ex);
                M.AbrirMensagem("Erro ao carregar funcionário: " + ex.Message, "Erro");
            }
        }

        /// <summary>
        /// Preenche os combos de tamanho para o funcionário indicado.
        /// Usa fallback para os defaults da linha Nr=0 se o funcionário não tiver tamanho definido.
        /// Chamado no Load, depois de CarregarFamiliasDinamicas.
        /// </summary>
        private void CarregarTamanhosFuncionario(int nr)
        {
            if (_combosSize.Count == 0) return;
            try
            {
                using var cn = new SqlConnection(GetConn.ConnectionString);
                cn.Open();

                string sqlTam = @"SELECT d.Familia, ISNULL(f.Tamanho, d.Tamanho) AS Tamanho
                                  FROM FuncionarioTamanhos d
                                  LEFT JOIN FuncionarioTamanhos f ON f.Nr=@Nr AND f.Familia=d.Familia
                                  WHERE d.Nr = 0";
                using var cmdTam = new SqlCommand(sqlTam, cn);
                cmdTam.Parameters.AddWithValue("@Nr", nr);

                using var rdTam = cmdTam.ExecuteReader();
                while (rdTam.Read())
                {
                    string familia = rdTam["Familia"].ToString();
                    string tamanho = rdTam["Tamanho"].ToString();
                    if (_combosSize.TryGetValue(familia, out var cmb))
                        SelectByText(cmb, tamanho);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[EditFunc] Erro tamanhos: " + ex.Message);
            }
        }

        private void FormFuncionario_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            // Agora o layout está calculado e flpTamanhos.Height tem valor real
            CarregarFamiliasDinamicas();
            // Se for editar, recarrega os tamanhos depois de os combos existirem
            if (NMEC.HasValue)
                CarregarTamanhosFuncionario(NMEC.Value);
        }

        private void SelectByText(Guna2ComboBox cb, string text)
        {
            if (cb == null || string.IsNullOrWhiteSpace(text)) return;
            int idx = cb.FindStringExact(text);
            if (idx < 0) idx = cb.FindString(text);
            if (idx >= 0) cb.SelectedIndex = idx;
        }

        // =====================================================================
        // GUARDAR
        // =====================================================================

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNr.Text) || string.IsNullOrWhiteSpace(txtNome.Text))
            {
                M.AbrirMensagem("O Número e o Nome são obrigatórios.", "Dados em Falta");
                return;
            }

            try
            {
                bool isEdicao = this.NMEC.HasValue;
                int nrFunc = isEdicao ? this.NMEC.Value : int.Parse(txtNr.Text);

                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                {
                    conn.Open();
                    GetConn.SetContext(conn);

                    // Dados base do funcionário
                    using (SqlCommand cmd = new SqlCommand("sp_UPSERT_FUNC", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Modo",     isEdicao ? "U" : "I");
                        cmd.Parameters.AddWithValue("@Nr",       nrFunc);
                        cmd.Parameters.AddWithValue("@Nome",     txtNome.Text.Trim());
                        cmd.Parameters.AddWithValue("@FuncaoId", cmbFuncoes.SelectedValue ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EstabId",  cmbEstab.SelectedValue   ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    // Tamanhos — UPSERT em FuncionarioTamanhos para cada família ativa
                    string upsertTam = @"IF EXISTS (SELECT 1 FROM FuncionarioTamanhos WHERE Nr=@Nr AND Familia=@Familia)
                        UPDATE FuncionarioTamanhos SET Tamanho=@Tamanho WHERE Nr=@Nr AND Familia=@Familia
                    ELSE
                        INSERT INTO FuncionarioTamanhos (Nr, Familia, Tamanho) VALUES (@Nr, @Familia, @Tamanho)";

                    foreach (var (familia, cmb) in _combosSize)
                    {
                        string tamanho = cmb.Text?.Trim();
                        if (string.IsNullOrWhiteSpace(tamanho)) continue;

                        using (var cmdTam = new SqlCommand(upsertTam, conn))
                        {
                            cmdTam.Parameters.AddWithValue("@Nr",      nrFunc);
                            cmdTam.Parameters.AddWithValue("@Familia", familia);
                            cmdTam.Parameters.AddWithValue("@Tamanho", tamanho);
                            cmdTam.ExecuteNonQuery();
                        }
                    }

                    if (!isEdicao)
                        GestorDeLogins.RegistarOuAtualizarLogin(nrFunc.ToString());
                }

                M.AbrirMensagem(isEdicao ? "Dados atualizados com sucesso!" : "Funcionário criado com sucesso!", "Sucesso");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (SqlException ex)
            {
                M.AbrirMensagem(ex.Message, "Erro de Validação");
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Erro inesperado: " + ex.Message, "Erro");
            }
        }

        // =====================================================================
        // COMBOS AUXILIARES
        // =====================================================================

        private void CarregarComboFuncoes(int NivelAcesso)
        {
            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            {
                try
                {
                    conn.Open();
                    using var cmd = new SqlCommand(
                        "SELECT ID, Nome FROM Funcoes WHERE ISNULL(NivelAcesso, 0) >= @nivel ORDER BY Nome", conn);
                    cmd.Parameters.AddWithValue("@nivel", NivelAcesso);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmbFuncoes.DataSource = dt;
                    cmbFuncoes.DisplayMember = "Nome";
                    cmbFuncoes.ValueMember = "ID";

                    if (cmbFuncoes.Items.Count > 0)
                        cmbFuncoes.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    M.AbrirMensagem("Erro ao carregar funções: " + ex.Message, "Erro");
                }
            }
        }

        private void CarregaComboEstabs()
        {
            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT ID, Nome FROM Estabelecimentos ORDER BY ID";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbEstab.DataSource = dt;
                    cmbEstab.DisplayMember = "Nome";
                    cmbEstab.ValueMember = "ID";
                }
                catch (Exception ex)
                {
                    M.AbrirMensagem("Erro ao carregar estabelecimentos: " + ex.Message, "Erro");
                }
            }
        }

        // =====================================================================
        // EVENTOS
        // =====================================================================

        private void lblFechar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormFuncionario_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
                this.Close();
        }

        private void txtNr_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void txtNr_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNr.Text)) return;

            if (VerificarSeNrExiste(txtNr.Text))
            {
                M.AbrirMensagem("Este Número Mecanográfico já está atribuído!", "Erro de Duplicado");
                txtNr.Clear();
                txtNr.Focus();
            }
        }

        private bool VerificarSeNrExiste(string nr)
        {
            using (SqlConnection conn = GetConn.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Funcionarios WHERE Nr = @nr", conn);
                cmd.Parameters.AddWithValue("@nr", nr);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}
