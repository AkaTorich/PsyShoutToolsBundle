using System;
using System.Windows.Forms;

namespace BackupManager.Forms
{
    partial class CompareForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompareForm));
            this.differencesListView = new System.Windows.Forms.ListView();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // differencesListView
            // 
            this.differencesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(688, 418);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 30);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "Закрыть";
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // CompareForm
            // 
            this.ClientSize = new System.Drawing.Size(784, 456);
            this.Controls.Add(this.differencesListView);
            this.Controls.Add(this.closeButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CompareForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Результаты сравнения";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView differencesListView;
        private System.Windows.Forms.Button closeButton;
    }
}