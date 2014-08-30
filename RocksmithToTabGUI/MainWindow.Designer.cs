namespace RocksmithToTabGUI
{
    partial class MainWindow
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.RocksmithFolder = new System.Windows.Forms.TextBox();
            this.RocksmithFolderSelect = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.OutputFolder = new System.Windows.Forms.TextBox();
            this.OutputFolderSelect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.OutputFormat = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.FileNameTemplate = new System.Windows.Forms.TextBox();
            this.CreateTabs = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.RocksmithFolder, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.RocksmithFolderSelect, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.OutputFolder, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.OutputFolderSelect, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.OutputFormat, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.FileNameTemplate, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.CreateTabs, 0, 8);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.RowCount = 9;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(533, 277);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rocksmith 2014 directory:";
            // 
            // RocksmithFolder
            // 
            this.RocksmithFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.RocksmithFolder.Location = new System.Drawing.Point(5, 37);
            this.RocksmithFolder.Name = "RocksmithFolder";
            this.RocksmithFolder.Size = new System.Drawing.Size(493, 20);
            this.RocksmithFolder.TabIndex = 1;
            // 
            // RocksmithFolderSelect
            // 
            this.RocksmithFolderSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.RocksmithFolderSelect.Location = new System.Drawing.Point(504, 35);
            this.RocksmithFolderSelect.Name = "RocksmithFolderSelect";
            this.RocksmithFolderSelect.Size = new System.Drawing.Size(24, 23);
            this.RocksmithFolderSelect.TabIndex = 2;
            this.RocksmithFolderSelect.Text = "...";
            this.RocksmithFolderSelect.UseVisualStyleBackColor = true;
            this.RocksmithFolderSelect.Click += new System.EventHandler(this.RocksmithFolderSelect_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(224, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Where do you want to save the created tabs?";
            // 
            // OutputFolder
            // 
            this.OutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFolder.Location = new System.Drawing.Point(5, 97);
            this.OutputFolder.Name = "OutputFolder";
            this.OutputFolder.Size = new System.Drawing.Size(493, 20);
            this.OutputFolder.TabIndex = 4;
            // 
            // OutputFolderSelect
            // 
            this.OutputFolderSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFolderSelect.Location = new System.Drawing.Point(504, 95);
            this.OutputFolderSelect.Name = "OutputFolderSelect";
            this.OutputFolderSelect.Size = new System.Drawing.Size(24, 23);
            this.OutputFolderSelect.TabIndex = 5;
            this.OutputFolderSelect.Text = "...";
            this.OutputFolderSelect.UseVisualStyleBackColor = true;
            this.OutputFolderSelect.Click += new System.EventHandler(this.OutputFolderSelect_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(172, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Please select the output file format:";
            // 
            // OutputFormat
            // 
            this.OutputFormat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OutputFormat.FormattingEnabled = true;
            this.OutputFormat.Items.AddRange(new object[] {
            ".gp5 (Guitar Pro 5 / TuxGuitar)",
            ".gpx (Guitar Pro 6)",
            ".gpif (Guitar Pro 6)"});
            this.OutputFormat.Location = new System.Drawing.Point(5, 156);
            this.OutputFormat.Name = "OutputFormat";
            this.OutputFormat.Size = new System.Drawing.Size(493, 21);
            this.OutputFormat.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 190);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(471, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Filename template for generated tabs. If you don\'t know what this does, leave it " +
    "at its default value!";
            // 
            // FileNameTemplate
            // 
            this.FileNameTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.FileNameTemplate.Location = new System.Drawing.Point(5, 217);
            this.FileNameTemplate.Name = "FileNameTemplate";
            this.FileNameTemplate.Size = new System.Drawing.Size(493, 20);
            this.FileNameTemplate.TabIndex = 9;
            this.FileNameTemplate.Text = "{artist} - {title}";
            // 
            // CreateTabs
            // 
            this.CreateTabs.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.CreateTabs.Location = new System.Drawing.Point(214, 245);
            this.CreateTabs.Name = "CreateTabs";
            this.CreateTabs.Size = new System.Drawing.Size(75, 23);
            this.CreateTabs.TabIndex = 10;
            this.CreateTabs.Text = "Create tabs!";
            this.CreateTabs.UseVisualStyleBackColor = true;
            this.CreateTabs.Click += new System.EventHandler(this.CreateTabs_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 277);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainWindow";
            this.Text = "Rocksmith To Tab Converter";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox RocksmithFolder;
        private System.Windows.Forms.Button RocksmithFolderSelect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox OutputFolder;
        private System.Windows.Forms.Button OutputFolderSelect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox OutputFormat;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FileNameTemplate;
        private System.Windows.Forms.Button CreateTabs;
    }
}

