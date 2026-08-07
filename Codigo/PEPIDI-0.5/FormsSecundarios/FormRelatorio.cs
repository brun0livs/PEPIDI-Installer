using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using PEPIDI.Organizers; // Ajusta se as tuas classes PDFGenerator e GetConn estiverem noutro sítio

namespace PEPIDI.FormsSecundarios
{
    /// <summary>
    /// Diálogo para gerar relatórios PDF de pedidos e/ou devoluções selecionados na grelha.
    /// As checkboxes de pedidos/devoluções são automaticamente desativadas se as
    /// listas correspondentes estiverem vazias — evita que o utilizador tente gerar
    /// um relatório sem dados. Após geração, abre os PDFs diretamente no Explorer.
    /// Quando fechado com DialogResult.OK, o chamador (Pedidos.cs) limpa os vistos da grelha.
    /// </summary>
    public partial class FormRelatorio : Form
    {
        private List<int> idsPedidosSelecionados;
        private List<int> idsDevolucoesSelecionadas;

        public FormRelatorio(List<int> idsPedidos, List<int> idsDevolucoes)
        {
            InitializeComponent();

            // Garantir que as listas nunca são nulas
            this.idsPedidosSelecionados = idsPedidos ?? new List<int>();
            this.idsDevolucoesSelecionadas = idsDevolucoes ?? new List<int>();

            // Lógica inteligente: Se não houver IDs de pedidos, desativa a checkbox de pedidos
            if (this.idsPedidosSelecionados.Count == 0)
            {
                ckbPedidos.Checked = false;
                ckbPedidos.Enabled = false;
            }

            // Lógica inteligente: Se não houver IDs de devoluções, desativa a checkbox
            if (this.idsDevolucoesSelecionadas.Count == 0)
            {
                ckbDevolucoes.Checked = false;
                ckbDevolucoes.Enabled = false;
            }

            // Nota: os eventos btnOK, lblFechar e btnCancelar já estão ligados no Designer (InitializeComponent)
            // — não duplicar aqui para evitar que cada clique execute o handler duas vezes.
        }

        private void FecharFormulario(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // Validação
            if (!ckbPedidos.Checked && !ckbDevolucoes.Checked)
            {
                EfeitoUI M = new EfeitoUI(); // Assumindo que tens esta classe para mensagens
                M.AbrirMensagem("Por favor, seleciona pelo menos um tipo de relatório.", "Aviso");
                return;
            }

            try
            {
                // 1. GERAR PEDIDOS
                if (ckbPedidos.Checked && idsPedidosSelecionados.Count > 0)
                {
                    GerarRelatorioPedidos();
                }

                // 2. GERAR DEVOLUÇÕES
                if (ckbDevolucoes.Checked && idsDevolucoesSelecionadas.Count > 0)
                {
                    GerarRelatorioDevolucoes();
                }

                // Retornar OK para o ecrã principal saber que correu tudo bem e limpar os vistos das grelhas
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                EfeitoUI M = new EfeitoUI();
                string detalhe = ex.InnerException != null ? $"\nDetalhe: {ex.InnerException.Message}" : "";
                M.AbrirMensagem($"Erro ao gerar os relatórios: {ex.Message}{detalhe}", "Erro");
            }
        }

        private void GerarRelatorioPedidos()
        {
            var listaArmazem = new List<(string NMEC, string Nome, string Modelo, string Tamanho, int Qtd)>();

            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            {
                conn.Open();
                string inClause = string.Join(",", idsPedidosSelecionados);

                string sql = $@"
                    SELECT 
                        F.Nr AS NrFunc, F.Nome, E.Modelo, E.Tamanho, S.Estado, SUM(PP.Quantidade) AS QtdTotal 
                    FROM PedidoPacote PP
                    INNER JOIN EPI E ON PP.CodigoEPI = E.Codigo
                    INNER JOIN PedidoRegistos PR ON PP.IDPedidoRegisto = PR.ID
                    INNER JOIN Funcionarios F ON PR.NrFunc = F.Nr
                    LEFT JOIN Stock S ON PP.IDStock = S.ID
                    WHERE PP.IDPedidoRegisto IN ({inClause}) AND PP.Quantidade > 0
                    GROUP BY F.Nr, F.Nome, E.Modelo, E.Tamanho, S.Estado
                    ORDER BY F.Nome, E.Modelo, E.Tamanho";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string modeloBase = reader["Modelo"]?.ToString() ?? "Artigo";
                            int idEstado = reader["Estado"] != DBNull.Value ? Convert.ToInt32(reader["Estado"]) : 1;
                            string etiquetaEstado = (idEstado == 2) ? " [USADO]" : " [NOVO]";

                            listaArmazem.Add((
                                reader["NrFunc"]?.ToString() ?? "0000",
                                reader["Nome"]?.ToString() ?? "Desconhecido",
                                modeloBase + etiquetaEstado,
                                reader["Tamanho"]?.ToString() ?? "-",
                                Convert.ToInt32(reader["QtdTotal"])
                            ));
                        }
                    }
                }
            }

            if (listaArmazem.Count > 0)
            {
                string pdfPath = PDFGenerator.GerarListaSeparacaoPorFuncionario(listaArmazem);
                Process.Start("explorer.exe", pdfPath);
            }
        }

        private void GerarRelatorioDevolucoes()
        {
            var listaDevolucoes = new List<(string NMEC, string Nome, string Modelo, string Tamanho, int Qtd, string Motivo)>();

            using (SqlConnection conn = new SqlConnection(GetConn.ConnectionString))
            {
                conn.Open();
                string inClause = string.Join(",", idsDevolucoesSelecionadas);

                string sql = $@"
                    SELECT
                        F.Nr AS NrFunc, F.Nome, E.Modelo, E.Tamanho,
                        SUM(PP.Quantidade) AS QtdTotal,
                        CASE S.Estado WHEN 1 THEN 'Novo' WHEN 3 THEN 'Gasto/Inutilizável' ELSE 'Usado' END AS Motivo
                    FROM PedidoPacote PP
                    INNER JOIN EPI E ON PP.CodigoEPI = E.Codigo
                    INNER JOIN PedidoRegistos PR ON PP.IDPedidoRegisto = PR.ID
                    INNER JOIN Funcionarios F ON PR.NrFunc = F.Nr
                    LEFT JOIN Stock S ON PP.IDStock = S.ID
                    WHERE PP.IDPedidoRegisto IN ({inClause}) AND PP.TipoMovimento = 'D' AND PP.Quantidade > 0
                    GROUP BY F.Nr, F.Nome, E.Modelo, E.Tamanho, S.Estado
                    ORDER BY F.Nome, E.Modelo, E.Tamanho";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaDevolucoes.Add((
                                reader["NrFunc"]?.ToString() ?? "0000",
                                reader["Nome"]?.ToString() ?? "Desconhecido",
                                reader["Modelo"]?.ToString() ?? "Artigo",
                                reader["Tamanho"]?.ToString() ?? "-",
                                Convert.ToInt32(reader["QtdTotal"]),
                                reader["Motivo"]?.ToString() ?? "Sem Motivo"
                            ));
                        }
                    }
                }
            }

            if (listaDevolucoes.Count > 0)
            {
                string pdfPath = PDFGenerator.GerarListaDevolucoesArmazem(listaDevolucoes);
                Process.Start("explorer.exe", pdfPath);
            }
        }
    }
}