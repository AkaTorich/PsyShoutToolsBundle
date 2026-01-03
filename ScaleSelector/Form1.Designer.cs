namespace ScaleSelector
{
    partial class ScaleSelectorForm
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
            this.components = new System.ComponentModel.Container();
            this.comboBoxCategory = new System.Windows.Forms.ComboBox();
            this.comboBoxScale = new System.Windows.Forms.ComboBox();
            this.comboBoxTonic = new System.Windows.Forms.ComboBox();
            this.buttonShowNotes = new System.Windows.Forms.Button();
            this.richTextBoxNotes = new System.Windows.Forms.RichTextBox();
            this.labelCategory = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();

            // 
            // comboBoxCategory
            // 
            this.comboBoxCategory.BackColor = System.Drawing.Color.YellowGreen;
            this.comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCategory.FormattingEnabled = true;
            this.comboBoxCategory.Location = new System.Drawing.Point(103, 15);
            this.comboBoxCategory.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxCategory.Name = "comboBoxCategory";
            this.comboBoxCategory.Size = new System.Drawing.Size(437, 24);
            this.comboBoxCategory.TabIndex = 0;
            // 
            // comboBoxScale
            // 
            this.comboBoxScale.BackColor = System.Drawing.Color.YellowGreen;
            this.comboBoxScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxScale.FormattingEnabled = true;
            this.comboBoxScale.Location = new System.Drawing.Point(103, 48);
            this.comboBoxScale.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxScale.Name = "comboBoxScale";
            this.comboBoxScale.Size = new System.Drawing.Size(437, 24);
            this.comboBoxScale.TabIndex = 1;
            // 
            // comboBoxTonic
            // 
            this.comboBoxTonic.BackColor = System.Drawing.Color.YellowGreen;
            this.comboBoxTonic.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTonic.FormattingEnabled = true;
            this.comboBoxTonic.Location = new System.Drawing.Point(103, 81);
            this.comboBoxTonic.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxTonic.Name = "comboBoxTonic";
            this.comboBoxTonic.Size = new System.Drawing.Size(437, 24);
            this.comboBoxTonic.TabIndex = 2;
            // 
            // buttonShowNotes
            // 
            this.buttonShowNotes.BackColor = System.Drawing.Color.YellowGreen;
            this.buttonShowNotes.Location = new System.Drawing.Point(549, 15);
            this.buttonShowNotes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonShowNotes.Name = "buttonShowNotes";
            this.buttonShowNotes.Size = new System.Drawing.Size(179, 92);
            this.buttonShowNotes.TabIndex = 3;
            this.buttonShowNotes.Text = "Показать";
            this.buttonShowNotes.UseVisualStyleBackColor = false;
            this.buttonShowNotes.Click += new System.EventHandler(this.ButtonShowNotes_Click);
            // 
            // richTextBoxNotes
            // 
            this.richTextBoxNotes.BackColor = System.Drawing.Color.LightGreen;
            this.richTextBoxNotes.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBoxNotes.Location = new System.Drawing.Point(16, 114);
            this.richTextBoxNotes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.richTextBoxNotes.Name = "richTextBoxNotes";
            this.richTextBoxNotes.Size = new System.Drawing.Size(711, 219);
            this.richTextBoxNotes.TabIndex = 4;
            this.richTextBoxNotes.Text = "";
            // 
            // labelCategory
            // 
            this.labelCategory.AutoSize = true;
            this.labelCategory.BackColor = System.Drawing.Color.Transparent;
            this.labelCategory.ForeColor = System.Drawing.Color.Lime;
            this.labelCategory.Location = new System.Drawing.Point(16, 18);
            this.labelCategory.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCategory.Name = "labelCategory";
            this.labelCategory.Size = new System.Drawing.Size(78, 16);
            this.labelCategory.TabIndex = 5;
            this.labelCategory.Text = "Категория:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.Lime;
            this.label1.Location = new System.Drawing.Point(16, 52);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Гамма:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(16, 85);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "Тоника:";
            // 
            // ScaleSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::ScaleSelector.Properties.Resources.ABSTRACT__18_;
            this.ClientSize = new System.Drawing.Size(744, 350);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelCategory);
            this.Controls.Add(this.richTextBoxNotes);
            this.Controls.Add(this.buttonShowNotes);
            this.Controls.Add(this.comboBoxTonic);
            this.Controls.Add(this.comboBoxScale);
            this.Controls.Add(this.comboBoxCategory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "ScaleSelectorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ScaleSelector";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.ComboBox comboBoxScale;
        private System.Windows.Forms.ComboBox comboBoxTonic;
        private System.Windows.Forms.Button buttonShowNotes;
        private System.Windows.Forms.RichTextBox richTextBoxNotes;
        private System.Windows.Forms.Label labelCategory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}