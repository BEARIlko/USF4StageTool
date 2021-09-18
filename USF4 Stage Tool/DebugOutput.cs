using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USF4_Stage_Tool
{
    public partial class DebugOutput : Form
    {

        private string DebugOutputFileName = "USF4 Stage Tool Debug Output.txt";
        public DebugOutput()
        {
            InitializeComponent();
        }

        public void AddLineToConsole(string line)
        {
            tbVisualConsole.Items.Add(line);
            ScrollToBottom();
        }

        private void BtnClearVC_Click(object sender, EventArgs e)
        {
            tbVisualConsole.Items.Clear();
        }

        private void BtnSaveVC_Click(object sender, EventArgs e)
        {
            string filepath;
            //saveFileDialogDebug.RestoreDirectory = true;
            saveFileDialogDebug.InitialDirectory = string.Empty;
            saveFileDialogDebug.FileName = DebugOutputFileName;
            saveFileDialogDebug.Filter = "Text File (.txt)|*.txt";
            //saveFileDialogDebug.ShowDialog();
            if (saveFileDialogDebug.ShowDialog() == DialogResult.OK)
            {
                filepath = saveFileDialogDebug.FileName;
                if (filepath.Trim() != "")
                {
                    System.IO.File.WriteAllLines(filepath, tbVisualConsole.Items.Cast<string>().ToArray());
                }
            }
        }

        private void DebugOutput_Shown(object sender, EventArgs e)
        {
            ScrollToBottom();
        }

        public void ScrollToBottom() { tbVisualConsole.TopIndex = tbVisualConsole.Items.Count - 1; }

        private void DebugOutput_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
