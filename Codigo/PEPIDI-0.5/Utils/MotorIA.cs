using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// Motor de correção automática baseado em regras da BD.
    /// Usado na importação de funcionários e stock para normalizar texto livre
    /// do Excel (ex: "T-Shirt Azul Escuro" → família "TShirt", cor "Azul Escuro").
    ///
    /// As listas são caches em memória carregadas uma vez por CarregarRegrasDaBD().
    /// As regras são ordenadas por LEN(PalavraChave) DESC — assim "Polo Manga Curta"
    /// é testado antes de "Polo" e não resulta num match errado mais curto.
    /// </summary>
    public static class MotorIA
    {
        private static List<KeyValuePair<string, string>> _regrasFamilia = new List<KeyValuePair<string, string>>();
        private static List<KeyValuePair<string, string>> _regrasFuncao = new List<KeyValuePair<string, string>>();
        private static List<string> _coresConhecidas = new List<string>();

        /// <summary>
        /// Carrega (ou recarrega) todas as regras da BD para as caches em memória.
        /// Chamado ao abrir FormImportarFuncionarios e FormImportarStock, e após
        /// AprenderNovaRegra (quando recarregar=true).
        /// </summary>
        public static void CarregarRegrasDaBD()
        {
            _regrasFamilia.Clear();
            _regrasFuncao.Clear();
            _coresConhecidas.Clear();

            using (SqlConnection conn = GetConn.GetConnection())
            {
                conn.Open();

                // Regras de família (ex: "polo manga curta" → "PoloMCurta")
                string sqlFam = "SELECT PalavraChave, FamiliaDestino FROM RegrasFamilia ORDER BY LEN(PalavraChave) DESC";
                using (SqlCommand cmd = new SqlCommand(sqlFam, conn))
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        _regrasFamilia.Add(new KeyValuePair<string, string>(
                            rdr["PalavraChave"].ToString().ToLower().Trim(),
                            rdr["FamiliaDestino"].ToString().Trim()));
                    }
                }

                // Regras de função (ex: "operador produção" → "Produção")
                string sqlFunc = "SELECT PalavraChave, FuncaoDestino FROM RegrasFuncao ORDER BY LEN(PalavraChave) DESC";
                using (SqlCommand cmd = new SqlCommand(sqlFunc, conn))
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        _regrasFuncao.Add(new KeyValuePair<string, string>(
                            rdr["PalavraChave"].ToString().ToLower().Trim(),
                            rdr["FuncaoDestino"].ToString().Trim()
                        ));
                    }
                }

                // Cores oficiais da BD — ORDER BY LEN DESC garante que "Azul Escuro"
                // é testado antes de "Azul" para evitar match parcial errado
                string sqlCor = "SELECT Nome FROM Cor ORDER BY LEN(Nome) DESC";
                using (SqlCommand cmd = new SqlCommand(sqlCor, conn))
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        _coresConhecidas.Add(rdr["Nome"].ToString().Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Tenta mapear um texto livre de Excel para uma família de EPI conhecida.
        /// Retorna "Verificar Colagem" quando nenhuma regra corresponde — sinaliza
        /// ao operador que a linha precisa de revisão manual.
        /// </summary>
        public static string CorrigirFamilia(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "Verificar Colagem";
            string busca = texto.ToLower().Trim();

            foreach (var regra in _regrasFamilia)
            {
                if (busca.Contains(regra.Key)) return regra.Value;
            }
            return "Verificar Colagem";
        }

        /// <summary>
        /// Tenta mapear um texto livre de Excel para uma função (cargo) conhecida.
        /// Retorna "Verificar Colagem" quando não há match — mesma convenção que CorrigirFamilia.
        /// </summary>
        public static string CorrigirFuncao(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "Verificar Colagem";
            string busca = texto.ToLower().Trim();

            foreach (var regra in _regrasFuncao)
            {
                if (busca.Contains(regra.Key)) return regra.Value;
            }
            return "Verificar Colagem";
        }

        /// <summary>
        /// Procura o nome de uma cor oficial da BD dentro do texto do modelo (ex: "T-Shirt Azul Escuro").
        /// Retorna o nome oficial da cor (ex: "Azul Escuro") para pré-selecionar no combo de cor,
        /// ou string vazia se não encontrar nenhuma cor conhecida.
        /// </summary>
        public static string ExtrairCorDoModelo(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "";
            string busca = texto.ToLower().Trim();

            foreach (string cor in _coresConhecidas)
            {
                if (busca.Contains(cor.ToLower()))
                    return cor;
            }

            return "";
        }

        /// <summary>
        /// Grava ou atualiza uma regra de correção na BD e opcionalmente recarrega as caches.
        /// Usado quando o operador corrige manualmente uma família ou função na grelha de importação —
        /// a correção é "aprendida" para afetar automaticamente outras linhas com o mesmo texto original.
        /// tipo = "Familia" ou "Funcao".
        /// </summary>
        public static void AprenderNovaRegra(string errado, string certo, string tipo, bool recarregar = true)
        {
            if (string.IsNullOrWhiteSpace(errado) || string.IsNullOrWhiteSpace(certo)) return;

            using (SqlConnection conn = GetConn.GetConnection())
            {
                conn.Open();

                string tabela = (tipo == "Familia") ? "RegrasFamilia" : "RegrasFuncao";
                string colDestino = (tipo == "Familia") ? "FamiliaDestino" : "FuncaoDestino";

                // UPSERT: atualiza se a palavra-chave já existir, insere caso contrário
                string sql = $@"
            IF EXISTS (SELECT 1 FROM {tabela} WHERE PalavraChave = @e)
                UPDATE {tabela} SET {colDestino} = @c WHERE PalavraChave = @e
            ELSE
                INSERT INTO {tabela} (PalavraChave, {colDestino}) VALUES (@e, @c)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@e", errado.ToLower().Trim());
                    cmd.Parameters.AddWithValue("@c", certo.Trim());
                    cmd.ExecuteNonQuery();
                }
            }

            // Recarrega para que a nova regra entre em vigor nas próximas correções
            if (recarregar)
                CarregarRegrasDaBD();
        }
    }
}