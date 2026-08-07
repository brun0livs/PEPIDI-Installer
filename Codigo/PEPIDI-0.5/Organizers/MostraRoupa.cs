using Microsoft.Data.SqlClient;
using PEPIDI.Models;
using PEPIDI.Organizers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PEPIDI
{
    /// <summary>
    /// Repositório central de lógica de EPI por funcionário.
    /// Centraliza: obtenção de artigos disponíveis, artigos já consumidos,
    /// resolução de códigos EPI, cálculo de stock líquido (pedidos menos devoluções),
    /// submissão de movimentos (pedido/devolução) e gestão do pedido pendente ativo.
    /// </summary>
    internal class MostraRoupa
    {
        private static readonly Dictionary<string, string> _nomesVista = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _nomesVistaLock = new();

        private static string ResolverNomeVista(string familia)
        {
            if (_nomesVista.Count == 0)
            {
                lock (_nomesVistaLock)
                {
                    if (_nomesVista.Count == 0)
                    {
                        using var conn = GetConn.GetConnection();
                        using var cmd = new SqlCommand("SELECT Nome, NomeVista FROM Familias WHERE Ativo = 1", conn);
                        conn.Open();
                        using var r = cmd.ExecuteReader();
                        while (r.Read()) _nomesVista[r.GetString(0)] = r.GetString(1);
                    }
                }
            }
            return _nomesVista.TryGetValue(familia, out var nv) ? nv : familia;
        }

        // ====================================================================================
        // 1. OBTER DADOS
        // ====================================================================================

        /// <summary>
        /// Devolve os EPIs a que o funcionário tem direito (novos para pedir),
        /// incluindo os tamanhos disponíveis em stock e todos os modelos da mesma família.
        /// O tamanho pré-selecionado é o que está registado no perfil do funcionário.
        /// </summary>
        public List<LinhaPedidoInfo> ObterRoupaPorFuncionarioNovo(int nrFuncionario)
        {
            var resultado = new List<LinhaPedidoInfo>();

            using (SqlConnection conn = GetConn.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_RoupaPorFuncionario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NrFunc", nrFuncionario);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var modelo = reader["Modelo"].ToString();
                        var familia = reader["Familia"].ToString();
                        string corStr = reader["Cor"] != DBNull.Value ? reader["Cor"].ToString().Trim() : "00";

                        resultado.Add(new LinhaPedidoInfo
                        {
                            CodigoEpi = reader["Codigo"].ToString(),
                            Cor = corStr,
                            Modelo = modelo,
                            Familia = familia,
                            NomeVista = ResolverNomeVista(familia),
                            TamanhoAtual = reader["Tamanho"].ToString(),
                            TamanhosDisponiveis = ObterTamanhosDisponiveis(modelo, corStr),
                            ModelosDisponiveis = ObterModelosPorFamilia(familia)
                        });
                    }
                }
            }
            return resultado;
        }

        /// <summary>
        /// Devolve todos os modelos distintos de uma família de EPI,
        /// para popular o combo de modelos quando há mais do que um na família.
        /// </summary>
        private List<string> ObterModelosPorFamilia(string familia)
        {
            var modelos = new List<string>();
            using (SqlConnection conn = GetConn.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT Modelo FROM EPI WHERE Familia = @fam", conn))
            {
                cmd.Parameters.AddWithValue("@fam", familia);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) modelos.Add(r.GetString(0));
                }
            }
            return modelos;
        }

        /// <summary>
        /// Devolve os EPIs que o funcionário já tem em seu poder (para devolver).
        /// Filtra por modelo único para não repetir artigos — o tamanho devolvido
        /// vem do histórico de pedidos aprovados do funcionário.
        /// </summary>
        public List<LinhaPedidoInfo> ObterRoupaUsadaPorFuncionarioNovo(int nrFuncionario)
        {
            var resultado = new List<LinhaPedidoInfo>();

            using (SqlConnection conn = GetConn.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_ProdutosConsumidosPorFuncionario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NrFunc", nrFuncionario);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var modelo = reader["Modelo"].ToString();
                        var tamanho = reader["Tamanho"].ToString();
                        var familia = reader["Familia"].ToString();
                        string corStr = reader["Cor"] != DBNull.Value ? reader["Cor"].ToString().Trim() : "00";

                        if (!resultado.Any(x => x.Modelo == modelo))
                        {
                            resultado.Add(new LinhaPedidoInfo
                            {
                                CodigoEpi = reader["Codigo"].ToString(),
                                Cor = corStr,
                                Modelo = modelo,
                                Familia = familia,
                                NomeVista = ResolverNomeVista(familia),
                                TamanhoAtual = tamanho,
                                TamanhosDisponiveis = ObterTamanhosUsadosPorFuncionario(modelo, nrFuncionario)
                            });
                        }
                    }
                }
            }
            return resultado;
        }

        /// <summary>
        /// Devolve os tamanhos de um modelo que este funcionário já recebeu em pedidos anteriores.
        /// Usado no modo Devolução para mostrar apenas os tamanhos que fazem sentido devolver.
        /// </summary>
        public List<string> ObterTamanhosUsadosPorFuncionario(string modelo, int nrFunc)
        {
            List<string> tamanhos = new List<string>();

            using (SqlConnection conn = GetConn.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT DISTINCT E.Tamanho FROM PedidoPacote PP
                INNER JOIN PedidoRegistos PR ON PP.IDPedidoRegisto = PR.ID
                INNER JOIN EPI E ON PP.CodigoEPI = E.Codigo
                WHERE PR.NrFunc = @NrFunc AND E.Modelo = @Modelo AND PP.TipoMovimento = 'P'", conn))
            {
                cmd.Parameters.AddWithValue("@NrFunc", nrFunc);
                cmd.Parameters.AddWithValue("@Modelo", modelo);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tamanhos.Add(reader["Tamanho"].ToString().Trim());
                    }
                }
            }
            return tamanhos;
        }

        private List<string> ObterTamanhosDisponiveis(string modelo, string cor)
        {
            List<string> tamanhos = new List<string>();
            using (SqlConnection conn = GetConn.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_GetTamanhosPorModelo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Cor", cor);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tamanhos.Add(reader.GetString(0).Trim());
                    }
                }
            }
            return tamanhos;
        }

        // ====================================================================================
        // 2. RESOLVEDORES DE IDs E STOCK
        // ====================================================================================

        /// <summary>
        /// Resolve o código EPI (string de 7 dígitos) a partir de Modelo + Tamanho + Cor.
        /// Retorna null se não encontrar — o chamador deve tratar este caso como erro antes
        /// de tentar inserir na BD, pois o código é a chave estrangeira em PedidoPacote.
        /// </summary>
        public string ResolveCodigoEpi(string modelo, string tamanho, string cor)
        {
            string codigo = null;

            modelo = modelo?.Trim() ?? "";
            tamanho = tamanho?.Trim() ?? "";
            cor = string.IsNullOrWhiteSpace(cor) ? "00" : cor.Trim();

            string sql = @"SELECT TOP 1 Codigo 
                           FROM EPI 
                           WHERE LTRIM(RTRIM(Modelo)) = @m 
                             AND LTRIM(RTRIM(Tamanho)) = @t 
                             AND ISNULL(CorID, '00') = @c
                             AND Ativo = 1";

            using (SqlConnection conn = GetConn.GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@m", modelo);
                cmd.Parameters.AddWithValue("@t", tamanho);
                cmd.Parameters.AddWithValue("@c", cor);

                try
                {
                    conn.Open();
                    object res = cmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        codigo = res.ToString();
                    }
                }
                catch { /* Evita crashes em background */ }
            }

            return codigo;
        }

        /// <summary>
        /// Calcula quantas unidades de um EPI o funcionário tem atualmente em sua posse.
        /// A fórmula é: SUM(pedidos 'P') - SUM(devoluções 'D') — o saldo líquido.
        /// Permite saber se faz sentido mostrar um artigo no modo de devolução.
        /// </summary>
        public int GetConsumidoDisponivel(int nrFunc, string codigoEpi)
        {
            using (SqlConnection conn = GetConn.GetConnection())
            {
                // Soma as entregas ('P') e subtrai as devoluções ('D') para obter o saldo real
                string sql = @"SELECT ISNULL(SUM(CASE WHEN PP.TipoMovimento = 'P' THEN PP.Quantidade ELSE -PP.Quantidade END), 0) 
                               FROM PedidoPacote PP 
                               INNER JOIN PedidoRegistos PR ON PP.IDPedidoRegisto = PR.ID
                               WHERE PR.NrFunc = @nr AND PP.CodigoEPI = @codigo;";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nr", nrFunc);
                    cmd.Parameters.AddWithValue("@codigo", codigoEpi);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        // ====================================================================================
        // 3. SUBMISSÃO UNIVERSAL
        // ====================================================================================

        /// <summary>
        /// Insere ou acumula linhas em PedidoPacote para um dado cabeçalho (idPedReg).
        /// Serve tanto para pedidos ('P') como para devoluções ('D') — o tipoMovimento
        /// separa os dois fluxos na mesma tabela. Toda a operação corre numa transação:
        /// se uma linha falhar, nenhuma é gravada.
        /// </summary>
        public void SubmeterMovimento(int idPedReg, List<(string codigoEpi, int qtd)> itens, char tipoMovimento)
        {
            using (SqlConnection conn = GetConn.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in itens)
                        {
                            // UPSERT: se já existe linha para este EPI e tipo, acumula a quantidade;
                            // caso contrário cria uma nova linha. IDStock fica NULL até à aprovação.
                            string sql = @"
                                IF EXISTS (SELECT 1 FROM PedidoPacote WHERE IDPedidoRegisto = @idReg AND CodigoEPI = @codigo AND TipoMovimento = @tipo)
                                BEGIN
                                    UPDATE PedidoPacote 
                                    SET Quantidade = Quantidade + @qtd 
                                    WHERE IDPedidoRegisto = @idReg AND CodigoEPI = @codigo AND TipoMovimento = @tipo;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO PedidoPacote (IDPedidoRegisto, CodigoEPI, Quantidade, IDStock, TipoMovimento) 
                                    VALUES (@idReg, @codigo, @qtd, NULL, @tipo);
                                END";

                            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@idReg", idPedReg);
                                cmd.Parameters.AddWithValue("@codigo", item.codigoEpi);
                                cmd.Parameters.AddWithValue("@qtd", item.qtd);
                                cmd.Parameters.AddWithValue("@tipo", tipoMovimento);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Devolve o ID do pedido pendente mais recente do funcionário,
        /// ou cria um novo registo em PedidoRegistos se não existir nenhum.
        /// A verificação e a criação correm na mesma transação para evitar
        /// que dois processos simultâneos criem dois cabeçalhos duplicados.
        /// </summary>
        public int GetOrCreatePedidoPendente(int nrFunc)
        {
            using (var conn = GetConn.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var check = new SqlCommand(@"
                            SELECT TOP 1 ID FROM PedidoRegistos
                            WHERE NrFunc = @NrFunc AND Estado = 'Pendente'
                            ORDER BY Data DESC;", conn, tran))
                        {
                            check.Parameters.AddWithValue("@NrFunc", nrFunc);
                            object res = check.ExecuteScalar();
                            if (res != null && res != DBNull.Value)
                            {
                                tran.Commit();
                                return Convert.ToInt32(res);
                            }
                        }

                        using (var insert = new SqlCommand(@"
                            INSERT INTO PedidoRegistos (Data, NrFunc, Estado)
                            VALUES (@Data, @NrFunc, 'Pendente');
                            SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tran))
                        {
                            insert.Parameters.AddWithValue("@Data", DateTime.Now);
                            insert.Parameters.AddWithValue("@NrFunc", nrFunc);
                            int idNovo = Convert.ToInt32(insert.ExecuteScalar());
                            tran.Commit();
                            return idNovo;
                        }
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Guarda o tamanho escolhido pelo funcionário na coluna correspondente à família
        /// na tabela Funcionarios. Assim, da próxima vez que abrir o pedido, o tamanho
        /// já vem pré-selecionado sem ter de confirmar de novo.
        /// Falhas são ignoradas silenciosamente — não é crítico para o fluxo principal.
        /// </summary>
        public void AtualizarTamanhoPadraoFuncionario(int nrFunc, string codigoEpi, string novoTamanho)
        {
            try
            {
                using (var conn = GetConn.GetConnection())
                {
                    conn.Open();

                    string familia = "";
                    using (var cmdBusca = new SqlCommand("SELECT Familia FROM EPI WHERE Codigo = @Codigo", conn))
                    {
                        cmdBusca.Parameters.AddWithValue("@Codigo", codigoEpi);
                        var result = cmdBusca.ExecuteScalar();
                        if (result != null) familia = result.ToString().Trim();
                    }

                    if (string.IsNullOrEmpty(familia)) return;

                    string upsert = @"IF EXISTS (SELECT 1 FROM FuncionarioTamanhos WHERE Nr=@Nr AND Familia=@Familia)
                        UPDATE FuncionarioTamanhos SET Tamanho=@Tamanho WHERE Nr=@Nr AND Familia=@Familia
                    ELSE
                        INSERT INTO FuncionarioTamanhos (Nr, Familia, Tamanho) VALUES (@Nr, @Familia, @Tamanho)";

                    using (var cmdUpdate = new SqlCommand(upsert, conn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@Nr", nrFunc);
                        cmdUpdate.Parameters.AddWithValue("@Familia", familia);
                        cmdUpdate.Parameters.AddWithValue("@Tamanho", novoTamanho);
                        cmdUpdate.ExecuteNonQuery();
                    }
                }
            }
            catch { /* Ignorado silenciosamente */ }
        }

        /// <summary>
        /// Devolve o último modelo que o funcionário recebeu numa determinada família.
        /// Usado para pré-selecionar o modelo no combo quando há vários da mesma família —
        /// preferência pelo histórico em vez do primeiro da lista por ordem alfabética.
        /// </summary>
        public string ObterUltimoModeloConsumidoPorFamilia(int nrFunc, string familia)
        {
            using (SqlConnection conn = GetConn.GetConnection())
            {
                string sql = @"
        SELECT TOP 1 E.Modelo 
        FROM PedidoPacote PP
        INNER JOIN PedidoRegistos PR ON PP.IDPedidoRegisto = PR.ID
        INNER JOIN EPI E ON PP.CodigoEPI = E.Codigo 
        WHERE PR.NrFunc = @nr AND E.Familia = @fam AND PP.TipoMovimento = 'P'
        ORDER BY PR.Data DESC, PR.ID DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nr", nrFunc);
                    cmd.Parameters.AddWithValue("@fam", familia);
                    conn.Open();
                    object res = cmd.ExecuteScalar();
                    return res?.ToString() ?? string.Empty;
                }
            }
        }
    }
}