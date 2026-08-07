using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Utilitário de encriptação/desencriptação AES-256 para o ficheiro conn.bin.
/// A string de ligação à BD nunca é guardada em claro no disco — passa sempre
/// por aqui antes de ser escrita ou lida. O IV é gerado aleatoriamente a cada
/// encriptação e guardado nos primeiros 16 bytes do ficheiro, o que significa
/// que dois ficheiros com o mesmo conteúdo produzem bytes diferentes no disco.
/// </summary>
public static class Criptografia
{
    // Chave de 32 caracteres = AES-256. Tem de ser igual na app principal e no agente.
    private static readonly string chave = "PE*br2025!IDnruo*5Pi!2D0!eBUNr!2";

    /// <summary>
    /// Encripta o texto com AES-256 e guarda-o no caminho indicado.
    /// O IV (vetor de inicialização) é gerado aleatoriamente e escrito
    /// nos primeiros 16 bytes do ficheiro — necessário para desencriptar depois.
    /// </summary>
    public static void EncriptarParaFicheiro(string texto, string caminho)
    {
        byte[] bytesChave = Encoding.UTF8.GetBytes(chave.PadRight(32));
        using (Aes aes = Aes.Create())
        {
            aes.Key = bytesChave;
            aes.GenerateIV(); // IV aleatório a cada encriptação

            using (FileStream fs = new FileStream(caminho, FileMode.Create))
            {
                // Os primeiros 16 bytes do ficheiro são o IV em claro (não é segredo)
                fs.Write(aes.IV, 0, aes.IV.Length);

                using (CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(texto);
                }
            }
        }
    }

    /// <summary>
    /// Lê e desencripta o ficheiro no caminho indicado, devolvendo o texto original.
    /// Pressupõe que os primeiros 16 bytes são o IV gerado na encriptação.
    /// Lança exceção se o ficheiro não existir ou a chave for diferente.
    /// </summary>
    public static string DesencriptarDeFicheiro(string caminho)
    {
        byte[] bytesChave = Encoding.UTF8.GetBytes(chave.PadRight(32));
        using (FileStream fs = new FileStream(caminho, FileMode.Open))
        {
            // Lê os primeiros 16 bytes para recuperar o IV original
            byte[] iv = new byte[16];
            fs.Read(iv, 0, iv.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = bytesChave;
                aes.IV = iv;

                using (CryptoStream cs = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}

