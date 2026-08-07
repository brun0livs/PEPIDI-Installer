using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// Utilitário de hashing de palavras-passe com SHA-256.
    /// Usado ao criar/importar funcionários e na verificação de login.
    /// A BD nunca guarda a senha em claro — apenas o hash.
    /// </summary>
    internal class Hash
    {
        /// <summary>
        /// Gera o hash SHA-256 da senha recebida e devolve-o em hexadecimal minúsculo.
        /// O formato lowercase ("x2") é importante para comparação consistente com
        /// os hashes já guardados na tabela LogIn.
        /// </summary>
        public string GerarHashSenha(string senha)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(senha));

                // Converte cada byte para dois caracteres hex ('x2' = minúsculo, sem prefixo 0x)
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
