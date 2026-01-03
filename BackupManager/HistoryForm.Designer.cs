using System;
using System.Windows.Forms;

namespace BackupManager.Forms
{
    partial class HistoryForm
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
        void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HistoryForm));
            this.historyListView = new System.Windows.Forms.ListView();
            this.refreshButton = new System.Windows.Forms.Button();
            this.clearHistoryButton = new System.Windows.Forms.Button();
            this.viewLogsButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // historyListView
            // 
            this.historyListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.historyListView.FullRowSelect = true;
            this.historyListView.GridLines = true;
            this.historyListView.HideSelection = false;
            this.historyListView.Location = new System.Drawing.Point(12, 12);
            this.historyListView.MultiSelect = false;
            this.historyListView.Name = "historyListView";
            this.historyListView.Size = new System.Drawing.Size(760, 412);
            this.historyListView.TabIndex = 0;
            this.historyListView.UseCompatibleStateImageBehavior = false;
            this.historyListView.View = System.Windows.Forms.View.Details;

            // Добавляем столбцы для списка истории
            this.historyListView.Columns.Add("Исходная директория", 180);
            this.historyListView.Columns.Add("Директория копии", 180);
            this.historyListView.Columns.Add("Время создания", 140);
            this.historyListView.Columns.Add("Время завершения", 140);
            this.historyListView.Columns.Add("Статус", 90);
            // 
            // refreshButton
            // 
            this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.refreshButton.Location = new System.Drawing.Point(16, 430);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(100, 30);
            this.refreshButton.TabIndex = 1;
            this.refreshButton.Text = "Обновить";
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // clearHistoryButton
            // 
            this.clearHistoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.clearHistoryButton.Location = new System.Drawing.Point(122, 430);
            this.clearHistoryButton.Name = "clearHistoryButton";
            this.clearHistoryButton.Size = new System.Drawing.Size(120, 30);
            this.clearHistoryButton.TabIndex = 2;
            this.clearHistoryButton.Text = "Очистить историю";
            this.clearHistoryButton.Click += new System.EventHandler(this.ClearHistoryButton_Click);
            // 
            // viewLogsButton
            // 
            this.viewLogsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.viewLogsButton.Location = new System.Drawing.Point(248, 430);
            this.viewLogsButton.Name = "viewLogsButton";
            this.viewLogsButton.Size = new System.Drawing.Size(120, 30);
            this.viewLogsButton.TabIndex = 3;
            this.viewLogsButton.Text = "Просмотр журналов";
            this.viewLogsButton.Click += new System.EventHandler(this.ViewLogsButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(672, 430);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 30);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Закрыть";
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // HistoryForm
            // 
            this.ClientSize = new System.Drawing.Size(784, 468);
            this.Controls.Add(this.historyListView);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.clearHistoryButton);
            this.Controls.Add(this.viewLogsButton);
            this.Controls.Add(this.closeButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HistoryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "История резервного копирования";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView historyListView;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button clearHistoryButton;
        private System.Windows.Forms.Button viewLogsButton;
        private System.Windows.Forms.Button closeButton;
    }
}