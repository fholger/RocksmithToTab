using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

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
            // Restore values from stored app settings
            var settings = RocksmithToTabGUI.Properties.Settings.Default;
            OutputFormat.SelectedIndex = settings.OutputFormat;
            IncrementalGeneration.SelectedIndex = settings.IncrementalGeneration;
            // Note: the text fields are automatically bound to the app settings

            if (String.IsNullOrEmpty(RocksmithFolder.Text))
            {
                // Try to determine the Rocksmith installation directory automatically
                string rocksmithPath = RocksmithLocator.Rocksmith2014Folder();
                if (rocksmithPath != null)
                    RocksmithFolder.Text = rocksmithPath;
                else
                    MessageBox.Show("I could not determine your Rocksmith 2014 installation directory. Please enter the location manually.", "Rocksmith 2014 not found");
            }

            if (String.IsNullOrEmpty(OutputFolder.Text))
            {
                // Set a default output location in the user's my documents folder
                string myDocuments = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string outputPath = System.IO.Path.Combine(myDocuments, "RocksmithTabs");
                OutputFolder.Text = outputPath;
            }
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Save current settings in user config file for next start
            var settings = RocksmithToTabGUI.Properties.Settings.Default;
            settings.OutputFormat = OutputFormat.SelectedIndex;
            settings.IncrementalGeneration = IncrementalGeneration.SelectedIndex;
            // Note: the text fields are automatically bound to the app settings

            settings.Save();
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
            // sanity check: test if given Rocksmith folder is legit
            string songsPsarc = Path.Combine(RocksmithFolder.Text, "songs.psarc");
            if (!File.Exists(songsPsarc))
            {
                MessageBox.Show("Could not find songs.psarc file in the Rocksmith folder. Are you sure you provided the correct path to your Rocksmith 2014 installation directory?", "Rocksmith 2014 path invalid");
                return;
            }

            string[] fileFormats = new string[] { "gp5", "gpx", "gpif" };
            using (var callProgram = new CallProgram())
            {
                callProgram.RocksmithPath = RocksmithFolder.Text;
                callProgram.OutputPath = OutputFolder.Text;
                callProgram.FileNameTemplate = FileNameTemplate.Text;
                callProgram.FileFormat = fileFormats[OutputFormat.SelectedIndex];
                callProgram.OnlyNewFiles = (IncrementalGeneration.SelectedIndex == 1);

                callProgram.ShowDialog();
            }
        }
    }
}
