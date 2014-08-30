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

            // Try to determine the Rocksmith installation directory
            string rocksmithPath = RocksmithLocator.Rocksmith2014Folder();
            if (rocksmithPath != null)
                RocksmithFolder.Text = rocksmithPath;

            // Set a default output location in the user's my documents folder
            string myDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outputPath = System.IO.Path.Combine(myDocuments, "RocksmithTabs");
            OutputFolder.Text = outputPath;
        }

        private void RocksmithFolderSelect_Click(object sender, EventArgs e)
        {
            // let the user manually select the Rocksmith folder, if it wasn't found by default
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where Rocksmith 2014 is installed";
                dialog.ShowNewFolderButton = false;
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                if (dialog.ShowDialog() == DialogResult.OK)
                    RocksmithFolder.Text = dialog.SelectedPath;
            }
        }

        private void OutputFolderSelect_Click(object sender, EventArgs e)
        {
            // let the user select the output folder
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the folder where the generated tabs should be saved";
                dialog.ShowNewFolderButton = true;
                dialog.RootFolder = Environment.SpecialFolder.MyDocuments;
                if (dialog.ShowDialog() == DialogResult.OK)
                    OutputFolder.Text = dialog.SelectedPath;
            }

        }

        private void CreateTabs_Click(object sender, EventArgs e)
        {
            string[] fileFormats = new string[] { "gp5", "gpx", "gpif" };
            using (var callProgram = new CallProgram())
            {
                callProgram.RocksmithPath = RocksmithFolder.Text;
                callProgram.OutputPath = OutputFolder.Text;
                callProgram.FileNameTemplate = FileNameTemplate.Text;
                callProgram.FileFormat = fileFormats[OutputFormat.SelectedIndex];

                callProgram.ShowDialog();
            }
        }
    }
}
