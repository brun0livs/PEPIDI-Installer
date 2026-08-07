/// <summary>
/// Apresenta mensagens ao utilizador com um overlay semitransparente sobre a janela ativa.
/// O efeito de fundo escurecido impede que o utilizador interaja com a app enquanto a mensagem está aberta,
/// sem bloquear o processo (como faria um MessageBox nativo).
/// Tem dois modos: mensagem simples (botão OK/Fechar) e inputbox (com TextBox para entrada de texto).
/// </summary>
public class EfeitoUI
{
    /// <summary>
    /// Abre a mensagem com overlay. Se txt=true, mostra um TextBox e devolve o valor
    /// inserido via parâmetro 'out'. O overlay cobre exatamente a janela ativa (Form.ActiveForm)
    /// para manter o foco visual; se não houver janela ativa, maximiza para cobrir o ecrã todo.
    /// </summary>
    public DialogResult AbrirMensagem(string mensagem, string titulo, bool txt, out string valorInserido)
    {
        Form pai = Form.ActiveForm;
        DialogResult resultado = DialogResult.None;
        valorInserido = string.Empty;

        using (Form overlay = new Form())
        {
            overlay.StartPosition = FormStartPosition.Manual;
            overlay.FormBorderStyle = FormBorderStyle.None;
            overlay.Opacity = 0.50d;
            overlay.BackColor = Color.Black;
            overlay.ShowInTaskbar = false;

            if (pai != null)
            {
                // Cobre exatamente a janela pai — importante para janelas não-maximizadas
                overlay.Location = pai.Location;
                overlay.Size = pai.Size;
                overlay.Show(pai);
            }
            else
            {
                overlay.WindowState = FormWindowState.Maximized;
                overlay.Show();
            }

            using (PEPIDI.FormsSecundarios.MSGBX frm = new PEPIDI.FormsSecundarios.MSGBX(mensagem, titulo, txt))
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.TopMost = true;

                resultado = frm.ShowDialog(overlay);

                if (txt && resultado == DialogResult.OK)
                    valorInserido = frm.ValorInserido;
            }
        }
        return resultado;
    }

    /// <summary>
    /// Overload para mensagens simples sem campo de texto.
    /// </summary>
    public DialogResult AbrirMensagem(string mensagem, string titulo)
    {
        return AbrirMensagem(mensagem, titulo, false, out _);
    }
}