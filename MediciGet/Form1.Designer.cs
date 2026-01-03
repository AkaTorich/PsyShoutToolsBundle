namespace MediciGet
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
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
                cts?.Cancel();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора — не изменяйте
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panelCards = new System.Windows.Forms.Panel();
            this.listBoxFinalStack = new System.Windows.Forms.ListBox();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonFindChain = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.numericUpDownShuffleTimes = new System.Windows.Forms.NumericUpDown();
            this.labelShuffleTimes = new System.Windows.Forms.Label();
            this.labelAttemptCounter = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShuffleTimes)).BeginInit();
            this.SuspendLayout();
            // 
            // panelCards
            // 
            this.panelCards.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCards.Location = new System.Drawing.Point(12, 12);
            this.panelCards.Name = "panelCards";
            this.panelCards.Size = new System.Drawing.Size(309, 344);
            this.panelCards.TabIndex = 0;
            // 
            // listBoxFinalStack
            // 
            this.listBoxFinalStack.FormattingEnabled = true;
            this.listBoxFinalStack.Location = new System.Drawing.Point(327, 28);
            this.listBoxFinalStack.Name = "listBoxFinalStack";
            this.listBoxFinalStack.Size = new System.Drawing.Size(64, 433);
            this.listBoxFinalStack.TabIndex = 1;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Location = new System.Drawing.Point(397, 28);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(379, 436);
            this.textBoxLog.TabIndex = 2;
            // 
            // buttonFindChain
            // 
            this.buttonFindChain.Location = new System.Drawing.Point(12, 392);
            this.buttonFindChain.Name = "buttonFindChain";
            this.buttonFindChain.Size = new System.Drawing.Size(309, 20);
            this.buttonFindChain.TabIndex = 3;
            this.buttonFindChain.Text = "Найти цепочку";
            this.buttonFindChain.UseVisualStyleBackColor = true;
            this.buttonFindChain.Click += new System.EventHandler(this.ButtonFindChain_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(12, 418);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(309, 20);
            this.buttonReset.TabIndex = 5;
            this.buttonReset.Text = "Сбросить";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(12, 444);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(309, 20);
            this.buttonStop.TabIndex = 6;
            this.buttonStop.Text = "Стоп";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // numericUpDownShuffleTimes
            // 
            this.numericUpDownShuffleTimes.Location = new System.Drawing.Point(140, 366);
            this.numericUpDownShuffleTimes.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownShuffleTimes.Name = "numericUpDownShuffleTimes";
            this.numericUpDownShuffleTimes.Size = new System.Drawing.Size(181, 20);
            this.numericUpDownShuffleTimes.TabIndex = 7;
            this.numericUpDownShuffleTimes.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // labelShuffleTimes
            // 
            this.labelShuffleTimes.AutoSize = true;
            this.labelShuffleTimes.Location = new System.Drawing.Point(12, 368);
            this.labelShuffleTimes.Name = "labelShuffleTimes";
            this.labelShuffleTimes.Size = new System.Drawing.Size(125, 13);
            this.labelShuffleTimes.TabIndex = 8;
            this.labelShuffleTimes.Text = "Количество тасований:";
            // 
            // labelAttemptCounter
            // 
            this.labelAttemptCounter.AutoSize = true;
            this.labelAttemptCounter.Location = new System.Drawing.Point(394, 12);
            this.labelAttemptCounter.Name = "labelAttemptCounter";
            this.labelAttemptCounter.Size = new System.Drawing.Size(124, 13);
            this.labelAttemptCounter.TabIndex = 9;
            this.labelAttemptCounter.Text = "Количество попыток: 0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(327, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Цель:";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(788, 477);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelAttemptCounter);
            this.Controls.Add(this.labelShuffleTimes);
            this.Controls.Add(this.numericUpDownShuffleTimes);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonFindChain);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.listBoxFinalStack);
            this.Controls.Add(this.panelCards);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Пасьянс Медичи";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShuffleTimes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelCards;
        private System.Windows.Forms.ListBox listBoxFinalStack;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button buttonFindChain;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.NumericUpDown numericUpDownShuffleTimes;
        private System.Windows.Forms.Label labelShuffleTimes;
        private System.Windows.Forms.Label labelAttemptCounter; // Новый Label
        private System.Windows.Forms.Label label1;
    }
}
