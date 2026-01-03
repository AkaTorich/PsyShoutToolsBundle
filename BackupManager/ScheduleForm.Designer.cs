using System;
using System.Windows.Forms;

namespace BackupManager.Forms
{
    partial class ScheduleForm
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
            this.components = new System.ComponentModel.Container();
            this.Text = "Планирование резервного копирования";
            this.Size = new System.Drawing.Size(500, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Группа общих настроек
            GroupBox generalGroup = new GroupBox();
            generalGroup.Text = "Общие настройки";
            generalGroup.Location = new System.Drawing.Point(12, 12);
            generalGroup.Size = new System.Drawing.Size(460, 190);
            generalGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Название задания
            Label nameLabel = new Label();
            nameLabel.Text = "Название:";
            nameLabel.Location = new System.Drawing.Point(10, 25);
            nameLabel.AutoSize = true;

            this.nameTextBox = new TextBox();
            this.nameTextBox.Location = new System.Drawing.Point(150, 22);
            this.nameTextBox.Size = new System.Drawing.Size(290, 23);
            this.nameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Исходная директория
            Label sourceLabel = new Label();
            sourceLabel.Text = "Исходная директория:";
            sourceLabel.Location = new System.Drawing.Point(10, 55);
            sourceLabel.AutoSize = true;

            this.sourceDirectoryTextBox = new TextBox();
            this.sourceDirectoryTextBox.Location = new System.Drawing.Point(150, 52);
            this.sourceDirectoryTextBox.Size = new System.Drawing.Size(220, 23);
            this.sourceDirectoryTextBox.ReadOnly = true;
            this.sourceDirectoryTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Button browseSourceButton = new Button();
            browseSourceButton.Text = "Обзор...";
            browseSourceButton.Location = new System.Drawing.Point(380, 51);
            browseSourceButton.Size = new System.Drawing.Size(60, 25);
            browseSourceButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseSourceButton.Click += new EventHandler(this.BrowseSourceButton_Click);

            // Директория резервной копии
            Label backupLabel = new Label();
            backupLabel.Text = "Директория копии:";
            backupLabel.Location = new System.Drawing.Point(10, 85);
            backupLabel.AutoSize = true;

            this.backupDirectoryTextBox = new TextBox();
            this.backupDirectoryTextBox.Location = new System.Drawing.Point(150, 82);
            this.backupDirectoryTextBox.Size = new System.Drawing.Size(220, 23);
            this.backupDirectoryTextBox.ReadOnly = true;
            this.backupDirectoryTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            Button browseBackupButton = new Button();
            browseBackupButton.Text = "Обзор...";
            browseBackupButton.Location = new System.Drawing.Point(380, 81);
            browseBackupButton.Size = new System.Drawing.Size(60, 25);
            browseBackupButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseBackupButton.Click += new EventHandler(this.BrowseBackupButton_Click);

            // Активность задания
            this.enabledCheckBox = new CheckBox();
            this.enabledCheckBox.Text = "Активно";
            this.enabledCheckBox.Location = new System.Drawing.Point(150, 112);
            this.enabledCheckBox.AutoSize = true;

            // Тип синхронизации
            this.syncTypeLabel = new Label();
            this.syncTypeLabel.Text = "Тип синхронизации:";
            this.syncTypeLabel.Location = new System.Drawing.Point(10, 145);
            this.syncTypeLabel.AutoSize = true;

            this.syncTypeComboBox = new ComboBox();
            this.syncTypeComboBox.Location = new System.Drawing.Point(150, 142);
            this.syncTypeComboBox.Size = new System.Drawing.Size(290, 25);
            this.syncTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.syncTypeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Добавление элементов в группу общих настроек
            generalGroup.Controls.Add(nameLabel);
            generalGroup.Controls.Add(this.nameTextBox);
            generalGroup.Controls.Add(sourceLabel);
            generalGroup.Controls.Add(this.sourceDirectoryTextBox);
            generalGroup.Controls.Add(browseSourceButton);
            generalGroup.Controls.Add(backupLabel);
            generalGroup.Controls.Add(this.backupDirectoryTextBox);
            generalGroup.Controls.Add(browseBackupButton);
            generalGroup.Controls.Add(this.enabledCheckBox);
            generalGroup.Controls.Add(this.syncTypeLabel);
            generalGroup.Controls.Add(this.syncTypeComboBox);

            // Группа настроек расписания
            GroupBox scheduleGroup = new GroupBox();
            scheduleGroup.Text = "Расписание";
            scheduleGroup.Location = new System.Drawing.Point(12, 212);
            scheduleGroup.Size = new System.Drawing.Size(460, 280);
            scheduleGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Тип расписания
            Label scheduleTypeLabel = new Label();
            scheduleTypeLabel.Text = "Тип расписания:";
            scheduleTypeLabel.Location = new System.Drawing.Point(10, 25);
            scheduleTypeLabel.AutoSize = true;

            this.scheduleTypeComboBox = new ComboBox();
            this.scheduleTypeComboBox.Location = new System.Drawing.Point(150, 22);
            this.scheduleTypeComboBox.Size = new System.Drawing.Size(290, 25);
            this.scheduleTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.scheduleTypeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.scheduleTypeComboBox.SelectedIndexChanged += new EventHandler(this.ScheduleTypeComboBox_SelectedIndexChanged);

            // Дата и время для однократного запуска
            this.dateTimeLabel = new Label();
            this.dateTimeLabel.Text = "Дата и время:";
            this.dateTimeLabel.Location = new System.Drawing.Point(10, 60);
            this.dateTimeLabel.AutoSize = true;

            this.dateTimePicker = new DateTimePicker();
            this.dateTimePicker.Location = new System.Drawing.Point(150, 57);
            this.dateTimePicker.Size = new System.Drawing.Size(290, 25);
            this.dateTimePicker.Format = DateTimePickerFormat.Custom;
            this.dateTimePicker.CustomFormat = "dd.MM.yyyy HH:mm";
            this.dateTimePicker.ShowUpDown = true;
            this.dateTimePicker.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Время для ежедневного расписания
            this.hourLabel = new Label();
            this.hourLabel.Text = "Часы (0-23):";
            this.hourLabel.Location = new System.Drawing.Point(10, 90);
            this.hourLabel.AutoSize = true;

            this.hourNumericUpDown = new NumericUpDown();
            this.hourNumericUpDown.Location = new System.Drawing.Point(150, 88);
            this.hourNumericUpDown.Size = new System.Drawing.Size(60, 25);
            this.hourNumericUpDown.Minimum = 0;
            this.hourNumericUpDown.Maximum = 23;
            this.hourNumericUpDown.Value = 0;

            this.minuteLabel = new Label();
            this.minuteLabel.Text = "Минуты (0-59):";
            this.minuteLabel.Location = new System.Drawing.Point(230, 90);
            this.minuteLabel.AutoSize = true;

            this.minuteNumericUpDown = new NumericUpDown();
            this.minuteNumericUpDown.Location = new System.Drawing.Point(350, 88);
            this.minuteNumericUpDown.Size = new System.Drawing.Size(60, 25);
            this.minuteNumericUpDown.Minimum = 0;
            this.minuteNumericUpDown.Maximum = 59;
            this.minuteNumericUpDown.Value = 0;

            // День месяца для ежемесячного расписания
            this.dayLabel = new Label();
            this.dayLabel.Text = "День месяца (1-31):";
            this.dayLabel.Location = new System.Drawing.Point(10, 120);
            this.dayLabel.AutoSize = true;

            this.dayNumericUpDown = new NumericUpDown();
            this.dayNumericUpDown.Location = new System.Drawing.Point(150, 118);
            this.dayNumericUpDown.Size = new System.Drawing.Size(60, 25);
            this.dayNumericUpDown.Minimum = 1;
            this.dayNumericUpDown.Maximum = 31;
            this.dayNumericUpDown.Value = 1;

            // День недели для еженедельного расписания
            this.dayOfWeekLabel = new Label();
            this.dayOfWeekLabel.Text = "День недели:";
            this.dayOfWeekLabel.Location = new System.Drawing.Point(10, 150);
            this.dayOfWeekLabel.AutoSize = true;

            this.dayOfWeekComboBox = new ComboBox();
            this.dayOfWeekComboBox.Location = new System.Drawing.Point(150, 148);
            this.dayOfWeekComboBox.Size = new System.Drawing.Size(290, 25);
            this.dayOfWeekComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.dayOfWeekComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Интервал для интервального расписания
            this.intervalLabel = new Label();
            this.intervalLabel.Text = "Интервал (мин.):";
            this.intervalLabel.Location = new System.Drawing.Point(10, 180);
            this.intervalLabel.AutoSize = true;

            this.intervalNumericUpDown = new NumericUpDown();
            this.intervalNumericUpDown.Location = new System.Drawing.Point(150, 178);
            this.intervalNumericUpDown.Size = new System.Drawing.Size(100, 25);
            this.intervalNumericUpDown.Minimum = 1;
            this.intervalNumericUpDown.Maximum = 10080; // 7 days
            this.intervalNumericUpDown.Value = 60;

            // Добавление элементов в группу настроек расписания
            scheduleGroup.Controls.Add(scheduleTypeLabel);
            scheduleGroup.Controls.Add(this.scheduleTypeComboBox);
            scheduleGroup.Controls.Add(this.dateTimeLabel);
            scheduleGroup.Controls.Add(this.dateTimePicker);
            scheduleGroup.Controls.Add(this.hourLabel);
            scheduleGroup.Controls.Add(this.hourNumericUpDown);
            scheduleGroup.Controls.Add(this.minuteLabel);
            scheduleGroup.Controls.Add(this.minuteNumericUpDown);
            scheduleGroup.Controls.Add(this.dayLabel);
            scheduleGroup.Controls.Add(this.dayNumericUpDown);
            scheduleGroup.Controls.Add(this.dayOfWeekLabel);
            scheduleGroup.Controls.Add(this.dayOfWeekComboBox);
            scheduleGroup.Controls.Add(this.intervalLabel);
            scheduleGroup.Controls.Add(this.intervalNumericUpDown);

            // Кнопки сохранения и отмены
            Button saveButton = new Button();
            saveButton.Text = "Сохранить";
            saveButton.Location = new System.Drawing.Point(282, 510);
            saveButton.Size = new System.Drawing.Size(90, 30);
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveButton.Click += new EventHandler(this.SaveButton_Click);

            Button cancelButton = new Button();
            cancelButton.Text = "Отмена";
            cancelButton.Location = new System.Drawing.Point(382, 510);
            cancelButton.Size = new System.Drawing.Size(90, 30);
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.Click += new EventHandler(this.CancelButton_Click);

            // Добавление всех элементов на форму
            this.Controls.Add(generalGroup);
            this.Controls.Add(scheduleGroup);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
        }

        #endregion

        private TextBox nameTextBox;
        private TextBox sourceDirectoryTextBox;
        private TextBox backupDirectoryTextBox;
        private CheckBox enabledCheckBox;
        private ComboBox scheduleTypeComboBox;
        private Label dateTimeLabel;
        private DateTimePicker dateTimePicker;
        private Label hourLabel;
        private NumericUpDown hourNumericUpDown;
        private Label minuteLabel;
        private NumericUpDown minuteNumericUpDown;
        private Label dayLabel;
        private NumericUpDown dayNumericUpDown;
        private Label dayOfWeekLabel;
        private ComboBox dayOfWeekComboBox;
        private Label intervalLabel;
        private NumericUpDown intervalNumericUpDown;
        private Label syncTypeLabel;
        private ComboBox syncTypeComboBox;
    }
}