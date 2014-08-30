using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace RocksmithToTabGUI
{
    public partial class CallProgram : Form
    {
        public string RocksmithPath { get; set; }
        public string OutputPath { get; set; }
        public string FileNameTemplate { get; set; }
        public string FileFormat { get; set; }

        private Process process = null;
        private delegate void AddOutputDelegate(string output);
        private AddOutputDelegate addOutputDelegate;

        public CallProgram()
        {
            InitializeComponent();
            addOutputDelegate = new AddOutputDelegate(AddOutput);
        }


        protected override void OnClosed(EventArgs e)
        {
            if (process != null)
            {
                // if the RocksmithToTab process is still running, kill it
                if (!process.HasExited)
                    process.Kill();
                process.Dispose();
                process = null;
            }

            base.OnClosed(e);
        }


        protected override void OnShown(EventArgs e)
        {
            StartProcess();
            base.OnShown(e);
        }


        private void StartProcess()
        {
            // When the dialog is shown, start the RocksmithToTab process and
            // collect its output.
            string programPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "RocksmithToTab.exe");
            // DLC on Windows have _p suffix, on Mac it's _m
            string dlcFiles = "*_p.psarc";
            if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                dlcFiles = "*_m.psarc";
            // construct the argument that will convert all installed songs
            string filesToProcess = string.Format("\"{0}\" \"{1}\"", Path.Combine(RocksmithPath, "songs.psarc"),
                Path.Combine(RocksmithPath, "dlc", dlcFiles));
            process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = programPath,
                    Arguments = string.Format("-f {0} -o \"{1}\" -n \"{2}\" {3}", FileFormat, OutputPath, FileNameTemplate, filesToProcess)
                }
            };

            // register required event handlers
            process.OutputDataReceived += process_OutputDataReceived;
            process.ErrorDataReceived += process_OutputDataReceived;
            process.Exited += process_Exited;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }


        void process_Exited(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();            
        }


        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // add the received output from RocksmithToTab to the output textbox.
            // I'm delegating this to the UI thread, since the event handler will be called
            // from the background threads handling the output.
            try
            {
                this.Invoke(addOutputDelegate, e.Data);
            }
            catch (InvalidOperationException)
            {
                // happens if the dialog is closed while the process is still running, ignore
            }
        }


        void AddOutput(string output)
        {
            if (output == null)
                return;

            ConsoleOutput.AppendText(output);
            ConsoleOutput.AppendText("\n");

            // see if the line contains a progress report
            if (output.Length > 0 && (output[0] == '[' || output[0] == '('))
            {
                var words = output.Split(' ');
                var progress = words[0].Substring(1, words[0].Length - 2);
                var progressParts = progress.Split('/');
                int progressNom, progressDenom;
                if (int.TryParse(progressParts[0], out progressNom) && int.TryParse(progressParts[1], out progressDenom))
                {
                    if (output[0] == '[')
                    {
                        TotalProgress.Maximum = progressDenom;
                        TotalProgress.Value = progressNom;
                        // also get the name of the currently processed archive
                        var name = string.Join(" ", words.Skip(3).Take(words.Length - 4));
                        CurrentFileLabel.Text = "Converting " + name;
                    }
                    else
                    {
                        FileProgress.Maximum = progressDenom;
                        FileProgress.Value = progressNom;
                    }
                }
            }
        }
    }
}
