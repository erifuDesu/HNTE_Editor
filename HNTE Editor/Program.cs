using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HNTE_Editor
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ApplicationConfiguration.Initialize();
            var form = new Form1();

            // E�er komut sat�r�ndan bir .hnte dosyas� verildiyse a�may� dene
            if (args is { Length: > 0 } && File.Exists(args[0]))
            {
                try
                {
                    if (Path.GetExtension(args[0]).Equals(".hnte", StringComparison.OrdinalIgnoreCase))
                    {
                        string text = HnteCodec.LoadHnte(args[0]);
                        form.SetEditorText(text);
                    }
                    else
                    {
                        MessageBox.Show("Ge�ersiz dosya t�r�. L�tfen .hnte dosyas� verin.", "Uyar�",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosya a��lamad�.\n\nDetay: {ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Application.Run(form);
        }
    }
}