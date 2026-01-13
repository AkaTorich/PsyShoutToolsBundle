using System.Windows.Forms;

namespace BASSGeneratorVST
{
    partial class BASSGeneratorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BASSGeneratorForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbBoxTonica = new System.Windows.Forms.ComboBox();
            this.cmbBoxCategory = new System.Windows.Forms.ComboBox();
            this.cmbBoxScale = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbBoxBassPattern = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBassNotesCount = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTactsNumber = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.txtRepeatsNumber = new System.Windows.Forms.NumericUpDown();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.txtProgression = new System.Windows.Forms.TextBox();
            this.btnPlayMidi = new System.Windows.Forms.Button();
            this.btnPlayInDAW = new System.Windows.Forms.Button();
            this.rbKeyboard = new System.Windows.Forms.RadioButton();
            this.rbStaff = new System.Windows.Forms.RadioButton();
            this.lblNoteDisplay = new System.Windows.Forms.Label();
            this.noteDisplay = new BASSGeneratorVST.NoteDisplayControl();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Red;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(16, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Тоника:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Red;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(16, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Категория:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Red;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(16, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Лад/гамма (scale):";
            // 
            // cmbBoxTonica
            // 
            this.cmbBoxTonica.BackColor = System.Drawing.Color.Red;
            this.cmbBoxTonica.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxTonica.ForeColor = System.Drawing.Color.White;
            this.cmbBoxTonica.FormattingEnabled = true;
            this.cmbBoxTonica.Location = new System.Drawing.Point(150, 17);
            this.cmbBoxTonica.Name = "cmbBoxTonica";
            this.cmbBoxTonica.Size = new System.Drawing.Size(225, 21);
            this.cmbBoxTonica.TabIndex = 3;
            // 
            // cmbBoxCategory
            // 
            this.cmbBoxCategory.BackColor = System.Drawing.Color.Red;
            this.cmbBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxCategory.ForeColor = System.Drawing.Color.White;
            this.cmbBoxCategory.FormattingEnabled = true;
            this.cmbBoxCategory.Location = new System.Drawing.Point(150, 45);
            this.cmbBoxCategory.Name = "cmbBoxCategory";
            this.cmbBoxCategory.Size = new System.Drawing.Size(225, 21);
            this.cmbBoxCategory.TabIndex = 4;
            // 
            // cmbBoxScale
            // 
            this.cmbBoxScale.BackColor = System.Drawing.Color.Red;
            this.cmbBoxScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxScale.ForeColor = System.Drawing.Color.White;
            this.cmbBoxScale.FormattingEnabled = true;
            this.cmbBoxScale.Location = new System.Drawing.Point(150, 73);
            this.cmbBoxScale.Name = "cmbBoxScale";
            this.cmbBoxScale.Size = new System.Drawing.Size(225, 21);
            this.cmbBoxScale.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Red;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(16, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Басовый паттерн:";
            // 
            // cmbBoxBassPattern
            // 
            this.cmbBoxBassPattern.BackColor = System.Drawing.Color.Red;
            this.cmbBoxBassPattern.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxBassPattern.ForeColor = System.Drawing.Color.White;
            this.cmbBoxBassPattern.FormattingEnabled = true;
            this.cmbBoxBassPattern.Location = new System.Drawing.Point(150, 101);
            this.cmbBoxBassPattern.Name = "cmbBoxBassPattern";
            this.cmbBoxBassPattern.Size = new System.Drawing.Size(225, 21);
            this.cmbBoxBassPattern.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Red;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(16, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Кол-во нот в басе";
            // 
            // txtBassNotesCount
            // 
            this.txtBassNotesCount.BackColor = System.Drawing.Color.Red;
            this.txtBassNotesCount.ForeColor = System.Drawing.Color.White;
            this.txtBassNotesCount.Location = new System.Drawing.Point(150, 130);
            this.txtBassNotesCount.Name = "txtBassNotesCount";
            this.txtBassNotesCount.Size = new System.Drawing.Size(225, 20);
            this.txtBassNotesCount.TabIndex = 9;
            this.txtBassNotesCount.Minimum = 1;
            this.txtBassNotesCount.Maximum = 32;
            this.txtBassNotesCount.Value = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Red;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(16, 159);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(106, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Количество тактов:";
            // 
            // txtTactsNumber
            // 
            this.txtTactsNumber.BackColor = System.Drawing.Color.Red;
            this.txtTactsNumber.ForeColor = System.Drawing.Color.White;
            this.txtTactsNumber.Location = new System.Drawing.Point(150, 156);
            this.txtTactsNumber.Name = "txtTactsNumber";
            this.txtTactsNumber.Size = new System.Drawing.Size(225, 20);
            this.txtTactsNumber.TabIndex = 11;
            this.txtTactsNumber.Minimum = 1;
            this.txtTactsNumber.Maximum = 64;
            this.txtTactsNumber.Value = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Red;
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(16, 185);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(119, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Количество повторов:";
            // 
            // txtRepeatsNumber
            // 
            this.txtRepeatsNumber.BackColor = System.Drawing.Color.Red;
            this.txtRepeatsNumber.ForeColor = System.Drawing.Color.White;
            this.txtRepeatsNumber.Location = new System.Drawing.Point(150, 182);
            this.txtRepeatsNumber.Name = "txtRepeatsNumber";
            this.txtRepeatsNumber.Size = new System.Drawing.Size(225, 20);
            this.txtRepeatsNumber.TabIndex = 13;
            this.txtRepeatsNumber.Minimum = 0;
            this.txtRepeatsNumber.Maximum = 100;
            this.txtRepeatsNumber.Value = 0;
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.Red;
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.Location = new System.Drawing.Point(12, 208);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(363, 45);
            this.btnGenerate.TabIndex = 14;
            this.btnGenerate.Text = "Сгенерировать басовую линию";
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // txtProgression
            // 
            this.txtProgression.BackColor = System.Drawing.Color.Red;
            this.txtProgression.ForeColor = System.Drawing.Color.White;
            this.txtProgression.Location = new System.Drawing.Point(381, 84);
            this.txtProgression.Multiline = true;
            this.txtProgression.Name = "txtProgression";
            this.txtProgression.ReadOnly = true;
            this.txtProgression.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtProgression.Size = new System.Drawing.Size(344, 187);
            this.txtProgression.TabIndex = 0;
            // 
            // btnPlayMidi
            // 
            this.btnPlayMidi.BackColor = System.Drawing.Color.Red;
            this.btnPlayMidi.Enabled = false;
            this.btnPlayMidi.ForeColor = System.Drawing.Color.White;
            this.btnPlayMidi.Location = new System.Drawing.Point(381, 17);
            this.btnPlayMidi.Name = "btnPlayMidi";
            this.btnPlayMidi.Size = new System.Drawing.Size(346, 32);
            this.btnPlayMidi.TabIndex = 17;
            this.btnPlayMidi.Text = "⬇ Перетащить MIDI в DAW (схватите кнопку)";
            this.btnPlayMidi.UseVisualStyleBackColor = false;
            this.btnPlayMidi.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BtnDragMidi_MouseDown);
            // 
            // btnPlayInDAW
            // 
            this.btnPlayInDAW.BackColor = System.Drawing.Color.Green;
            this.btnPlayInDAW.ForeColor = System.Drawing.Color.White;
            this.btnPlayInDAW.Location = new System.Drawing.Point(381, 48);
            this.btnPlayInDAW.Name = "btnPlayInDAW";
            this.btnPlayInDAW.Size = new System.Drawing.Size(346, 30);
            this.btnPlayInDAW.TabIndex = 18;
            this.btnPlayInDAW.Text = "▶ Воспроизвести";
            this.btnPlayInDAW.UseVisualStyleBackColor = false;
            this.btnPlayInDAW.Click += new System.EventHandler(this.btnPlayInDAW_Click);
            // 
            // rbKeyboard
            // 
            this.rbKeyboard.AutoSize = true;
            this.rbKeyboard.BackColor = System.Drawing.Color.Red;
            this.rbKeyboard.Checked = true;
            this.rbKeyboard.ForeColor = System.Drawing.Color.White;
            this.rbKeyboard.Location = new System.Drawing.Point(120, 254);
            this.rbKeyboard.Name = "rbKeyboard";
            this.rbKeyboard.Size = new System.Drawing.Size(84, 17);
            this.rbKeyboard.TabIndex = 19;
            this.rbKeyboard.TabStop = true;
            this.rbKeyboard.Text = "Клавиатура";
            this.rbKeyboard.UseVisualStyleBackColor = false;
            this.rbKeyboard.CheckedChanged += new System.EventHandler(this.rbKeyboard_CheckedChanged);
            // 
            // rbStaff
            // 
            this.rbStaff.AutoSize = true;
            this.rbStaff.BackColor = System.Drawing.Color.Red;
            this.rbStaff.ForeColor = System.Drawing.Color.White;
            this.rbStaff.Location = new System.Drawing.Point(210, 254);
            this.rbStaff.Name = "rbStaff";
            this.rbStaff.Size = new System.Drawing.Size(90, 17);
            this.rbStaff.TabIndex = 20;
            this.rbStaff.Text = "Нотный стан";
            this.rbStaff.UseVisualStyleBackColor = false;
            this.rbStaff.CheckedChanged += new System.EventHandler(this.rbStaff_CheckedChanged);
            // 
            // lblNoteDisplay
            // 
            this.lblNoteDisplay.AutoSize = true;
            this.lblNoteDisplay.BackColor = System.Drawing.Color.Red;
            this.lblNoteDisplay.ForeColor = System.Drawing.Color.White;
            this.lblNoteDisplay.Location = new System.Drawing.Point(12, 256);
            this.lblNoteDisplay.Name = "lblNoteDisplay";
            this.lblNoteDisplay.Size = new System.Drawing.Size(102, 13);
            this.lblNoteDisplay.TabIndex = 21;
            this.lblNoteDisplay.Text = "Визуализация нот:";
            // 
            // noteDisplay
            // 
            this.noteDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.noteDisplay.BackColor = System.Drawing.Color.Red;
            this.noteDisplay.ForeColor = System.Drawing.Color.White;
            this.noteDisplay.Location = new System.Drawing.Point(10, 277);
            this.noteDisplay.MinimumSize = new System.Drawing.Size(100, 150);
            this.noteDisplay.Name = "noteDisplay";
            this.noteDisplay.Size = new System.Drawing.Size(715, 451);
            this.noteDisplay.TabIndex = 18;
            // 
            // BASSGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(739, 740);
            this.Controls.Add(this.lblNoteDisplay);
            this.Controls.Add(this.rbStaff);
            this.Controls.Add(this.rbKeyboard);
            this.Controls.Add(this.noteDisplay);
            this.Controls.Add(this.txtProgression);
            this.Controls.Add(this.btnPlayInDAW);
            this.Controls.Add(this.btnPlayMidi);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.txtRepeatsNumber);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtTactsNumber);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtBassNotesCount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbBoxBassPattern);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbBoxScale);
            this.Controls.Add(this.cmbBoxCategory);
            this.Controls.Add(this.cmbBoxTonica);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(755, 595);
            this.Name = "BASSGeneratorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Генератор басовых линий";
            this.Resize += new System.EventHandler(this.BassGeneratorForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbBoxTonica;
        private System.Windows.Forms.ComboBox cmbBoxCategory;
        private System.Windows.Forms.ComboBox cmbBoxScale;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbBoxBassPattern;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown txtBassNotesCount;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown txtTactsNumber;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown txtRepeatsNumber;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TextBox txtProgression;
        private System.Windows.Forms.Button btnPlayMidi;
        private System.Windows.Forms.Button btnPlayInDAW;
        private NoteDisplayControl noteDisplay;
        private System.Windows.Forms.RadioButton rbKeyboard;
        private System.Windows.Forms.RadioButton rbStaff;
        private System.Windows.Forms.Label lblNoteDisplay;
    }
}