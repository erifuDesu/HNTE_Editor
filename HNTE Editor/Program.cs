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

            // Eðer komut satýrýndan bir .hnte dosyasý verildiyse açmayý dene
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
                        MessageBox.Show("Geçersiz dosya türü. Lütfen .hnte dosyasý verin.", "Uyarý",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dosya açýlamadý.\n\nDetay: {ex.Message}", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Application.Run(form);
        }
    }
}