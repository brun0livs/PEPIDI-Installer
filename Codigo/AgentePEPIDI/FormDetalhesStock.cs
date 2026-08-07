using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace AgentePEPIDI
{
    /// <summary>
    /// Janela de detalhe que lista os EPIs com stock abaixo do mínimo definido
    /// na tabela Definicoes (chave 'StockMinimo'). Abre quando o utilizador clica
    /// no aviso em balão do AgentePEPIDI; o limite é relido da BD em cada abertura
    /// para refletir sempre a configuração mais recente.
    /// </summary>
    public partial class FormDetalhesStock : Form
    {
        public FormDetalhesStock()
        {
            InitializeComponent();
        }

        private async void FormDetalhesStock_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = CONN.GetConnection()) // Usa a tua ponte CONN
                {
                    await conn.OpenAsync();

                    // 1. Vai buscar o limite atual às definições
                    int limite = 0;
                    string sqlDef = "SELECT Valor FROM Definicoes WHERE Chave = 'StockMinimo'";
                    using (SqlCommand cmdDef = new SqlCommand(sqlDef, conn))
                    {
                        var result = await cmdDef.ExecuteScalarAsync();
                        if (result != null) limite = Convert.ToInt32(result);
                    }

                    // 2. A Query que vai buscar o NOME e a QUANTIDADE dos EPIs
                    // ATENÇÃO: Ajusta os nomes das colunas/tabelas conforme a tua BD real!
                    string query = @"SELECT
                                        E.Modelo AS 'Equipamento',
                                        S.Quantidade AS 'Quantidade Atual'
                                    FROM Stock S
                                    INNER JOIN EPI E ON S.Codigo = E.Codigo
                                    WHERE S.Quantidade <= @limite
                                      AND S.Estado = 1";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@limite", limite);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // 3. Preenche a grelha do Guna!
                            dgvStockBaixo.DataSource = dt;
                        }
                    }
                }

                // 4. Aplica cores às linhas conforme a gravidade do stock
                ColorirLinhasPorGravidade();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar detalhes: " + ex.Message);
            }
        }

        /// <summary>
        /// Pinta o cartão de cada linha conforme a quantidade:
        ///   • Quantidade = 0     → vermelho forte (esgotado)
        ///   • Quantidade ≤ 2     → vermelho claro (crítico)
        ///   • Quantidade ≤ 5     → amarelo claro  (alerta)
        ///   • Acima de 5         → mantém o estilo padrão (sem cor especial)
        /// Usa o hook RowCardColorProvider do PEPIDIDataGridView (a cor de fundo
        /// das células é sobreposta pelo paint dos cartões — daí ter de ser por aqui).
        /// O texto é gerido em paralelo via ForeColor por linha, que o paint do card respeita.
        /// </summary>
        private void ColorirLinhasPorGravidade()
        {
            if (dgvStockBaixo.Columns.Count == 0 || !dgvStockBaixo.Columns.Contains("Quantidade Atual"))
                return;

            // Cores: (fundo do cartão, texto). null = manter padrão.
            (Color fundo, Color texto)? CoresPara(int qtd)
            {
                if (qtd == 0)  return (Color.FromArgb(231,  76,  60), Color.White);                 // vermelho forte
                if (qtd <= 2)  return (Color.FromArgb(255, 205, 210), Color.FromArgb(176,   0, 32)); // vermelho claro
                if (qtd <= 5)  return (Color.FromArgb(255, 243, 205), Color.FromArgb(165,  92,  0)); // amarelo claro
                return null;
            }

            int LerQuantidade(int rowIndex)
            {
                if (rowIndex < 0 || rowIndex >= dgvStockBaixo.Rows.Count) return -1;
                var v = dgvStockBaixo.Rows[rowIndex].Cells["Quantidade Atual"].Value;
                if (v == null) return -1;
                return int.TryParse(v.ToString(), out int q) ? q : -1;
            }

            // 1) Cor de fundo do cartão via hook do PEPIDIDataGridView
            dgvStockBaixo.RowCardColorProvider = idx =>
            {
                int q = LerQuantidade(idx);
                return q < 0 ? null : CoresPara(q)?.fundo;
            };

            // 2) Cor do texto por linha (não é sobreposta pelo paint do cartão)
            foreach (DataGridViewRow row in dgvStockBaixo.Rows)
            {
                int q = LerQuantidade(row.Index);
                var cores = q < 0 ? null : CoresPara(q);
                if (cores.HasValue)
                {
                    row.DefaultCellStyle.ForeColor = cores.Value.texto;
                    row.DefaultCellStyle.SelectionForeColor = cores.Value.texto;
                }
            }

            dgvStockBaixo.Invalidate();
        }
    }
}