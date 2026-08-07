using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PEPIDI
{
    /// <summary>
    /// Gere o registo do AgentePEPIDI no arranque automático do Windows.
    /// Usa a chave HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run — afeta
    /// apenas o utilizador atual, sem precisar de permissões de administrador.
    /// Chamado em Definicoes.cs quando o utilizador ativa/desativa o switch do agente.
    /// </summary>
    public static class StartupHelper
    {
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "AgentePEPIDI";

        /// <summary>
        /// Adiciona AgentePEPIDI.exe à chave Run do registo do utilizador atual.
        /// O executável é procurado na mesma pasta da app principal (Application.StartupPath).
        /// O caminho é delimitado por aspas para suportar pastas com espaços.
        /// </summary>
        public static void RegistarAgenteNoArranque()
        {
            try
            {
                string exePath = Path.Combine(Application.StartupPath, "AgentePEPIDI.exe");

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(AppName, "\"" + exePath + "\"");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Aviso: Não foi possível ativar o arranque automático com o Windows.\n" + ex.Message,
                                "Registo do Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Remove a entrada do AgentePEPIDI da chave Run, se existir.
        /// Verifica GetValue antes de DeleteValue para evitar exceção quando a chave já não existe.
        /// </summary>
        public static void RemoverAgenteDoArranque()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key != null && key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Aviso: Não foi possível desativar o arranque automático.\n" + ex.Message,
                                "Registo do Windows", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}