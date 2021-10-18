using System;
using System.IO;
using System.Windows.Forms;

namespace Jtex
{
    public partial class MainForm : Form
    {
        public MainForm()
            => InitializeComponent();

        public MainForm(string path)
        {
            InitializeComponent();
            OpenFile(path);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (Directory.Exists(files[0])) // allow folder
                files = Directory.GetFiles(files[0], "*.*", SearchOption.TopDirectoryOnly);
            foreach (string f in files)
                OpenFile(f);
        }
        private void OpenFile(string path)
        {
            // Handle file
            try
            {
                if (!File.Exists(path))
                    throw new Exception("Can only accept files, not folders.");
                displayBox.Image = Jupiter.MakeBmp(path, autoSaveInput.Checked, false);
            }
            catch (Exception) { System.Media.SystemSounds.Asterisk.Play(); }
        }
    }
}
