using System;

namespace PEPIDI.Utils
{
    /// <summary>
    /// Guarda o estado da sessão ativa — quem está autenticado neste momento.
    /// Preenchido em FrmLogIn após login bem-sucedido e lido em qualquer UC ou Form
    /// sem ter de passar o ID por parâmetro em todo o lado.
    /// Não persiste entre sessões (memória apenas).
    /// </summary>
    public static class Sessao
    {
        public static int IdFuncionarioAtual { get; set; }
        public static int NivelAcessoAtual { get; set; }
        public static string NomeFuncionarioAtual { get; set; }
    }
}