using Microsoft.Data.SqlClient;
using PEPIDI.FormsSecundarios;
using PEPIDI.Models;
using PEPIDI.Organizers;
using PEPIDI.UCs.DGVS;
using PEPIDI.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PEPIDI.UCs.UcsSecundarios
{
    /// <summary>
    /// UC de detalhe de um pedido de EPI — o UC mais complexo da aplicação.
    /// Implementa IMessageFilter para intercetar WM_MOUSEWHEEL a nível global,
    /// com throttle de 20 mensagens por 100ms (proteção contra o MX Master 3S).
    ///
    /// _carregando=true durante CarregarPPacoteAsync e CarregarDPacoteAsync bloqueia
    /// os event handlers Linha_QuantidadeAlterada e Linha_EstadoAlterado para não
    /// gerar notas automáticas inválidas enquanto os dados estão a ser populados.
    ///
    /// O fluxo do botão de ação (btnAprovar) tem três ramos:
    ///   Pendente  → valida stock, reserva, muda para 'Aprovado'
    ///   Aprovado  → abre FormAssinatura, gera PDF, finaliza
    ///   Finalizado → abre o PDF já gerado
    ///
    /// AcaoConcluida é disparado quando uma ação termina com sucesso, para que o UC pai
    /// (Pedidos.cs) feche este detalhe e recarregue a grelha.
    /// </summary>
    public partial class PedidosDetalhes : UserControl, IMessageFilter
    {
        int ID;
        int IDGestor;
        string NomeGestor;
        string Estado;
        // _carregando=true bloqueia os handlers de quantidade e estado durante o carregamento das linhas
        private bool _carregando = false;
        string NomeFuncionario = "Funcionário Desconhecido";
        string NMEC = "0000";
        string FuncaoFuncionario = "-";
        EfeitoUI M = new EfeitoUI();
        /// <summary>Disparado após aprovar/finalizar — notifica Pedidos.cs para fechar e recarregar.</summary>
        public event EventHandler AcaoConcluida;

        // Throttle para o MX Master 3S: máximo 20 mensagens de scroll por 100ms
        private DateTime lastScrollTime = DateTime.Now;
        private int scrollCount = 0;
        private const int MaxScrollMessages = 20;

        public PedidosDetalhes(int _ID, int _IDGestor, string _Estado)
        {
            InitializeComponent();
            
            // Força a UC a usar a memória para desenhar antes de mostrar (GPU friendly)
            this.DoubleBuffered = true;

            foreach (Control control in this.Controls)
            {
                EnableDoubleBuffer(control);
            }

            ID = _ID;
            IDGestor = _IDGestor;
            Estado = _Estado;

            // MATAR O DUPLO SCROLL
            if (pnlScroll != null) pnlScroll.AutoScroll = false;
            if (pnlScroll2 != null) pnlScroll2.AutoScroll = false;

            flpLinhas.AutoScroll = true;
            flpDevolucoes.AutoScroll = true;

            // FORÇAR PADDING ZERO NOS PAINÉIS PARA ELES NÃO EMPURRAREM AS LINHAS
            flpLinhas.Padding = new Padding(0);
            flpLinhas.Margin = new Padding(0);
            flpDevolucoes.Padding = new Padding(0);
            flpDevolucoes.Margin = new Padding(0);

            flpLinhas.SizeChanged += (s, e) => AjustarLarguras(flpLinhas);
            flpDevolucoes.SizeChanged += (s, e) => AjustarLarguras(flpDevolucoes);

        }

        private static readonly System.Reflection.PropertyInfo _doubleBufferProp =
            typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private void EnableDoubleBuffer(Control c)
        {
            _doubleBufferProp?.SetValue(c, true, null);
            foreach (Control child in c.Controls) EnableDoubleBuffer(child);
        }

        // =========================================================================
        // METODO QUE ESMAGA QUALQUER BURACO LATERAL E FORÇA A LARGURA MÁXIMA
        // =========================================================================
        private void AjustarLarguras(FlowLayoutPanel flp)
        {
            if (flp.IsDisposed || !flp.IsHandleCreated) return;

            flp.SuspendLayout();
            foreach (Control c in flp.Controls)
            {
                // A elegância voltou! 5px de cada lado.
                c.Margin = new Padding(5, 2, 5, 2);
                // Desconta os 10px das laterais + 2px de segurança
                c.Width = flp.ClientSize.Width - 12;
            }
            flp.ResumeLayout();
        }

        private async void PedidosDetalhes_Load(object sender, EventArgs e)
        {
            Application.AddMessageFilter(this);
            var info = Details.GetInfoGestor(IDGestor);
            NomeGestor = info.Nome;
            await GereEstadoAsync(Estado);
            GestorTema.AplicarEstilos(this);

        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Application.RemoveMessageFilter(this);
            base.OnHandleDestroyed(e);
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x020A && pnlScroll != null && pnlScroll.IsHandleCreated)
            {
                Point pos = PointToClient(Cursor.Position);
                if (this.ClientRectangle.Contains(pos))
                {
                    TimeSpan elapsed = DateTime.Now - lastScrollTime;
                    if (elapsed.TotalMilliseconds > 100)
                    {
                        scrollCount = 0;
                        lastScrollTime = DateTime.Now;
                    }

                    if (scrollCount < MaxScrollMessages)
                    {
                        scrollCount++;
                        SendMessage(flpLinhas.Handle, m.Msg, m.WParam, m.LParam);
                    }
                    return true;
                }
            }
            return false;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private async Task GereEstadoAsync(string estado)
        {
            lblDGVQuant1.Text = (estado == "Aprovado") ? "Selecionar" : "Quantidade";

            if (estado == "Pendente")
            {
                btnAprovar.Text = "Aprovar";
                btnReprovar.Enabled = true;
                lblDGVQuantDisp.Text = "Quant. Disp";
            }
            else if (estado == "Aprovado")
            {
                btnAprovar.Text = "Finalizar";
                btnReprovar.Enabled = false;
                lblDGVQuantDisp.Text = "Quantidade";
            }
            else
            {
                btnAprovar.Text = "Comprovativo";
                btnReprovar.Enabled = false;
                // A MARRETA VISUAL: Protege o histórico!
                txtObs.ReadOnly = true;
                txtObs.BackColor = Color.WhiteSmoke; // Dá um aspeto de "bloqueado"

            }


            // Carrega primeiro as notas da BD para que os auto-comentários do PEPIDI
            // sejam acrescentados POR CIMA e não apagados depois
            VerComentario(ID);
            // NMEC é necessário para a query de alternativas de modelo em Pendente
            CarregarDadosFuncionario(ID);

            await CarregarPPacoteAsync(pnlConteudo, ID, estado, pnlScroll, flpLinhas);
            await CarregarDPacoteAsync(flpDevolucoes, pnlScroll2, ID, estado);
        }

        private void CarregarDadosFuncionario(int idPedido)
        {
            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_ObterFuncionarioPorPedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IDPedido", idPedido);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                this.NomeFuncionario = reader["Nome"]?.ToString() ?? "Sem Nome";
                                this.NMEC = reader["Nr"]?.ToString() ?? "0000";
                                this.FuncaoFuncionario = reader["Funcao"]?.ToString() ?? "-";
                            }
                            else
                            {
                                this.NomeFuncionario = "Erro ao ler Funcionário";
                                this.NMEC = "0000";
                                this.FuncaoFuncionario = "Erro";
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    this.NomeFuncionario = "Erro SQL";
                    this.NMEC = "0000";
                    this.FuncaoFuncionario = "Erro";
                }
            }
        }

        public async Task CarregarDPacoteAsync(FlowLayoutPanel flpLinhasDev, Panel pnlScrollDev, int idPedido, string estado)
        {
            flpLinhasDev.Visible = false;
            flpLinhasDev.SuspendLayout();
            flpLinhasDev.Controls.Clear();

            DataTable dt = new DataTable();

            await Task.Run(() =>
            {
                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_DetalhesDaDevolucao", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@IDPedido", idPedido);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            });

            foreach (DataRow row in dt.Rows)
            {
                // 1. Usar o nome exato que aparece no teu print do SSMS
                int idLinha = Convert.ToInt32(row["IDLinhaDevolucao"]);
                string modelo = row["ModeloComCor"].ToString().Trim();
                string tamanho = row["Tamanho"].ToString().Trim();
                string codigoEpi = row["Codigo"].ToString();

                // 2. Quantidade (O teu print diz 'QuantidadeDevolvida')
                int quantDevolvida = Convert.ToInt32(row["QuantidadeDevolvida"]);
                if (quantDevolvida == 0) quantDevolvida = 1;

                // 3. Estado gravado na BD (1=Novo, 2=Usado, 3=Gasto)
                int estadoIdDev = row.Table.Columns.Contains("EstadoID") && row["EstadoID"] != DBNull.Value
                    ? Convert.ToInt32(row["EstadoID"]) : 2;
                string cena = estadoIdDev == 1 ? "Novo" : (estadoIdDev == 3 ? "Gasto" : "Usado");

                PEPIDI.UCs.DGVS.LinhaDevolucao novaLinha = new PEPIDI.UCs.DGVS.LinhaDevolucao(idLinha, modelo, tamanho, cena, quantDevolvida);
                novaLinha.Tag = codigoEpi;

                // ATRELA O EVENTO AQUI!
                novaLinha.EstadoAlterado += LinhaDev_EstadoAlterado;

                novaLinha.AutoSize = false;
                novaLinha.Margin = new Padding(0, 2, 0, 2);
                novaLinha.Width = flpLinhasDev.ClientSize.Width;
                novaLinha.Height = 40;

                flpLinhasDev.Controls.Add(novaLinha);
            }

            flpLinhasDev.ResumeLayout(true);
            flpLinhasDev.Visible = true;

            // Força o ajuste imediatamente após desenhar
            AjustarLarguras(flpLinhasDev);
        }

        private void LinhaDev_EstadoAlterado(object sender, EventArgs e)
        {
            if (this.IsDisposed || this.Disposing) return;

            if (sender is PEPIDI.UCs.DGVS.LinhaDevolucao linhaDev)
            {
                // O texto de busca exato que vai aparecer na nota!
                string logBusca = $"a devolução de '{linhaDev.DescricaoArtigo} ({linhaDev.TamanhoSelecionado})'";
                List<string> linhasLog = txtObs.Lines.ToList();

                // Limpa notas antigas DESTA devolução específica para não fazer spam (agora já encontra!)
                linhasLog.RemoveAll(l => l.TrimStart().StartsWith("[") && l.Contains(logBusca) && l.Contains("Classificou"));

                string estadoAtual = linhaDev.EstadoSelecionado;

                // Se o estado for o normal (Usado), não chateia. Mas se forcar a "Novo" ou mandar para o Lixo ("Gasto"), escreve!
                if (!estadoAtual.Equals("Usado", StringComparison.OrdinalIgnoreCase))
                {
                    string novaNota = $"[{NomeGestor}]: Classificou {logBusca} como {estadoAtual.ToUpper()}.";
                    linhasLog.Add(novaNota);
                }

                txtObs.Text = string.Join(Environment.NewLine, linhasLog.Where(l => !string.IsNullOrWhiteSpace(l)));
            }
        }

        public async Task CarregarPPacoteAsync(Panel pnlConteudo, int idPedido, string estado, Panel pnlScroll, FlowLayoutPanel flpLinhas)
        {
            flpLinhas.Visible = false;
            flpLinhas.SuspendLayout();
            flpLinhas.Controls.Clear();

            DataTable dt = new DataTable();

            await Task.Run(() =>
            {
                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_DetalhesDoPedido", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@IDPedido", idPedido);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            });

            foreach (DataRow row in dt.Rows)
            {
                string codigoEpi = row["Codigo"].ToString().Trim(); // Cortei o mal pela raiz!
                string modelo = row["ModeloComCor"].ToString().Trim();
                string tamanho = row["Tamanho"].ToString().Trim();

                int stockNovo = Convert.ToInt32(row["QtdStockNovo"]);
                int stockUsado = Convert.ToInt32(row["QtdStockUsado"]);
                int quantPedida = Convert.ToInt32(row["QuantidadePedida"]);
                int estadoGravado = row["EstadoID"] != DBNull.Value ? Convert.ToInt32(row["EstadoID"]) : 1;

                // Define a quantidade disponível BASEADA NO ESTADO GRAVADO (1=Novo, 2=Usado)
                int quantDispParaControlo = (estadoGravado == 2) ? stockUsado : stockNovo;

                // REGRA 2: Se está aprovado e a quantidade ficou a 0 (porque faltou stock), esconde!
                if ((estado == "Aprovado" || estado == "Finalizado") && quantPedida == 0)
                {
                    continue;
                }

                if (row.Table.Columns.Contains("EstadoID") && row["EstadoID"] != DBNull.Value)
                {
                    estadoGravado = Convert.ToInt32(row["EstadoID"]);
                }

                LinhaItem novaLinha = new LinhaItem(modelo, tamanho, quantDispParaControlo, quantPedida, estado);
                novaLinha.Tag = codigoEpi;
                novaLinha.IDEPI = Convert.ToInt32(row["IDLinhaPedido"]);
                novaLinha.QuantidadeOriginal = quantPedida;

                novaLinha.QuantidadeAlterada += Linha_QuantidadeAlterada;
                novaLinha.EstadoAlterado += Linha_EstadoAlterado;
                novaLinha.ModeloSubstituido += Linha_ModeloSubstituido;

                novaLinha.AutoSize = false;
                novaLinha.Margin = new Padding(0, 2, 0, 2);
                novaLinha.Width = flpLinhas.ClientSize.Width - 10;
                novaLinha.Height = 40;

                // Bloquear eventos durante o setup inicial para não gerar comentários falsos
                _carregando = true;

                if (estado == "Aprovado" || estado == "Finalizado")
                {
                    novaLinha.Enabled = true;
                    novaLinha.Selecionado = (quantPedida > 0);

                    if (estadoGravado == 2)
                    {
                        novaLinha.ConfigurarSugestaoStock(0, quantPedida);
                        novaLinha.EstadoSelecionado = "Usado";
                    }
                    else
                    {
                        novaLinha.ConfigurarSugestaoStock(quantPedida, 0);
                        novaLinha.EstadoSelecionado = "Novo";
                    }
                }
                else
                {
                    novaLinha.ConfigurarSugestaoStock(stockNovo, stockUsado);
                }

                // Desbloquear antes de QuantidadeAlterada para o PEPIDI poder comentar stock=0
                _carregando = false;
                Linha_QuantidadeAlterada(novaLinha, EventArgs.Empty);

                // Alternativas de modelo: só relevante em Pendente
                if (estado == "Pendente")
                {
                    var alts = ObterAlternativasModelo(codigoEpi, tamanho);
                    if (alts.Count > 1)
                        novaLinha.ConfigurarAlternativas(alts);
                }

                flpLinhas.Controls.Add(novaLinha);
            }

            flpLinhas.ResumeLayout(true);
            flpLinhas.Visible = true;
            AjustarLarguras(flpLinhas);
        }

        /// <summary>
        /// Consulta a BD para encontrar todos os modelos da mesma família, tamanho e função
        /// disponíveis como alternativa ao artigo atual. Usado em Pendente para popular a
        /// combo de troca de modelo em ConfigurarAlternativas de LinhaItem.
        /// </summary>
        private List<(string Codigo, string ModeloNome)> ObterAlternativasModelo(string codigoAtual, string tamanho)
        {
            var result = new List<(string Codigo, string ModeloNome)>();
            if (string.IsNullOrEmpty(NMEC) || NMEC == "0000") return result;

            try
            {
                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                {
                    conn.Open();
                    const string sql = @"
                        SELECT DISTINCT E.Codigo,
                               LTRIM(RTRIM(E.Modelo + ISNULL(' ' + NULLIF(LTRIM(RTRIM(E.CorID)), ''), ''))) AS ModeloComCor
                        FROM EPI E
                        INNER JOIN AcessoFuncoes AF ON E.AcessoID = AF.AcessoID
                        WHERE E.Familia  = (SELECT Familia FROM EPI WHERE Codigo = @cod)
                          AND E.Tamanho  = @tamanho
                          AND AF.FuncaoID = (SELECT FuncaoID FROM Funcionarios WHERE Nr = @nrFunc)
                          AND E.Ativo    = 1
                        ORDER BY E.Codigo";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@cod", codigoAtual);
                        cmd.Parameters.AddWithValue("@tamanho", tamanho);
                        cmd.Parameters.AddWithValue("@nrFunc", NMEC);
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                                result.Add((dr["Codigo"].ToString().Trim(), dr["ModeloComCor"].ToString().Trim()));
                        }
                    }
                }
            }
            catch { /* silently return — não bloqueia o carregamento do pedido */ }

            return result;
        }

        private void Linha_ModeloSubstituido(object sender, LinhaItem.ModeloSubstituidoEventArgs e)
        {
            if (this.IsDisposed || this.Disposing) return;
            if (!(sender is LinhaItem linha)) return;

            // Remove nota anterior deste artigo (identificada pelo modelo original + tamanho)
            List<string> linhasLog = txtObs.Lines.ToList();
            linhasLog.RemoveAll(l =>
                l.TrimStart().StartsWith("[") &&
                l.Contains("Modelo substituído") &&
                l.Contains($"'{e.ModeloOriginal} ({linha.T})'"));

            if (linha.CodigoEpiSubstituto != null)
            {
                string novaNota = $"[{NomeGestor}]: Modelo substituído de '{e.ModeloOriginal} ({linha.T})' para '{e.ModeloNovo}'.";
                linhasLog.Add(novaNota);
            }

            txtObs.Text = string.Join(Environment.NewLine, linhasLog.Where(l => !string.IsNullOrWhiteSpace(l)));
        }

        private void Linha_QuantidadeAlterada(object sender, EventArgs e)
        {
            if (this.Estado != "Pendente")
            {
                return;
            }

            if (this.IsDisposed || this.Disposing) return;

            if (sender is LinhaItem linha)
            {
                if (linha.IsDisposed || linha.Disposing) return;

                string logBusca = $"{linha.M} ({linha.T})";
                List<string> linhasLog = txtObs.Lines.ToList();

                // O SEGREDO ESTÁ AQUI: Só apaga se contiver o nome da peça E a palavra "quantidade"
                linhasLog.RemoveAll(l => l.TrimStart().StartsWith("[") && l.Contains(logBusca) && l.Contains("quantidade"));

                if (linha.QuantidadeSelecionada != linha.QuantidadeOriginal)
                {
                    string autor = NomeGestor;
                    string notaExtra = "";

                    if (linha.QuantidadeOriginal > linha.QD && linha.QuantidadeSelecionada == linha.QD)
                    {
                        autor = "PEPIDI";
                        notaExtra = " (falta de stock)";
                    }

                    string novaNota = $"[{autor}]: Alterou a quantidade de '{logBusca}' de {linha.QuantidadeOriginal} para {linha.QuantidadeSelecionada}{notaExtra}.";
                    linhasLog.Add(novaNota);
                }

                txtObs.Text = string.Join(Environment.NewLine, linhasLog.Where(l => !string.IsNullOrWhiteSpace(l)));
            }
        }

        private void Linha_EstadoAlterado(object sender, EventArgs e)
        {
            if (_carregando) return;
            if (this.IsDisposed || this.Disposing) return;
            if (!(sender is LinhaItem linha)) return;
            if (linha.IsDisposed || linha.Disposing) return;

            string logBusca = $"estado de '{linha.M} ({linha.T})'";
            List<string> linhasLog = txtObs.Lines.ToList();
            linhasLog.RemoveAll(l => l.TrimStart().StartsWith("[") && l.Contains(logBusca));
            string estadoAtual = linha.EstadoSelecionado;

            if (this.Estado.Equals("Aprovado", StringComparison.OrdinalIgnoreCase))
            {
                // Gestor alterou o tipo de stock a entregar durante o Finalizar
                string novaNota = $"[{NomeGestor}]: Alterou o {logBusca} para {estadoAtual.ToUpper()} na entrega.";
                linhasLog.Add(novaNota);
            }
            else
            {
                // Fase Pendente: lógica original
                if (linha.EstadoForcadoPeloSistema && estadoAtual == "Usado")
                {
                    string novaNota = $"[PEPIDI]: Definiu o {logBusca} como USADO{linha.ObservacoesAutomaticas}.";
                    linhasLog.Add(novaNota);
                }
                else if (!linha.EstadoForcadoPeloSistema && estadoAtual == "Usado")
                {
                    string novaNota = $"[{NomeGestor}]: Definiu o {logBusca} como USADO.";
                    linhasLog.Add(novaNota);
                }
            }

            txtObs.Text = string.Join(Environment.NewLine, linhasLog.Where(l => !string.IsNullOrWhiteSpace(l)));
        }

        private void VerComentario(int idPedido)
        {
            txtObs.Text = string.Empty;
            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Notas FROM PedidoRegistos WHERE ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", idPedido);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) txtObs.Text = reader["Notas"].ToString();
                }
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            // O UC avisa o Form Principal que quer fechar. O Form trata do resto!
            AcaoConcluida?.Invoke(this, EventArgs.Empty);
        }

        private void btnAprovar_Click(object sender, EventArgs e)
        {
            string estadoAtual = this.Estado.Trim();
            string notasFinaisParaGravar = txtObs.Text.Trim();

            if (estadoAtual.Equals("Pendente", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                    {
                        conn.Open();
                        GetConn.SetContext(conn);
                        SqlTransaction trans = conn.BeginTransaction();
                        try
                        {
                            foreach (Control c in flpLinhas.Controls)
                            {
                                if (c is LinhaItem linha)
                                {
                                    // Se não houver stock, o sistema agora corta sozinho e não bloqueia!
                                    int qtdParaAprovar = Math.Min(linha.QuantidadeSelecionada, linha.QD);
                                    if (qtdParaAprovar < 0) qtdParaAprovar = 0;

                                    // Usa o substituto se o gestor trocou de modelo, senão o original
                                    string codEpi = (linha.CodigoEpiSubstituto ?? linha.Tag.ToString()).Trim();
                                    int idEstado = (linha.EstadoSelecionado == "Novo") ? 1 : 2;

                                    // Buscar gaveta do stock baseada no Estado Selecionado
                                    int idStockReal = 0;
                                    using (SqlCommand cmdS = new SqlCommand("SELECT TOP 1 ID FROM Stock WHERE Codigo = @c AND Estado = @e", conn, trans))
                                    {
                                        cmdS.Parameters.AddWithValue("@c", codEpi);
                                        cmdS.Parameters.AddWithValue("@e", idEstado);
                                        object r = cmdS.ExecuteScalar();
                                        if (r != null) idStockReal = Convert.ToInt32(r);
                                    }

                                    // Se a gaveta não existe ou qtd é 0, liberta o stock
                                    if (idStockReal == 0 || qtdParaAprovar == 0)
                                    {
                                        string sqlZ = "UPDATE PedidoPacote SET Quantidade = 0, IDStock = NULL WHERE ID = @id";
                                        using (SqlCommand cmdZ = new SqlCommand(sqlZ, conn, trans))
                                        {
                                            cmdZ.Parameters.AddWithValue("@id", linha.IDEPI);
                                            cmdZ.ExecuteNonQuery();
                                        }
                                    }
                                    else
                                    {
                                        // Atualizar Pedido e Abater Stock
                                        string sqlUp = "UPDATE PedidoPacote SET Quantidade = @q, IDStock = @is WHERE ID = @id";
                                        using (SqlCommand cmdUp = new SqlCommand(sqlUp, conn, trans))
                                        {
                                            cmdUp.Parameters.AddWithValue("@q", qtdParaAprovar);
                                            cmdUp.Parameters.AddWithValue("@is", idStockReal);
                                            cmdUp.Parameters.AddWithValue("@id", linha.IDEPI);
                                            cmdUp.ExecuteNonQuery();
                                        }
                                        using (SqlCommand cmdA = new SqlCommand("UPDATE Stock SET Quantidade = Quantidade - @q WHERE ID = @is", conn, trans))
                                        {
                                            cmdA.Parameters.AddWithValue("@q", qtdParaAprovar);
                                            cmdA.Parameters.AddWithValue("@is", idStockReal);
                                            cmdA.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            // Guardar estado selecionado para cada devolução
                            foreach (Control cd in flpDevolucoes.Controls)
                            {
                                if (cd is LinhaDevolucao d)
                                {
                                    int est = d.EstadoSelecionado == "Novo" ? 1 : (d.EstadoSelecionado == "Gasto" ? 3 : 2);
                                    string codEpi = d.Tag?.ToString().Trim() ?? "";
                                    int idSDev = 0;
                                    using (SqlCommand cmdSD = new SqlCommand("SELECT TOP 1 ID FROM Stock WHERE Codigo = @c AND Estado = @e", conn, trans))
                                    {
                                        cmdSD.Parameters.AddWithValue("@c", codEpi);
                                        cmdSD.Parameters.AddWithValue("@e", est);
                                        var res = cmdSD.ExecuteScalar();
                                        if (res != null) idSDev = Convert.ToInt32(res);
                                    }
                                    if (idSDev > 0)
                                    {
                                        using (SqlCommand cmdUpDev = new SqlCommand("UPDATE PedidoPacote SET IDStock = @ids WHERE ID = @id AND TipoMovimento = 'D'", conn, trans))
                                        {
                                            cmdUpDev.Parameters.AddWithValue("@ids", idSDev);
                                            cmdUpDev.Parameters.AddWithValue("@id", d.IDEPI);
                                            cmdUpDev.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            string sqlFinal = "UPDATE PedidoRegistos SET Estado = 'Aprovado', AprovadoPor = @g, AlteradoPor = @g, Notas = @n, AlteracaoData = GETDATE() WHERE ID = @id";
                            using (SqlCommand cmdF = new SqlCommand(sqlFinal, conn, trans))
                            {
                                cmdF.Parameters.AddWithValue("@id", this.ID);
                                cmdF.Parameters.AddWithValue("@g", IDGestor);
                                cmdF.Parameters.AddWithValue("@n", notasFinaisParaGravar);
                                cmdF.ExecuteNonQuery();
                            }
                            trans.Commit();
                            M.AbrirMensagem("Aprovado! Stock reservado.", "Sucesso");
                            AcaoConcluida?.Invoke(this, EventArgs.Empty);
                        }
                        catch { trans.Rollback(); throw; }
                    }
                }
                catch (Exception ex) { M.AbrirMensagem("Erro: " + ex.Message, "Erro"); }
            }
            else if (estadoAtual.Equals("Aprovado", StringComparison.OrdinalIgnoreCase))
            {
                CarregarDadosFuncionario(this.ID);
                // 1. Mudamos de "int ID" para "string Codigo"
                var listaReceber = new List<(string Codigo, string Artigo, string Tamanho, int Qtd)>();
                var listaDevolver = new List<(string Codigo, string Artigo, string Tamanho, int Qtd)>();
                var todosOsItens = new List<(int IDLinha, int QtdFinal, bool ComVisto)>();

                foreach (Control c in flpLinhas.Controls)
                {
                    if (c is LinhaItem l)
                    {
                        int qtdFinal = l.Selecionado ? l.QuantidadeSelecionada : 0;
                        todosOsItens.Add((l.IDEPI, qtdFinal, l.Selecionado));

                        if (l.Selecionado && l.QuantidadeSelecionada > 0)
                        {
                            // Enviamos o l.Tag (o código real) em vez do l.IDEPI
                            listaReceber.Add((l.Tag.ToString().Trim(), $"{l.DescricaoArtigo} [{l.EstadoSelecionado.ToUpper()}]", l.TamanhoSelecionado, l.QuantidadeSelecionada));
                        }
                    }
                }

                foreach (Control c in flpDevolucoes.Controls)
                {
                    if (c is LinhaDevolucao d && d.QuantidadeDevolvida > 0)
                    {
                        // Enviamos o d.Tag (o código real) em vez do d.IDEPI
                        listaDevolver.Add((d.Tag.ToString().Trim(), $"{d.DescricaoArtigo} [{d.EstadoSelecionado.ToUpper()}]", d.TamanhoSelecionado, d.QuantidadeDevolvida));
                    }
                }

                // Overlay (sombra) para escurecer o fundo enquanto o FormAssinatura está aberto
                Form paiAssinatura = this.FindForm();
                using (Form overlay = new Form())
                {
                    overlay.StartPosition = FormStartPosition.Manual;
                    overlay.FormBorderStyle = FormBorderStyle.None;
                    overlay.Opacity = 0.50d;
                    overlay.BackColor = Color.Black;
                    overlay.ShowInTaskbar = false;

                    if (paiAssinatura != null)
                    {
                        overlay.Location = paiAssinatura.Location;
                        overlay.Size = paiAssinatura.Size;
                        overlay.Show(paiAssinatura);
                    }
                    else
                    {
                        overlay.WindowState = FormWindowState.Maximized;
                        overlay.Show();
                    }

                using (var frm = new FormAssinatura(NomeFuncionario))
                {
                    foreach (var i in listaReceber) frm.AdicionarItemReceber(i.Artigo, i.Tamanho, i.Qtd);
                    foreach (var i in listaDevolver) frm.AdicionarItemDevolver(i.Artigo, i.Tamanho, i.Qtd);
                    if (frm.ShowDialog(overlay) != DialogResult.OK) return;

                    try
                    {
                        string path = PDFGenerator.GerarComprovativo(this.ID, NomeFuncionario, NMEC, FuncaoFuncionario, NomeGestor, listaReceber, listaDevolver, frm.AssinaturaFinal);
                        string notas = txtObs.Text.Trim() + Environment.NewLine + $"[Finalizado por {NomeGestor} com assinatura de {NomeFuncionario}]";

                        using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                        {
                            conn.Open();
                            GetConn.SetContext(conn);
                            using (SqlTransaction trans = conn.BeginTransaction())
                            {
                                try
                                {
                                    // 1. Fechar Pedido (Parametrizado para evitar erro de sintaxe nas notas)
                                    string sqlFechar = @"UPDATE PedidoRegistos
                                       SET Estado = 'Finalizado', CaminhoPDF = @path, EntregadoPor = @gestor,
                                           AlteradoPor = @gestor, AlteracaoData = GETDATE(), Notas = @notas
                                       WHERE ID = @id";
                                    using (SqlCommand cmdF = new SqlCommand(sqlFechar, conn, trans))
                                    {
                                        cmdF.Parameters.AddWithValue("@path", path);
                                        cmdF.Parameters.AddWithValue("@gestor", IDGestor);
                                        cmdF.Parameters.AddWithValue("@notas", notas);
                                        cmdF.Parameters.AddWithValue("@id", this.ID);
                                        cmdF.ExecuteNonQuery();
                                    }

                                    // 2. Devolver stock se o Gestor desmarcou a checkbox
                                    foreach (var item in todosOsItens)
                                    {
                                        if (!item.ComVisto)
                                        {
                                            using (SqlCommand cmdB = new SqlCommand("SELECT IDStock, Quantidade FROM PedidoPacote WHERE ID = @id", conn, trans))
                                            {
                                                cmdB.Parameters.AddWithValue("@id", item.IDLinha);
                                                using (SqlDataReader dr = cmdB.ExecuteReader())
                                                {
                                                    if (dr.Read() && dr["IDStock"] != DBNull.Value)
                                                    {
                                                        int idS = Convert.ToInt32(dr["IDStock"]);
                                                        int q = Convert.ToInt32(dr["Quantidade"]);

                                                        using (SqlCommand cmdUpStock = new SqlCommand("UPDATE Stock SET Quantidade = Quantidade + @q WHERE ID = @ids", conn, trans))
                                                        {
                                                            cmdUpStock.Parameters.AddWithValue("@q", q);
                                                            cmdUpStock.Parameters.AddWithValue("@ids", idS);
                                                            cmdUpStock.ExecuteNonQuery();
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // Atualiza a quantidade final no pacote
                                        using (SqlCommand cmdUpPP = new SqlCommand("UPDATE PedidoPacote SET Quantidade = @q WHERE ID = @id", conn, trans))
                                        {
                                            cmdUpPP.Parameters.AddWithValue("@q", item.QtdFinal);
                                            cmdUpPP.Parameters.AddWithValue("@id", item.IDLinha);
                                            cmdUpPP.ExecuteNonQuery();
                                        }
                                    }

                                    // 3. Tratar Retomas
                                    foreach (Control c in flpDevolucoes.Controls)
                                    {
                                        if (c is LinhaDevolucao d && d.QuantidadeDevolvida > 0)
                                        {
                                            int est = (d.EstadoSelecionado == "Novo") ? 1 : (d.EstadoSelecionado == "Usado" ? 2 : 3);
                                            int idS = 0;
                                            string codEpi = d.Tag.ToString().Trim();

                                            using (SqlCommand cmdS = new SqlCommand("SELECT ID FROM Stock WHERE Codigo = @c AND Estado = @e", conn, trans))
                                            {
                                                cmdS.Parameters.AddWithValue("@c", codEpi);
                                                cmdS.Parameters.AddWithValue("@e", est);
                                                var res = cmdS.ExecuteScalar();

                                                if (res == null)
                                                {
                                                    using (SqlCommand cmdI = new SqlCommand("INSERT INTO Stock (Codigo, Estado, Quantidade) OUTPUT INSERTED.ID VALUES (@c, @e, 0)", conn, trans))
                                                    {
                                                        cmdI.Parameters.AddWithValue("@c", codEpi);
                                                        cmdI.Parameters.AddWithValue("@e", est);
                                                        idS = Convert.ToInt32(cmdI.ExecuteScalar());
                                                    }
                                                }
                                                else idS = Convert.ToInt32(res);
                                            }

                                            // Somar à prateleira
                                            using (SqlCommand cmdSum = new SqlCommand("UPDATE Stock SET Quantidade = Quantidade + @q WHERE ID = @ids", conn, trans))
                                            {
                                                cmdSum.Parameters.AddWithValue("@q", d.QuantidadeDevolvida);
                                                cmdSum.Parameters.AddWithValue("@ids", idS);
                                                cmdSum.ExecuteNonQuery();
                                            }

                                            // Registar no pacote de devolução
                                            using (SqlCommand cmdUpRP = new SqlCommand("UPDATE PedidoPacote SET IDStock = @ids WHERE ID = @id AND TipoMovimento = 'D'", conn, trans))
                                            {
                                                cmdUpRP.Parameters.AddWithValue("@ids", idS);
                                                cmdUpRP.Parameters.AddWithValue("@id", d.IDEPI);
                                                cmdUpRP.ExecuteNonQuery();
                                            }
                                        }
                                    }

                                    trans.Commit();
                                    System.Diagnostics.Process.Start("explorer.exe", path);
                                    AcaoConcluida?.Invoke(this, EventArgs.Empty);
                                }
                                catch { trans.Rollback(); throw; }
                            }
                        }
                    }
                    catch (Exception ex) { M.AbrirMensagem("Erro ao finalizar: " + ex.Message, "Erro"); }
                }
                } // fim do using (Form overlay)
            }
            else { AbrirComprovativoExistente(); }
        }

        private void AbrirComprovativoExistente()
        {
            try
            {
                string caminhoPDF = "";

                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT CaminhoPDF FROM PedidoRegistos WHERE ID = @ID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", this.ID);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            caminhoPDF = result.ToString();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(caminhoPDF) && System.IO.File.Exists(caminhoPDF))
                {
                    System.Diagnostics.Process.Start("explorer.exe", caminhoPDF);
                }
                else
                {
                    M.AbrirMensagem("O ficheiro do comprovativo não foi encontrado ou não existe.", "Erro de Ficheiro");
                }
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Erro ao abrir comprovativo: " + ex.Message, "Erro");
            }
        }

        private void btnReprovar_Click(object sender, EventArgs e)
        {
            string justificativaFinal = "";
            List<string> linhasAtuais = txtObs.Lines.ToList();

            // O NOVO FILTRO MÁGICO: É considerado "Manual" se não for vazio E não começar por "["
            var linhasManuais = linhasAtuais.Where(l =>
                !string.IsNullOrWhiteSpace(l) &&
                !l.TrimStart().StartsWith("[")
            ).ToList();

            if (linhasManuais.Count > 0)
            {
                // Se o gestor escreveu texto livre no meio da confusão, usamos isso!
                string textoPuro = string.Join(" ", linhasManuais).Trim();
                justificativaFinal = $"[{NomeGestor}]: MOTIVO REPROVAÇÃO - {textoPuro}";
            }
            else
            {
                // Se só há notas do sistema, OBRIGA a abrir o popup!
                using (Form overlay = new Form())
                {
                    overlay.StartPosition = FormStartPosition.Manual;
                    overlay.FormBorderStyle = FormBorderStyle.None;
                    overlay.Opacity = 0.50d;
                    overlay.BackColor = Color.Black;
                    overlay.ShowInTaskbar = false;
                    Form formPrincipal = this.FindForm();
                    overlay.Location = formPrincipal.Location;
                    overlay.Size = formPrincipal.Size;
                    overlay.Show(formPrincipal);

                    using (FormMotivo frm = new FormMotivo())
                    {
                        if (frm.ShowDialog(overlay) == DialogResult.OK)
                        {
                            justificativaFinal = $"[{NomeGestor}]: MOTIVO REPROVAÇÃO - {frm.Motivo}";
                        }
                        else
                        {
                            overlay.Close();
                            return; // Abortar reprovação se ele fechar a janela no X
                        }
                    }
                    overlay.Close();
                }
            }

            ExecutarReprovacao(justificativaFinal);
        }

        private void ExecutarReprovacao(string notaLimpa)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
                {
                    conn.Open();
                    GetConn.SetContext(conn);

                    // O SQL agora preenche a auditoria toda: Quem reprovou (AprovadoPor / AlteradoPor) e Quando (AlteracaoData)
                    string sql = @"UPDATE PedidoRegistos
                           SET Estado = 'Reprovado',
                               Notas = @Notas,
                               AprovadoPor = @Gestor,
                               AlteracaoData = GETDATE(),
                               AlteradoPor = @Gestor
                           WHERE ID = @ID";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ID", this.ID);
                    cmd.Parameters.AddWithValue("@Notas", notaLimpa);
                    cmd.Parameters.AddWithValue("@Gestor", this.IDGestor); // O teu ID que vem do Form Principal

                    cmd.ExecuteNonQuery();
                    M.AbrirMensagem("Pedido reprovado com sucesso.", "Sucesso");

                    AcaoConcluida?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Erro ao reprovar: " + ex.Message, "Erro");
            }
        }
    }
}