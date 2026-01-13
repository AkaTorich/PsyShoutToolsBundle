namespace PADGeneratorVST
{
    partial class PADGeneratorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PADGeneratorForm));
            this.cmbBoxTonality = new System.Windows.Forms.ComboBox();
            this.cmbBoxCategory = new System.Windows.Forms.ComboBox();
            this.cmbBoxScale = new System.Windows.Forms.ComboBox();
            this.txtNumberOfChords = new System.Windows.Forms.NumericUpDown();
            this.txtProgression = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnPlayInDAW = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtProgressionLength = new System.Windows.Forms.NumericUpDown();
            this.btnPlayMIDI = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbStaffNotation = new System.Windows.Forms.RadioButton();
            this.rbPianoRoll = new System.Windows.Forms.RadioButton();
            this.noteDisplayControl = new PADGeneratorVST.NoteDisplayControl();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmbBoxTonality
            // 
            this.cmbBoxTonality.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.cmbBoxTonality.ForeColor = System.Drawing.Color.White;
            this.cmbBoxTonality.FormattingEnabled = true;
            this.cmbBoxTonality.Location = new System.Drawing.Point(154, 12);
            this.cmbBoxTonality.Name = "cmbBoxTonality";
            this.cmbBoxTonality.Size = new System.Drawing.Size(279, 21);
            this.cmbBoxTonality.TabIndex = 0;
            // 
            // cmbBoxCategory
            // 
            this.cmbBoxCategory.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.cmbBoxCategory.ForeColor = System.Drawing.Color.White;
            this.cmbBoxCategory.FormattingEnabled = true;
            this.cmbBoxCategory.Location = new System.Drawing.Point(154, 39);
            this.cmbBoxCategory.Name = "cmbBoxCategory";
            this.cmbBoxCategory.Size = new System.Drawing.Size(279, 21);
            this.cmbBoxCategory.TabIndex = 1;
            // 
            // cmbBoxScale
            // 
            this.cmbBoxScale.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.cmbBoxScale.ForeColor = System.Drawing.Color.White;
            this.cmbBoxScale.FormattingEnabled = true;
            this.cmbBoxScale.Location = new System.Drawing.Point(154, 66);
            this.cmbBoxScale.Name = "cmbBoxScale";
            this.cmbBoxScale.Size = new System.Drawing.Size(279, 21);
            this.cmbBoxScale.TabIndex = 2;
            // 
            // txtNumberOfChords
            // 
            this.txtNumberOfChords.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.txtNumberOfChords.ForeColor = System.Drawing.Color.White;
            this.txtNumberOfChords.Location = new System.Drawing.Point(154, 93);
            this.txtNumberOfChords.Name = "txtNumberOfChords";
            this.txtNumberOfChords.Size = new System.Drawing.Size(279, 20);
            this.txtNumberOfChords.TabIndex = 3;
            this.txtNumberOfChords.Minimum = 1;
            this.txtNumberOfChords.Maximum = 16;
            this.txtNumberOfChords.Value = 4;
            // 
            // txtProgression
            // 
            this.txtProgression.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.txtProgression.ForeColor = System.Drawing.Color.White;
            this.txtProgression.Location = new System.Drawing.Point(439, 66);
            this.txtProgression.Multiline = true;
            this.txtProgression.Name = "txtProgression";
            this.txtProgression.Size = new System.Drawing.Size(260, 152);
            this.txtProgression.TabIndex = 4;
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.Location = new System.Drawing.Point(7, 148);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(426, 44);
            this.btnGenerate.TabIndex = 5;
            this.btnGenerate.Text = "Генерировать";
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnPlayInDAW
            // 
            this.btnPlayInDAW.BackColor = System.Drawing.Color.Green;
            this.btnPlayInDAW.ForeColor = System.Drawing.Color.White;
            this.btnPlayInDAW.Location = new System.Drawing.Point(439, 33);
            this.btnPlayInDAW.Name = "btnPlayInDAW";
            this.btnPlayInDAW.Size = new System.Drawing.Size(260, 30);
            this.btnPlayInDAW.TabIndex = 15;
            this.btnPlayInDAW.Text = "▶ Воспроизвести";
            this.btnPlayInDAW.UseVisualStyleBackColor = false;
            this.btnPlayInDAW.Click += new System.EventHandler(this.btnPlayInDAW_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Red;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Тоника";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Red;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(9, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Категория";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Red;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(9, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Лад";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Red;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(9, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(121, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Уникальных аккордов";
            // 
            // txtProgressionLength
            // 
            this.txtProgressionLength.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.txtProgressionLength.ForeColor = System.Drawing.Color.White;
            this.txtProgressionLength.Location = new System.Drawing.Point(154, 119);
            this.txtProgressionLength.Name = "txtProgressionLength";
            this.txtProgressionLength.Size = new System.Drawing.Size(279, 20);
            this.txtProgressionLength.TabIndex = 10;
            this.txtProgressionLength.Minimum = 1;
            this.txtProgressionLength.Maximum = 16;
            this.txtProgressionLength.Value = 4;
            // 
            // btnPlayMIDI
            // 
            this.btnPlayMIDI.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnPlayMIDI.ForeColor = System.Drawing.Color.White;
            this.btnPlayMIDI.Location = new System.Drawing.Point(440, 11);
            this.btnPlayMIDI.Name = "btnPlayMIDI";
            this.btnPlayMIDI.Size = new System.Drawing.Size(259, 22);
            this.btnPlayMIDI.TabIndex = 11;
            this.btnPlayMIDI.Text = "⬇ Перетащить MIDI в DAW (схватите кнопку)";
            this.btnPlayMIDI.UseVisualStyleBackColor = false;
            this.btnPlayMIDI.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BtnDragMidi_MouseDown);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Red;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(9, 122);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(102, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Длина прогрессии";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Red;
            this.panel1.Controls.Add(this.rbStaffNotation);
            this.panel1.Controls.Add(this.rbPianoRoll);
            this.panel1.Location = new System.Drawing.Point(7, 198);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(426, 22);
            this.panel1.TabIndex = 15;
            // 
            // rbStaffNotation
            // 
            this.rbStaffNotation.AutoSize = true;
            this.rbStaffNotation.ForeColor = System.Drawing.Color.White;
            this.rbStaffNotation.Location = new System.Drawing.Point(102, 3);
            this.rbStaffNotation.Name = "rbStaffNotation";
            this.rbStaffNotation.Size = new System.Drawing.Size(90, 17);
            this.rbStaffNotation.TabIndex = 1;
            this.rbStaffNotation.Text = "Нотный стан";
            this.rbStaffNotation.UseVisualStyleBackColor = true;
            this.rbStaffNotation.CheckedChanged += new System.EventHandler(this.rbStaffNotation_CheckedChanged);
            // 
            // rbPianoRoll
            // 
            this.rbPianoRoll.AutoSize = true;
            this.rbPianoRoll.Checked = true;
            this.rbPianoRoll.ForeColor = System.Drawing.Color.White;
            this.rbPianoRoll.Location = new System.Drawing.Point(12, 3);
            this.rbPianoRoll.Name = "rbPianoRoll";
            this.rbPianoRoll.Size = new System.Drawing.Size(84, 17);
            this.rbPianoRoll.TabIndex = 0;
            this.rbPianoRoll.TabStop = true;
            this.rbPianoRoll.Text = "Пиано-ролл";
            this.rbPianoRoll.UseVisualStyleBackColor = true;
            this.rbPianoRoll.CheckedChanged += new System.EventHandler(this.rbPianoRoll_CheckedChanged);
            // 
            // noteDisplayControl
            // 
            this.noteDisplayControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.noteDisplayControl.Location = new System.Drawing.Point(7, 226);
            this.noteDisplayControl.Name = "noteDisplayControl";
            this.noteDisplayControl.Size = new System.Drawing.Size(692, 409);
            this.noteDisplayControl.TabIndex = 14;
            // 
            // PADGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Coral;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(711, 639);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.noteDisplayControl);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnPlayMIDI);
            this.Controls.Add(this.txtProgressionLength);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnPlayInDAW);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.txtProgression);
            this.Controls.Add(this.txtNumberOfChords);
            this.Controls.Add(this.cmbBoxScale);
            this.Controls.Add(this.cmbBoxCategory);
            this.Controls.Add(this.cmbBoxTonality);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "PADGeneratorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PADGenerator";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ComboBox cmbBoxTonality;
        private System.Windows.Forms.ComboBox cmbBoxCategory;
        private System.Windows.Forms.ComboBox cmbBoxScale;
        private System.Windows.Forms.TextBox txtProgression;
        private System.Windows.Forms.NumericUpDown txtNumberOfChords;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown txtProgressionLength;
        private System.Windows.Forms.Button btnPlayMIDI;
        private System.Windows.Forms.Button btnPlayInDAW;
        private System.Windows.Forms.Label label5;
        private PADGeneratorVST.NoteDisplayControl noteDisplayControl;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rbStaffNotation;
        private System.Windows.Forms.RadioButton rbPianoRoll;
    }
}