using Guna.UI2.WinForms;
using PEPIDI.FormsSecundarios;
using PEPIDI.Models;
using System.Drawing;
using System.Windows.Forms;

namespace PEPIDI.Utils
{
    /// <summary>
    /// Identifica o tipo de ecrã onde a app está a correr.
    /// Lido nas Definições e gravado em Properties.Settings para persistir entre sessões.
    /// </summary>
    public enum TipoEcra
    {
        Portatil,       // Portáteis e ecrãs pequenos (ex: 1366x768)
        MonitorFullHD,  // Monitor standard de secretária (ex: 1920x1080)
        Surface,        // Tablets ou ecrãs com DPI alto/zoom elevado
        Televisao       // TV de grande formato (55"+)
    }

    /// <summary>
    /// Tema adaptativo da aplicação: centraliza fontes, tamanhos e espaçamentos
    /// para todos os tipos de ecrã definidos no TipoEcra.
    /// AplicarEstilos() percorre a árvore de controlos recursivamente e aplica
    /// os valores corretos para o ModoAtual em vigor.
    /// </summary>
    public static class GestorTema
    {
        private static TipoEcra _modoAtual = TipoEcra.MonitorFullHD;

        /// <summary>
        /// Modo de ecrã ativo. Definido no arranque a partir das settings guardadas.
        /// Alterar em runtime invalida automaticamente as fontes em cache.
        /// </summary>
        public static TipoEcra ModoAtual
        {
            get => _modoAtual;
            set { if (_modoAtual != value) { _modoAtual = value; ResetarFontes(); } }
        }

        // Fontes em cache — criadas uma vez por modo, descartadas quando o modo muda.
        private static Font _fonteLabel, _fonteTitulo, _fonteNome,
                            _fonteBotaoNAV, _fonteBotao, _fonteData;

        private static void ResetarFontes()
        {
            _fonteLabel?.Dispose();    _fonteLabel    = null;
            _fonteTitulo?.Dispose();   _fonteTitulo   = null;
            _fonteNome?.Dispose();     _fonteNome     = null;
            _fonteBotaoNAV?.Dispose(); _fonteBotaoNAV = null;
            _fonteBotao?.Dispose();    _fonteBotao    = null;
            _fonteData?.Dispose();     _fonteData     = null;
        }

        // ==========================================
        // AS REGRAS MULTI-ECRÃ
        // ==========================================

        // LABELS
        public static Font FonteLabel => _fonteLabel ??= ModoAtual switch
        {
            TipoEcra.Portatil => new Font("Roboto", 9F, FontStyle.Regular),
            TipoEcra.MonitorFullHD => new Font("Roboto", 11F, FontStyle.Regular),
            TipoEcra.Surface => new Font("Roboto", 12F, FontStyle.Regular),
            TipoEcra.Televisao => new Font("Roboto", 24F, FontStyle.Regular),
            _ => new Font("Roboto", 11F, FontStyle.Regular)
        };

        public static Font FonteTitulo => _fonteTitulo ??= ModoAtual switch
        {
            TipoEcra.Portatil => new Font("Roboto Medium", 12F, FontStyle.Regular),
            TipoEcra.MonitorFullHD => new Font("Roboto Medium", 16F, FontStyle.Regular),
            TipoEcra.Surface => new Font("Roboto Medium", 16F, FontStyle.Regular),
            TipoEcra.Televisao => new Font("Roboto Medium", 25F, FontStyle.Regular),
            _ => new Font("Roboto Medium", 16F, FontStyle.Regular)
        };

        public static Font FonteNome => _fonteNome ??= ModoAtual switch
        {
            TipoEcra.Portatil => new Font("Roboto", 12F, FontStyle.Regular),
            TipoEcra.MonitorFullHD => new Font("Roboto", 16F, FontStyle.Regular),
            TipoEcra.Surface => new Font("Roboto", 16F, FontStyle.Regular),
            TipoEcra.Televisao => new Font("Roboto", 25F, FontStyle.Regular),
            _ => new Font("Roboto", 16F, FontStyle.Regular)
        };

        // BOTÕES
        public static Font FonteBotaoNAV => _fonteBotaoNAV ??= ModoAtual switch
        {
            TipoEcra.Portatil => new Font("Roboto", 12F, FontStyle.Regular),
            TipoEcra.MonitorFullHD => new Font("Roboto", 16F, FontStyle.Regular),
            TipoEcra.Surface => new Font("Roboto", 16F, FontStyle.Regular),
            TipoEcra.Televisao => new Font("Roboto", 25F, FontStyle.Regular),
            _ => new Font("Roboto", 16F, FontStyle.Regular)
        };

        public static Font FonteBotao => _fonteBotao ??= ModoAtual switch
        {
            TipoEcra.Portatil => new Font("Roboto", 11F, FontStyle.Regular),
            TipoEcra.MonitorFullHD => new Font("Roboto", 12F, FontStyle.Regular),
            TipoEcra.Surface => new Font("Roboto", 12F, FontStyle.Regular),
            TipoEcra.Televisao => new Font("Roboto", 20F, FontStyle.Regular),
            _ => new Font("Roboto", 12F, FontStyle.Regular)
        };

        public static Padding PaddingBotao => ModoAtual switch
        {
            TipoEcra.Portatil => new Padding(5, 2, 5, 2),
            TipoEcra.MonitorFullHD => new Padding(10, 5, 10, 5),
            TipoEcra.Surface => new Padding(20, 15, 20, 15),
            TipoEcra.Televisao => new Padding(30, 20, 30, 20),
            _ => new Padding(10, 5, 10, 5)
        };

        public static Size TamanhoIconeBotao => ModoAtual switch
        {
            TipoEcra.Portatil => new Size(32, 32),
            TipoEcra.MonitorFullHD => new Size(43, 43),
            TipoEcra.Surface => new Size(80, 80), // O teu ícone grande para o Surface!
            TipoEcra.Televisao => new Size(128, 128),
            _ => new Size(43, 43)
        };

        // GRELHAS (DATAGRIDVIEWS)
        public static int AlturaLinhaDgv => ModoAtual switch
        {
            TipoEcra.Portatil => 50,
            TipoEcra.MonitorFullHD => 54,
            TipoEcra.Surface => 80,
            TipoEcra.Televisao => 85,
            _ => 35
        };

        // LARGURAS DO MENU LATERAL
        public static int LarguraMenuExpandido => ModoAtual switch
        {
            TipoEcra.Portatil => 250,
            TipoEcra.MonitorFullHD => 310,
            TipoEcra.Surface => 450,
            TipoEcra.Televisao => 600,
            _ => 310
        };

        public static int LarguraMenuRecolhido => ModoAtual switch
        {
            TipoEcra.Portatil => 60,
            TipoEcra.MonitorFullHD => 79,
            TipoEcra.Surface => 115,
            TipoEcra.Televisao => 150,
            _ => 79
        };

        public static int AlturaCombos => ModoAtual switch
        {
            TipoEcra.Portatil => 28,
            TipoEcra.MonitorFullHD => 36,
            TipoEcra.Surface => 50,
            TipoEcra.Televisao => 50,
            _ => 36
        };


        public static Font FonteData => _fonteData ??= ModoAtual switch
        {
            TipoEcra.Portatil => new Font("Roboto", 9),
            TipoEcra.MonitorFullHD => new Font("Roboto", 11),
            TipoEcra.Surface => new Font("Roboto", 12),
            TipoEcra.Televisao => new Font("Roboto", 24F),
            _ => new Font("Roboto", 9F)
        };

        public static void AplicarEstilos(Control pai)
        {
            AplicarEstilos(pai, AlturaCombos);
        }

        /// <summary>
        /// Percorre toda a árvore de controlos do painel pai e aplica fontes/tamanhos
        /// do tema atual. A lógica usa dois loops:
        /// 1º loop — processa os Guna2ComboBox para capturar o tamanho de referência
        ///            (os combos são o "calibrador" dos outros controlos no mesmo painel).
        /// 2º loop — aplica os estilos ao resto, usando o tamanho capturado onde aplicável.
        /// A recursividade garante que UCs aninhados dentro de painéis também são tratados.
        /// Botões com nome começando por "Nav" usam a fonte de nav; os restantes ("btn") usam a fonte standard.
        /// Labels: "lblNome" usam FonteNome; que contenha "Titulo" usam FonteTitulo; resto usa FonteLabel.
        /// </summary>
        public static void AplicarEstilos(Control pai, int alturaCombos)
        {
            Size tamanhoReferencia = Size.Empty;

            // 1º loop: trata os combos primeiro e guarda o seu tamanho para calibrar os outros
            foreach (Control c in pai.Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2ComboBox cmbRef)
                {
                    cmbRef.Font = FonteLabel;
                    cmbRef.ItemHeight = AlturaCombos;
                    tamanhoReferencia = cmbRef.Size;
                }
            }

            // 2º loop: aplica ao resto e entra em controlos filhos recursivamente
            foreach (Control c in pai.Controls)
            {
                if (c is Guna.UI2.WinForms.Guna2ComboBox) continue; // já tratado acima

                if (c is Guna.UI2.WinForms.Guna2DateTimePicker dtp)
                {
                    // O DateTimePicker fica com o mesmo tamanho do combo para alinhamento visual
                    dtp.Size = tamanhoReferencia;
                    dtp.Font = FonteData;
                }

                if (c is Guna.UI2.WinForms.Guna2Button btn)
                {
                    if (btn.Name.StartsWith("Nav"))
                    {
                        btn.Font = FonteBotaoNAV;
                        btn.CheckedState.Font = FonteBotaoNAV;
                        btn.Padding = PaddingBotao;
                        btn.ImageSize = TamanhoIconeBotao;
                    }
                    else if (btn.Name.Contains("btn"))
                    {
                        btn.Font = FonteBotao;
                        btn.Cursor = Cursors.Hand;
                    }
                }
                else if (c is Label lbl)
                {
                    if (lbl.Name == "lblMessage")
                    {
                        lbl.Font = FonteNome;
                    }
                    else
                    {
                        // Nome do controlo dita a fonte: lblNome → grande, Titulo → médio, resto → pequeno
                        lbl.Font = lbl.Name.Contains("lblNome") ? FonteNome :
                               lbl.Name.Contains("Titulo") ? FonteTitulo : FonteLabel;
                    }
                }
                else if (c is Guna.UI2.WinForms.Guna2TextBox txt)
                {
                    txt.Font = FonteLabel;
                    // TextBoxes relacionados com EPI ficam com o mesmo tamanho do combo para alinhamento
                    if (txt.Name.Contains("EPI") && tamanhoReferencia != Size.Empty)
                    {
                        txt.Size = tamanhoReferencia;
                    }
                }
                else if (c is PEPIDIDataGridView dgv)
                {
                    dgv.RowTemplate.Height = AlturaLinhaDgv;
                }

                if (c.HasChildren) AplicarEstilos(c);
            }
        }
    }
}