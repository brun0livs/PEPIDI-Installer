using System.Reflection;
using System.Windows.Forms;

namespace PEPIDI.Organizers
{
    /// <summary>
    /// Utilitários de otimização de renderização WinForms.
    /// </summary>
    public static class HelperPerformance
    {
        /// <summary>
        /// Ativa DoubleBuffered em todos os controlos dentro de um painel, recursivamente.
        /// O WinForms não expõe DoubleBuffered publicamente em Control, daí ser necessária reflexão.
        /// Evita o efeito de flicker ("tremelique") quando há muitos redesenhos seguidos,
        /// especialmente em painéis com muitos UCs aninhados.
        /// </summary>
        public static void AtivarDoubleBufferRecursivo(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(c, true, null);

                if (c.HasChildren)
                    AtivarDoubleBufferRecursivo(c);
            }
        }
    }
}