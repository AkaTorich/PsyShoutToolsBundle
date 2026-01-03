// File: NoteWindow.Designer.cs
namespace NoteTray
{
    partial class NoteWindow
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
            this.controlPanel = new System.Windows.Forms.Panel();
            this.pinDesktopButton = new System.Windows.Forms.Button();
            this.pinTopMostButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.controlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlPanel
            // 
            this.controlPanel.BackColor = System.Drawing.Color.Gold;
            this.controlPanel.Controls.Add(this.pinDesktopButton);
            this.controlPanel.Controls.Add(this.pinTopMostButton);
            this.controlPanel.Controls.Add(this.closeButton);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.controlPanel.Location = new System.Drawing.Point(0, 0);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(300, 30);
            this.controlPanel.TabIndex = 0;
            this.controlPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ControlPanel_MouseDown);
            // 
            // pinDesktopButton
            // 
            this.pinDesktopButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pinDesktopButton.Location = new System.Drawing.Point(258, 2);
            this.pinDesktopButton.Name = "pinDesktopButton";
            this.pinDesktopButton.Size = new System.Drawing.Size(30, 25);
            this.pinDesktopButton.TabIndex = 3;
            this.pinDesktopButton.Text = "🏠";
            this.pinDesktopButton.UseVisualStyleBackColor = true;
            this.pinDesktopButton.Click += new System.EventHandler(this.PinDesktopButton_Click);
            // 
            // pinTopMostButton
            // 
            this.pinTopMostButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.pinTopMostButton.Location = new System.Drawing.Point(222, 2);
            this.pinTopMostButton.Name = "pinTopMostButton";
            this.pinTopMostButton.Size = new System.Drawing.Size(30, 25);
            this.pinTopMostButton.TabIndex = 2;
            this.pinTopMostButton.Text = "📌";
            this.pinTopMostButton.UseVisualStyleBackColor = true;
            this.pinTopMostButton.Click += new System.EventHandler(this.PinTopMostButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Location = new System.Drawing.Point(5, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(30, 25);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "✕";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // textBox
            // 
            this.textBox.BackColor = System.Drawing.Color.LightYellow;
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox.Location = new System.Drawing.Point(0, 30);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox.Size = new System.Drawing.Size(300, 170);
            this.textBox.TabIndex = 1;
            // 
            // NoteWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 200);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.controlPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "NoteWindow";
            this.Opacity = 0.9D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Заметка";
            this.controlPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}