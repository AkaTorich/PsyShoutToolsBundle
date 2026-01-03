using System;
using System.Windows.Forms;
using System.Drawing;

namespace BackupManager.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.directoriesGroup = new System.Windows.Forms.GroupBox();
            this.sourceLabel = new System.Windows.Forms.Label();
            this.sourceDirTextBox = new System.Windows.Forms.TextBox();
            this.browseSourceButton = new System.Windows.Forms.Button();
            this.backupLabel = new System.Windows.Forms.Label();
            this.backupDirTextBox = new System.Windows.Forms.TextBox();
            this.browseBackupButton = new System.Windows.Forms.Button();
            this.actionsGroup = new System.Windows.Forms.GroupBox();
            this.createBackupButton = new System.Windows.Forms.Button();
            this.compareButton = new System.Windows.Forms.Button();
            this.restoreButton = new System.Windows.Forms.Button();
            this.scheduleButton = new System.Windows.Forms.Button();
            this.historyButton = new System.Windows.Forms.Button();
            this.clearLogsButton = new System.Windows.Forms.Button();
            this.logGroup = new System.Windows.Forms.GroupBox();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.autoStartCheckBox = new System.Windows.Forms.CheckBox();
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createBackupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.separatorMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.directoriesGroup.SuspendLayout();
            this.actionsGroup.SuspendLayout();
            this.logGroup.SuspendLayout();
            this.trayContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // directoriesGroup
            // 
            this.directoriesGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.directoriesGroup.Controls.Add(this.sourceLabel);
            this.directoriesGroup.Controls.Add(this.sourceDirTextBox);
            this.directoriesGroup.Controls.Add(this.browseSourceButton);
            this.directoriesGroup.Controls.Add(this.backupLabel);
            this.directoriesGroup.Controls.Add(this.backupDirTextBox);
            this.directoriesGroup.Controls.Add(this.browseBackupButton);
            this.directoriesGroup.Location = new System.Drawing.Point(12, 12);
            this.directoriesGroup.Name = "directoriesGroup";
            this.directoriesGroup.Size = new System.Drawing.Size(776, 100);
            this.directoriesGroup.TabIndex = 0;
            this.directoriesGroup.TabStop = false;
            this.directoriesGroup.Text = "Директории";
            // 
            // sourceLabel
            // 
            this.sourceLabel.AutoSize = true;
            this.sourceLabel.Location = new System.Drawing.Point(10, 25);
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Size = new System.Drawing.Size(121, 13);
            this.sourceLabel.TabIndex = 0;
            this.sourceLabel.Text = "Исходная директория:";
            // 
            // sourceDirTextBox
            // 
            this.sourceDirTextBox.Location = new System.Drawing.Point(150, 22);
            this.sourceDirTextBox.Name = "sourceDirTextBox";
            this.sourceDirTextBox.ReadOnly = true;
            this.sourceDirTextBox.Size = new System.Drawing.Size(500, 20);
            this.sourceDirTextBox.TabIndex = 1;
            // 
            // browseSourceButton
            // 
            this.browseSourceButton.Location = new System.Drawing.Point(660, 19);
            this.browseSourceButton.Name = "browseSourceButton";
            this.browseSourceButton.Size = new System.Drawing.Size(100, 25);
            this.browseSourceButton.TabIndex = 2;
            this.browseSourceButton.Text = "Обзор...";
            this.browseSourceButton.Click += new System.EventHandler(this.BrowseSourceButton_Click);
            // 
            // backupLabel
            // 
            this.backupLabel.AutoSize = true;
            this.backupLabel.Location = new System.Drawing.Point(10, 60);
            this.backupLabel.Name = "backupLabel";
            this.backupLabel.Size = new System.Drawing.Size(105, 13);
            this.backupLabel.TabIndex = 3;
            this.backupLabel.Text = "Директория копии:";
            // 
            // backupDirTextBox
            // 
            this.backupDirTextBox.Location = new System.Drawing.Point(150, 57);
            this.backupDirTextBox.Name = "backupDirTextBox";
            this.backupDirTextBox.ReadOnly = true;
            this.backupDirTextBox.Size = new System.Drawing.Size(500, 20);
            this.backupDirTextBox.TabIndex = 4;
            // 
            // browseBackupButton
            // 
            this.browseBackupButton.Location = new System.Drawing.Point(660, 57);
            this.browseBackupButton.Name = "browseBackupButton";
            this.browseBackupButton.Size = new System.Drawing.Size(100, 25);
            this.browseBackupButton.TabIndex = 5;
            this.browseBackupButton.Text = "Обзор...";
            this.browseBackupButton.Click += new System.EventHandler(this.BrowseBackupButton_Click);
            // 
            // actionsGroup
            // 
            this.actionsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.actionsGroup.Controls.Add(this.createBackupButton);
            this.actionsGroup.Controls.Add(this.compareButton);
            this.actionsGroup.Controls.Add(this.restoreButton);
            this.actionsGroup.Controls.Add(this.scheduleButton);
            this.actionsGroup.Controls.Add(this.historyButton);
            this.actionsGroup.Controls.Add(this.clearLogsButton);
            this.actionsGroup.Location = new System.Drawing.Point(12, 118);
            this.actionsGroup.Name = "actionsGroup";
            this.actionsGroup.Size = new System.Drawing.Size(776, 60);
            this.actionsGroup.TabIndex = 1;
            this.actionsGroup.TabStop = false;
            this.actionsGroup.Text = "Действия";
            // 
            // createBackupButton
            // 
            this.createBackupButton.Location = new System.Drawing.Point(10, 22);
            this.createBackupButton.Name = "createBackupButton";
            this.createBackupButton.Size = new System.Drawing.Size(100, 25);
            this.createBackupButton.TabIndex = 0;
            this.createBackupButton.Text = "Создать копию";
            this.createBackupButton.Click += new System.EventHandler(this.CreateBackupButton_Click);
            // 
            // compareButton
            // 
            this.compareButton.Location = new System.Drawing.Point(120, 22);
            this.compareButton.Name = "compareButton";
            this.compareButton.Size = new System.Drawing.Size(100, 25);
            this.compareButton.TabIndex = 1;
            this.compareButton.Text = "Сравнить";
            this.compareButton.Click += new System.EventHandler(this.CompareButton_Click);
            // 
            // restoreButton
            // 
            this.restoreButton.Location = new System.Drawing.Point(230, 22);
            this.restoreButton.Name = "restoreButton";
            this.restoreButton.Size = new System.Drawing.Size(100, 25);
            this.restoreButton.TabIndex = 2;
            this.restoreButton.Text = "Восстановить";
            this.restoreButton.Click += new System.EventHandler(this.RestoreButton_Click);
            // 
            // scheduleButton
            // 
            this.scheduleButton.Location = new System.Drawing.Point(340, 22);
            this.scheduleButton.Name = "scheduleButton";
            this.scheduleButton.Size = new System.Drawing.Size(100, 25);
            this.scheduleButton.TabIndex = 3;
            this.scheduleButton.Text = "Расписание";
            this.scheduleButton.Click += new System.EventHandler(this.ScheduleButton_Click);
            // 
            // historyButton
            // 
            this.historyButton.Location = new System.Drawing.Point(450, 22);
            this.historyButton.Name = "historyButton";
            this.historyButton.Size = new System.Drawing.Size(100, 25);
            this.historyButton.TabIndex = 4;
            this.historyButton.Text = "История";
            this.historyButton.Click += new System.EventHandler(this.HistoryButton_Click);
            // 
            // clearLogsButton
            // 
            this.clearLogsButton.Location = new System.Drawing.Point(660, 22);
            this.clearLogsButton.Name = "clearLogsButton";
            this.clearLogsButton.Size = new System.Drawing.Size(100, 25);
            this.clearLogsButton.TabIndex = 5;
            this.clearLogsButton.Text = "Очистить логи";
            this.clearLogsButton.Click += new System.EventHandler(this.ClearLogsButton_Click);
            // 
            // logGroup
            // 
            this.logGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logGroup.Controls.Add(this.logTextBox);
            this.logGroup.Controls.Add(this.autoStartCheckBox);
            this.logGroup.Location = new System.Drawing.Point(12, 184);
            this.logGroup.Name = "logGroup";
            this.logGroup.Size = new System.Drawing.Size(776, 350);
            this.logGroup.TabIndex = 2;
            this.logGroup.TabStop = false;
            this.logGroup.Text = "Журнал операций";
            // 
            // logTextBox
            // 
            this.logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTextBox.BackColor = System.Drawing.Color.White;
            this.logTextBox.Location = new System.Drawing.Point(6, 22);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(754, 322);
            this.logTextBox.TabIndex = 0;
            this.logTextBox.Text = "";
            // 
            // autoStartCheckBox
            // 
            this.autoStartCheckBox.Location = new System.Drawing.Point(560, -4);
            this.autoStartCheckBox.Name = "autoStartCheckBox";
            this.autoStartCheckBox.Size = new System.Drawing.Size(200, 20);
            this.autoStartCheckBox.TabIndex = 3;
            this.autoStartCheckBox.Text = "Запускать при старте Windows";
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.trayContextMenu;
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.trayIcon.Text = "Backup Manager";
            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += new System.EventHandler(this.TrayIcon_DoubleClick);
            // 
            // trayContextMenu
            // 
            this.trayContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMenuItem,
            this.createBackupMenuItem,
            this.separatorMenuItem,
            this.exitMenuItem});
            this.trayContextMenu.Name = "trayContextMenu";
            this.trayContextMenu.Size = new System.Drawing.Size(158, 76);
            // 
            // openMenuItem
            // 
            this.openMenuItem.Name = "openMenuItem";
            this.openMenuItem.Size = new System.Drawing.Size(157, 22);
            this.openMenuItem.Text = "Открыть";
            this.openMenuItem.Click += new System.EventHandler(this.OpenMenuItem_Click);
            // 
            // createBackupMenuItem
            // 
            this.createBackupMenuItem.Name = "createBackupMenuItem";
            this.createBackupMenuItem.Size = new System.Drawing.Size(157, 22);
            this.createBackupMenuItem.Text = "Создать копию";
            this.createBackupMenuItem.Click += new System.EventHandler(this.CreateBackupMenuItem_Click);
            // 
            // separatorMenuItem
            // 
            this.separatorMenuItem.Name = "separatorMenuItem";
            this.separatorMenuItem.Size = new System.Drawing.Size(154, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(157, 22);
            this.exitMenuItem.Text = "Выход";
            this.exitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(784, 546);
            this.Controls.Add(this.directoriesGroup);
            this.Controls.Add(this.actionsGroup);
            this.Controls.Add(this.logGroup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Backup Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.directoriesGroup.ResumeLayout(false);
            this.directoriesGroup.PerformLayout();
            this.actionsGroup.ResumeLayout(false);
            this.logGroup.ResumeLayout(false);
            this.trayContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        // И не забудьте объявить переменную:
        private CheckBox autoStartCheckBox;
        private TextBox sourceDirTextBox;
        private TextBox backupDirTextBox;
        private RichTextBox logTextBox;
        private GroupBox actionsGroup;
        private Button scheduleButton;
        private Button historyButton;
        private GroupBox directoriesGroup;
        private Label sourceLabel;
        private Button browseSourceButton;
        private Label backupLabel;
        private Button browseBackupButton;
        private Button createBackupButton;
        private Button compareButton;
        private Button restoreButton;
        private Button clearLogsButton;
        private GroupBox logGroup;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayContextMenu;
        private ToolStripMenuItem openMenuItem;
        private ToolStripMenuItem createBackupMenuItem;
        private ToolStripSeparator separatorMenuItem;
        private ToolStripMenuItem exitMenuItem;
    }
}