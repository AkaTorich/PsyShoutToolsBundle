namespace MelodyGenerator
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTonica;
        private System.Windows.Forms.ComboBox cmbBoxTonica;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cmbBoxCategory;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.ComboBox cmbBoxScale;
        private System.Windows.Forms.Label lblArpNotesCount;
        private System.Windows.Forms.TextBox txtArpNotesCount;
        private System.Windows.Forms.Label lblTactsNumber;
        private System.Windows.Forms.TextBox txtTactsNumber;
        private System.Windows.Forms.Label lblRepeatsNumber;
        private System.Windows.Forms.TextBox txtRepeatsNumber;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnPlayMidi;
        private System.Windows.Forms.TextBox txtProgression;
        // Новые компоненты для отображения нот
        private System.Windows.Forms.Panel panelNoteDisplay;
        private BASSGenerator.NoteDisplayControl noteDisplayControl;
        private System.Windows.Forms.RadioButton rbKeyboard;
        private System.Windows.Forms.RadioButton rbStaff;
        private System.Windows.Forms.Label lblNoteDisplay;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lblTonica = new System.Windows.Forms.Label();
            this.cmbBoxTonica = new System.Windows.Forms.ComboBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.cmbBoxCategory = new System.Windows.Forms.ComboBox();
            this.lblScale = new System.Windows.Forms.Label();
            this.cmbBoxScale = new System.Windows.Forms.ComboBox();
            this.lblArpNotesCount = new System.Windows.Forms.Label();
            this.txtArpNotesCount = new System.Windows.Forms.TextBox();
            this.lblTactsNumber = new System.Windows.Forms.Label();
            this.txtTactsNumber = new System.Windows.Forms.TextBox();
            this.lblRepeatsNumber = new System.Windows.Forms.Label();
            this.txtRepeatsNumber = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnPlayMidi = new System.Windows.Forms.Button();
            this.txtProgression = new System.Windows.Forms.TextBox();
            this.panelNoteDisplay = new System.Windows.Forms.Panel();
            this.rbKeyboard = new System.Windows.Forms.RadioButton();
            this.rbStaff = new System.Windows.Forms.RadioButton();
            this.lblNoteDisplay = new System.Windows.Forms.Label();
            this.noteDisplayControl = new BASSGenerator.NoteDisplayControl();
            this.SuspendLayout();
            // 
            // lblTonica
            // 
            this.lblTonica.AutoSize = true;
            this.lblTonica.BackColor = System.Drawing.Color.Transparent;
            this.lblTonica.ForeColor = System.Drawing.Color.Lime;
            this.lblTonica.Location = new System.Drawing.Point(12, 15);
            this.lblTonica.Name = "lblTonica";
            this.lblTonica.Size = new System.Drawing.Size(44, 13);
            this.lblTonica.TabIndex = 0;
            this.lblTonica.Text = "Тоника";
            // 
            // cmbBoxTonica
            // 
            this.cmbBoxTonica.BackColor = System.Drawing.Color.CornflowerBlue;
            this.cmbBoxTonica.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxTonica.FormattingEnabled = true;
            this.cmbBoxTonica.Location = new System.Drawing.Point(113, 12);
            this.cmbBoxTonica.Name = "cmbBoxTonica";
            this.cmbBoxTonica.Size = new System.Drawing.Size(282, 21);
            this.cmbBoxTonica.TabIndex = 1;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.BackColor = System.Drawing.Color.Transparent;
            this.lblCategory.ForeColor = System.Drawing.Color.Lime;
            this.lblCategory.Location = new System.Drawing.Point(12, 42);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(60, 13);
            this.lblCategory.TabIndex = 2;
            this.lblCategory.Text = "Категория";
            // 
            // cmbBoxCategory
            // 
            this.cmbBoxCategory.BackColor = System.Drawing.Color.CornflowerBlue;
            this.cmbBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxCategory.FormattingEnabled = true;
            this.cmbBoxCategory.Location = new System.Drawing.Point(113, 39);
            this.cmbBoxCategory.Name = "cmbBoxCategory";
            this.cmbBoxCategory.Size = new System.Drawing.Size(282, 21);
            this.cmbBoxCategory.TabIndex = 3;
            // 
            // lblScale
            // 
            this.lblScale.AutoSize = true;
            this.lblScale.BackColor = System.Drawing.Color.Transparent;
            this.lblScale.ForeColor = System.Drawing.Color.Lime;
            this.lblScale.Location = new System.Drawing.Point(12, 69);
            this.lblScale.Name = "lblScale";
            this.lblScale.Size = new System.Drawing.Size(27, 13);
            this.lblScale.TabIndex = 4;
            this.lblScale.Text = "Лад";
            // 
            // cmbBoxScale
            // 
            this.cmbBoxScale.BackColor = System.Drawing.Color.CornflowerBlue;
            this.cmbBoxScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxScale.FormattingEnabled = true;
            this.cmbBoxScale.Location = new System.Drawing.Point(113, 66);
            this.cmbBoxScale.Name = "cmbBoxScale";
            this.cmbBoxScale.Size = new System.Drawing.Size(282, 21);
            this.cmbBoxScale.TabIndex = 5;
            // 
            // lblArpNotesCount
            // 
            this.lblArpNotesCount.AutoSize = true;
            this.lblArpNotesCount.BackColor = System.Drawing.Color.Transparent;
            this.lblArpNotesCount.ForeColor = System.Drawing.Color.Lime;
            this.lblArpNotesCount.Location = new System.Drawing.Point(12, 96);
            this.lblArpNotesCount.Name = "lblArpNotesCount";
            this.lblArpNotesCount.Size = new System.Drawing.Size(61, 13);
            this.lblArpNotesCount.TabIndex = 6;
            this.lblArpNotesCount.Text = "Кол-во нот";
            // 
            // txtArpNotesCount
            // 
            this.txtArpNotesCount.BackColor = System.Drawing.Color.LightYellow;
            this.txtArpNotesCount.Location = new System.Drawing.Point(113, 93);
            this.txtArpNotesCount.Name = "txtArpNotesCount";
            this.txtArpNotesCount.Size = new System.Drawing.Size(282, 20);
            this.txtArpNotesCount.TabIndex = 7;
            this.txtArpNotesCount.Text = "4";
            this.txtArpNotesCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtNumeric_KeyPress);
            // 
            // lblTactsNumber
            // 
            this.lblTactsNumber.AutoSize = true;
            this.lblTactsNumber.BackColor = System.Drawing.Color.Transparent;
            this.lblTactsNumber.ForeColor = System.Drawing.Color.Lime;
            this.lblTactsNumber.Location = new System.Drawing.Point(12, 122);
            this.lblTactsNumber.Name = "lblTactsNumber";
            this.lblTactsNumber.Size = new System.Drawing.Size(78, 13);
            this.lblTactsNumber.TabIndex = 8;
            this.lblTactsNumber.Text = "Кол-во тактов";
            // 
            // txtTactsNumber
            // 
            this.txtTactsNumber.BackColor = System.Drawing.Color.LightYellow;
            this.txtTactsNumber.Location = new System.Drawing.Point(113, 119);
            this.txtTactsNumber.Name = "txtTactsNumber";
            this.txtTactsNumber.Size = new System.Drawing.Size(282, 20);
            this.txtTactsNumber.TabIndex = 9;
            this.txtTactsNumber.Text = "1";
            this.txtTactsNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtNumeric_KeyPress);
            // 
            // lblRepeatsNumber
            // 
            this.lblRepeatsNumber.AutoSize = true;
            this.lblRepeatsNumber.BackColor = System.Drawing.Color.Transparent;
            this.lblRepeatsNumber.ForeColor = System.Drawing.Color.Lime;
            this.lblRepeatsNumber.Location = new System.Drawing.Point(12, 148);
            this.lblRepeatsNumber.Name = "lblRepeatsNumber";
            this.lblRepeatsNumber.Size = new System.Drawing.Size(91, 13);
            this.lblRepeatsNumber.TabIndex = 10;
            this.lblRepeatsNumber.Text = "Кол-во повторов";
            // 
            // txtRepeatsNumber
            // 
            this.txtRepeatsNumber.BackColor = System.Drawing.Color.LightYellow;
            this.txtRepeatsNumber.Location = new System.Drawing.Point(113, 145);
            this.txtRepeatsNumber.Name = "txtRepeatsNumber";
            this.txtRepeatsNumber.Size = new System.Drawing.Size(282, 20);
            this.txtRepeatsNumber.TabIndex = 11;
            this.txtRepeatsNumber.Text = "0";
            this.txtRepeatsNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtNumeric_KeyPress);
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.DarkViolet;
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.Location = new System.Drawing.Point(12, 171);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(383, 42);
            this.btnGenerate.TabIndex = 12;
            this.btnGenerate.Text = "Генерировать";
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnPlayMidi
            // 
            this.btnPlayMidi.BackColor = System.Drawing.Color.DarkViolet;
            this.btnPlayMidi.ForeColor = System.Drawing.Color.White;
            this.btnPlayMidi.Location = new System.Drawing.Point(401, 10);
            this.btnPlayMidi.Name = "btnPlayMidi";
            this.btnPlayMidi.Size = new System.Drawing.Size(299, 23);
            this.btnPlayMidi.TabIndex = 13;
            this.btnPlayMidi.Text = "Воспроизвести MIDI";
            this.btnPlayMidi.UseVisualStyleBackColor = false;
            this.btnPlayMidi.Click += new System.EventHandler(this.BtnPlayMidi_Click);
            // 
            // txtProgression
            // 
            this.txtProgression.BackColor = System.Drawing.Color.LightYellow;
            this.txtProgression.Location = new System.Drawing.Point(401, 37);
            this.txtProgression.Multiline = true;
            this.txtProgression.Name = "txtProgression";
            this.txtProgression.ReadOnly = true;
            this.txtProgression.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtProgression.Size = new System.Drawing.Size(299, 230);
            this.txtProgression.TabIndex = 14;
            // 
            // panelNoteDisplay
            // 
            this.panelNoteDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelNoteDisplay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelNoteDisplay.Location = new System.Drawing.Point(12, 275);
            this.panelNoteDisplay.Name = "panelNoteDisplay";
            this.panelNoteDisplay.Size = new System.Drawing.Size(688, 426);
            this.panelNoteDisplay.TabIndex = 15;
            // 
            // rbKeyboard
            // 
            this.rbKeyboard.AutoSize = true;
            this.rbKeyboard.BackColor = System.Drawing.Color.DarkViolet;
            this.rbKeyboard.Checked = true;
            this.rbKeyboard.ForeColor = System.Drawing.Color.Lime;
            this.rbKeyboard.Location = new System.Drawing.Point(15, 250);
            this.rbKeyboard.Name = "rbKeyboard";
            this.rbKeyboard.Size = new System.Drawing.Size(84, 17);
            this.rbKeyboard.TabIndex = 17;
            this.rbKeyboard.TabStop = true;
            this.rbKeyboard.Text = "Клавиатура";
            this.rbKeyboard.UseVisualStyleBackColor = false;
            this.rbKeyboard.CheckedChanged += new System.EventHandler(this.rbKeyboard_CheckedChanged);
            // 
            // rbStaff
            // 
            this.rbStaff.AutoSize = true;
            this.rbStaff.BackColor = System.Drawing.Color.DarkViolet;
            this.rbStaff.ForeColor = System.Drawing.Color.Lime;
            this.rbStaff.Location = new System.Drawing.Point(113, 250);
            this.rbStaff.Name = "rbStaff";
            this.rbStaff.Size = new System.Drawing.Size(90, 17);
            this.rbStaff.TabIndex = 18;
            this.rbStaff.Text = "Нотный стан";
            this.rbStaff.UseVisualStyleBackColor = false;
            this.rbStaff.CheckedChanged += new System.EventHandler(this.rbStaff_CheckedChanged);
            // 
            // lblNoteDisplay
            // 
            this.lblNoteDisplay.AutoSize = true;
            this.lblNoteDisplay.BackColor = System.Drawing.Color.DarkViolet;
            this.lblNoteDisplay.ForeColor = System.Drawing.Color.Lime;
            this.lblNoteDisplay.Location = new System.Drawing.Point(12, 230);
            this.lblNoteDisplay.Name = "lblNoteDisplay";
            this.lblNoteDisplay.Size = new System.Drawing.Size(102, 13);
            this.lblNoteDisplay.TabIndex = 16;
            this.lblNoteDisplay.Text = "Визуализация нот:";
            // 
            // noteDisplayControl
            // 
            this.noteDisplayControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.noteDisplayControl.Location = new System.Drawing.Point(0, 0);
            this.noteDisplayControl.Name = "noteDisplayControl";
            this.noteDisplayControl.Size = new System.Drawing.Size(0, 0);
            this.noteDisplayControl.TabIndex = 0;
            // 
            // Form1
            // 
            this.BackColor = System.Drawing.Color.CornflowerBlue;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(712, 711);
            this.Controls.Add(this.rbStaff);
            this.Controls.Add(this.rbKeyboard);
            this.Controls.Add(this.lblNoteDisplay);
            this.Controls.Add(this.panelNoteDisplay);
            this.Controls.Add(this.txtProgression);
            this.Controls.Add(this.btnPlayMidi);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.txtRepeatsNumber);
            this.Controls.Add(this.lblRepeatsNumber);
            this.Controls.Add(this.txtTactsNumber);
            this.Controls.Add(this.lblTactsNumber);
            this.Controls.Add(this.txtArpNotesCount);
            this.Controls.Add(this.lblArpNotesCount);
            this.Controls.Add(this.cmbBoxScale);
            this.Controls.Add(this.lblScale);
            this.Controls.Add(this.cmbBoxCategory);
            this.Controls.Add(this.lblCategory);
            this.Controls.Add(this.cmbBoxTonica);
            this.Controls.Add(this.lblTonica);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(728, 640);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Arpeggio Generator";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}