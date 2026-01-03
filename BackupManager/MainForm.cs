using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Forms
{
    public partial class MainForm : Form
    {
        private FileSystemManager _fileManager;
        private BackupComparer _backupComparer;
        private MemoryLogger _logger;
        private BackupScheduler _scheduler;
        private BackupJobManager _jobManager;
        private SystemTrayManager _trayManager;

        private string _sourceDirectory;
        private string _backupDirectory;
        private bool _exitingApplication = false;

        // Добавляем публичный метод для передачи иконки в другие классы
        public Icon GetApplicationIcon()
        {
            return this.Icon;
        }

        public MainForm()
        {
            InitializeComponent();

            _logger = new MemoryLogger();
            _fileManager = new FileSystemManager();
            _backupComparer = new BackupComparer(_logger);

            // Инициализация менеджера истории заданий
            _jobManager = new BackupJobManager(_logger);

            // Инициализация планировщика резервного копирования
            _scheduler = new BackupScheduler(_logger);

            // Настройка обработчиков событий резервного копирования
            _scheduler.BackupStarted += OnBackupStarted;
            _scheduler.BackupCompleted += OnBackupCompleted;
            _scheduler.BackupFailed += OnBackupFailed;

            // Убедимся, что иконка имеет корректное значение перед созданием менеджера трея
            if (this.Icon == null)
            {
                // Можно также добавить загрузку иконки из ресурсов здесь, если нужно
                try
                {
                    ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
                    Icon appIcon = (Icon)resources.GetObject("$this.Icon");
                    if (appIcon != null)
                    {
                        this.Icon = appIcon;
                        _logger.Log("Иконка приложения установлена из ресурсов");
                    }
                    else
                    {
                        // Устанавливаем иконку из встроенных
                        this.Icon = SystemIcons.Application;
                        _logger.Log("Установлена стандартная иконка приложения", LogLevel.Warning);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log($"Ошибка при установке иконки: {ex.Message}", LogLevel.Error);
                }
            }

            // Инициализация менеджера системного трея
            _trayManager = new SystemTrayManager(this, _scheduler, _logger);
            InitializeAutoStart();
            UpdateLogView();
        }

        private void InitializeAutoStart()
        {
            // Добавляем обработчик только здесь
            this.autoStartCheckBox.Checked = AutoStartManager.IsAutoStartEnabled();
            this.autoStartCheckBox.CheckedChanged += new EventHandler(this.AutoStartCheckBox_CheckedChanged);
        }

        // В MainForm.cs добавим обработчик для чекбокса автозагрузки
        private void AutoStartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                if (checkBox.Checked)
                {
                    if (AutoStartManager.EnableAutoStart())
                    {
                        _logger.Log("Автозапуск при старте Windows включен");
                    }
                    else
                    {
                        // Если не удалось включить, возвращаем чекбокс в предыдущее состояние
                        checkBox.Checked = false;
                    }
                }
                else
                {
                    if (AutoStartManager.DisableAutoStart())
                    {
                        _logger.Log("Автозапуск при старте Windows отключен");
                    }
                    else
                    {
                        // Если не удалось отключить, возвращаем чекбокс в предыдущее состояние
                        checkBox.Checked = true;
                    }
                }
            }
        }
        private void BrowseSourceButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите исходную директорию";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    _sourceDirectory = folderDialog.SelectedPath;
                    sourceDirTextBox.Text = _sourceDirectory;
                    _logger.Log($"Выбрана исходная директория: {_sourceDirectory}");
                    UpdateLogView();
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
                    _backupDirectory = folderDialog.SelectedPath;
                    backupDirTextBox.Text = _backupDirectory;
                    _logger.Log($"Выбрана директория резервной копии: {_backupDirectory}");
                    UpdateLogView();
                }
            }
        }

        private void CreateBackupButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sourceDirectory) || string.IsNullOrEmpty(_backupDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите исходную директорию и директорию для резервной копии.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_sourceDirectory == _backupDirectory)
            {
                MessageBox.Show("Исходная директория и директория для резервной копии не могут совпадать.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show("Создать резервную копию?", "Подтверждение",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    _logger.Log($"Начало создания резервной копии из {_sourceDirectory} в {_backupDirectory}");
                    UpdateLogView();

                    // Создаем запись о задании
                    var backupJob = new BackupJob(_sourceDirectory, _backupDirectory);
                    // Создаем отдельный логгер для этого задания
                    var jobLogger = new MemoryLogger();

                    // Запускаем операцию в отдельном потоке
                    var backgroundWorker = new System.ComponentModel.BackgroundWorker();
                    backgroundWorker.DoWork += (s, args) =>
                    {
                        _fileManager.CopyDirectory(_sourceDirectory, _backupDirectory, jobLogger);
                    };
                    backgroundWorker.RunWorkerCompleted += (s, args) =>
                    {
                        if (args.Error != null)
                        {
                            _logger.Log($"Ошибка при создании резервной копии: {args.Error.Message}", LogLevel.Error);
                            jobLogger.Log($"Ошибка при создании резервной копии: {args.Error.Message}", LogLevel.Error);
                            backupJob.IsSuccessful = false;
                        }
                        else
                        {
                            _logger.Log("Резервная копия успешно создана.", LogLevel.Info);
                            jobLogger.Log("Резервная копия успешно создана.", LogLevel.Info);
                            backupJob.IsSuccessful = true;
                        }

                        // Завершаем задание и сохраняем только логи этого задания
                        backupJob.CompletedAt = DateTime.Now;
                        backupJob.Logs = jobLogger.GetLogs(); // Используем логи только этого задания
                        _jobManager.AddJob(backupJob);

                        UpdateLogView();
                    };
                    backgroundWorker.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    _logger.Log($"Ошибка при создании резервной копии: {ex.Message}", LogLevel.Error);
                    UpdateLogView();
                }
            }
        }

        private void CompareButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sourceDirectory) || string.IsNullOrEmpty(_backupDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите исходную директорию и директорию резервной копии.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(_sourceDirectory) || !Directory.Exists(_backupDirectory))
            {
                MessageBox.Show("Одна из выбранных директорий не существует.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _logger.Log("Начало сравнения директорий...");
                UpdateLogView();

                // Запускаем операцию в отдельном потоке
                var backgroundWorker = new System.ComponentModel.BackgroundWorker();
                backgroundWorker.DoWork += (s, args) =>
                {
                    _backupComparer.ScanSourceDirectory(_sourceDirectory);
                    _backupComparer.ScanBackupDirectory(_backupDirectory);
                    var differences = _backupComparer.CompareDirectories();
                    args.Result = differences;
                };
                backgroundWorker.RunWorkerCompleted += (s, args) =>
                {
                    if (args.Error != null)
                    {
                        _logger.Log($"Ошибка при сравнении: {args.Error.Message}", LogLevel.Error);
                        UpdateLogView();
                    }
                    else
                    {
                        var differences = args.Result as System.Collections.Generic.List<FileDifference>;
                        _logger.Log($"Сравнение завершено. Найдено различий: {differences.Count}");
                        UpdateLogView();

                        if (differences.Count > 0)
                        {
                            // Открываем форму сравнения
                            var compareForm = new CompareForm(differences, _sourceDirectory, _backupDirectory, _logger);
                            compareForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Различий не найдено.", "Результат сравнения",
                                           MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };
                backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при сравнении: {ex.Message}", LogLevel.Error);
                UpdateLogView();
            }
        }

        private void RestoreButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sourceDirectory) || string.IsNullOrEmpty(_backupDirectory))
            {
                MessageBox.Show("Пожалуйста, выберите исходную директорию и директорию резервной копии.",
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _logger.Log("Начало подготовки к восстановлению...");
                UpdateLogView();

                // Запускаем операцию в отдельном потоке
                var backgroundWorker = new System.ComponentModel.BackgroundWorker();
                backgroundWorker.DoWork += (s, args) =>
                {
                    _backupComparer.ScanSourceDirectory(_sourceDirectory);
                    _backupComparer.ScanBackupDirectory(_backupDirectory);
                    var differences = _backupComparer.CompareDirectories();
                    args.Result = differences;
                };
                backgroundWorker.RunWorkerCompleted += (s, args) =>
                {
                    if (args.Error != null)
                    {
                        _logger.Log($"Ошибка при подготовке к восстановлению: {args.Error.Message}", LogLevel.Error);
                        UpdateLogView();
                    }
                    else
                    {
                        var differences = args.Result as System.Collections.Generic.List<FileDifference>;
                        _logger.Log($"Подготовка к восстановлению завершена. Найдено различий: {differences.Count}");
                        UpdateLogView();

                        if (differences.Count > 0)
                        {
                            // Открываем форму восстановления
                            var restoreForm = new RestoreForm(differences, _sourceDirectory, _backupDirectory, _fileManager, _logger);
                            restoreForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Нет файлов для восстановления.", "Восстановление",
                                           MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };
                backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при подготовке к восстановлению: {ex.Message}", LogLevel.Error);
                UpdateLogView();
            }
        }

        private void ClearLogsButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Очистить журнал операций?", "Подтверждение",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _logger = new MemoryLogger();
                _logger.Log("Журнал операций очищен.");
                _backupComparer = new BackupComparer(_logger);
                UpdateLogView();
            }
        }

        private void ScheduleButton_Click(object sender, EventArgs e)
        {
            var scheduleForm = new ScheduleListForm(_scheduler, _logger);
            scheduleForm.ShowDialog();
        }

        private void HistoryButton_Click(object sender, EventArgs e)
        {
            var historyForm = new HistoryForm(_jobManager, _logger);
            historyForm.ShowDialog();
        }

        private void UpdateLogView()
        {
            if (logTextBox.IsDisposed || !logTextBox.IsHandleCreated)
                return;

            // Используем более надежный способ обновления UI из другого потока
            try
            {
                if (logTextBox.InvokeRequired)
                {
                    logTextBox.Invoke(new Action(() =>
                    {
                        UpdateLogTextbox();
                    }));
                }
                else
                {
                    UpdateLogTextbox();
                }
            }
            catch (ObjectDisposedException)
            {
                // Форма уже закрыта, игнорируем
            }
            catch (InvalidOperationException)
            {
                // Дескриптор еще не создан или уже уничтожен
            }
        }

        private void UpdateLogTextbox()
        {
            logTextBox.Clear();
            foreach (var log in _logger.GetLogs())
            {
                logTextBox.SelectionColor = log.Level == LogLevel.Error
                    ? System.Drawing.Color.Red
                    : (log.Level == LogLevel.Warning
                        ? System.Drawing.Color.Orange
                        : System.Drawing.Color.Black);
                logTextBox.AppendText(log.ToString() + Environment.NewLine);
            }
            logTextBox.ScrollToCaret();
        }

        private void OnBackupStarted(object sender, BackupJobEventArgs e)
        {
            _logger.Log($"Начато фоновое резервное копирование: {e.Job.SourceDirectory} -> {e.Job.BackupDirectory}");
            UpdateLogView();
        }

        private void OnBackupCompleted(object sender, BackupJobEventArgs e)
        {
            _logger.Log($"Успешно завершено фоновое резервное копирование: {e.Job.SourceDirectory} -> {e.Job.BackupDirectory}");

            // Добавляем выполненное задание в историю
            // Логи уже установлены в BackupScheduler
            _jobManager.AddJob(e.Job);

            UpdateLogView();
        }

        private void OnBackupFailed(object sender, BackupJobEventArgs e)
        {
            _logger.Log($"Ошибка при фоновом резервном копировании: {e.Job.SourceDirectory} -> {e.Job.BackupDirectory}", LogLevel.Error);

            // Добавляем выполненное задание в историю
            // Логи уже установлены в BackupScheduler
            _jobManager.AddJob(e.Job);

            UpdateLogView();
        }

        #region TrayIcon Handlers

        // Этот метод вызывается при нажатии на кнопку "X" (закрыть)
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Если причина закрытия - не выключение компьютера и не прямой вызов Exit
            if (e.CloseReason == CloseReason.UserClosing && !_exitingApplication)
            {
                e.Cancel = true; // Отменяем закрытие
                Hide(); // Скрываем форму
                trayIcon.Visible = true; // Показываем иконку в трее
                trayIcon.ShowBalloonTip(3000, "Backup Manager", "Приложение продолжает работать в фоновом режиме", ToolTipIcon.Info);
            }
            else if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                // При выключении компьютера нужно корректно освободить ресурсы
                if (_trayManager != null)
                {
                    _trayManager.Dispose();
                }

                if (_scheduler != null)
                {
                    _scheduler.Dispose();
                }
            }
        }

        // Метод, вызываемый при изменении размера окна (для сворачивания в трей)
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                trayIcon.Visible = true;
                trayIcon.ShowBalloonTip(3000, "Backup Manager", "Приложение свернуто в трей", ToolTipIcon.Info);
            }
        }

        // Обработчик двойного клика по иконке в трее
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate(); // Активируем окно
        }

        // Открытие приложения из контекстного меню трея
        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate(); // Активируем окно
        }

        // Создание резервной копии из контекстного меню трея
        private void CreateBackupMenuItem_Click(object sender, EventArgs e)
        {
            // Проверка наличия выбранных директорий
            if (string.IsNullOrEmpty(_sourceDirectory) || string.IsNullOrEmpty(_backupDirectory))
            {
                trayIcon.ShowBalloonTip(3000, "Ошибка", "Не выбраны директории для резервного копирования", ToolTipIcon.Error);
                return;
            }

            // Вызываем тот же метод, что и для кнопки на форме
            try
            {
                // Показываем уведомление о начале операции
                trayIcon.ShowBalloonTip(3000, "Backup Manager", "Начато создание резервной копии", ToolTipIcon.Info);

                // Создаем запись о задании
                var backupJob = new BackupJob(_sourceDirectory, _backupDirectory);
                // Создаем отдельный логгер для этого задания
                var jobLogger = new MemoryLogger();

                // Запускаем операцию в отдельном потоке
                var backgroundWorker = new System.ComponentModel.BackgroundWorker();
                backgroundWorker.DoWork += (s, args) =>
                {
                    _fileManager.CopyDirectory(_sourceDirectory, _backupDirectory, jobLogger);
                };
                backgroundWorker.RunWorkerCompleted += (s, args) =>
                {
                    if (args.Error != null)
                    {
                        _logger.Log($"Ошибка при создании резервной копии: {args.Error.Message}", LogLevel.Error);
                        jobLogger.Log($"Ошибка при создании резервной копии: {args.Error.Message}", LogLevel.Error);
                        backupJob.IsSuccessful = false;

                        // Показываем уведомление об ошибке
                        trayIcon.ShowBalloonTip(3000, "Ошибка", "Не удалось создать резервную копию", ToolTipIcon.Error);
                    }
                    else
                    {
                        _logger.Log("Резервная копия успешно создана.", LogLevel.Info);
                        jobLogger.Log("Резервная копия успешно создана.", LogLevel.Info);
                        backupJob.IsSuccessful = true;

                        // Показываем уведомление об успешном завершении
                        trayIcon.ShowBalloonTip(3000, "Успешно", "Резервная копия создана", ToolTipIcon.Info);
                    }

                    // Завершаем задание и сохраняем только логи этого задания
                    backupJob.CompletedAt = DateTime.Now;
                    backupJob.Logs = jobLogger.GetLogs(); // Используем логи только этого задания
                    _jobManager.AddJob(backupJob);

                    UpdateLogView();
                };
                backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при создании резервной копии из трея: {ex.Message}", LogLevel.Error);
                trayIcon.ShowBalloonTip(3000, "Ошибка", $"Ошибка: {ex.Message}", ToolTipIcon.Error);
                UpdateLogView();
            }
        }

        // Полный выход из приложения через контекстное меню трея
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            // Устанавливаем флаг выхода из приложения
            _exitingApplication = true;

            // Корректно освобождаем ресурсы
            if (_trayManager != null)
            {
                _trayManager.Dispose();
            }

            if (_scheduler != null)
            {
                _scheduler.Dispose();
            }

            // Закрываем приложение
            Application.Exit();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_trayManager != null)
                {
                    _trayManager.Dispose();
                    _trayManager = null;
                }

                if (_scheduler != null)
                {
                    _scheduler.BackupStarted -= OnBackupStarted;
                    _scheduler.BackupCompleted -= OnBackupCompleted;
                    _scheduler.BackupFailed -= OnBackupFailed;
                    _scheduler.Dispose();
                    _scheduler = null;
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}