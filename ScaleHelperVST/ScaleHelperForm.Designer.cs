// ScaleHelperForm.Designer.cs - Полный дизайнер формы
namespace ScaleHelperVST
{
    partial class ScaleHelperForm
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
            this.comboBoxCategory = new System.Windows.Forms.ComboBox();
            this.comboBoxScale = new System.Windows.Forms.ComboBox();
            this.comboBoxTonic = new System.Windows.Forms.ComboBox();
            this.buttonShowNotes = new System.Windows.Forms.Button();
            this.richTextBoxNotes = new System.Windows.Forms.RichTextBox();
            this.labelCategory = new System.Windows.Forms.Label();
            this.labelScale = new System.Windows.Forms.Label();
            this.labelTonic = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxCategory
            // 
            this.comboBoxCategory.BackColor = System.Drawing.Color.YellowGreen;
            this.comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboBoxCategory.FormattingEnabled = true;
            this.comboBoxCategory.Location = new System.Drawing.Point(105, 12);
            this.comboBoxCategory.Name = "comboBoxCategory";
            this.comboBoxCategory.Size = new System.Drawing.Size(495, 23);
            this.comboBoxCategory.TabIndex = 0;
            // 
            // comboBoxScale
            // 
            this.comboBoxScale.BackColor = System.Drawing.Color.YellowGreen;
            this.comboBoxScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxScale.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboBoxScale.FormattingEnabled = true;
            this.comboBoxScale.Location = new System.Drawing.Point(105, 38);
            this.comboBoxScale.Name = "comboBoxScale";
            this.comboBoxScale.Size = new System.Drawing.Size(495, 23);
            this.comboBoxScale.TabIndex = 1;
            // 
            // comboBoxTonic
            // 
            this.comboBoxTonic.BackColor = System.Drawing.Color.YellowGreen;
            this.comboBoxTonic.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTonic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboBoxTonic.FormattingEnabled = true;
            this.comboBoxTonic.Location = new System.Drawing.Point(105, 64);
            this.comboBoxTonic.Name = "comboBoxTonic";
            this.comboBoxTonic.Size = new System.Drawing.Size(495, 23);
            this.comboBoxTonic.TabIndex = 2;
            // 
            // buttonShowNotes
            // 
            this.buttonShowNotes.BackColor = System.Drawing.Color.YellowGreen;
            this.buttonShowNotes.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonShowNotes.ForeColor = System.Drawing.Color.Black;
            this.buttonShowNotes.Location = new System.Drawing.Point(606, 9);
            this.buttonShowNotes.Name = "buttonShowNotes";
            this.buttonShowNotes.Size = new System.Drawing.Size(144, 75);
            this.buttonShowNotes.TabIndex = 3;
            this.buttonShowNotes.Text = "Показать ноты";
            this.buttonShowNotes.UseVisualStyleBackColor = false;
            // 
            // richTextBoxNotes
            // 
            this.richTextBoxNotes.BackColor = System.Drawing.Color.YellowGreen;
            this.richTextBoxNotes.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBoxNotes.Location = new System.Drawing.Point(12, 93);
            this.richTextBoxNotes.Name = "richTextBoxNotes";
            this.richTextBoxNotes.ReadOnly = true;
            this.richTextBoxNotes.Size = new System.Drawing.Size(738, 261);
            this.richTextBoxNotes.TabIndex = 4;
            this.richTextBoxNotes.Text = "Выбери категорию, гамму и тонику, затем нажми \"Показать ноты\"";
            // 
            // labelCategory
            // 
            this.labelCategory.AutoSize = true;
            this.labelCategory.BackColor = System.Drawing.Color.Transparent;
            this.labelCategory.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelCategory.ForeColor = System.Drawing.Color.Lime;
            this.labelCategory.Location = new System.Drawing.Point(12, 15);
            this.labelCategory.Name = "labelCategory";
            this.labelCategory.Size = new System.Drawing.Size(82, 15);
            this.labelCategory.TabIndex = 5;
            this.labelCategory.Text = "Категория:";
            // 
            // labelScale
            // 
            this.labelScale.AutoSize = true;
            this.labelScale.BackColor = System.Drawing.Color.Transparent;
            this.labelScale.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelScale.ForeColor = System.Drawing.Color.Lime;
            this.labelScale.Location = new System.Drawing.Point(12, 42);
            this.labelScale.Name = "labelScale";
            this.labelScale.Size = new System.Drawing.Size(55, 15);
            this.labelScale.TabIndex = 6;
            this.labelScale.Text = "Гамма:";
            // 
            // labelTonic
            // 
            this.labelTonic.AutoSize = true;
            this.labelTonic.BackColor = System.Drawing.Color.Transparent;
            this.labelTonic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelTonic.ForeColor = System.Drawing.Color.DarkOrange;
            this.labelTonic.Location = new System.Drawing.Point(12, 69);
            this.labelTonic.Name = "labelTonic";
            this.labelTonic.Size = new System.Drawing.Size(58, 15);
            this.labelTonic.TabIndex = 7;
            this.labelTonic.Text = "Тоника:";
            // 
            // ScaleHelperForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::ScaleHelperVST.Properties.Resources.ABSTRACT__18_;
            this.ClientSize = new System.Drawing.Size(788, 366);
            this.Controls.Add(this.labelTonic);
            this.Controls.Add(this.labelScale);
            this.Controls.Add(this.labelCategory);
            this.Controls.Add(this.richTextBoxNotes);
            this.Controls.Add(this.buttonShowNotes);
            this.Controls.Add(this.comboBoxTonic);
            this.Controls.Add(this.comboBoxScale);
            this.Controls.Add(this.comboBoxCategory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScaleHelperForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ScaleHelper";
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
        private System.Windows.Forms.Label labelScale;
        private System.Windows.Forms.Label labelTonic;
    }
}