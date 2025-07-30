namespace WordDiffMerger
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblOriginal;
        private System.Windows.Forms.TextBox txtOriginalFile;
        private System.Windows.Forms.Button btnSelectOriginal;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtChangedFolder;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.ListBox lstChangedFiles;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.Button btnMerge;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblOriginal = new System.Windows.Forms.Label();
            this.txtOriginalFile = new System.Windows.Forms.TextBox();
            this.btnSelectOriginal = new System.Windows.Forms.Button();
            this.lblFolder = new System.Windows.Forms.Label();
            this.txtChangedFolder = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.lstChangedFiles = new System.Windows.Forms.ListBox();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.btnMerge = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblOriginal
            // 
            this.lblOriginal.AutoSize = true;
            this.lblOriginal.Location = new System.Drawing.Point(12, 15);
            this.lblOriginal.Name = "lblOriginal";
            this.lblOriginal.Size = new System.Drawing.Size(108, 13);
            this.lblOriginal.TabIndex = 0;
            this.lblOriginal.Text = "Файл-оригинал:";
            // 
            // txtOriginalFile
            // 
            this.txtOriginalFile.Location = new System.Drawing.Point(130, 12);
            this.txtOriginalFile.Name = "txtOriginalFile";
            this.txtOriginalFile.Size = new System.Drawing.Size(300, 20);
            this.txtOriginalFile.TabIndex = 1;
            // 
            // btnSelectOriginal
            // 
            this.btnSelectOriginal.Location = new System.Drawing.Point(440, 10);
            this.btnSelectOriginal.Name = "btnSelectOriginal";
            this.btnSelectOriginal.Size = new System.Drawing.Size(40, 23);
            this.btnSelectOriginal.TabIndex = 2;
            this.btnSelectOriginal.Text = "...";
            this.btnSelectOriginal.UseVisualStyleBackColor = true;
            this.btnSelectOriginal.Click += new System.EventHandler(this.btnSelectOriginal_Click);
            // 
            // lblFolder
            // 
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(12, 45);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(112, 13);
            this.lblFolder.TabIndex = 3;
            this.lblFolder.Text = "Папка с файлами:";
            // 
            // txtChangedFolder
            // 
            this.txtChangedFolder.Location = new System.Drawing.Point(130, 42);
            this.txtChangedFolder.Name = "txtChangedFolder";
            this.txtChangedFolder.Size = new System.Drawing.Size(300, 20);
            this.txtChangedFolder.TabIndex = 4;
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(440, 40);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(40, 23);
            this.btnSelectFolder.TabIndex = 5;
            this.btnSelectFolder.Text = "...";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // lstChangedFiles
            // 
            this.lstChangedFiles.FormattingEnabled = true;
            this.lstChangedFiles.Location = new System.Drawing.Point(15, 70);
            this.lstChangedFiles.Name = "lstChangedFiles";
            this.lstChangedFiles.Size = new System.Drawing.Size(465, 95);
            this.lstChangedFiles.TabIndex = 6;
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Location = new System.Drawing.Point(15, 180);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(120, 30);
            this.btnAnalyze.TabIndex = 7;
            this.btnAnalyze.Text = "Анализировать";
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(360, 180);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(120, 30);
            this.btnMerge.TabIndex = 8;
            this.btnMerge.Text = "Применить";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(500, 230);
            this.Controls.Add(this.btnMerge);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.lstChangedFiles);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtChangedFolder);
            this.Controls.Add(this.lblFolder);
            this.Controls.Add(this.btnSelectOriginal);
            this.Controls.Add(this.txtOriginalFile);
            this.Controls.Add(this.lblOriginal);
            this.Name = "MainForm";
            this.Text = "WordDiffMerger";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}