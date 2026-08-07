using Guna.Charts.Interfaces;
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using PEPIDI.Organizers;
using PEPIDI.Utils;
using PEPIDI.FormsSecundarios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PEPIDI.Models;
using ClosedXML.Excel;
using System.IO;

namespace PEPIDI.UCs
{
    /// <summary>
    /// UC de análise e visualização de dados de consumo de EPIs.
    /// Carrega os filtros (Função, Família, Modelo, Funcionário, Departamento, datas)
    /// em paralelo no Load para minimizar o tempo de espera inicial.
    /// Os botões de filtro são "toggle" — clicar novamente desativa o filtro.
    /// O debounce de 400ms (CancellationTokenSource) garante que a query só é executada
    /// quando o utilizador parou de clicar, não a cada clique individual.
    /// O gráfico de barras é desenhado manualmente com GDI+ (sem controlo externo).
    /// A exportação Excel usa um template embutido com ClosedXML para redimensionar a tabela.
    /// idFuncionario opcional: quando fornecido, pré-filtra pelo funcionário (usado em Funcionarios.cs).
    /// </summary>
    public partial class Graficos : UserControl
    {
        // WM_SETREDRAW suspende o redesenho do painel durante a montagem dos filtros,
        // eliminando o efeito de "montagem" visual quando há muitos botões a ser criados
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;

        private EfeitoUI M = new EfeitoUI();

        private bool _modoMensal = false;
        private Label _lblKpiUnidades, _lblKpiGasto, _lblKpiEPI, _lblKpiFunc;

        // CancellationTokenSource do debounce — cancelado e recriado a cada clique de filtro
        private System.Threading.CancellationTokenSource _filtroCts = new();

        private int? _idFuncionarioFiltroInicial = null;

        public Graficos(int? idFuncionario = null)
        {
            InitializeComponent();
            _idFuncionarioFiltroInicial = idFuncionario;
        }

        // ==========================================
        // 1. CARREGAMENTO INICIAL (LOAD)
        // ==========================================
        private async void Grafico_Load(object sender, EventArgs e)
        {
            // Suspende o redesenho para evitar o efeito de "montagem" visual dos filtros
            SendMessage(this.Handle, WM_SETREDRAW, false, 0);

            // 2. Performance bruta: Ativa DoubleBuffer via Organizer
            PEPIDI.Organizers.HelperPerformance.AtivarDoubleBufferRecursivo(this);

            try
            {
                // 3. Cria a barra de KPIs e o botão de modo mensal (sem tocar no Designer)
                CriarBarraKPIs();
                AdicionarBotaoMeses();

                // 4. Executa todas as chamadas SQL em paralelo (Multi-core)
                var tarefas = new List<Task>
                {
                    CarregarFuncionariosAsync(),
                    CarregarFuncoesAsync(),
                    CarregarFiltrosTextoAsync("Familia", flpFamilia),
                    CarregarFiltrosTextoAsync("Modelo", flpModelos),
                    CarregarFiltrosTextoAsync("Tamanho", flpTamanhos)
                };

                await Task.WhenAll(tarefas);

                dtpInicio.Value = new DateTime(DateTime.Now.Year, 1, 1);
                dtpFim.Value = DateTime.Now;

                // 4. Desenha o gráfico inicial
                FiltrosWorking(tbNivelGrafico.Value, dgvTabela1);
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Erro no carregamento inicial: " + ex.Message, "Erro");
            }
            finally
            {
                // 5. Liberta o desenho e força um refresh único
                SendMessage(this.Handle, WM_SETREDRAW, true, 0);
                // Ligar o Scroll Automático para os painéis de filtros não esconderem botões!
                flpFuncoes.AutoScroll = true;
                flpFamilia.AutoScroll = true;
                flpModelos.AutoScroll = true;
                flpTamanhos.AutoScroll = true;

                // Para garantir que a barra de scroll não tapa o último botão à direita
                flpFuncoes.WrapContents = true;
                flpFamilia.WrapContents = true;
                flpModelos.WrapContents = true;
                flpTamanhos.WrapContents = true;
                this.Refresh();
                GestorTema.AplicarEstilos(this);
            }
        }

        // ==========================================
        // 2. MÉTODOS DE CONSTRUÇÃO VISUAL (OTIMIZADOS)
        // ==========================================

        private async Task CarregarFuncionariosAsync()
        {
            var dtFuncs = new DataTable();
            dtFuncs.Columns.Add("Nr", typeof(int));
            dtFuncs.Columns.Add("NomeCompleto", typeof(string));

            // 1. O NOVO ID FALSO (Impossível de conflitar com a BD)
            dtFuncs.Rows.Add(-999, "— TODOS OS FUNCIONÁRIOS —");

            await Task.Run(() =>
            {
                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                // 2. Filtramos os IDs de sistema (-1, 0, etc.) para não aparecerem na ComboBox
                using (SqlCommand cmd = new SqlCommand("SELECT Nr, Nome FROM Funcionarios WHERE Nr > 0 ORDER BY Nr", conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int nr = Convert.ToInt32(r["Nr"]);
                            dtFuncs.Rows.Add(nr, $"{nr} - {r["Nome"]}");
                        }
                    }
                }
            });

            cmbFuncs.SelectedIndexChanged -= Filtros_Changed;
            cmbFuncs.DataSource = dtFuncs;
            cmbFuncs.DisplayMember = "NomeCompleto";
            cmbFuncs.ValueMember = "Nr";

            // A NOSSA MAGIA: Se o ecrã foi chamado com um ID de funcionário, seleciona-o já!
            if (_idFuncionarioFiltroInicial.HasValue)
            {
                cmbFuncs.SelectedValue = _idFuncionarioFiltroInicial.Value;
                // Limpamos para não bloquear se o utilizador depois quiser ver "Todos"
                _idFuncionarioFiltroInicial = null;
            }
            else
            {
                cmbFuncs.SelectedIndex = -1;
            }

            cmbFuncs.SelectedIndexChanged += Filtros_Changed;
        }

        private async Task CarregarFuncoesAsync() => await CarregarGenericoAsync("SELECT ID, Nome FROM Funcoes ORDER BY Nome", "ID", "Nome", flpFuncoes);

        private async Task CarregarFiltrosTextoAsync(string coluna, FlowLayoutPanel painel)
            => await CarregarGenericoAsync($"SELECT DISTINCT [{coluna}] FROM EPI WHERE [{coluna}] IS NOT NULL AND [{coluna}] <> '' ORDER BY [{coluna}]", coluna, coluna, painel);

        private async Task CarregarGenericoAsync(string query, string colID, string colNome, FlowLayoutPanel painel)
        {
            var lista = new List<KeyValuePair<string, string>>();
            await Task.Run(() =>
            {
                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            lista.Add(new KeyValuePair<string, string>(r[colID].ToString(), r[colNome].ToString().Trim()));
                    }
                }
            });

            var botoes = lista.Select(item => GerarTagBotao(item.Key, item.Value)).ToArray();

            painel.SuspendLayout();
            painel.Controls.Clear();
            painel.Controls.AddRange(botoes);
            painel.ResumeLayout(true);
        }

        private Guna2Button GerarTagBotao(string idOuTexto, string labelVisivel)
        {
            Guna2Button tag = new Guna2Button
            {
                Name = idOuTexto,
                Text = labelVisivel,
                Font = new Font("Roboto", 9F, FontStyle.Regular),
                BorderRadius = 15,
                Cursor = Cursors.Hand,
                Animated = false,
                FillColor = Color.FromArgb(230, 232, 235),
                ForeColor = Color.FromArgb(64, 64, 64),
                Margin = new Padding(0, 0, 8, 8),
                Tag = false
            };

            int larguraTexto = TextRenderer.MeasureText(labelVisivel, tag.Font).Width;
            tag.Size = new Size(larguraTexto + 25, 32);

            tag.Click += (s, e) =>
            {
                tag.Tag = !(bool)tag.Tag;
                tag.FillColor = (bool)tag.Tag ? Color.FromArgb(242, 103, 34) : Color.FromArgb(230, 232, 235);
                tag.ForeColor = (bool)tag.Tag ? Color.White : Color.FromArgb(64, 64, 64);
                AcionarFiltroComDelay();
            };

            return tag;
        }

        // ==========================================
        // 3. MOTOR DE FILTROS (DEBOUNCE)
        // ==========================================

        private void Filtros_Changed(object sender, EventArgs e) => AcionarFiltroComDelay();

        private async void AcionarFiltroComDelay()
        {
            _filtroCts.Cancel();
            _filtroCts = new System.Threading.CancellationTokenSource();

            try
            {
                await Task.Delay(400, _filtroCts.Token);
                FiltrosWorking(tbNivelGrafico.Value, dgvTabela1);
            }
            catch (OperationCanceledException) { }
        }

        private string GetSelectedTags(FlowLayoutPanel flp)
        {
            var ativos = flp.Controls.OfType<Guna2Button>()
                             .Where(b => (bool)b.Tag == true)
                             .Select(b => b.Name);
            return string.Join(",", ativos);
        }

        private async void FiltrosWorking(int nivelDetalhe, PEPIDIDataGridView dgvTabela)
        {
            int nrFunc = cmbFuncs.SelectedValue is int i ? i : -999;
            string funcoesStr = GetSelectedTags(flpFuncoes);
            string familiasStr = GetSelectedTags(flpFamilia);
            string modelosStr = GetSelectedTags(flpModelos);
            string tamanhosStr = GetSelectedTags(flpTamanhos);
            DateTime inicio = dtpInicio.Value.Date;
            DateTime fim = dtpFim.Value.Date;

            DataTable dt = new DataTable();

            await Task.Run(() =>
            {
                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                using (SqlCommand cmd = new SqlCommand("sp_ConsumosFiltrados", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NrFunc", nrFunc != -999 ? (object)nrFunc : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Funcoes", string.IsNullOrEmpty(funcoesStr) ? (object)DBNull.Value : funcoesStr);
                    cmd.Parameters.AddWithValue("@Familias", string.IsNullOrEmpty(familiasStr) ? (object)DBNull.Value : familiasStr);
                    cmd.Parameters.AddWithValue("@Modelos", string.IsNullOrEmpty(modelosStr) ? (object)DBNull.Value : modelosStr);
                    cmd.Parameters.AddWithValue("@Tamanhos", string.IsNullOrEmpty(tamanhosStr) ? (object)DBNull.Value : tamanhosStr);
                    cmd.Parameters.AddWithValue("@DataInicio", inicio);
                    cmd.Parameters.AddWithValue("@DataFim", fim);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            });

            // 1. Atribuir os dados à tabela
            dgvTabela.DataSource = dt;

            // 2. Ocultar colunas desnecessárias para a visualização da tabela
            string[] colsToHide = { "Funcao", "Familia", "PrecoUnitario", "TotalGasto" };
            foreach (string colName in colsToHide)
            {
                if (dgvTabela.Columns.Contains(colName))
                    dgvTabela.Columns[colName].Visible = false;
            }

            // 3. Configuração de AutoSize (O SEGREDO DO DESIGN)
            // Primeiro: todas as colunas ocupam apenas o espaço do texto
            dgvTabela.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            // Segundo: A coluna do Nome "estica" para ocupar o resto do ecrã
            if (dgvTabela.Columns.Contains("NomeFuncionario"))
            {
                dgvTabela.Columns["NomeFuncionario"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            // 4. Atualizar o gráfico e os KPIs
            AtualizarGrafico(dt, nivelDetalhe);
            AtualizarKPIs(dt);
        }

        private void AtualizarGrafico(DataTable dt, int nivelDetalhe)
        {
            Grafico.Datasets.Clear();
            Grafico.Update();
            if (dt.Rows.Count == 0) return;

            var dataset = new GunaLineDataset { Label = _modoMensal ? "Consumo por Mês" : "Qtd Consumida" };
            dataset.BorderColor = Color.FromArgb(242, 103, 34);
            dataset.BorderWidth = 2;
            dataset.FillColor = Color.FromArgb(60, 242, 103, 34);
            dataset.PointBorderColors.Add(Color.FromArgb(242, 103, 34));
            dataset.PointFillColors.Add(Color.FromArgb(242, 103, 34));
            dataset.PointRadius = 5;

            if (_modoMensal)
            {
                var porMes = dt.AsEnumerable()
                    .Where(r => r["Data"] != DBNull.Value)
                    .GroupBy(r => Convert.ToDateTime(r["Data"]).ToString("yyyy-MM"))
                    .Select(g => new { Mes = g.Key, Total = g.Sum(r => Convert.ToInt32(r["Quantidade"])) })
                    .OrderBy(x => x.Mes)
                    .ToList();

                foreach (var m in porMes)
                    dataset.DataPoints.Add(m.Mes, m.Total);
            }
            else
            {
                string colunaAgrupamento = nivelDetalhe switch
                {
                    1 => "Familia",
                    2 => dt.Columns.Contains("Modelo") ? "Modelo" : (dt.Columns.Contains("Artigo") ? "Artigo" : "Descricao"),
                    3 => "NomeFuncionario",
                    _ => "Funcao"
                };

                if (!dt.Columns.Contains(colunaAgrupamento)) return;

                var agrupado = dt.AsEnumerable()
                    .Where(r => nivelDetalhe != 3 || (r["NomeFuncionario"]?.ToString() != "-1" && r["NomeFuncionario"]?.ToString() != "0" && r["NomeFuncionario"]?.ToString() != "-999"))
                    .GroupBy(r => r[colunaAgrupamento]?.ToString() ?? "N/A")
                    .Select(g => new { Chave = g.Key, Total = g.Sum(r => Convert.ToInt32(r["Quantidade"])) })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                if (Grafico.Parent is Panel painelPai)
                {
                    painelPai.AutoScroll = true;
                    Grafico.Dock = DockStyle.None;
                    Grafico.Location = new Point(0, 0);
                    Grafico.Height = painelPai.ClientSize.Height - 22;
                    Grafico.Width = Math.Max(painelPai.ClientSize.Width, agrupado.Count * 90);
                }

                foreach (var item in agrupado)
                {
                    string nomeLabel = string.IsNullOrWhiteSpace(item.Chave) ? "Desconhecido" : item.Chave;
                    dataset.DataPoints.Add(nomeLabel, item.Total);
                }
            }

            Grafico.Datasets.Add(dataset);
            Grafico.XAxes.Display = true;
            if (Grafico.XAxes.Ticks != null) Grafico.XAxes.Ticks.Display = true;
            Grafico.Update();
        }

        private void AtualizarKPIs(DataTable dt)
        {
            if (_lblKpiUnidades == null || dt == null) return;

            if (dt.Rows.Count == 0)
            {
                _lblKpiUnidades.Text = "0";
                _lblKpiGasto.Text = "0,00 €";
                _lblKpiEPI.Text = "—";
                _lblKpiFunc.Text = "—";
                return;
            }

            int totalUnidades = dt.AsEnumerable().Sum(r => r["Quantidade"] != DBNull.Value ? Convert.ToInt32(r["Quantidade"]) : 0);
            decimal totalGasto = dt.AsEnumerable().Sum(r => r["TotalGasto"] != DBNull.Value ? Convert.ToDecimal(r["TotalGasto"]) : 0m);

            var topEPI = dt.AsEnumerable()
                .GroupBy(r => r["Modelo"]?.ToString() ?? "")
                .Select(g => new { Nome = g.Key, Total = g.Sum(r => Convert.ToInt32(r["Quantidade"])) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            var topFunc = dt.AsEnumerable()
                .Where(r => r["NomeFuncionario"]?.ToString() is string n && n != "-1" && n != "0" && n != "-999")
                .GroupBy(r => r["NomeFuncionario"]?.ToString() ?? "")
                .Select(g => new { Nome = g.Key, Total = g.Sum(r => Convert.ToInt32(r["Quantidade"])) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            _lblKpiUnidades.Text = totalUnidades.ToString("N0");
            _lblKpiGasto.Text = totalGasto.ToString("N2") + " €";
            _lblKpiEPI.Text = topEPI?.Nome?.Length > 20 ? topEPI.Nome[..17] + "…" : (topEPI?.Nome ?? "—");
            _lblKpiFunc.Text = topFunc?.Nome?.Length > 20 ? topFunc.Nome[..17] + "…" : (topFunc?.Nome ?? "—");
        }

        private void CriarBarraKPIs()
        {
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Insert(0, new RowStyle(SizeType.Absolute, 80F));
            tableLayoutPanel2.RowStyles[1] = new RowStyle(SizeType.Percent, 100F);
            tableLayoutPanel2.SetRow(flowLayoutPanel1, 1);
            tableLayoutPanel2.SetRow(dgvTabela1, 1);

            var tlpKPIs = new TableLayoutPanel
            {
                RowCount = 1, ColumnCount = 4,
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 4, 6, 4),
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 4; i++)
                tlpKPIs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            (_lblKpiUnidades, var c0) = CriarCartaoKPI("—", "EPIs Consumidos");
            (_lblKpiGasto,    var c1) = CriarCartaoKPI("—", "Total Gasto");
            (_lblKpiEPI,      var c2) = CriarCartaoKPI("—", "EPI Mais Pedido");
            (_lblKpiFunc,     var c3) = CriarCartaoKPI("—", "Funcionário Mais Ativo");

            tlpKPIs.Controls.Add(c0, 0, 0);
            tlpKPIs.Controls.Add(c1, 1, 0);
            tlpKPIs.Controls.Add(c2, 2, 0);
            tlpKPIs.Controls.Add(c3, 3, 0);

            tableLayoutPanel2.Controls.Add(tlpKPIs, 0, 0);
            tableLayoutPanel2.SetColumnSpan(tlpKPIs, 2);
            tableLayoutPanel2.ResumeLayout(true);
        }

        private (Label lblValor, Panel card) CriarCartaoKPI(string valorInicial, string descricao)
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Margin = new Padding(8, 3, 8, 3), BackColor = Color.White };
            var lblValor = new Label
            {
                Text = valorInicial,
                Font = new Font("Roboto Medium", 16F, FontStyle.Regular),
                ForeColor = Color.FromArgb(242, 103, 34),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 44,
                AutoSize = false
            };
            var lblDesc = new Label
            {
                Text = descricao,
                Font = new Font("Roboto", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(130, 130, 130),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                AutoSize = false
            };
            pnl.Controls.Add(lblDesc);
            pnl.Controls.Add(lblValor);
            return (lblValor, pnl);
        }

        private void AdicionarBotaoMeses()
        {
            var btnMeses = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "Por Mês",
                BorderRadius = 15,
                Cursor = Cursors.Hand,
                Dock = DockStyle.Fill,
                FillColor = Color.FromArgb(230, 232, 235),
                ForeColor = Color.FromArgb(64, 64, 64),
                Font = new Font("Roboto", 11.25F),
                Margin = new Padding(10),
                Name = "btnMeses"
            };
            btnMeses.Click += (s, e) =>
            {
                _modoMensal = !_modoMensal;
                btnMeses.FillColor = _modoMensal ? Color.FromArgb(242, 103, 34) : Color.FromArgb(230, 232, 235);
                btnMeses.ForeColor = _modoMensal ? Color.White : Color.FromArgb(64, 64, 64);
                AcionarFiltroComDelay();
            };
            tableLayoutPanel6.Controls.Add(btnMeses, 2, 0);
        }

        // ==========================================
        // 4. AÇÕES E EXPORTAÇÃO
        // ==========================================
        private void btnClear_Click(object sender, EventArgs e)
        {
            // 1. Desligar temporariamente o evento para não disparar pesquisas falsas
            cmbFuncs.SelectedIndexChanged -= Filtros_Changed;

            // 2. Limpar a ComboBox a 100% (Usar SelectedVALUE e não Index!)
            cmbFuncs.SelectedValue = -999;
            cmbFuncs.Text = "";

            // 3. Voltar a ligar o evento
            cmbFuncs.SelectedIndexChanged += Filtros_Changed;

            // 4. Repor as Datas para o primeiro dia do ano atual!
            dtpInicio.Value = new DateTime(DateTime.Now.Year, 1, 1);
            dtpFim.Value = DateTime.Now;

            // 5. Varredura Total aos Filtros Visuais (Botões)
            foreach (var painel in new[] { flpFuncoes, flpFamilia, flpModelos, flpTamanhos })
            {
                foreach (Guna2Button btn in painel.Controls.OfType<Guna2Button>())
                {
                    btn.Tag = false;
                    btn.FillColor = Color.FromArgb(230, 232, 235); // Cor base
                    btn.ForeColor = Color.FromArgb(64, 64, 64);    // Texto escuro
                }
            }

            // 6. Atualizar os gráficos INSTANTANEAMENTE
            FiltrosWorking(tbNivelGrafico.Value, dgvTabela1);
        }

        private async void ExpTab_Click(object sender, EventArgs e)
        {
            // 1. Verificação de segurança: temos dados?
            if (!(dgvTabela1.DataSource is DataTable dt) || dt.Rows.Count == 0)
            {
                M.AbrirMensagem("Não existem dados para exportar.", "Aviso");
                return;
            }

            // 2. Configuração do SaveFileDialog
            using (SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"PEPIDI_Analise_{DateTime.Now:yyyyMMdd}.xlsx"
            })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // Chamamos a Task e passamos o caminho escolhido
                    await ExportarParaTemplateHardcore(dt, sfd.FileName);
                }
            }
        }

        public async Task ExportarParaTemplateHardcore(DataTable dt, string destinationPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    string resourceName = "PEPIDI.RelatorioConsumos.xlsx";

                    using (Stream s = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (s == null) throw new Exception("Template não encontrado!");

                        using (var workbook = new XLWorkbook(s))
                        {
                            var ws = workbook.Worksheet("Dados");

                            // 1. Vai buscar a Tabela que JÁ EXISTE no template
                            var tabelaExcel = ws.Table("TabelaDados");

                            // 2. Limpa a linha em branco original para não atrapalhar
                            if (tabelaExcel.DataRange != null)
                            {
                                tabelaExcel.DataRange.Clear();
                            }

                            // 3. Injeta os dados E GUARDA na variável 'novoIntervalo' o espaço que eles ocuparam
                            var novoIntervalo = ws.Cell(2, 1).InsertData(dt);

                            // 4. O SEGREDO: Obrigar a Tabela a redimensionar-se!
                            tabelaExcel.Resize(ws.Range(1, 1, novoIntervalo.LastRow().RowNumber(), dt.Columns.Count));

                            ws.Columns().AdjustToContents();

                            // Gravar o ficheiro destino
                            workbook.SaveAs(destinationPath);
                        }
                    }
                });

                M.AbrirMensagem("Dashboard gerado com sucesso! O Excel vai abrir agora.", "PEPIDI");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(destinationPath) { UseShellExecute = true });
            }
            catch (System.IO.IOException ex)
            {
                // Verifica se o erro é especificamente de "Ficheiro em uso" (HResult 0x80070020)
                if ((uint)ex.HResult == 0x80070020)
                {
                    M.AbrirMensagem("O ficheiro Excel que estás a tentar substituir encontra-se aberto.\n\nPor favor, fecha o ficheiro no Excel e tenta novamente.", "Ficheiro em Uso");
                }
                else
                {
                    M.AbrirMensagem($"Ocorreu um erro ao aceder ao ficheiro:\n{ex.Message}", "Erro de Ficheiro");
                }
            }
            catch (Exception ex)
            {
                // Captura outros erros genéricos (template em falta, tabela não encontrada, etc.)
                M.AbrirMensagem($"Erro na exportação: {ex.Message}", "Erro de Recurso");
            }
        }

        private void ExpGraf_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFile = new SaveFileDialog { Filter = "Imagem PNG (*.png)|*.png", FileName = $"Grafico_{DateTime.Now:yyyyMMdd}.png" })
            {
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    Bitmap bmp = new Bitmap(Grafico.Width, Grafico.Height);
                    Grafico.DrawToBitmap(bmp, new Rectangle(0, 0, Grafico.Width, Grafico.Height));
                    bmp.Save(saveFile.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    M.AbrirMensagem("Gráfico guardado!", "Sucesso");
                }
            }
        }

        private void lblFS_Click(object sender, EventArgs e)
        {
            // 1. Verificar se existem dados
            if (!(dgvTabela1.DataSource is DataTable dtDados) || dtDados.Rows.Count == 0)
            {
                M.AbrirMensagem("Não existem dados para expandir.", "PEPIDI");
                return;
            }

            // 2. Criar o formulário de Zoom com estilo moderno
            using (Form formZoom = new Form())
            {
                formZoom.Text = "PEPIDI - Vista Detalhada: " + Grafico.Title.Text;
                formZoom.WindowState = FormWindowState.Maximized;
                formZoom.StartPosition = FormStartPosition.CenterScreen;
                formZoom.BackColor = Color.White;
                formZoom.ShowIcon = false;
                formZoom.KeyPreview = true; // Para fechar com ESC

                // 3. Instanciar o novo gráfico de Zoom
                Guna.Charts.WinForms.GunaChart zoomChart = new Guna.Charts.WinForms.GunaChart();
                zoomChart.Dock = DockStyle.Fill;
                zoomChart.BackColor = Color.White;

                // --- COPIAR CONFIGURAÇÕES VISUAIS DO ORIGINAL ---
                zoomChart.Title.Text = Grafico.Title.Text;
                zoomChart.Title.Font = Grafico.Title.Font;
                zoomChart.Legend.Position = Grafico.Legend.Position;
                zoomChart.XAxes.Display = Grafico.XAxes.Display;
                zoomChart.YAxes.Display = Grafico.YAxes.Display;

                // Copiar as cores das grids (opcional, para ficar igualzinho)
                zoomChart.XAxes.GridLines.Color = Grafico.XAxes.GridLines.Color;
                zoomChart.YAxes.GridLines.Color = Grafico.YAxes.GridLines.Color;

                // 4. CLONAR OS DATASETS (O segredo está em recriar o objeto)
                foreach (var originalDs in Grafico.Datasets)
                {
                    if (originalDs is Guna.Charts.WinForms.GunaLineDataset lineDs)
                    {
                        var newDs = new Guna.Charts.WinForms.GunaLineDataset();
                        newDs.Label = lineDs.Label;
                        newDs.BorderColor = lineDs.BorderColor;
                        newDs.BorderWidth = lineDs.BorderWidth;
                        newDs.FillColor = lineDs.FillColor;
                        newDs.PointRadius = lineDs.PointRadius;

                        foreach (object colorObj in lineDs.PointBorderColors)
                        {
                            if (colorObj is Color c) newDs.PointBorderColors.Add(c);
                        }
                        foreach (object colorObj in lineDs.PointFillColors)
                        {
                            if (colorObj is Color c) newDs.PointFillColors.Add(c);
                        }

                        foreach (var pointObj in lineDs.DataPoints)
                        {
                            dynamic p = pointObj;
                            newDs.DataPoints.Add(p.Label, p.Y);
                        }

                        zoomChart.Datasets.Add(newDs);
                    }
                }

                // Botão para fechar (opcional, além do X da janela)
                formZoom.KeyDown += (s, args) => { if (args.KeyCode == Keys.Escape) formZoom.Close(); };

                formZoom.Controls.Add(zoomChart);

                // 5. Forçar atualização e mostrar
                zoomChart.Update();
                formZoom.ShowDialog();
            }
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            // Procura o Form onde o UserControl foi colocado e fecha-o.
            this.FindForm()?.Close();
        }

        private void tbNivelGrafico_ValueChanged(object sender, EventArgs e) => AcionarFiltroComDelay();

        private void Grafico_DoubleClick(object sender, EventArgs e)
        {
            // 1. Verificar se existem dados
            if (!(dgvTabela1.DataSource is DataTable dtDados) || dtDados.Rows.Count == 0)
            {
                M.AbrirMensagem("Não existem dados para expandir.", "PEPIDI");
                return;
            }

            // 2. Criar o formulário de Zoom
            using (Form formZoom = new Form())
            {
                formZoom.Text = "PEPIDI - Vista Detalhada: " + Grafico.Title.Text;
                formZoom.WindowState = FormWindowState.Maximized;
                formZoom.StartPosition = FormStartPosition.CenterScreen;
                formZoom.BackColor = Color.White;
                formZoom.ShowIcon = false;
                formZoom.KeyPreview = true; // Para fechar com ESC

                Guna.Charts.WinForms.GunaChart zoomChart = new Guna.Charts.WinForms.GunaChart();
                zoomChart.Dock = DockStyle.Fill;
                zoomChart.BackColor = Color.White;

                // --- COPIAR CONFIGURAÇÕES VISUAIS DO ORIGINAL ---
                zoomChart.Title.Text = Grafico.Title.Text;
                zoomChart.Title.Font = Grafico.Title.Font;
                zoomChart.Legend.Position = Grafico.Legend.Position;
                zoomChart.XAxes.Display = Grafico.XAxes.Display;
                zoomChart.YAxes.Display = Grafico.YAxes.Display;

                // 3. CLONAR OS DATASETS
                foreach (var originalDs in Grafico.Datasets)
                {
                    if (originalDs is Guna.Charts.WinForms.GunaLineDataset lineDs)
                    {
                        var newDs = new Guna.Charts.WinForms.GunaLineDataset();
                        newDs.Label = lineDs.Label;
                        newDs.BorderColor = lineDs.BorderColor;
                        newDs.BorderWidth = lineDs.BorderWidth;
                        newDs.FillColor = lineDs.FillColor;
                        newDs.PointRadius = lineDs.PointRadius;

                        foreach (object colorObj in lineDs.PointBorderColors)
                        {
                            if (colorObj is Color c) newDs.PointBorderColors.Add(c);
                        }
                        foreach (object colorObj in lineDs.PointFillColors)
                        {
                            if (colorObj is Color c) newDs.PointFillColors.Add(c);
                        }

                        foreach (var pointObj in lineDs.DataPoints)
                        {
                            dynamic p = pointObj;
                            newDs.DataPoints.Add(p.Label, p.Y);
                        }
                        zoomChart.Datasets.Add(newDs);
                    }
                }

                formZoom.KeyDown += (s, args) => { if (args.KeyCode == Keys.Escape) formZoom.Close(); };
                formZoom.Controls.Add(zoomChart);

                zoomChart.Update();
                formZoom.ShowDialog();
            }
        }
    }
}