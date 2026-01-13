namespace VSTDistortion
{
    partial class DistortionForm
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
            this.groupBoxMain = new System.Windows.Forms.GroupBox();
            this.labelToneValue = new System.Windows.Forms.Label();
            this.labelMixValue = new System.Windows.Forms.Label();
            this.labelOutputValue = new System.Windows.Forms.Label();
            this.labelDriveValue = new System.Windows.Forms.Label();
            this.labelTone = new System.Windows.Forms.Label();
            this.labelMix = new System.Windows.Forms.Label();
            this.labelType = new System.Windows.Forms.Label();
            this.labelOutput = new System.Windows.Forms.Label();
            this.labelDrive = new System.Windows.Forms.Label();
            this.comboBoxType = new System.Windows.Forms.ComboBox();
            this.trackBarTone = new System.Windows.Forms.TrackBar();
            this.trackBarMix = new System.Windows.Forms.TrackBar();
            this.trackBarOutput = new System.Windows.Forms.TrackBar();
            this.trackBarDrive = new System.Windows.Forms.TrackBar();
            this.groupBoxPresets = new System.Windows.Forms.GroupBox();
            this.labelPresetName = new System.Windows.Forms.Label();
            this.textBoxPresetName = new System.Windows.Forms.TextBox();
            this.buttonDeletePreset = new System.Windows.Forms.Button();
            this.buttonSavePreset = new System.Windows.Forms.Button();
            this.comboBoxPresets = new System.Windows.Forms.ComboBox();
            this.groupBoxMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTone)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMix)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDrive)).BeginInit();
            this.groupBoxPresets.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxMain
            // 
            this.groupBoxMain.BackColor = System.Drawing.Color.Transparent;
            this.groupBoxMain.Controls.Add(this.labelToneValue);
            this.groupBoxMain.Controls.Add(this.labelMixValue);
            this.groupBoxMain.Controls.Add(this.labelOutputValue);
            this.groupBoxMain.Controls.Add(this.labelDriveValue);
            this.groupBoxMain.Controls.Add(this.labelTone);
            this.groupBoxMain.Controls.Add(this.labelMix);
            this.groupBoxMain.Controls.Add(this.labelType);
            this.groupBoxMain.Controls.Add(this.labelOutput);
            this.groupBoxMain.Controls.Add(this.labelDrive);
            this.groupBoxMain.Controls.Add(this.comboBoxType);
            this.groupBoxMain.Controls.Add(this.trackBarTone);
            this.groupBoxMain.Controls.Add(this.trackBarMix);
            this.groupBoxMain.Controls.Add(this.trackBarOutput);
            this.groupBoxMain.Controls.Add(this.trackBarDrive);
            this.groupBoxMain.ForeColor = System.Drawing.Color.White;
            this.groupBoxMain.Location = new System.Drawing.Point(12, 12);
            this.groupBoxMain.Name = "groupBoxMain";
            this.groupBoxMain.Size = new System.Drawing.Size(360, 260);
            this.groupBoxMain.TabIndex = 0;
            this.groupBoxMain.TabStop = false;
            this.groupBoxMain.Text = "Distortion Controls";
            // 
            // labelToneValue
            // 
            this.labelToneValue.BackColor = System.Drawing.Color.Transparent;
            this.labelToneValue.ForeColor = System.Drawing.Color.Yellow;
            this.labelToneValue.Location = new System.Drawing.Point(304, 215);
            this.labelToneValue.Name = "labelToneValue";
            this.labelToneValue.Size = new System.Drawing.Size(50, 13);
            this.labelToneValue.TabIndex = 13;
            this.labelToneValue.Text = "0";
            this.labelToneValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelMixValue
            // 
            this.labelMixValue.BackColor = System.Drawing.Color.Transparent;
            this.labelMixValue.ForeColor = System.Drawing.Color.Yellow;
            this.labelMixValue.Location = new System.Drawing.Point(304, 170);
            this.labelMixValue.Name = "labelMixValue";
            this.labelMixValue.Size = new System.Drawing.Size(50, 13);
            this.labelMixValue.TabIndex = 12;
            this.labelMixValue.Text = "100";
            this.labelMixValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelOutputValue
            // 
            this.labelOutputValue.BackColor = System.Drawing.Color.Transparent;
            this.labelOutputValue.ForeColor = System.Drawing.Color.Yellow;
            this.labelOutputValue.Location = new System.Drawing.Point(304, 80);
            this.labelOutputValue.Name = "labelOutputValue";
            this.labelOutputValue.Size = new System.Drawing.Size(50, 13);
            this.labelOutputValue.TabIndex = 11;
            this.labelOutputValue.Text = "100";
            this.labelOutputValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDriveValue
            // 
            this.labelDriveValue.BackColor = System.Drawing.Color.Transparent;
            this.labelDriveValue.ForeColor = System.Drawing.Color.Yellow;
            this.labelDriveValue.Location = new System.Drawing.Point(304, 35);
            this.labelDriveValue.Name = "labelDriveValue";
            this.labelDriveValue.Size = new System.Drawing.Size(50, 13);
            this.labelDriveValue.TabIndex = 10;
            this.labelDriveValue.Text = "50";
            this.labelDriveValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelTone
            // 
            this.labelTone.AutoSize = true;
            this.labelTone.BackColor = System.Drawing.Color.Transparent;
            this.labelTone.ForeColor = System.Drawing.Color.Yellow;
            this.labelTone.Location = new System.Drawing.Point(15, 215);
            this.labelTone.Name = "labelTone";
            this.labelTone.Size = new System.Drawing.Size(32, 13);
            this.labelTone.TabIndex = 9;
            this.labelTone.Text = "Tone";
            // 
            // labelMix
            // 
            this.labelMix.AutoSize = true;
            this.labelMix.BackColor = System.Drawing.Color.Transparent;
            this.labelMix.ForeColor = System.Drawing.Color.Yellow;
            this.labelMix.Location = new System.Drawing.Point(15, 170);
            this.labelMix.Name = "labelMix";
            this.labelMix.Size = new System.Drawing.Size(23, 13);
            this.labelMix.TabIndex = 8;
            this.labelMix.Text = "Mix";
            // 
            // labelType
            // 
            this.labelType.AutoSize = true;
            this.labelType.BackColor = System.Drawing.Color.Transparent;
            this.labelType.ForeColor = System.Drawing.Color.Yellow;
            this.labelType.Location = new System.Drawing.Point(15, 125);
            this.labelType.Name = "labelType";
            this.labelType.Size = new System.Drawing.Size(31, 13);
            this.labelType.TabIndex = 7;
            this.labelType.Text = "Type";
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.BackColor = System.Drawing.Color.Transparent;
            this.labelOutput.ForeColor = System.Drawing.Color.Yellow;
            this.labelOutput.Location = new System.Drawing.Point(15, 80);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(39, 13);
            this.labelOutput.TabIndex = 6;
            this.labelOutput.Text = "Output";
            // 
            // labelDrive
            // 
            this.labelDrive.AutoSize = true;
            this.labelDrive.BackColor = System.Drawing.Color.Transparent;
            this.labelDrive.ForeColor = System.Drawing.Color.Yellow;
            this.labelDrive.Location = new System.Drawing.Point(15, 35);
            this.labelDrive.Name = "labelDrive";
            this.labelDrive.Size = new System.Drawing.Size(32, 13);
            this.labelDrive.TabIndex = 5;
            this.labelDrive.Text = "Drive";
            // 
            // comboBoxType
            // 
            this.comboBoxType.BackColor = System.Drawing.Color.Yellow;
            this.comboBoxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxType.FormattingEnabled = true;
            this.comboBoxType.Items.AddRange(new object[] {
            "Soft Clip",
            "Hard Clip",
            "Tube",
            "Fuzz"});
            this.comboBoxType.Location = new System.Drawing.Point(80, 120);
            this.comboBoxType.Name = "comboBoxType";
            this.comboBoxType.Size = new System.Drawing.Size(120, 21);
            this.comboBoxType.TabIndex = 4;
            // 
            // trackBarTone
            // 
            this.trackBarTone.BackColor = System.Drawing.Color.Yellow;
            this.trackBarTone.Location = new System.Drawing.Point(80, 205);
            this.trackBarTone.Maximum = 100;
            this.trackBarTone.Minimum = -100;
            this.trackBarTone.Name = "trackBarTone";
            this.trackBarTone.Size = new System.Drawing.Size(200, 45);
            this.trackBarTone.TabIndex = 3;
            this.trackBarTone.TickFrequency = 20;
            // 
            // trackBarMix
            // 
            this.trackBarMix.BackColor = System.Drawing.Color.Yellow;
            this.trackBarMix.Location = new System.Drawing.Point(80, 160);
            this.trackBarMix.Maximum = 100;
            this.trackBarMix.Name = "trackBarMix";
            this.trackBarMix.Size = new System.Drawing.Size(200, 45);
            this.trackBarMix.TabIndex = 2;
            this.trackBarMix.TickFrequency = 10;
            this.trackBarMix.Value = 100;
            // 
            // trackBarOutput
            // 
            this.trackBarOutput.BackColor = System.Drawing.Color.Yellow;
            this.trackBarOutput.Location = new System.Drawing.Point(80, 70);
            this.trackBarOutput.Maximum = 200;
            this.trackBarOutput.Name = "trackBarOutput";
            this.trackBarOutput.Size = new System.Drawing.Size(200, 45);
            this.trackBarOutput.TabIndex = 1;
            this.trackBarOutput.TickFrequency = 20;
            this.trackBarOutput.Value = 100;
            // 
            // trackBarDrive
            // 
            this.trackBarDrive.BackColor = System.Drawing.Color.Yellow;
            this.trackBarDrive.Location = new System.Drawing.Point(80, 25);
            this.trackBarDrive.Maximum = 100;
            this.trackBarDrive.Name = "trackBarDrive";
            this.trackBarDrive.Size = new System.Drawing.Size(200, 45);
            this.trackBarDrive.TabIndex = 0;
            this.trackBarDrive.TickFrequency = 10;
            this.trackBarDrive.Value = 50;
            // 
            // groupBoxPresets
            // 
            this.groupBoxPresets.BackColor = System.Drawing.Color.Transparent;
            this.groupBoxPresets.Controls.Add(this.labelPresetName);
            this.groupBoxPresets.Controls.Add(this.textBoxPresetName);
            this.groupBoxPresets.Controls.Add(this.buttonDeletePreset);
            this.groupBoxPresets.Controls.Add(this.buttonSavePreset);
            this.groupBoxPresets.Controls.Add(this.comboBoxPresets);
            this.groupBoxPresets.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.groupBoxPresets.Location = new System.Drawing.Point(12, 278);
            this.groupBoxPresets.Name = "groupBoxPresets";
            this.groupBoxPresets.Size = new System.Drawing.Size(360, 120);
            this.groupBoxPresets.TabIndex = 1;
            this.groupBoxPresets.TabStop = false;
            this.groupBoxPresets.Text = "Presets";
            // 
            // labelPresetName
            // 
            this.labelPresetName.AutoSize = true;
            this.labelPresetName.BackColor = System.Drawing.Color.Transparent;
            this.labelPresetName.ForeColor = System.Drawing.Color.Yellow;
            this.labelPresetName.Location = new System.Drawing.Point(225, 61);
            this.labelPresetName.Name = "labelPresetName";
            this.labelPresetName.Size = new System.Drawing.Size(35, 13);
            this.labelPresetName.TabIndex = 4;
            this.labelPresetName.Text = "Name";
            // 
            // textBoxPresetName
            // 
            this.textBoxPresetName.BackColor = System.Drawing.Color.Yellow;
            this.textBoxPresetName.ForeColor = System.Drawing.Color.Black;
            this.textBoxPresetName.Location = new System.Drawing.Point(15, 58);
            this.textBoxPresetName.Name = "textBoxPresetName";
            this.textBoxPresetName.Size = new System.Drawing.Size(200, 20);
            this.textBoxPresetName.TabIndex = 3;
            // 
            // buttonDeletePreset
            // 
            this.buttonDeletePreset.BackColor = System.Drawing.Color.Yellow;
            this.buttonDeletePreset.ForeColor = System.Drawing.Color.Black;
            this.buttonDeletePreset.Location = new System.Drawing.Point(100, 85);
            this.buttonDeletePreset.Name = "buttonDeletePreset";
            this.buttonDeletePreset.Size = new System.Drawing.Size(75, 23);
            this.buttonDeletePreset.TabIndex = 2;
            this.buttonDeletePreset.Text = "Delete";
            this.buttonDeletePreset.UseVisualStyleBackColor = false;
            // 
            // buttonSavePreset
            // 
            this.buttonSavePreset.BackColor = System.Drawing.Color.Yellow;
            this.buttonSavePreset.ForeColor = System.Drawing.Color.Black;
            this.buttonSavePreset.Location = new System.Drawing.Point(15, 85);
            this.buttonSavePreset.Name = "buttonSavePreset";
            this.buttonSavePreset.Size = new System.Drawing.Size(75, 23);
            this.buttonSavePreset.TabIndex = 1;
            this.buttonSavePreset.Text = "Save";
            this.buttonSavePreset.UseVisualStyleBackColor = false;
            // 
            // comboBoxPresets
            // 
            this.comboBoxPresets.BackColor = System.Drawing.Color.Yellow;
            this.comboBoxPresets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPresets.FormattingEnabled = true;
            this.comboBoxPresets.Location = new System.Drawing.Point(15, 25);
            this.comboBoxPresets.Name = "comboBoxPresets";
            this.comboBoxPresets.Size = new System.Drawing.Size(200, 21);
            this.comboBoxPresets.TabIndex = 0;
            // 
            // DistortionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::VSTDistortion.Properties.Resources.background;
            this.ClientSize = new System.Drawing.Size(384, 410);
            this.Controls.Add(this.groupBoxPresets);
            this.Controls.Add(this.groupBoxMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DistortionForm";
            this.Text = "VST Distortion";
            this.groupBoxMain.ResumeLayout(false);
            this.groupBoxMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarTone)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMix)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDrive)).EndInit();
            this.groupBoxPresets.ResumeLayout(false);
            this.groupBoxPresets.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.GroupBox groupBoxMain;
        private System.Windows.Forms.TrackBar trackBarDrive;
        private System.Windows.Forms.TrackBar trackBarOutput;
        private System.Windows.Forms.TrackBar trackBarMix;
        private System.Windows.Forms.TrackBar trackBarTone;
        private System.Windows.Forms.ComboBox comboBoxType;
        private System.Windows.Forms.Label labelDrive;
        private System.Windows.Forms.Label labelOutput;
        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.Label labelMix;
        private System.Windows.Forms.Label labelTone;
        private System.Windows.Forms.Label labelDriveValue;
        private System.Windows.Forms.Label labelOutputValue;
        private System.Windows.Forms.Label labelMixValue;
        private System.Windows.Forms.Label labelToneValue;
        private System.Windows.Forms.GroupBox groupBoxPresets;
        private System.Windows.Forms.ComboBox comboBoxPresets;
        private System.Windows.Forms.Button buttonSavePreset;
        private System.Windows.Forms.Button buttonDeletePreset;
        private System.Windows.Forms.TextBox textBoxPresetName;
        private System.Windows.Forms.Label labelPresetName;
    }
}