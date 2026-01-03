using System;
using System.Windows.Forms;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Forms
{
    public partial class ScheduleForm : Form
    {
        private ScheduledBackupJob _job;
        private readonly bool _isNewJob;
        private readonly ILogger _logger;

        public ScheduledBackupJob Job => _job;

        public ScheduleForm(ILogger logger, ScheduledBackupJob job = null)
        {
            _logger = logger;
            _isNewJob = job == null;
            _job = job ?? new ScheduledBackupJob();

            InitializeComponent();
            LoadJobToForm();
        }

        private void LoadJobToForm()
        {
            // Заполняем основные данные задания
            nameTextBox.Text = _job.Name;
            sourceDirectoryTextBox.Text = _job.SourceDirectory;
            backupDirectoryTextBox.Text = _job.BackupDirectory;
            enabledCheckBox.Checked = _job.Enabled;

            // Заполняем тип расписания
            scheduleTypeComboBox.Items.Clear();
            var scheduleTypes = (ScheduleType[])Enum.GetValues(typeof(ScheduleType));
            foreach (ScheduleType scheduleType in scheduleTypes)
            {
                scheduleTypeComboBox.Items.Add(GetScheduleTypeName(scheduleType));
                scheduleTypeComboBox.Tag = scheduleTypes; // Сохраняем массив для обратного преобразования
            }
            
            // Находим индекс текущего типа расписания
            for (int i = 0; i < scheduleTypes.Length; i++)
            {
                if (scheduleTypes[i] == _job.ScheduleType)
                {
                    scheduleTypeComboBox.SelectedIndex = i;
                    break;
                }
            }

            // Заполняем типы синхронизации
            syncTypeComboBox.Items.Clear();
            var syncTypes = (SyncType[])Enum.GetValues(typeof(SyncType));
            foreach (SyncType syncType in syncTypes)
            {
                syncTypeComboBox.Items.Add(GetSyncTypeName(syncType));
                syncTypeComboBox.Tag = syncTypes; // Сохраняем массив для обратного преобразования
            }
            
            // Находим индекс текущего типа синхронизации
            for (int i = 0; i < syncTypes.Length; i++)
            {
                if (syncTypes[i] == _job.SyncType)
                {
                    syncTypeComboBox.SelectedIndex = i;
                    break;
                }
            }

            // Заполняем настройки времени
            hourNumericUpDown.Value = _job.Hour;
            minuteNumericUpDown.Value = _job.Minute;
            dayNumericUpDown.Value = _job.Day;
            intervalNumericUpDown.Value = _job.IntervalMinutes;

            // Заполняем день недели
            dayOfWeekComboBox.Items.Clear();
            var dayOfWeekValues = (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek));
            foreach (DayOfWeek day in dayOfWeekValues)
            {
                dayOfWeekComboBox.Items.Add(GetDayOfWeekName(day));
                dayOfWeekComboBox.Tag = dayOfWeekValues; // Сохраняем массив для обратного преобразования
            }
            
            // Находим индекс текущего дня недели
            for (int i = 0; i < dayOfWeekValues.Length; i++)
            {
                if (dayOfWeekValues[i] == _job.DayOfWeek)
                {
                    dayOfWeekComboBox.SelectedIndex = i;
                    break;
                }
            }

            // Настраиваем дату и время для однократного запуска
            dateTimePicker.Value = _job.ScheduledTime;

            // Обновляем видимость элементов управления
            UpdateControlsVisibility();
        }

        private string GetScheduleTypeName(ScheduleType scheduleType)
        {
            switch (scheduleType)
            {
                case ScheduleType.Once:
                    return "Однократно";
                case ScheduleType.Hourly:
                    return "Ежечасно";
                case ScheduleType.Daily:
                    return "Ежедневно";
                case ScheduleType.Weekly:
                    return "Еженедельно";
                case ScheduleType.Monthly:
                    return "Ежемесячно";
                case ScheduleType.Interval:
                    return "С интервалом";
                default:
                    return scheduleType.ToString();
            }
        }

        private string GetSyncTypeName(SyncType syncType)
        {
            switch (syncType)
            {
                case SyncType.Full:
                    return "Полная синхронизация";
                case SyncType.Incremental:
                    return "Инкрементальное копирование";
                case SyncType.Decremental:
                    return "Зеркальная синхронизация";
                case SyncType.TwoWay:
                    return "Односторонняя синхронизация";
                default:
                    return syncType.ToString();
            }
        }

        private string GetDayOfWeekName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Понедельник";
                case DayOfWeek.Tuesday: return "Вторник";
                case DayOfWeek.Wednesday: return "Среда";
                case DayOfWeek.Thursday: return "Четверг";
                case DayOfWeek.Friday: return "Пятница";
                case DayOfWeek.Saturday: return "Суббота";
                case DayOfWeek.Sunday: return "Воскресенье";
                default: return day.ToString();
            }
        }

        private void UpdateControlsVisibility()
        {
            // Скрыть все элементы управления для настройки расписания
            hourLabel.Visible = false;
            hourNumericUpDown.Visible = false;
            minuteLabel.Visible = false;
            minuteNumericUpDown.Visible = false;
            dayLabel.Visible = false;
            dayNumericUpDown.Visible = false;
            dayOfWeekLabel.Visible = false;
            dayOfWeekComboBox.Visible = false;
            dateTimeLabel.Visible = false;
            dateTimePicker.Visible = false;
            intervalLabel.Visible = false;
            intervalNumericUpDown.Visible = false;

            // Показываем нужные элементы в зависимости от типа расписания
            // Получаем выбранный тип расписания безопасным способом
            var scheduleTypes = (ScheduleType[])scheduleTypeComboBox.Tag;
            ScheduleType selectedType = ScheduleType.Daily; // Значение по умолчанию
            if (scheduleTypeComboBox.SelectedIndex >= 0 && scheduleTypeComboBox.SelectedIndex < scheduleTypes.Length)
            {
                selectedType = scheduleTypes[scheduleTypeComboBox.SelectedIndex];
            }

            switch (selectedType)
            {
                case ScheduleType.Once:
                    dateTimeLabel.Visible = true;
                    dateTimePicker.Visible = true;
                    break;

                case ScheduleType.Hourly:
                    minuteLabel.Visible = true;
                    minuteNumericUpDown.Visible = true;
                    break;

                case ScheduleType.Daily:
                    hourLabel.Visible = true;
                    hourNumericUpDown.Visible = true;
                    minuteLabel.Visible = true;
                    minuteNumericUpDown.Visible = true;
                    break;

                case ScheduleType.Weekly:
                    dayOfWeekLabel.Visible = true;
                    dayOfWeekComboBox.Visible = true;
                    hourLabel.Visible = true;
                    hourNumericUpDown.Visible = true;
                    minuteLabel.Visible = true;
                    minuteNumericUpDown.Visible = true;
                    break;

                case ScheduleType.Monthly:
                    dayLabel.Visible = true;
                    dayNumericUpDown.Visible = true;
                    hourLabel.Visible = true;
                    hourNumericUpDown.Visible = true;
                    minuteLabel.Visible = true;
                    minuteNumericUpDown.Visible = true;
                    break;

                case ScheduleType.Interval:
                    intervalLabel.Visible = true;
                    intervalNumericUpDown.Visible = true;
                    break;
            }
        }

        private void ScheduleTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControlsVisibility();
        }

        private void BrowseSourceButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите исходную директорию";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    sourceDirectoryTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void BrowseBackupButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите директорию для резервной копии";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    backupDirectoryTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                // Сохраняем основные настройки задания
                _job.Name = nameTextBox.Text;
                _job.SourceDirectory = sourceDirectoryTextBox.Text;
                _job.BackupDirectory = backupDirectoryTextBox.Text;
                _job.Enabled = enabledCheckBox.Checked;
                // Получаем выбранный тип синхронизации безопасным способом
                var syncTypes = (SyncType[])syncTypeComboBox.Tag;
                if (syncTypeComboBox.SelectedIndex >= 0 && syncTypeComboBox.SelectedIndex < syncTypes.Length)
                {
                    _job.SyncType = syncTypes[syncTypeComboBox.SelectedIndex];
                }
                else
                {
                    _job.SyncType = SyncType.Incremental; // Значение по умолчанию
                }

                // Сохраняем настройки расписания безопасным способом
                var scheduleTypes = (ScheduleType[])scheduleTypeComboBox.Tag;
                if (scheduleTypeComboBox.SelectedIndex >= 0 && scheduleTypeComboBox.SelectedIndex < scheduleTypes.Length)
                {
                    _job.ScheduleType = scheduleTypes[scheduleTypeComboBox.SelectedIndex];
                }
                else
                {
                    _job.ScheduleType = ScheduleType.Daily; // Значение по умолчанию
                }
                _job.Hour = (int)hourNumericUpDown.Value;
                _job.Minute = (int)minuteNumericUpDown.Value;
                _job.Day = (int)dayNumericUpDown.Value;
                // Сохраняем день недели безопасным способом
                var dayOfWeekValues = (DayOfWeek[])dayOfWeekComboBox.Tag;
                if (dayOfWeekComboBox.SelectedIndex >= 0 && dayOfWeekComboBox.SelectedIndex < dayOfWeekValues.Length)
                {
                    _job.DayOfWeek = dayOfWeekValues[dayOfWeekComboBox.SelectedIndex];
                }
                else
                {
                    _job.DayOfWeek = DayOfWeek.Monday; // Значение по умолчанию
                }
                _job.IntervalMinutes = (int)intervalNumericUpDown.Value;
                _job.ScheduledTime = dateTimePicker.Value;

                _logger.Log($"Задание {_job.Name} сохранено. Расписание: {_job.GetScheduleDescription()}, Тип синхронизации: {_job.GetSyncTypeDescription()}");
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название задания.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(sourceDirectoryTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, выберите исходную директорию.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(backupDirectoryTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, выберите директорию для резервной копии.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (sourceDirectoryTextBox.Text == backupDirectoryTextBox.Text)
            {
                MessageBox.Show("Исходная директория и директория для резервной копии не могут совпадать.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка на выбор корневой директории диска
            if (sourceDirectoryTextBox.Text.Length <= 3 && sourceDirectoryTextBox.Text.EndsWith(":\\"))
            {
                var result = MessageBox.Show(
                    "Вы выбрали корневую директорию диска. Это может занять много времени и места, " +
                    "а также привести к ошибкам при попытке копирования системных папок. " +
                    "Системные папки будут автоматически исключены из копирования.\n\n" +
                    "Рекомендуется выбрать конкретную папку. Продолжить?",
                    "Предупреждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    return false;
                }
            }

            return true;
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}