using PEPIDI.FormsSecundarios;
using PEPIDI.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace PEPIDI
{
    public partial class FormPedidos : Form
    {
        private int _nrFunc;
        private Form _loginForm;
        private MostraRoupa _mr;
        private string _notasPedido = string.Empty;
        EfeitoUI M = new EfeitoUI();
        private enum ModoPedidos
        {
            Pedido,
            Devolucao
        }
        private ModoPedidos _modoAtual = ModoPedidos.Pedido;

        // ----------------- CLASSES DE SESSÃO -----------------
        private class LinhaSessao
        {
            public string Modelo { get; set; }
            public string Tamanho { get; set; }
            public string Cor { get; set; }
            public int Quantidade { get; set; }
        }

        private class SessaoPedido
        {
            public int? IdPedidoReg { get; set; }                                  // ID em PedidoRegistos
            public List<LinhaSessao> Pedido { get; } = new List<LinhaSessao>();     // Itens para 'P'
            public List<LinhaSessao> Devolucao { get; } = new List<LinhaSessao>();  // Itens para 'D'

        }

        private SessaoPedido _sessao = new SessaoPedido();

        public FormPedidos(int nrFunc, Form loginForm)
        {
            InitializeComponent();
            _nrFunc = nrFunc;
            _loginForm = loginForm;
        }

        private void FormPedidos_Load(object sender, EventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            if (_nrFunc == 0)
                return;

            try
            {
                if (_mr == null)
                    _mr = new MostraRoupa();

                if (_sessao == null)
                    _sessao = new SessaoPedido();

                AtualizarNavButtons();
                CarregarLinhas();

                var info = Details.GetInfoGestor(_nrFunc);
                lblNome.Text = info.Nome + " · " + info.Funcao;
            }
            catch (Exception ex)
            {
                M.AbrirMensagem(
                    "Erro em Pedidos_Load / CarregarLinhas:\n\n" + ex,
                    "Erro");

            }
        }

        private void CarregarLinhas()
        {
            if (_mr == null) return;

            var itensRaw = (_modoAtual == ModoPedidos.Pedido)
                ? _mr.ObterRoupaPorFuncionarioNovo(_nrFunc)
                : _mr.ObterRoupaUsadaPorFuncionarioNovo(_nrFunc);

            if (flpLinhas == null) return;
            flpLinhas.SuspendLayout();
            flpLinhas.Controls.Clear();

            // Agrupamos por Família
            var grupos = itensRaw.GroupBy(x => x.Familia).ToList();

            foreach (var grupo in grupos)
            {
                // 1. Pergunta ao MostraRoupa qual foi o último modelo desta família
                string ultimoModelo = _mr.ObterUltimoModeloConsumidoPorFamilia(_nrFunc, grupo.Key);

                // 2. Tenta encontrar esse modelo na lista
                var itemPrincipal = grupo.FirstOrDefault(x => x.Modelo == ultimoModelo) ?? grupo.First();

                var listaModelos = grupo.Select(x => x.Modelo).Distinct().ToList();
                bool variosModelos = listaModelos.Count > 1;

                var linha = new PEPIDI.Organizers.LinhaPedido
                {
                    Size = new Size(flpLinhas.Width - 25, 100),
                    Dock = DockStyle.Top,
                    Tag = itemPrincipal
                };

                // SUBSCREVER O EVENTO QUE ACABÁMOS DE CRIAR
                linha.TamanhoAlteradoPeloUtilizador += Linha_TamanhoAlteradoPeloUtilizador;

                // 3. Configura o Layout
                linha.ConfigurarLayout(variosModelos, itemPrincipal.Familia, itemPrincipal.NomeVista ?? itemPrincipal.Familia, itemPrincipal.Modelo);

                if (variosModelos)
                {
                    linha.ComboModelo.Items.Clear();
                    foreach (var mod in listaModelos) linha.ComboModelo.Items.Add(mod);

                    // FORÇA a seleção do modelo que o histórico indicou
                    linha.ComboModelo.SelectedItem = itemPrincipal.Modelo;
                }

                // 4. Carrega os tamanhos baseados no modelo histórico encontrado
                var tamanhos = itemPrincipal.TamanhosDisponiveis ?? new List<string>();
                if (!string.IsNullOrEmpty(itemPrincipal.TamanhoAtual) && !tamanhos.Contains(itemPrincipal.TamanhoAtual))
                    tamanhos.Add(itemPrincipal.TamanhoAtual);

                linha.ComboTamanho.Items.Clear();
                foreach (var t in tamanhos.OrderBy(x => x)) linha.ComboTamanho.Items.Add(t);

                linha.DefinirTamanhoSemConfirmar(itemPrincipal.TamanhoAtual);
                flpLinhas.Controls.Add(linha);
            }
            flpLinhas.ResumeLayout(true);
        }

        private void Linha_TamanhoAlteradoPeloUtilizador(object sender, PEPIDI.Organizers.LinhaPedido.TamanhoAlteradoEventArgs e)
        {
            if (string.IsNullOrEmpty(e.CodigoEpi)) return;

            try
            {
                _mr.AtualizarTamanhoPadraoFuncionario(_nrFunc, e.CodigoEpi, e.NovoTamanho);
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Erro ao atualizar o tamanho predefinido do funcionário:\n\n" + ex.Message, "Erro");
            }
        }

        private void AtualizarNavButtons()
        {
            Nav1.Checked = _modoAtual == ModoPedidos.Pedido;
            Nav2.Checked = _modoAtual == ModoPedidos.Devolucao;
        }

        private void NavButtons_Click(object sender, EventArgs e)
        {
            var btnClicado = sender as Guna.UI2.WinForms.Guna2Button;

            if (btnClicado == null || (btnClicado != Nav1 && btnClicado != Nav2))
            {
                if (sender != Nav1 && sender != Nav2) Close();
                return;
            }

            ModoPedidos modoAlvo = (btnClicado == Nav1) ? ModoPedidos.Pedido : ModoPedidos.Devolucao;

            if (modoAlvo == _modoAtual) return;

            if (modoAlvo == ModoPedidos.Devolucao)
            {
                var itensUsados = _mr.ObterRoupaUsadaPorFuncionarioNovo(_nrFunc);
                if (itensUsados == null || itensUsados.Count == 0)
                {
                    M.AbrirMensagem("Nenhum produto usado encontrado para o funcionário.", "Aviso");
                    Nav1.Checked = true;
                    Nav2.Checked = false;
                    return;
                }
            }

            GuardarEstadoModoAtual();
            _modoAtual = modoAlvo;

            AtualizarNavButtons();
            CarregarLinhas();
        }

        private void GuardarEstadoModoAtual()
        {
            if (flpLinhas == null || _sessao == null)
                return;

            var lista = (_modoAtual == ModoPedidos.Pedido)
                ? _sessao.Pedido
                : _sessao.Devolucao;

            lista.Clear();

            foreach (var linha in flpLinhas.Controls.OfType<PEPIDI.Organizers.LinhaPedido>())
            {
                var info = linha.Tag as LinhaPedidoInfo;
                if (info == null) continue;

                string modeloFinal = linha.ComboModelo.Visible
                                     ? linha.ComboModelo.SelectedItem?.ToString()
                                     : info.Modelo;

                string tamanho = linha.ComboTamanho?.SelectedItem?.ToString();
                int qtd = linha.Quantidade;

                if (string.IsNullOrWhiteSpace(tamanho) || string.IsNullOrWhiteSpace(modeloFinal) || qtd <= 0)
                    continue;

                lista.Add(new LinhaSessao
                {
                    Modelo = modeloFinal,
                    Tamanho = tamanho,
                    Cor = info.Cor,
                    Quantidade = qtd
                });
            }
        }

        private void btnSubmeter_Click(object sender, EventArgs e)
        {
            try
            {
                if (_mr == null) _mr = new MostraRoupa();
                if (_sessao == null) _sessao = new SessaoPedido();

                GuardarEstadoModoAtual();

                var itensPedido = _sessao.Pedido.ToList();
                var itensDevolucao = _sessao.Devolucao.ToList();

                if (!itensPedido.Any() && !itensDevolucao.Any())
                {
                    M.AbrirMensagem("Nenhum item com quantidade > 0 para submeter.", "Informação");
                    return;
                }

                // ----- 1. PREPARAR PEDIDOS ('P') -----
                var listaPedidoInsert = new List<(string codigoEpi, int qtd)>();
                foreach (var it in itensPedido)
                {
                    // A base agora é a string do Código
                    string codigoEpi = _mr.ResolveCodigoEpi(it.Modelo, it.Tamanho, it.Cor);

                    if (string.IsNullOrEmpty(codigoEpi))
                    {
                        M.AbrirMensagem($"Erro: Não encontrei Código para Modelo: '{it.Modelo}' | Tam: '{it.Tamanho}' | Cor: '{it.Cor}'", "Erro");
                        return;
                    }
                    listaPedidoInsert.Add((codigoEpi, it.Quantidade));
                }

                // ----- 2. PREPARAR DEVOLUÇÕES ('D') -----
                var listaDevInsert = new List<(string codigoEpi, int qtd)>();
                foreach (var it in itensDevolucao)
                {
                    // A Devolução vai para o mesmo sítio, portanto o Resolver é o mesmo!
                    string codigoEpi = _mr.ResolveCodigoEpi(it.Modelo, it.Tamanho, it.Cor);

                    if (string.IsNullOrEmpty(codigoEpi))
                    {
                        M.AbrirMensagem($"Erro: O artigo devolvido '{it.Modelo}' não tem código registado.", "Erro");
                        return;
                    }
                    listaDevInsert.Add((codigoEpi, it.Quantidade));
                }

                // ----- 3. CRIAR/OBTER CABEÇALHO DO PEDIDO -----
                int idPedReg = _sessao.IdPedidoReg ?? _mr.GetOrCreatePedidoPendente(_nrFunc);
                _sessao.IdPedidoReg = idPedReg;

                // ----- 4. GRAVAR NA BD UNIVERSAL -----
                if (listaPedidoInsert.Any())
                    _mr.SubmeterMovimento(idPedReg, listaPedidoInsert, 'P');

                if (listaDevInsert.Any())
                    _mr.SubmeterMovimento(idPedReg, listaDevInsert, 'D');

                // ----- 5. GUARDAR NOTAS (se existirem) -----
                if (!string.IsNullOrWhiteSpace(_notasPedido))
                {
                    using (var conn = Organizers.GetConn.GetConnection())
                    {
                        conn.Open();
                        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                            "UPDATE PedidoRegistos SET Notas = @n WHERE ID = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@n", _notasPedido.Trim());
                            cmd.Parameters.AddWithValue("@id", idPedReg);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                M.AbrirMensagem("Operação submetida com sucesso!", "Sucesso");
                this.Close();
            }
            catch (Exception ex)
            {
                M.AbrirMensagem("Erro ao submeter pedido:\n\n" + ex.Message, "Erro SQL");
            }
        }

        private void NavD_Click(object sender, EventArgs e)
        {
            using (Form overlay = new Form())
            {
                overlay.StartPosition = FormStartPosition.CenterScreen;
                overlay.WindowState = FormWindowState.Maximized;
                overlay.FormBorderStyle = FormBorderStyle.None;
                overlay.Opacity = 0.50d;
                overlay.BackColor = Color.Black;
                overlay.ShowInTaskbar = false;
                overlay.Location = this.Location;
                overlay.Size = this.Size;
                overlay.Show(this);
                using (FormNovaPasse frm = new FormNovaPasse(_nrFunc))
                    frm.ShowDialog();
            }
        }

        private void EscreverObs_Click(object sender, EventArgs e)
        {
            // Carrega notas existentes: da sessão local, ou da BD se já houver pedido pendente
            string notasAtuais = _notasPedido;
            if (_sessao?.IdPedidoReg.HasValue == true)
            {
                try
                {
                    using (var conn = Organizers.GetConn.GetConnection())
                    {
                        conn.Open();
                        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                            "SELECT ISNULL(Notas,'') FROM PedidoRegistos WHERE ID = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", _sessao.IdPedidoReg.Value);
                            var r = cmd.ExecuteScalar();
                            if (r != null) notasAtuais = r.ToString();
                        }
                    }
                }
                catch { }
            }

            using (Form overlay = new Form())
            {
                overlay.StartPosition = FormStartPosition.CenterScreen;
                overlay.WindowState = FormWindowState.Maximized;
                overlay.FormBorderStyle = FormBorderStyle.None;
                overlay.Opacity = 0.50d;
                overlay.BackColor = Color.Black;
                overlay.ShowInTaskbar = false;
                overlay.Location = this.Location;
                overlay.Size = this.Size;
                overlay.Show(this);

                using (var frm = new FormsSecundarios.FormEscreverObs(notasAtuais))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        _notasPedido = frm.Notas;

                        // Se já existe pedido na BD, guarda imediatamente
                        if (_sessao?.IdPedidoReg.HasValue == true)
                        {
                            try
                            {
                                using (var conn = Organizers.GetConn.GetConnection())
                                {
                                    conn.Open();
                                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                                        "UPDATE PedidoRegistos SET Notas = @n WHERE ID = @id", conn))
                                    {
                                        cmd.Parameters.AddWithValue("@n", _notasPedido);
                                        cmd.Parameters.AddWithValue("@id", _sessao.IdPedidoReg.Value);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                M.AbrirMensagem("Observações guardadas.", "Sucesso");
                            }
                            catch (Exception ex)
                            {
                                M.AbrirMensagem("Erro ao guardar: " + ex.Message, "Erro");
                            }
                        }
                        else
                        {
                            M.AbrirMensagem("Observações guardadas. Serão incluídas quando submeteres o pedido.", "Info");
                        }
                    }
                }
            }
        }
    }
}