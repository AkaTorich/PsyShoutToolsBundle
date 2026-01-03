using System.Windows.Forms;

namespace MailClient
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.accountSelector = new System.Windows.Forms.ComboBox();
            this.folderTreeView = new System.Windows.Forms.TreeView();
            this.emailListView = new System.Windows.Forms.ListView();
            this.emailWebBrowser = new System.Windows.Forms.WebBrowser();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.addAccountButton = new System.Windows.Forms.Button();
            this.deleteAccountButton = new System.Windows.Forms.Button();
            this.sendEmailButton = new System.Windows.Forms.Button();
            this.deleteEmailButton = new System.Windows.Forms.Button();
            this.downloadAttachmentsButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // accountSelector
            // 
            this.accountSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.accountSelector.Location = new System.Drawing.Point(10, 10);
            this.accountSelector.Name = "accountSelector";
            this.accountSelector.Size = new System.Drawing.Size(200, 21);
            this.accountSelector.TabIndex = 0;
            this.accountSelector.SelectedIndexChanged += new System.EventHandler(this.AccountSelector_SelectedIndexChanged);
            // 
            // folderTreeView
            // 
            this.folderTreeView.Location = new System.Drawing.Point(10, 37);
            this.folderTreeView.Name = "folderTreeView";
            this.folderTreeView.Size = new System.Drawing.Size(200, 527);
            this.folderTreeView.TabIndex = 1;
            this.folderTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FolderTreeView_AfterSelect);
            // 
            // emailListView
            // 


            // emailListView
            this.emailListView.FullRowSelect = true;
            this.emailListView.HideSelection = false;
            this.emailListView.Location = new System.Drawing.Point(220, 37);
            this.emailListView.Name = "emailListView";
            this.emailListView.Size = new System.Drawing.Size(779, 213);
            this.emailListView.TabIndex = 2;
            this.emailListView.UseCompatibleStateImageBehavior = false;
            this.emailListView.View = System.Windows.Forms.View.Details;
            this.emailListView.Columns.Add("От кого", 200, HorizontalAlignment.Left);
            this.emailListView.Columns.Add("Тема", 300, HorizontalAlignment.Left);
            this.emailListView.Columns.Add("Дата", 150, HorizontalAlignment.Left);
            this.emailListView.Columns.Add("Вложения", 100, HorizontalAlignment.Center);
            this.emailListView.SelectedIndexChanged += new System.EventHandler(this.DisplayEmailContent);

            // 
            // emailWebBrowser
            // 
            this.emailWebBrowser.Location = new System.Drawing.Point(220, 260);
            this.emailWebBrowser.Name = "emailWebBrowser";
            this.emailWebBrowser.Size = new System.Drawing.Size(779, 304);
            this.emailWebBrowser.TabIndex = 3;
            // 
            // searchBox
            // 
            this.searchBox.Location = new System.Drawing.Point(578, 10);
            this.searchBox.Name = "searchBox";
            this.searchBox.Size = new System.Drawing.Size(319, 20);
            this.searchBox.TabIndex = 4;
            // 
            // searchButton
            // 
            this.searchButton.Location = new System.Drawing.Point(903, 8);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(96, 23);
            this.searchButton.TabIndex = 5;
            this.searchButton.Text = "Поиск";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.SearchEmails);
            // 
            // addAccountButton
            // 
            this.addAccountButton.Location = new System.Drawing.Point(10, 570);
            this.addAccountButton.Name = "addAccountButton";
            this.addAccountButton.Size = new System.Drawing.Size(109, 23);
            this.addAccountButton.TabIndex = 6;
            this.addAccountButton.Text = "Добавить аккаунт";
            this.addAccountButton.UseVisualStyleBackColor = true;
            this.addAccountButton.Click += new System.EventHandler(this.ShowAddAccountForm);
            // 
            // deleteAccountButton
            // 
            this.deleteAccountButton.Location = new System.Drawing.Point(125, 570);
            this.deleteAccountButton.Name = "deleteAccountButton";
            this.deleteAccountButton.Size = new System.Drawing.Size(104, 23);
            this.deleteAccountButton.TabIndex = 7;
            this.deleteAccountButton.Text = "Удалить аккаунт";
            this.deleteAccountButton.UseVisualStyleBackColor = true;
            this.deleteAccountButton.Click += new System.EventHandler(this.DeleteAccount);
            // 
            // sendEmailButton
            // 
            this.sendEmailButton.Location = new System.Drawing.Point(591, 571);
            this.sendEmailButton.Name = "sendEmailButton";
            this.sendEmailButton.Size = new System.Drawing.Size(89, 23);
            this.sendEmailButton.TabIndex = 8;
            this.sendEmailButton.Text = "Новое письмо";
            this.sendEmailButton.UseVisualStyleBackColor = true;
            this.sendEmailButton.Click += new System.EventHandler(this.ShowSendEmailForm);
            // 
            // deleteEmailButton
            // 
            this.deleteEmailButton.Location = new System.Drawing.Point(686, 571);
            this.deleteEmailButton.Name = "deleteEmailButton";
            this.deleteEmailButton.Size = new System.Drawing.Size(101, 23);
            this.deleteEmailButton.TabIndex = 9;
            this.deleteEmailButton.Text = "Удалить письмо";
            this.deleteEmailButton.UseVisualStyleBackColor = true;
            this.deleteEmailButton.Click += new System.EventHandler(this.DeleteEmailAsync);
            // 
            // downloadAttachmentsButton
            // 
            this.downloadAttachmentsButton.Location = new System.Drawing.Point(793, 570);
            this.downloadAttachmentsButton.Name = "downloadAttachmentsButton";
            this.downloadAttachmentsButton.Size = new System.Drawing.Size(97, 23);
            this.downloadAttachmentsButton.TabIndex = 10;
            this.downloadAttachmentsButton.Text = "Скачать вложения";
            this.downloadAttachmentsButton.UseVisualStyleBackColor = true;
            this.downloadAttachmentsButton.Click += new System.EventHandler(this.DownloadAttachments);
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(896, 570);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(103, 23);
            this.refreshButton.TabIndex = 11;
            this.refreshButton.Text = "Обновить";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1011, 605);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.downloadAttachmentsButton);
            this.Controls.Add(this.deleteEmailButton);
            this.Controls.Add(this.sendEmailButton);
            this.Controls.Add(this.deleteAccountButton);
            this.Controls.Add(this.addAccountButton);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.searchBox);
            this.Controls.Add(this.emailWebBrowser);
            this.Controls.Add(this.emailListView);
            this.Controls.Add(this.folderTreeView);
            this.Controls.Add(this.accountSelector);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Mail Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ComboBox accountSelector;
        private System.Windows.Forms.TreeView folderTreeView;
        private System.Windows.Forms.ListView emailListView;
        private System.Windows.Forms.WebBrowser emailWebBrowser;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.Button addAccountButton;
        private System.Windows.Forms.Button deleteAccountButton;
        private System.Windows.Forms.Button sendEmailButton;
        private System.Windows.Forms.Button deleteEmailButton;
        private System.Windows.Forms.Button downloadAttachmentsButton;
        private System.Windows.Forms.Button refreshButton;
    }
}