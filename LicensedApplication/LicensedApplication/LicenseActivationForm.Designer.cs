namespace LicensedApplication
{
    partial class LicenseActivationForm
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.groupBoxActivation = new System.Windows.Forms.GroupBox();
            this.btnActivate = new System.Windows.Forms.Button();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.txtLicenseKey = new System.Windows.Forms.TextBox();
            this.lblLicenseKey = new System.Windows.Forms.Label();
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            this.btnCopy = new System.Windows.Forms.Button();
            this.txtHardwareId = new System.Windows.Forms.TextBox();
            this.lblHardwareId = new System.Windows.Forms.Label();
            this.btnDeactivate = new System.Windows.Forms.Button();
            this.txtLicenseInfo = new System.Windows.Forms.TextBox();
            this.lblLicenseInfo = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBoxActivation.SuspendLayout();
            this.groupBoxInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblStatus.Location = new System.Drawing.Point(12, 9);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(118, 16);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Статус лицензии";
            // 
            // groupBoxActivation
            // 
            this.groupBoxActivation.Controls.Add(this.btnActivate);
            this.groupBoxActivation.Controls.Add(this.txtEmail);
            this.groupBoxActivation.Controls.Add(this.lblEmail);
            this.groupBoxActivation.Controls.Add(this.txtUserName);
            this.groupBoxActivation.Controls.Add(this.lblUserName);
            this.groupBoxActivation.Controls.Add(this.txtLicenseKey);
            this.groupBoxActivation.Controls.Add(this.lblLicenseKey);
            this.groupBoxActivation.Location = new System.Drawing.Point(12, 35);
            this.groupBoxActivation.Name = "groupBoxActivation";
            this.groupBoxActivation.Size = new System.Drawing.Size(440, 167);
            this.groupBoxActivation.TabIndex = 1;
            this.groupBoxActivation.TabStop = false;
            this.groupBoxActivation.Text = "Активация лицензии";
            // 
            // btnActivate
            // 
            this.btnActivate.Location = new System.Drawing.Point(152, 127);
            this.btnActivate.Name = "btnActivate";
            this.btnActivate.Size = new System.Drawing.Size(126, 28);
            this.btnActivate.TabIndex = 6;
            this.btnActivate.Text = "Активировать";
            this.btnActivate.UseVisualStyleBackColor = true;
            this.btnActivate.Click += new System.EventHandler(this.btnActivate_Click);
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(128, 91);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(278, 20);
            this.txtEmail.TabIndex = 5;
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new System.Drawing.Point(17, 94);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(35, 13);
            this.lblEmail.TabIndex = 4;
            this.lblEmail.Text = "Email:";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(128, 59);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(278, 20);
            this.txtUserName.TabIndex = 3;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(17, 62);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(106, 13);
            this.lblUserName.TabIndex = 2;
            this.lblUserName.Text = "Имя пользователя:";
            // 
            // txtLicenseKey
            // 
            this.txtLicenseKey.Location = new System.Drawing.Point(128, 28);
            this.txtLicenseKey.Name = "txtLicenseKey";
            this.txtLicenseKey.Size = new System.Drawing.Size(278, 20);
            this.txtLicenseKey.TabIndex = 1;
            // 
            // lblLicenseKey
            // 
            this.lblLicenseKey.AutoSize = true;
            this.lblLicenseKey.Location = new System.Drawing.Point(17, 31);
            this.lblLicenseKey.Name = "lblLicenseKey";
            this.lblLicenseKey.Size = new System.Drawing.Size(105, 13);
            this.lblLicenseKey.TabIndex = 0;
            this.lblLicenseKey.Text = "Лицензионный ключ:";
            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Controls.Add(this.btnCopy);
            this.groupBoxInfo.Controls.Add(this.txtHardwareId);
            this.groupBoxInfo.Controls.Add(this.lblHardwareId);
            this.groupBoxInfo.Controls.Add(this.btnDeactivate);
            this.groupBoxInfo.Controls.Add(this.txtLicenseInfo);
            this.groupBoxInfo.Controls.Add(this.lblLicenseInfo);
            this.groupBoxInfo.Location = new System.Drawing.Point(12, 208);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Size = new System.Drawing.Size(440, 183);
            this.groupBoxInfo.TabIndex = 2;
            this.groupBoxInfo.TabStop = false;
            this.groupBoxInfo.Text = "Информация о лицензии";
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(358, 147);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(48, 23);
            this.btnCopy.TabIndex = 5;
            this.btnCopy.Text = "Копировать";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // txtHardwareId
            // 
            this.txtHardwareId.Location = new System.Drawing.Point(128, 148);
            this.txtHardwareId.Name = "txtHardwareId";
            this.txtHardwareId.ReadOnly = true;
            this.txtHardwareId.Size = new System.Drawing.Size(224, 20);
            this.txtHardwareId.TabIndex = 4;
            // 
            // lblHardwareId
            // 
            this.lblHardwareId.AutoSize = true;
            this.lblHardwareId.Location = new System.Drawing.Point(17, 151);
            this.lblHardwareId.Name = "lblHardwareId";
            this.lblHardwareId.Size = new System.Drawing.Size(74, 13);
            this.lblHardwareId.TabIndex = 3;
            this.lblHardwareId.Text = "Аппаратный ID:";
            // 
            // btnDeactivate
            // 
            this.btnDeactivate.Location = new System.Drawing.Point(152, 109);
            this.btnDeactivate.Name = "btnDeactivate";
            this.btnDeactivate.Size = new System.Drawing.Size(126, 23);
            this.btnDeactivate.TabIndex = 2;
            this.btnDeactivate.Text = "Деактивировать";
            this.btnDeactivate.UseVisualStyleBackColor = true;
            this.btnDeactivate.Click += new System.EventHandler(this.btnDeactivate_Click);
            // 
            // txtLicenseInfo
            // 
            this.txtLicenseInfo.Location = new System.Drawing.Point(128, 19);
            this.txtLicenseInfo.Multiline = true;
            this.txtLicenseInfo.Name = "txtLicenseInfo";
            this.txtLicenseInfo.ReadOnly = true;
            this.txtLicenseInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLicenseInfo.Size = new System.Drawing.Size(278, 84);
            this.txtLicenseInfo.TabIndex = 1;
            // 
            // lblLicenseInfo
            // 
            this.lblLicenseInfo.AutoSize = true;
            this.lblLicenseInfo.Location = new System.Drawing.Point(17, 22);
            this.lblLicenseInfo.Name = "lblLicenseInfo";
            this.lblLicenseInfo.Size = new System.Drawing.Size(67, 13);
            this.lblLicenseInfo.TabIndex = 0;
            this.lblLicenseInfo.Text = "Информация:";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(164, 397);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(126, 28);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Закрыть";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // LicenseActivationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 436);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBoxInfo);
            this.Controls.Add(this.groupBoxActivation);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LicenseActivationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Активация лицензии";
            this.Load += new System.EventHandler(this.LicenseActivationForm_Load);
            this.groupBoxActivation.ResumeLayout(false);
            this.groupBoxActivation.PerformLayout();
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox groupBoxActivation;
        private System.Windows.Forms.Button btnActivate;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtLicenseKey;
        private System.Windows.Forms.Label lblLicenseKey;
        private System.Windows.Forms.GroupBox groupBoxInfo;
        private System.Windows.Forms.Button btnDeactivate;
        private System.Windows.Forms.TextBox txtLicenseInfo;
        private System.Windows.Forms.Label lblLicenseInfo;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.TextBox txtHardwareId;
        private System.Windows.Forms.Label lblHardwareId;
    }
}