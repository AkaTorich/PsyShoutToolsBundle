namespace TrycryperServer
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
            this.startServer = new System.Windows.Forms.Button();
            this.serverIp = new System.Windows.Forms.TextBox();
            this.serverIpLabel = new System.Windows.Forms.Label();
            this.serverPort = new System.Windows.Forms.TextBox();
            this.serverPortLabel = new System.Windows.Forms.Label();
            this.serverStatus = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // startServer
            // 
            this.startServer.Location = new System.Drawing.Point(1052, 36);
            this.startServer.Name = "startServer";
            this.startServer.Size = new System.Drawing.Size(182, 62);
            this.startServer.TabIndex = 0;
            this.startServer.Text = "Start Server";
            this.startServer.UseVisualStyleBackColor = true;
            this.startServer.Click += new System.EventHandler(this.startServer_Click);
            // 
            // serverIp
            // 
            this.serverIp.Location = new System.Drawing.Point(131, 52);
            this.serverIp.Name = "serverIp";
            this.serverIp.Size = new System.Drawing.Size(403, 31);
            this.serverIp.TabIndex = 1;
            this.serverIp.Text = "127.0.0.1";
            this.serverIp.TextChanged += new System.EventHandler(this.serverIp_TextChanged);
            // 
            // serverIpLabel
            // 
            this.serverIpLabel.AutoSize = true;
            this.serverIpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.serverIpLabel.Location = new System.Drawing.Point(12, 55);
            this.serverIpLabel.Name = "serverIpLabel";
            this.serverIpLabel.Size = new System.Drawing.Size(109, 25);
            this.serverIpLabel.TabIndex = 2;
            this.serverIpLabel.Text = "Server IP";
            // 
            // serverPort
            // 
            this.serverPort.Location = new System.Drawing.Point(745, 49);
            this.serverPort.Name = "serverPort";
            this.serverPort.Size = new System.Drawing.Size(177, 31);
            this.serverPort.TabIndex = 3;
            this.serverPort.Text = "8888";
            this.serverPort.TextChanged += new System.EventHandler(this.serverPort_TextChanged);
            // 
            // serverPortLabel
            // 
            this.serverPortLabel.AutoSize = true;
            this.serverPortLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.serverPortLabel.Location = new System.Drawing.Point(581, 55);
            this.serverPortLabel.Name = "serverPortLabel";
            this.serverPortLabel.Size = new System.Drawing.Size(131, 25);
            this.serverPortLabel.TabIndex = 4;
            this.serverPortLabel.Text = "Server Port";
            this.serverPortLabel.Click += new System.EventHandler(this.serverPortLabel_Click);
            // 
            // serverStatus
            // 
            this.serverStatus.Location = new System.Drawing.Point(19, 127);
            this.serverStatus.Name = "serverStatus";
            this.serverStatus.Size = new System.Drawing.Size(1214, 638);
            this.serverStatus.TabIndex = 5;
            this.serverStatus.Text = "Not connected...";
            this.serverStatus.TextChanged += new System.EventHandler(this.serverStatus_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.DarkRed;
            this.ClientSize = new System.Drawing.Size(1259, 779);
            this.Controls.Add(this.serverStatus);
            this.Controls.Add(this.serverPortLabel);
            this.Controls.Add(this.serverPort);
            this.Controls.Add(this.serverIpLabel);
            this.Controls.Add(this.serverIp);
            this.Controls.Add(this.startServer);
            this.Name = "Form1";
            this.Text = "TrycryperServer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startServer;
        private System.Windows.Forms.TextBox serverIp;
        private System.Windows.Forms.Label serverIpLabel;
        private System.Windows.Forms.TextBox serverPort;
        private System.Windows.Forms.Label serverPortLabel;
        private System.Windows.Forms.RichTextBox serverStatus;
    }
}

