using System.Windows.Forms;
using System.Drawing;

namespace SpirePresetsGenerator.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private NumericUpDown presetCount;
        private ComboBox presetType;
        private CheckBox enableArp;
        private Panel arpPanel;
        private ComboBox arpMode;
        private ComboBox arpOctave;
        private ComboBox arpSpeed;
        private ComboBox arpPattern;
        private ComboBox scaleCategory;
        private ComboBox scaleSelection;
        private TextBox authorName;
        private Button btnGenerate;
        private Label statusLabel;
        private ListBox presetList;
        private Panel containerPanel;

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
            this.containerPanel = new System.Windows.Forms.Panel();
            this.title = new System.Windows.Forms.Label();
            this.subtitle = new System.Windows.Forms.Label();
            this.lblCount = new System.Windows.Forms.Label();
            this.presetCount = new System.Windows.Forms.NumericUpDown();
            this.lblType = new System.Windows.Forms.Label();
            this.presetType = new System.Windows.Forms.ComboBox();
            this.enableArp = new System.Windows.Forms.CheckBox();
            this.arpPanel = new System.Windows.Forms.Panel();
            this.lblArpMode = new System.Windows.Forms.Label();
            this.arpMode = new System.Windows.Forms.ComboBox();
            this.lblArpOct = new System.Windows.Forms.Label();
            this.arpOctave = new System.Windows.Forms.ComboBox();
            this.lblArpSpd = new System.Windows.Forms.Label();
            this.arpSpeed = new System.Windows.Forms.ComboBox();
            this.lblArpPat = new System.Windows.Forms.Label();
            this.arpPattern = new System.Windows.Forms.ComboBox();
            this.lblScaleCategory = new System.Windows.Forms.Label();
            this.scaleCategory = new System.Windows.Forms.ComboBox();
            this.lblScaleSelection = new System.Windows.Forms.Label();
            this.scaleSelection = new System.Windows.Forms.ComboBox();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.authorName = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.presetList = new System.Windows.Forms.ListBox();
            this.containerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.presetCount)).BeginInit();
            this.arpPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // containerPanel
            // 
            this.containerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.containerPanel.Controls.Add(this.title);
            this.containerPanel.Controls.Add(this.subtitle);
            this.containerPanel.Controls.Add(this.lblCount);
            this.containerPanel.Controls.Add(this.presetCount);
            this.containerPanel.Controls.Add(this.lblType);
            this.containerPanel.Controls.Add(this.presetType);
            this.containerPanel.Controls.Add(this.enableArp);
            this.containerPanel.Controls.Add(this.arpPanel);
            this.containerPanel.Controls.Add(this.lblAuthor);
            this.containerPanel.Controls.Add(this.authorName);
            this.containerPanel.Controls.Add(this.btnGenerate);
            this.containerPanel.Controls.Add(this.statusLabel);
            this.containerPanel.Controls.Add(this.presetList);
            this.containerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.containerPanel.Location = new System.Drawing.Point(0, 0);
            this.containerPanel.Name = "containerPanel";
            this.containerPanel.Padding = new System.Windows.Forms.Padding(20);
            this.containerPanel.Size = new System.Drawing.Size(727, 464);
            this.containerPanel.TabIndex = 0;
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(126)))), ((int)(((byte)(234)))));
            this.title.Location = new System.Drawing.Point(11, 9);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(285, 30);
            this.title.TabIndex = 0;
            this.title.Text = "🎹 Spire Preset Generator";
            // 
            // subtitle
            // 
            this.subtitle.AutoSize = true;
            this.subtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.subtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.subtitle.Location = new System.Drawing.Point(16, 42);
            this.subtitle.Name = "subtitle";
            this.subtitle.Size = new System.Drawing.Size(242, 15);
            this.subtitle.TabIndex = 1;
            this.subtitle.Text = "Генератор пресетов для Reveal Sound Spire";
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblCount.Location = new System.Drawing.Point(16, 74);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(126, 13);
            this.lblCount.TabIndex = 2;
            this.lblCount.Text = "Количество пресетов:";
            // 
            // presetCount
            // 
            this.presetCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.presetCount.Location = new System.Drawing.Point(16, 93);
            this.presetCount.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.presetCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.presetCount.Name = "presetCount";
            this.presetCount.Size = new System.Drawing.Size(178, 23);
            this.presetCount.TabIndex = 3;
            this.presetCount.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblType.Location = new System.Drawing.Point(200, 74);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(61, 13);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "Тип звука:";
            // 
            // presetType
            // 
            this.presetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.presetType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.presetType.Items.AddRange(new object[] {
            "random",
            "lead",
            "pad",
            "bass",
            "pluck",
            "fx",
            "sequence",
            "atmo",
            "key",
            "gt",
            "sy"});
            this.presetType.Location = new System.Drawing.Point(200, 93);
            this.presetType.Name = "presetType";
            this.presetType.Size = new System.Drawing.Size(165, 23);
            this.presetType.TabIndex = 5;
            // 
            // enableArp
            // 
            this.enableArp.AutoSize = true;
            this.enableArp.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.enableArp.Location = new System.Drawing.Point(371, 95);
            this.enableArp.Name = "enableArp";
            this.enableArp.Size = new System.Drawing.Size(157, 19);
            this.enableArp.TabIndex = 6;
            this.enableArp.Text = "Включить арпеджиатор";
            this.enableArp.CheckedChanged += new System.EventHandler(this.enableArp_CheckedChanged);
            // 
            // arpPanel
            // 
            this.arpPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.arpPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.arpPanel.Controls.Add(this.lblArpMode);
            this.arpPanel.Controls.Add(this.arpMode);
            this.arpPanel.Controls.Add(this.lblArpOct);
            this.arpPanel.Controls.Add(this.arpOctave);
            this.arpPanel.Controls.Add(this.lblArpSpd);
            this.arpPanel.Controls.Add(this.arpSpeed);
            this.arpPanel.Controls.Add(this.lblArpPat);
            this.arpPanel.Controls.Add(this.arpPattern);
            this.arpPanel.Controls.Add(this.lblScaleCategory);
            this.arpPanel.Controls.Add(this.scaleCategory);
            this.arpPanel.Controls.Add(this.lblScaleSelection);
            this.arpPanel.Controls.Add(this.scaleSelection);
            this.arpPanel.Location = new System.Drawing.Point(16, 124);
            this.arpPanel.Name = "arpPanel";
            this.arpPanel.Size = new System.Drawing.Size(699, 127);
            this.arpPanel.TabIndex = 7;
            this.arpPanel.Visible = false;
            // 
            // lblArpMode
            // 
            this.lblArpMode.AutoSize = true;
            this.lblArpMode.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblArpMode.Location = new System.Drawing.Point(5, 8);
            this.lblArpMode.Name = "lblArpMode";
            this.lblArpMode.Size = new System.Drawing.Size(48, 13);
            this.lblArpMode.TabIndex = 0;
            this.lblArpMode.Text = "Режим:";
            // 
            // arpMode
            // 
            this.arpMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.arpMode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.arpMode.Items.AddRange(new object[] {
            "0 - Up",
            "0.2 - Down",
            "0.4 - Up-Down",
            "0.6 - Random",
            "0.8 - As Played",
            "1.0 - Step"});
            this.arpMode.Location = new System.Drawing.Point(8, 28);
            this.arpMode.Name = "arpMode";
            this.arpMode.Size = new System.Drawing.Size(170, 23);
            this.arpMode.TabIndex = 1;
            // 
            // lblArpOct
            // 
            this.lblArpOct.AutoSize = true;
            this.lblArpOct.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblArpOct.Location = new System.Drawing.Point(184, 8);
            this.lblArpOct.Name = "lblArpOct";
            this.lblArpOct.Size = new System.Drawing.Size(50, 13);
            this.lblArpOct.TabIndex = 2;
            this.lblArpOct.Text = "Октавы:";
            // 
            // arpOctave
            // 
            this.arpOctave.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.arpOctave.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.arpOctave.Items.AddRange(new object[] {
            "0 - 1 октава",
            "0.25 - 2 октавы",
            "0.5 - 3 октавы",
            "0.75 - 4 октавы"});
            this.arpOctave.Location = new System.Drawing.Point(184, 28);
            this.arpOctave.Name = "arpOctave";
            this.arpOctave.Size = new System.Drawing.Size(221, 23);
            this.arpOctave.TabIndex = 3;
            // 
            // lblArpSpd
            // 
            this.lblArpSpd.AutoSize = true;
            this.lblArpSpd.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblArpSpd.Location = new System.Drawing.Point(411, 8);
            this.lblArpSpd.Name = "lblArpSpd";
            this.lblArpSpd.Size = new System.Drawing.Size(60, 13);
            this.lblArpSpd.TabIndex = 4;
            this.lblArpSpd.Text = "Скорость:";
            // 
            // arpSpeed
            // 
            this.arpSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.arpSpeed.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.arpSpeed.Items.AddRange(new object[] {
            "0.032 - 1/32",
            "0.024 - 1/16T",
            "0.016 - 1/16",
            "0.012 - 1/8T",
            "0.008 - 1/8",
            "0.006 - 1/4T",
            "0.004 - 1/4"});
            this.arpSpeed.Location = new System.Drawing.Point(411, 28);
            this.arpSpeed.Name = "arpSpeed";
            this.arpSpeed.Size = new System.Drawing.Size(276, 23);
            this.arpSpeed.TabIndex = 5;
            // 
            // lblArpPat
            // 
            this.lblArpPat.AutoSize = true;
            this.lblArpPat.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblArpPat.Location = new System.Drawing.Point(8, 62);
            this.lblArpPat.Name = "lblArpPat";
            this.lblArpPat.Size = new System.Drawing.Size(76, 13);
            this.lblArpPat.TabIndex = 6;
            this.lblArpPat.Text = "Паттерн нот:";
            // 
            // arpPattern
            // 
            this.arpPattern.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.arpPattern.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.arpPattern.Items.AddRange(new object[] {
            "melody",
            "chord",
            "rhythmic",
            "bass",
            "random"});
            this.arpPattern.Location = new System.Drawing.Point(8, 82);
            this.arpPattern.Name = "arpPattern";
            this.arpPattern.Size = new System.Drawing.Size(170, 23);
            this.arpPattern.TabIndex = 7;
            // 
            // lblScaleCategory
            // 
            this.lblScaleCategory.AutoSize = true;
            this.lblScaleCategory.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblScaleCategory.Location = new System.Drawing.Point(184, 62);
            this.lblScaleCategory.Name = "lblScaleCategory";
            this.lblScaleCategory.Size = new System.Drawing.Size(66, 13);
            this.lblScaleCategory.TabIndex = 8;
            this.lblScaleCategory.Text = "Категория:";
            // 
            // scaleCategory
            // 
            this.scaleCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scaleCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.scaleCategory.Location = new System.Drawing.Point(184, 82);
            this.scaleCategory.Name = "scaleCategory";
            this.scaleCategory.Size = new System.Drawing.Size(221, 23);
            this.scaleCategory.TabIndex = 9;
            this.scaleCategory.SelectedIndexChanged += new System.EventHandler(this.scaleCategory_SelectedIndexChanged);
            // 
            // lblScaleSelection
            // 
            this.lblScaleSelection.AutoSize = true;
            this.lblScaleSelection.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblScaleSelection.Location = new System.Drawing.Point(411, 62);
            this.lblScaleSelection.Name = "lblScaleSelection";
            this.lblScaleSelection.Size = new System.Drawing.Size(46, 13);
            this.lblScaleSelection.TabIndex = 10;
            this.lblScaleSelection.Text = "Шкала:";
            // 
            // scaleSelection
            // 
            this.scaleSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scaleSelection.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.scaleSelection.Location = new System.Drawing.Point(411, 82);
            this.scaleSelection.Name = "scaleSelection";
            this.scaleSelection.Size = new System.Drawing.Size(276, 23);
            this.scaleSelection.TabIndex = 11;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblAuthor.Location = new System.Drawing.Point(12, 257);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(74, 13);
            this.lblAuthor.TabIndex = 8;
            this.lblAuthor.Text = "Имя автора:";
            // 
            // authorName
            // 
            this.authorName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.authorName.Location = new System.Drawing.Point(12, 276);
            this.authorName.Name = "authorName";
            this.authorName.Size = new System.Drawing.Size(699, 23);
            this.authorName.TabIndex = 9;
            this.authorName.Text = "Preset Generator v1.1";
            // 
            // btnGenerate
            // 
            this.btnGenerate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(126)))), ((int)(((byte)(234)))));
            this.btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerate.ForeColor = System.Drawing.Color.White;
            this.btnGenerate.Location = new System.Drawing.Point(12, 311);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(160, 34);
            this.btnGenerate.TabIndex = 10;
            this.btnGenerate.Text = "Генерировать";
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Location = new System.Drawing.Point(12, 348);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(699, 32);
            this.statusLabel.TabIndex = 12;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.statusLabel.Visible = false;
            // 
            // presetList
            // 
            this.presetList.Location = new System.Drawing.Point(12, 383);
            this.presetList.Name = "presetList";
            this.presetList.Size = new System.Drawing.Size(699, 69);
            this.presetList.TabIndex = 13;
            this.presetList.Visible = false;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(727, 464);
            this.Controls.Add(this.containerPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PsyShout Spire Preset Generator";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.containerPanel.ResumeLayout(false);
            this.containerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.presetCount)).EndInit();
            this.arpPanel.ResumeLayout(false);
            this.arpPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        private Label title;
        private Label subtitle;
        private Label lblCount;
        private Label lblType;
        private Label lblArpMode;
        private Label lblArpOct;
        private Label lblArpSpd;
        private Label lblArpPat;
        private Label lblScaleCategory;
        private Label lblScaleSelection;
        private Label lblAuthor;
    }
} 