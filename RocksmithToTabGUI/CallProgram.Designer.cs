namespace RocksmithToTabGUI
{
    partial class CallProgram
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CallProgram));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.TotalProgress = new System.Windows.Forms.ProgressBar();
            this.FileProgress = new System.Windows.Forms.ProgressBar();
            this.CurrentFileLabel = new System.Windows.Forms.Label();
            this.CancelProcess = new System.Windows.Forms.Button();
            this.ConsoleOutput = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.TotalProgress, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.FileProgress, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.CurrentFileLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.CancelProcess, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.ConsoleOutput, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(470, 324);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // TotalProgress
            // 
            this.TotalProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TotalProgress.Location = new System.Drawing.Point(6, 6);
            this.TotalProgress.Name = "TotalProgress";
            this.TotalProgress.Size = new System.Drawing.Size(458, 23);
            this.TotalProgress.TabIndex = 0;
            // 
            // FileProgress
            // 
            this.FileProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.FileProgress.Location = new System.Drawing.Point(6, 48);
            this.FileProgress.Name = "FileProgress";
            this.FileProgress.Size = new System.Drawing.Size(458, 23);
            this.FileProgress.TabIndex = 1;
            // 
            // CurrentFileLabel
            // 
            this.CurrentFileLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CurrentFileLabel.AutoSize = true;
            this.CurrentFileLabel.Location = new System.Drawing.Point(6, 32);
            this.CurrentFileLabel.Name = "CurrentFileLabel";
            this.CurrentFileLabel.Size = new System.Drawing.Size(131, 13);
            this.CurrentFileLabel.TabIndex = 2;
            this.CurrentFileLabel.Text = "Processing songs.psarc ...";
            // 
            // CancelProcess
            // 
            this.CancelProcess.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.CancelProcess.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelProcess.Location = new System.Drawing.Point(197, 294);
            this.CancelProcess.Name = "CancelProcess";
            this.CancelProcess.Size = new System.Drawing.Size(75, 23);
            this.CancelProcess.TabIndex = 4;
            this.CancelProcess.Text = "Cancel";
            this.CancelProcess.UseVisualStyleBackColor = true;
            // 
            // ConsoleOutput
            // 
            this.ConsoleOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsoleOutput.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConsoleOutput.Location = new System.Drawing.Point(6, 77);
            this.ConsoleOutput.Multiline = true;
            this.ConsoleOutput.Name = "ConsoleOutput";
            this.ConsoleOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ConsoleOutput.Size = new System.Drawing.Size(458, 211);
            this.ConsoleOutput.TabIndex = 5;
            // 
            // CallProgram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelProcess;
            this.ClientSize = new System.Drawing.Size(470, 324);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CallProgram";
            this.Text = "Converting tabs...";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ProgressBar TotalProgress;
        private System.Windows.Forms.ProgressBar FileProgress;
        private System.Windows.Forms.Label CurrentFileLabel;
        private System.Windows.Forms.Button CancelProcess;
        private System.Windows.Forms.TextBox ConsoleOutput;
    }
}