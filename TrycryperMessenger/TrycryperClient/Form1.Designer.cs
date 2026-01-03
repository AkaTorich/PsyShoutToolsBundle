// Form1.Designer.cs
namespace TrycryperClient
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.serverIpLabel = new System.Windows.Forms.Label();
            this.serverIp = new System.Windows.Forms.TextBox();
            this.serverPortLabel = new System.Windows.Forms.Label();
            this.serverPort = new System.Windows.Forms.TextBox();
            this.nickName = new System.Windows.Forms.Label();
            this.nickNameTextBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.clientStatus = new System.Windows.Forms.RichTextBox();
            this.messageRichTextBox = new System.Windows.Forms.RichTextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.emojiPanel = new System.Windows.Forms.Panel();
            this.emojiToggleButton = new System.Windows.Forms.Button();
            this.selectFileButton = new System.Windows.Forms.Button();
            this.sendFileButton = new System.Windows.Forms.Button();
            this.selectedFileLabel = new System.Windows.Forms.Label();
            this.fileProgressBar = new System.Windows.Forms.ProgressBar();
            this.fileProgressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // serverIpLabel
            // 
            this.serverIpLabel.AutoSize = true;
            this.serverIpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.serverIpLabel.Location = new System.Drawing.Point(6, 20);
            this.serverIpLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.serverIpLabel.Name = "serverIpLabel";
            this.serverIpLabel.Size = new System.Drawing.Size(59, 13);
            this.serverIpLabel.TabIndex = 0;
            this.serverIpLabel.Text = "Server Ip";
            this.serverIpLabel.Click += new System.EventHandler(this.serverIpLabel_Click);
            // 
            // serverIp
            // 
            this.serverIp.Location = new System.Drawing.Point(62, 17);
            this.serverIp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.serverIp.Name = "serverIp";
            this.serverIp.Size = new System.Drawing.Size(146, 20);
            this.serverIp.TabIndex = 1;
            this.serverIp.Text = "127.0.0.1";
            this.serverIp.TextChanged += new System.EventHandler(this.serverIp_TextChanged);
            // 
            // serverPortLabel
            // 
            this.serverPortLabel.AutoSize = true;
            this.serverPortLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.serverPortLabel.Location = new System.Drawing.Point(220, 20);
            this.serverPortLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.serverPortLabel.Name = "serverPortLabel";
            this.serverPortLabel.Size = new System.Drawing.Size(71, 13);
            this.serverPortLabel.TabIndex = 2;
            this.serverPortLabel.Text = "Server Port";
            this.serverPortLabel.Click += new System.EventHandler(this.serverPortLabel_Click);
            // 
            // serverPort
            // 
            this.serverPort.Location = new System.Drawing.Point(298, 18);
            this.serverPort.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.serverPort.Name = "serverPort";
            this.serverPort.Size = new System.Drawing.Size(61, 20);
            this.serverPort.TabIndex = 3;
            this.serverPort.Text = "8888";
            this.serverPort.TextChanged += new System.EventHandler(this.serverPort_TextChanged);
            // 
            // nickName
            // 
            this.nickName.AutoSize = true;
            this.nickName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nickName.Location = new System.Drawing.Point(375, 20);
            this.nickName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.nickName.Name = "nickName";
            this.nickName.Size = new System.Drawing.Size(65, 13);
            this.nickName.TabIndex = 4;
            this.nickName.Text = "NickName";
            this.nickName.Click += new System.EventHandler(this.nickName_Click);
            // 
            // nickNameTextBox
            // 
            this.nickNameTextBox.Location = new System.Drawing.Point(444, 17);
            this.nickNameTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nickNameTextBox.Name = "nickNameTextBox";
            this.nickNameTextBox.Size = new System.Drawing.Size(126, 20);
            this.nickNameTextBox.TabIndex = 5;
            this.nickNameTextBox.Text = "anon";
            this.nickNameTextBox.TextChanged += new System.EventHandler(this.nickNameTextBox_TextChanged);
            // 
            // connectButton
            // 
            this.connectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.connectButton.Location = new System.Drawing.Point(674, 12);
            this.connectButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(117, 29);
            this.connectButton.TabIndex = 6;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // clientStatus
            // 
            this.clientStatus.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clientStatus.Location = new System.Drawing.Point(4, 44);
            this.clientStatus.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.clientStatus.Name = "clientStatus";
            this.clientStatus.Size = new System.Drawing.Size(788, 426);
            this.clientStatus.TabIndex = 11;
            this.clientStatus.Text = "Not connected...";
            // 
            // messageRichTextBox
            // 
            this.messageRichTextBox.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageRichTextBox.Location = new System.Drawing.Point(4, 471);
            this.messageRichTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.messageRichTextBox.Name = "messageRichTextBox";
            this.messageRichTextBox.Size = new System.Drawing.Size(629, 63);
            this.messageRichTextBox.TabIndex = 10;
            this.messageRichTextBox.Text = "Type message...";
            this.messageRichTextBox.TextChanged += new System.EventHandler(this.messageRichTextBox_TextChanged);
            this.messageRichTextBox.Enter += new System.EventHandler(this.MessageRichTextBox_Enter);
            // 
            // sendButton
            // 
            this.sendButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.sendButton.Location = new System.Drawing.Point(692, 471);
            this.sendButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(99, 61);
            this.sendButton.TabIndex = 9;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // emojiPanel
            // 
            this.emojiPanel.AutoScroll = true;
            this.emojiPanel.BackColor = System.Drawing.Color.White;
            this.emojiPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.emojiPanel.Location = new System.Drawing.Point(4, 312);
            this.emojiPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.emojiPanel.Name = "emojiPanel";
            this.emojiPanel.Size = new System.Drawing.Size(652, 157);
            this.emojiPanel.TabIndex = 0;
            this.emojiPanel.Visible = false;
            // 
            // emojiToggleButton
            // 
            this.emojiToggleButton.Font = new System.Drawing.Font("Segoe UI Emoji", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.emojiToggleButton.Location = new System.Drawing.Point(637, 471);
            this.emojiToggleButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.emojiToggleButton.Name = "emojiToggleButton";
            this.emojiToggleButton.Size = new System.Drawing.Size(50, 61);
            this.emojiToggleButton.TabIndex = 1;
            this.emojiToggleButton.Text = "😊";
            this.emojiToggleButton.Click += new System.EventHandler(this.EmojiToggleButton_Click);
            // 
            // selectFileButton
            // 
            this.selectFileButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.selectFileButton.Location = new System.Drawing.Point(4, 558);
            this.selectFileButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectFileButton.Name = "selectFileButton";
            this.selectFileButton.Size = new System.Drawing.Size(120, 30);
            this.selectFileButton.TabIndex = 12;
            this.selectFileButton.Text = "Select File";
            this.selectFileButton.UseVisualStyleBackColor = true;
            this.selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
            // 
            // sendFileButton
            // 
            this.sendFileButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.sendFileButton.Location = new System.Drawing.Point(670, 558);
            this.sendFileButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sendFileButton.Name = "sendFileButton";
            this.sendFileButton.Size = new System.Drawing.Size(120, 30);
            this.sendFileButton.TabIndex = 13;
            this.sendFileButton.Text = "Send File";
            this.sendFileButton.UseVisualStyleBackColor = true;
            this.sendFileButton.Enabled = false;
            this.sendFileButton.Click += new System.EventHandler(this.sendFileButton_Click);
            // 
            // selectedFileLabel
            // 
            this.selectedFileLabel.AutoSize = true;
            this.selectedFileLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.selectedFileLabel.ForeColor = System.Drawing.Color.White;
            this.selectedFileLabel.Location = new System.Drawing.Point(130, 565);
            this.selectedFileLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.selectedFileLabel.Name = "selectedFileLabel";
            this.selectedFileLabel.Size = new System.Drawing.Size(91, 13);
            this.selectedFileLabel.TabIndex = 14;
            this.selectedFileLabel.Text = "No file selected";
            // 
            // fileProgressBar
            // 
            this.fileProgressBar.Location = new System.Drawing.Point(4, 538);
            this.fileProgressBar.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.fileProgressBar.Name = "fileProgressBar";
            this.fileProgressBar.Size = new System.Drawing.Size(695, 15);
            this.fileProgressBar.TabIndex = 15;
            this.fileProgressBar.Visible = false;
            // 
            // fileProgressLabel
            // 
            this.fileProgressLabel.AutoSize = true;
            this.fileProgressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.fileProgressLabel.ForeColor = System.Drawing.Color.White;
            this.fileProgressLabel.Location = new System.Drawing.Point(705, 540);
            this.fileProgressLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.fileProgressLabel.Name = "fileProgressLabel";
            this.fileProgressLabel.Size = new System.Drawing.Size(24, 13);
            this.fileProgressLabel.TabIndex = 16;
            this.fileProgressLabel.Text = "0%";
            this.fileProgressLabel.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkGreen;
            this.ClientSize = new System.Drawing.Size(801, 595);
            this.Controls.Add(this.emojiPanel);
            this.Controls.Add(this.emojiToggleButton);
            this.Controls.Add(this.fileProgressLabel);
            this.Controls.Add(this.fileProgressBar);
            this.Controls.Add(this.selectedFileLabel);
            this.Controls.Add(this.sendFileButton);
            this.Controls.Add(this.selectFileButton);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.messageRichTextBox);
            this.Controls.Add(this.clientStatus);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.nickNameTextBox);
            this.Controls.Add(this.nickName);
            this.Controls.Add(this.serverPort);
            this.Controls.Add(this.serverPortLabel);
            this.Controls.Add(this.serverIp);
            this.Controls.Add(this.serverIpLabel);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "TrycryperClient";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label serverIpLabel;
        private System.Windows.Forms.TextBox serverIp;
        private System.Windows.Forms.Label serverPortLabel;
        private System.Windows.Forms.TextBox serverPort;
        private System.Windows.Forms.Label nickName;
        private System.Windows.Forms.TextBox nickNameTextBox;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.RichTextBox clientStatus;
        private System.Windows.Forms.RichTextBox messageRichTextBox;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Panel emojiPanel;
        private System.Windows.Forms.Button emojiToggleButton;
        private System.Windows.Forms.Button selectFileButton;
        private System.Windows.Forms.Button sendFileButton;
        private System.Windows.Forms.Label selectedFileLabel;
        private System.Windows.Forms.ProgressBar fileProgressBar;
        private System.Windows.Forms.Label fileProgressLabel;
    }
}