using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Cifra simétrica AES-256 para proteger a string de ligação à base de dados.
/// O IV (vetor de inicialização) é gerado aleatoriamente a cada encriptação e
/// guardado nos primeiros 16 bytes do ficheiro, garantindo que dois ficheiros
/// com o mesmo conteúdo produzem resultados distintos.
/// A chave fixa de 32 bytes é partilhada entre a App Principal (que escreve
/// o conn.bin) e o Agente (que o lê), pelo que qualquer alteração à chave
/// invalida os ficheiros existentes.
/// </summary>
public static class Criptografia
{
    private static readonly string chave = "PE*br2025!IDnruo*5Pi!2D0!eBUNr!2";

    /// <summary>Cifra <paramref name="texto"/> com AES-256 e grava o resultado em <paramref name="caminho"/>.</summary>
    public static void EncriptarParaFicheiro(string texto, string caminho)
    {
        byte[] bytesChave = Encoding.UTF8.GetBytes(chave.PadRight(32));
        using (Aes aes = Aes.Create())
        {
            aes.Key = bytesChave;
            aes.GenerateIV();

            using (FileStream fs = new FileStream(caminho, FileMode.Create))
            {
                fs.Write(aes.IV, 0, aes.IV.Length); // guardar IV

                using (CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(texto);
                }
            }
        }
    }

    /// <summary>Lê o ficheiro em <paramref name="caminho"/>, extrai o IV e devolve o texto original.</summary>
    public static string DesencriptarDeFicheiro(string caminho)
    {
        byte[] bytesChave = Encoding.UTF8.GetBytes(chave.PadRight(32));
        using (FileStream fs = new FileStream(caminho, FileMode.Open))
        {
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

