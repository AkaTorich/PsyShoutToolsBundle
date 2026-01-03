using System;
using System.Windows.Forms;

namespace BackupManager.Forms
{
    partial class ScheduleListForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScheduleListForm));
            this.scheduledJobsListView = new System.Windows.Forms.ListView();
            this.addButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.runNowButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // scheduledJobsListView
            // 
            this.scheduledJobsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scheduledJobsListView.FullRowSelect = true;
            this.scheduledJobsListView.GridLines = true;
            this.scheduledJobsListView.HideSelection = false;
            this.scheduledJobsListView.Location = new System.Drawing.Point(12, 12);
            this.scheduledJobsListView.MultiSelect = false;
            this.scheduledJobsListView.Name = "scheduledJobsListView";
            this.scheduledJobsListView.Size = new System.Drawing.Size(860, 400);
            this.scheduledJobsListView.TabIndex = 0;
            this.scheduledJobsListView.UseCompatibleStateImageBehavior = false;
            this.scheduledJobsListView.View = System.Windows.Forms.View.Details;
            this.scheduledJobsListView.DoubleClick += new System.EventHandler(this.ScheduledJobsListView_DoubleClick);

            // Добавляем столбцы для списка заданий
            this.scheduledJobsListView.Columns.Add("Название", 150);
            this.scheduledJobsListView.Columns.Add("Расписание", 150);
            this.scheduledJobsListView.Columns.Add("Тип синхронизации", 150);
            this.scheduledJobsListView.Columns.Add("Исходная директория", 120);
            this.scheduledJobsListView.Columns.Add("Директория копии", 120);
            this.scheduledJobsListView.Columns.Add("Активно", 60);
            this.scheduledJobsListView.Columns.Add("Последний запуск", 120);
            this.scheduledJobsListView.Columns.Add("Статус", 60);
            // 
            // addButton
            // 
            this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addButton.Location = new System.Drawing.Point(12, 418);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(100, 30);
            this.addButton.TabIndex = 1;
            this.addButton.Text = "Добавить";
            this.addButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // editButton
            // 
            this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.editButton.Location = new System.Drawing.Point(122, 418);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(100, 30);
            this.editButton.TabIndex = 2;
            this.editButton.Text = "Редактировать";
            this.editButton.Click += new System.EventHandler(this.EditButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.removeButton.Location = new System.Drawing.Point(232, 418);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(100, 30);
            this.removeButton.TabIndex = 3;
            this.removeButton.Text = "Удалить";
            this.removeButton.Click += new System.EventHandler(this.RemoveButton_Click);
            // 
            // runNowButton
            // 
            this.runNowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.runNowButton.Location = new System.Drawing.Point(342, 418);
            this.runNowButton.Name = "runNowButton";
            this.runNowButton.Size = new System.Drawing.Size(120, 30);
            this.runNowButton.TabIndex = 4;
            this.runNowButton.Text = "Запустить сейчас";
            this.runNowButton.Click += new System.EventHandler(this.RunNowButton_Click);
            // 
            // refreshButton
            // 
            this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.refreshButton.Location = new System.Drawing.Point(472, 418);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(100, 30);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.Text = "Обновить";
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(772, 418);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 30);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "Закрыть";
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // ScheduleListForm
            // 
            this.ClientSize = new System.Drawing.Size(884, 456);
            this.Controls.Add(this.scheduledJobsListView);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.editButton);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.runNowButton);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.closeButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ScheduleListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Управление расписанием";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView scheduledJobsListView;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button runNowButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button closeButton;
    }
}