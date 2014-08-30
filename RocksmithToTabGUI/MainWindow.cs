using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RocksmithToTabGUI
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            // Ensure that the output format selection list starts with the default value
            OutputFormat.SelectedIndex = 0;
            // Set a default output location in the user's my documents folder
            string myDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outputPath = System.IO.Path.Combine(myDocuments, "RocksmithTabs");
            OutputFolder.Text = outputPath;
        }
    }
}
