using System;
using System.Windows.Forms;

namespace BackupManager.Forms
{
    partial class RestoreForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RestoreForm));
            this.differencesListView = new System.Windows.Forms.ListView();
            this.selectAllButton = new System.Windows.Forms.Button();
            this.restoreButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // differencesListView
            // 
            this.differencesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.differencesListView.CheckBoxes = true;
            this.differencesListView.FullRowSelect = true;
            this.differencesListView.GridLines = true;
            this.differencesListView.HideSelection = false;
            this.differencesListView.Location = new System.Drawing.Point(12, 12);
            this.differencesListView.Name = "differencesListView";
            this.differencesListView.Size = new System.Drawing.Size(760, 400);
            this.differencesListView.TabIndex = 0;
            this.differencesListView.UseCompatibleStateImageBehavior = false;
            this.differencesListView.View = System.Windows.Forms.View.Details;

            // Добавляем столбцы для списка различий
            this.differencesListView.Columns.Add("Файл", 350);
            this.differencesListView.Columns.Add("Статус", 150);
            this.differencesListView.Columns.Add("Размер (Источник)", 120);
            this.differencesListView.Columns.Add("Размер (Копия)", 120);
            // 
            // selectAllButton
            // 
            this.selectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selectAllButton.Location = new System.Drawing.Point(12, 418);
            this.selectAllButton.Name = "selectAllButton";
            this.selectAllButton.Size = new System.Drawing.Size(100, 30);
            this.selectAllButton.TabIndex = 1;
            this.selectAllButton.Text = "Выбрать все";
            this.selectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
            // 
            // restoreButton
            // 
            this.restoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.restoreButton.Location = new System.Drawing.Point(561, 418);
            this.restoreButton.Name = "restoreButton";
            this.restoreButton.Size = new System.Drawing.Size(100, 30);
            this.restoreButton.TabIndex = 2;
            this.restoreButton.Text = "Восстановить";
            this.restoreButton.Click += new System.EventHandler(this.RestoreButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(672, 418);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 30);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Закрыть";
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // RestoreForm
            // 
            this.ClientSize = new System.Drawing.Size(784, 456);
            this.Controls.Add(this.differencesListView);
            this.Controls.Add(this.selectAllButton);
            this.Controls.Add(this.restoreButton);
            this.Controls.Add(this.closeButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RestoreForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Восстановление файлов";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView differencesListView;
        private System.Windows.Forms.Button selectAllButton;
        private System.Windows.Forms.Button restoreButton;
        private System.Windows.Forms.Button closeButton;
    }
}