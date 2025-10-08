using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HNTE_Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitEditor();
        }

        private void InitEditor()
        {
            textBox1.AcceptsTab = true;
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.WordWrap = false;
            textBox1.Font = new System.Drawing.Font("Consolas", 11f);
            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;
        }

        public void SetEditorText(string text)
        {
            textBox1.Text = text ?? string.Empty;
        }

        private void Form1_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".hnte", StringComparison.OrdinalIgnoreCase))
                    e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object? sender, DragEventArgs e)
        {
            try
            {
                if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                    if (files.Length == 1 && Path.GetExtension(files[0]).Equals(".hnte", StringComparison.OrdinalIgnoreCase))
                    {
                        string text = HnteCodec.LoadHnte(files[0]);
                        SetEditorText(text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dosya açýlamadý.\n\nDetay: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "HNTE dosyasý aç",
                Filter = "HNTE Files (*.hnte)|*.hnte",
                CheckFileExists = true,
                Multiselect = false
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                string text = HnteCodec.LoadHnte(dlg.FileName);
                SetEditorText(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Dosya açýlamadý.\n\nDetay: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "HNTE olarak kaydet",
                Filter = "HNTE Files (*.hnte)|*.hnte",
                AddExtension = true,
                DefaultExt = "hnte",
                OverwritePrompt = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                HnteCodec.SaveHnte(dlg.FileName, textBox1.Text);
                MessageBox.Show(this, "Kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Kaydedilemedi.\n\nDetay: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}