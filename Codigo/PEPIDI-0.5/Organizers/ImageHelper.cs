using System.Drawing;
using System.Drawing.Imaging;

/// <summary>
/// Utilitários de manipulação de imagens. Usado principalmente para inverter
/// as cores dos ícones dos botões de navegação quando ficam "ativos" (selecionados).
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Devolve uma nova imagem com as cores RGB invertidas, mantendo o canal alpha intacto.
    /// Usado em FormGestao para alternar o ícone dos nav-buttons entre estado normal e ativo.
    /// Não modifica a imagem original — cria sempre um Bitmap novo.
    /// </summary>
    public static Image InverterCores(Image imagemOriginal)
    {
        if (imagemOriginal == null) return null;

        Bitmap novaImagem = new Bitmap(imagemOriginal.Width, imagemOriginal.Height);

        using (Graphics g = Graphics.FromImage(novaImagem))
        {
            // ColorMatrix que multiplica R, G, B por -1 e soma 1 em offset:
            // resultado: 0 → 1 (preto vira branco) e 1 → 0 (branco vira preto).
            // A linha do Alpha (linha 3) mantém o valor original para preservar transparência.
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] {-1, 0, 0, 0, 0}, // Red
                new float[] { 0,-1, 0, 0, 0}, // Green
                new float[] { 0, 0,-1, 0, 0}, // Blue
                new float[] { 0, 0, 0, 1, 0}, // Alpha (inalterado)
                new float[] { 1, 1, 1, 0, 1}  // Offset RGB +1
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            // Desenha a imagem original na nova imagem aplicando o filtro
            g.DrawImage(imagemOriginal,
                new Rectangle(0, 0, imagemOriginal.Width, imagemOriginal.Height),
                0, 0, imagemOriginal.Width, imagemOriginal.Height,
                GraphicsUnit.Pixel,
                attributes);
        }

        return novaImagem;
    }
}