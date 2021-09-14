
namespace DfuFwUpdater
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tboxFwFilePath = new System.Windows.Forms.TextBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.tboxLog = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnSearchDevices = new System.Windows.Forms.Button();
            this.cbxDevices = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkBoxVerify = new System.Windows.Forms.CheckBox();
            this.chkBoxEraseChip = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnAbout = new System.Windows.Forms.Button();
            this.btnQuit = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tboxFwFilePath
            // 
            this.tboxFwFilePath.Location = new System.Drawing.Point(47, 20);
            this.tboxFwFilePath.Name = "tboxFwFilePath";
            this.tboxFwFilePath.Size = new System.Drawing.Size(323, 21);
            this.tboxFwFilePath.TabIndex = 0;
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(376, 18);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(34, 23);
            this.btnSelectFile.TabIndex = 1;
            this.btnSelectFile.Text = "...";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // tboxLog
            // 
            this.tboxLog.Location = new System.Drawing.Point(6, 20);
            this.tboxLog.Multiline = true;
            this.tboxLog.Name = "tboxLog";
            this.tboxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tboxLog.Size = new System.Drawing.Size(404, 135);
            this.tboxLog.TabIndex = 2;
            this.tboxLog.WordWrap = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSearchDevices);
            this.groupBox1.Controls.Add(this.cbxDevices);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(416, 55);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Available DFU Devices";
            // 
            // btnSearchDevices
            // 
            this.btnSearchDevices.Location = new System.Drawing.Point(346, 18);
            this.btnSearchDevices.Name = "btnSearchDevices";
            this.btnSearchDevices.Size = new System.Drawing.Size(64, 23);
            this.btnSearchDevices.TabIndex = 8;
            this.btnSearchDevices.Text = "Search";
            this.btnSearchDevices.UseVisualStyleBackColor = true;
            this.btnSearchDevices.Click += new System.EventHandler(this.btnSearchDevices_Click);
            // 
            // cbxDevices
            // 
            this.cbxDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDevices.FormattingEnabled = true;
            this.cbxDevices.Location = new System.Drawing.Point(6, 20);
            this.cbxDevices.Name = "cbxDevices";
            this.cbxDevices.Size = new System.Drawing.Size(334, 20);
            this.cbxDevices.TabIndex = 7;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkBoxVerify);
            this.groupBox3.Controls.Add(this.chkBoxEraseChip);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.btnDownload);
            this.groupBox3.Controls.Add(this.tboxFwFilePath);
            this.groupBox3.Controls.Add(this.btnSelectFile);
            this.groupBox3.Location = new System.Drawing.Point(12, 73);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(416, 86);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Download Action";
            // 
            // chkBoxVerify
            // 
            this.chkBoxVerify.AutoSize = true;
            this.chkBoxVerify.Checked = true;
            this.chkBoxVerify.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxVerify.Location = new System.Drawing.Point(129, 56);
            this.chkBoxVerify.Name = "chkBoxVerify";
            this.chkBoxVerify.Size = new System.Drawing.Size(60, 16);
            this.chkBoxVerify.TabIndex = 8;
            this.chkBoxVerify.Text = "Verify";
            this.chkBoxVerify.UseVisualStyleBackColor = true;
            // 
            // chkBoxEraseChip
            // 
            this.chkBoxEraseChip.AutoSize = true;
            this.chkBoxEraseChip.Checked = true;
            this.chkBoxEraseChip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxEraseChip.Location = new System.Drawing.Point(8, 56);
            this.chkBoxEraseChip.Name = "chkBoxEraseChip";
            this.chkBoxEraseChip.Size = new System.Drawing.Size(84, 16);
            this.chkBoxEraseChip.TabIndex = 7;
            this.chkBoxEraseChip.Text = "Erase Chip";
            this.chkBoxEraseChip.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "File:";
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(287, 52);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(123, 23);
            this.btnDownload.TabIndex = 2;
            this.btnDownload.Text = "Start Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.progressBar1);
            this.groupBox2.Controls.Add(this.tboxLog);
            this.groupBox2.Location = new System.Drawing.Point(12, 165);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(416, 190);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Log";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(6, 161);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(404, 16);
            this.progressBar1.TabIndex = 7;
            // 
            // btnAbout
            // 
            this.btnAbout.Location = new System.Drawing.Point(12, 362);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(75, 23);
            this.btnAbout.TabIndex = 7;
            this.btnAbout.Text = "&About";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // btnQuit
            // 
            this.btnQuit.Location = new System.Drawing.Point(353, 362);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(75, 23);
            this.btnQuit.TabIndex = 8;
            this.btnQuit.Text = "&Quit";
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 395);
            this.Controls.Add(this.btnQuit);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "DfuFwUpdater - Westberry Technology";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tboxFwFilePath;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.TextBox tboxLog;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cbxDevices;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnSearchDevices;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox chkBoxVerify;
        private System.Windows.Forms.CheckBox chkBoxEraseChip;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Button btnQuit;
    }
}

