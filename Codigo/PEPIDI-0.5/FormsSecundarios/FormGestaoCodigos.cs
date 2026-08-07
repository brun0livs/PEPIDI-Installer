using Microsoft.Data.SqlClient;
using PEPIDI.Organizers;
using PEPIDI.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PEPIDI.FormsSecundarios
{
    /// <summary>
    /// Gestor central de Famílias / Códigos EPI.
    /// Permite editar inline o Prefixo, Nome Visível, Tipo (Letra/Numero) e estado Ativo de cada família,
    /// adicionar famílias novas e eliminar famílias sem EPIs associados.
    ///
    /// Conflitos de Prefixo são detectados ao Guardar: se família X passa para o prefixo de Y, abre o
    /// <see cref="FormConflitoPrefixo"/> para o utilizador decidir o novo prefixo de Y antes do commit.
    /// Toda a operação acontece numa única transação SQL — falha em qualquer linha → rollback total.
    /// </summary>
    public partial class FormGestaoCodigos : Form
    {
        private readonly EfeitoUI M = new();

        /// <summary>Snapshot do estado inicial vindo da BD — usado para detectar mudanças.</summary>
        private readonly Dictionary<string, string> _prefixoOriginal = new();

        /// <summary>Famílias eliminadas durante esta sessão (commit pendente até Guardar).</summary>
        private readonly HashSet<string> _eliminadas = new();

        public FormGestaoCodigos()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
        }

        // ================================================================================
        // CICLO DE VIDA
        // ================================================================================

        private void FormGestaoCodigos_Load(object sender, EventArgs e)
        {
            ConfigurarGrelha();
            CarregarFamilias();
            cmbTipoNova.SelectedIndex = 0;
            GestorTema.AplicarEstilos(this);
        }

        private void lblFechar_Click(object sender, EventArgs e) => Close();
        private void btnCancelar_Click(object sender, EventArgs e) => Close();

        // ================================================================================
        // GRELHA
        // ================================================================================

        private void ConfigurarGrelha()
        {
            dgvFamilias.DataError += (s, ev) => ev.Cancel = true;
            dgvFamilias.Columns.Clear();

            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Prefixo",
                HeaderText = "Prefixo",
                FillWeight = 12,
                MinimumWidth = 70,
                ToolTipText = "2 dígitos numéricos (01–99)"
            });
            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nome",
                HeaderText = "Código",
                ReadOnly = true,
                FillWeight = 22,
                MinimumWidth = 90
            });
            dgvFamilias.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NomeVista",
                HeaderText = "Nome Visível",
                FillWeight = 36,
                MinimumWidth = 140
            });
            var colTipo = new DataGridViewComboBoxColumn
            {
                Name = "TipoTamanho",
                HeaderText = "Tipo",
                FillWeight = 18,
                MinimumWidth = 90,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
            };
            colTipo.Items.AddRange("Letra", "Numero");
            dgvFamilias.Columns.Add(colTipo);
            dgvFamilias.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "Ativo",
                HeaderText = "Ativo",
                FillWeight = 12,
                MinimumWidth = 60
            });
        }

        private void CarregarFamilias()
        {
            dgvFamilias.Rows.Clear();
            _prefixoOriginal.Clear();
            _eliminadas.Clear();

            try
            {
                using var conn = GetConn.GetConnection();
                conn.Open();
                using var cmd = new SqlCommand(
                    "SELECT Nome, Prefixo, NomeVista, TipoTamanho, Ativo FROM Familias ORDER BY Prefixo", conn);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string nome  = rdr["Nome"].ToString();
                    string pref  = rdr["Prefixo"]?.ToString() ?? "";
                    string vista = rdr["NomeVista"]?.ToString() ?? nome;
                    string tipo  = rdr["TipoTamanho"]?.ToString() ?? "Letra";
                    bool ativo   = rdr["Ativo"] != DBNull.Value && (bool)rdr["Ativo"];

                    dgvFamilias.Rows.Add(pref, nome, vista, tipo, ativo);
                    _prefixoOriginal[nome] = pref;
                }
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Erro a carregar famílias: " + ex.Message, "Erro");
            }
        }

        // ================================================================================
        // ADICIONAR
        // ================================================================================

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            string nome  = (txtNomeNova.Text ?? "").Trim();
            string pref  = (txtPrefixoNova.Text ?? "").Trim();
            string vista = (txtVistaNova.Text ?? "").Trim();
            string tipo  = cmbTipoNova.SelectedItem?.ToString() ?? "Letra";

            if (string.IsNullOrEmpty(nome))
            {
                M.AbrirMensagem("Indica o código interno da família (ex: 'Capacete').", "Família Inválida");
                return;
            }
            if (pref.Length != 2 || !pref.All(char.IsDigit))
            {
                M.AbrirMensagem("O Prefixo tem de ter exactamente 2 dígitos (ex: 08).", "Prefixo Inválido");
                return;
            }
            if (string.IsNullOrEmpty(vista)) vista = nome;

            // Duplicados
            if (LinhasGrelha().Any(r => StringEquals(r.Cells["Nome"].Value, nome)))
            {
                M.AbrirMensagem($"Já existe uma família com o código '{nome}'.", "Duplicado");
                return;
            }
            if (LinhasGrelha().Any(r => StringEquals(r.Cells["Prefixo"].Value, pref)))
            {
                M.AbrirMensagem($"O Prefixo '{pref}' já está em uso. Escolhe outro ou usa o gestor de conflitos para reorganizar.", "Prefixo Ocupado");
                return;
            }

            dgvFamilias.Rows.Add(pref, nome, vista, tipo, true);
            // Não entra em _prefixoOriginal — sinaliza que é INSERT no Guardar

            txtNomeNova.Clear();
            txtPrefixoNova.Clear();
            txtVistaNova.Clear();
            cmbTipoNova.SelectedIndex = 0;
            txtNomeNova.Focus();
        }

        // ================================================================================
        // ELIMINAR
        // ================================================================================

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvFamilias.CurrentRow == null) return;
            var row = dgvFamilias.CurrentRow;
            string nome = row.Cells["Nome"].Value?.ToString();
            if (string.IsNullOrEmpty(nome)) return;

            // Verifica se a família tem EPIs associados
            int epis = ContarEpisDaFamilia(nome);
            if (epis > 0)
            {
                M.AbrirMensagem(
                    $"A família '{nome}' tem {epis} EPI(s) associado(s) e não pode ser eliminada.\n" +
                    "Para a desativar sem perder dados, desmarca a checkbox 'Ativo' e Guarda.",
                    "Eliminação Bloqueada");
                return;
            }

            var conf = MessageBox.Show(
                $"Eliminar definitivamente a família '{nome}'?\n\nEsta ação só será confirmada ao clicar em Guardar.",
                "Confirmar Eliminação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (conf != DialogResult.Yes) return;

            // Se a família existia na BD, marca para DELETE; senão, era uma adição local
            if (_prefixoOriginal.ContainsKey(nome))
                _eliminadas.Add(nome);

            dgvFamilias.Rows.Remove(row);
        }

        private int ContarEpisDaFamilia(string nome)
        {
            try
            {
                using var conn = GetConn.GetConnection();
                conn.Open();
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM EPI WHERE Familia = @f", conn);
                cmd.Parameters.AddWithValue("@f", nome);
                return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }
            catch { return -1; }
        }

        // ================================================================================
        // GUARDAR — com deteção e resolução de conflitos de prefixo
        // ================================================================================

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // 1. Validação de prefixos da grelha
            foreach (DataGridViewRow row in LinhasGrelha())
            {
                string pref = row.Cells["Prefixo"].Value?.ToString()?.Trim() ?? "";
                if (pref.Length != 2 || !pref.All(char.IsDigit))
                {
                    string nome = row.Cells["Nome"].Value?.ToString() ?? "(sem nome)";
                    M.AbrirMensagem($"Família '{nome}': o Prefixo deve ter exactamente 2 dígitos numéricos.", "Prefixo Inválido");
                    return;
                }
            }

            // 2. Detecta colisões na grelha (dois prefixos iguais)
            var grupos = LinhasGrelha()
                .GroupBy(r => r.Cells["Prefixo"].Value?.ToString()?.Trim())
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var g in grupos)
            {
                // Resolve colisão: pega na família que mudou (não estava com este prefixo originalmente)
                // e nas restantes que já tinham este prefixo
                string prefixoColidido = g.Key;
                var familias = g.Select(r => r.Cells["Nome"].Value?.ToString()).ToList();

                // Família que vem "de fora" — aquela cujo prefixo original era diferente
                var mudou = g.FirstOrDefault(r =>
                {
                    string nome = r.Cells["Nome"].Value?.ToString();
                    return _prefixoOriginal.ContainsKey(nome) && _prefixoOriginal[nome] != prefixoColidido;
                });
                if (mudou == null) mudou = g.First();

                // Famílias que precisam de novo prefixo (todas menos a que mudou para aqui)
                var familiasAfetadas = g.Where(r => r != mudou).ToList();

                foreach (var afet in familiasAfetadas)
                {
                    string nomeAfet = afet.Cells["Nome"].Value?.ToString();
                    string nomeIntruso = mudou.Cells["Nome"].Value?.ToString();
                    string proximoLivre = ProximoPrefixoLivre();

                    using var dlg = new FormConflitoPrefixo(prefixoColidido, nomeIntruso, nomeAfet, proximoLivre);
                    var res = dlg.ShowDialog(this);
                    if (res != DialogResult.OK)
                    {
                        // Utilizador cancelou → restaura prefixos para o que estava na BD
                        return;
                    }

                    afet.Cells["Prefixo"].Value = dlg.NovoPrefixo;
                }
            }

            // 3. Commit em transação
            try
            {
                using var conn = GetConn.GetConnection();
                conn.Open();
                GetConn.SetContext(conn);
                using var tran = conn.BeginTransaction();
                try
                {
                    // 3a. DELETE famílias marcadas
                    foreach (var nome in _eliminadas)
                    {
                        using var cmdDel = new SqlCommand("DELETE FROM Familias WHERE Nome = @N", conn, tran);
                        cmdDel.Parameters.AddWithValue("@N", nome);
                        cmdDel.ExecuteNonQuery();
                    }

                    // 3b. UPSERT cada linha visível
                    foreach (DataGridViewRow row in LinhasGrelha())
                    {
                        string nome  = row.Cells["Nome"].Value?.ToString();
                        string pref  = row.Cells["Prefixo"].Value?.ToString()?.Trim();
                        string vista = row.Cells["NomeVista"].Value?.ToString() ?? nome;
                        string tipo  = row.Cells["TipoTamanho"].Value?.ToString() ?? "Letra";
                        bool ativo   = Convert.ToBoolean(row.Cells["Ativo"].Value ?? false);

                        if (_prefixoOriginal.ContainsKey(nome))
                        {
                            // UPDATE
                            using var cmd = new SqlCommand(
                                "UPDATE Familias SET Prefixo=@P, NomeVista=@V, TipoTamanho=@T, Ativo=@A WHERE Nome=@N",
                                conn, tran);
                            cmd.Parameters.AddWithValue("@N", nome);
                            cmd.Parameters.AddWithValue("@P", pref);
                            cmd.Parameters.AddWithValue("@V", vista);
                            cmd.Parameters.AddWithValue("@T", tipo);
                            cmd.Parameters.AddWithValue("@A", ativo);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            // INSERT
                            using var cmd = new SqlCommand(
                                "INSERT INTO Familias (Nome, Prefixo, NomeVista, TipoTamanho, Ativo) " +
                                "VALUES (@N, @P, @V, @T, @A)", conn, tran);
                            cmd.Parameters.AddWithValue("@N", nome);
                            cmd.Parameters.AddWithValue("@P", pref);
                            cmd.Parameters.AddWithValue("@V", vista);
                            cmd.Parameters.AddWithValue("@T", tipo);
                            cmd.Parameters.AddWithValue("@A", ativo);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tran.Commit();
                    M.AbrirMensagem("Famílias atualizadas com sucesso.", "Sucesso");
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Erro ao guardar: " + ex.Message, "Erro");
            }
        }

        // ================================================================================
        // HELPERS
        // ================================================================================

        private IEnumerable<DataGridViewRow> LinhasGrelha()
        {
            foreach (DataGridViewRow r in dgvFamilias.Rows)
                if (!r.IsNewRow) yield return r;
        }

        private static bool StringEquals(object cellValue, string other)
            => string.Equals(cellValue?.ToString()?.Trim(), other?.Trim(), StringComparison.OrdinalIgnoreCase);

        /// <summary>Devolve o próximo prefixo livre (1..99) considerando o que está na grelha agora.</summary>
        private string ProximoPrefixoLivre()
        {
            var usados = LinhasGrelha()
                .Select(r => r.Cells["Prefixo"].Value?.ToString()?.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToHashSet();

            for (int i = 1; i <= 99; i++)
            {
                string candidato = i.ToString("D2");
                if (!usados.Contains(candidato)) return candidato;
            }
            return "99"; // capacidade esgotada — fallback
        }
    }
}
