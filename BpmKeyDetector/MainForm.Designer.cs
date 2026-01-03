using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;

namespace BpmKeyDetector
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnSelectFolder = new MaterialSkin.Controls.MaterialButton();
            this.btnAnalyzeFiles = new MaterialSkin.Controls.MaterialButton();
            this.btnRenameFiles = new MaterialSkin.Controls.MaterialButton();
            this.btnFastRename = new MaterialSkin.Controls.MaterialButton();
            this.lblStatus = new MaterialSkin.Controls.MaterialLabel();
            this.progressBar = new MaterialSkin.Controls.MaterialProgressBar();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.columnFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnBPM = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new MaterialSkin.Controls.MaterialLabel();
            this.panelHeader.SuspendLayout();
            this.SuspendLayout();
            //
            // btnSelectFolder
            //
            this.btnSelectFolder.AutoSize = false;
            this.btnSelectFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSelectFolder.Depth = 0;
            this.btnSelectFolder.HighEmphasis = true;
            this.btnSelectFolder.Icon = null;
            this.btnSelectFolder.Location = new System.Drawing.Point(20, 90);
            this.btnSelectFolder.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnSelectFolder.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(180, 42);
            this.btnSelectFolder.TabIndex = 0;
            this.btnSelectFolder.Text = "ВЫБРАТЬ ПАПКУ";
            this.btnSelectFolder.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnSelectFolder.UseAccentColor = false;
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            //
            // btnAnalyzeFiles
            //
            this.btnAnalyzeFiles.AutoSize = false;
            this.btnAnalyzeFiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAnalyzeFiles.Depth = 0;
            this.btnAnalyzeFiles.Enabled = false;
            this.btnAnalyzeFiles.HighEmphasis = true;
            this.btnAnalyzeFiles.Icon = null;
            this.btnAnalyzeFiles.Location = new System.Drawing.Point(210, 90);
            this.btnAnalyzeFiles.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnAnalyzeFiles.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnAnalyzeFiles.Name = "btnAnalyzeFiles";
            this.btnAnalyzeFiles.Size = new System.Drawing.Size(180, 42);
            this.btnAnalyzeFiles.TabIndex = 1;
            this.btnAnalyzeFiles.Text = "АНАЛИЗИРОВАТЬ";
            this.btnAnalyzeFiles.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnAnalyzeFiles.UseAccentColor = true;
            this.btnAnalyzeFiles.UseVisualStyleBackColor = true;
            this.btnAnalyzeFiles.Click += new System.EventHandler(this.btnAnalyzeFiles_Click);
            //
            // btnRenameFiles
            //
            this.btnRenameFiles.AutoSize = false;
            this.btnRenameFiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRenameFiles.Depth = 0;
            this.btnRenameFiles.Enabled = false;
            this.btnRenameFiles.HighEmphasis = true;
            this.btnRenameFiles.Icon = null;
            this.btnRenameFiles.Location = new System.Drawing.Point(400, 90);
            this.btnRenameFiles.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRenameFiles.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRenameFiles.Name = "btnRenameFiles";
            this.btnRenameFiles.Size = new System.Drawing.Size(180, 42);
            this.btnRenameFiles.TabIndex = 2;
            this.btnRenameFiles.Text = "ПЕРЕИМ. ТАМЖЕ";
            this.btnRenameFiles.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnRenameFiles.UseAccentColor = false;
            this.btnRenameFiles.UseVisualStyleBackColor = true;
            this.btnRenameFiles.Click += new System.EventHandler(this.btnRenameFiles_Click);
            //
            // btnFastRename
            //
            this.btnFastRename.AutoSize = false;
            this.btnFastRename.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnFastRename.Depth = 0;
            this.btnFastRename.Enabled = false;
            this.btnFastRename.HighEmphasis = true;
            this.btnFastRename.Icon = null;
            this.btnFastRename.Location = new System.Drawing.Point(590, 90);
            this.btnFastRename.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnFastRename.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnFastRename.Name = "btnFastRename";
            this.btnFastRename.Size = new System.Drawing.Size(180, 42);
            this.btnFastRename.TabIndex = 3;
            this.btnFastRename.Text = "ПЕРЕИМ. В ПАПКУ";
            this.btnFastRename.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined;
            this.btnFastRename.UseAccentColor = false;
            this.btnFastRename.UseVisualStyleBackColor = true;
            this.btnFastRename.Click += new System.EventHandler(this.btnFastRename_Click);
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Depth = 0;
            this.lblStatus.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblStatus.Location = new System.Drawing.Point(20, 145);
            this.lblStatus.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(239, 19);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Выберите папку с аудиофайлами";
            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Depth = 0;
            this.progressBar.Location = new System.Drawing.Point(20, 175);
            this.progressBar.MouseState = MaterialSkin.MouseState.HOVER;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(750, 5);
            this.progressBar.TabIndex = 5;
            //
            // listViewFiles
            //
            this.listViewFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.listViewFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnFilename,
            this.columnBPM,
            this.columnKey,
            this.columnStatus});
            this.listViewFiles.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.listViewFiles.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(200)))));
            this.listViewFiles.FullRowSelect = true;
            this.listViewFiles.GridLines = true;
            this.listViewFiles.HideSelection = false;
            this.listViewFiles.Location = new System.Drawing.Point(20, 195);
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.OwnerDraw = true;
            this.listViewFiles.Size = new System.Drawing.Size(750, 330);
            this.listViewFiles.TabIndex = 6;
            this.listViewFiles.UseCompatibleStateImageBehavior = false;
            this.listViewFiles.View = System.Windows.Forms.View.Details;
            this.listViewFiles.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listViewFiles_DrawColumnHeader);
            this.listViewFiles.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listViewFiles_DrawItem);
            this.listViewFiles.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listViewFiles_DrawSubItem);
            //
            // columnFilename
            //
            this.columnFilename.Text = "ИМЯ ФАЙЛА";
            this.columnFilename.Width = 320;
            //
            // columnBPM
            //
            this.columnBPM.Text = "BPM";
            this.columnBPM.Width = 80;
            //
            // columnKey
            //
            this.columnKey.Text = "ТОНАЛЬНОСТЬ";
            this.columnKey.Width = 120;
            //
            // columnStatus
            //
            this.columnStatus.Text = "СТАТУС";
            this.columnStatus.Width = 230;
            //
            // bgWorker
            //
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            //
            // panelHeader
            //
            this.panelHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Location = new System.Drawing.Point(0, 64);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(800, 1);
            this.panelHeader.TabIndex = 7;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Depth = 0;
            this.lblTitle.Font = new System.Drawing.Font("Roboto", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.lblTitle.FontType = MaterialSkin.MaterialSkinManager.fontType.H5;
            this.lblTitle.Location = new System.Drawing.Point(15, 10);
            this.lblTitle.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(219, 29);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "BPM & KEY DETECTOR";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(800, 550);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.listViewFiles);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnFastRename);
            this.Controls.Add(this.btnRenameFiles);
            this.Controls.Add(this.btnAnalyzeFiles);
            this.Controls.Add(this.btnSelectFolder);
            this.DrawerShowIconsWhenHidden = true;
            this.DrawerTabControl = null;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(800, 550);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(0, 64, 0, 0);
            this.Sizable = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BPM & Key Detector";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MaterialButton btnSelectFolder;
        private MaterialButton btnAnalyzeFiles;
        private MaterialButton btnRenameFiles;
        private MaterialButton btnFastRename;
        private MaterialLabel lblStatus;
        private MaterialProgressBar progressBar;
        private ListView listViewFiles;
        private ColumnHeader columnFilename;
        private ColumnHeader columnBPM;
        private ColumnHeader columnKey;
        private ColumnHeader columnStatus;
        private BackgroundWorker bgWorker;
        private Panel panelHeader;
        private MaterialLabel lblTitle;
    }
}
