using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using BackupManager.Services;

namespace BackupManager.Forms
{
    public class SystemTrayManager : IDisposable
    {
        private ContextMenuStrip _contextMenu;
        private MainForm _mainForm;
        private BackupScheduler _scheduler;
        private ILogger _logger;
        private bool _disposed = false;

        // ВАЖНО: NotifyIcon теперь не создается здесь, а используется из MainForm
        // private NotifyIcon _notifyIcon;

        public SystemTrayManager(MainForm mainForm, BackupScheduler scheduler, ILogger logger)
        {
            _mainForm = mainForm;
            _scheduler = scheduler;
            _logger = logger;

            InitializeNotifyIcon();

            // Подписываемся на события
            _scheduler.BackupCompleted += OnBackupCompleted;
            _scheduler.BackupFailed += OnBackupFailed;
            _mainForm.FormClosing += OnMainFormClosing;
        }

        private void InitializeNotifyIcon()
        {
            // Создаем контекстное меню
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Открыть", null, OnOpenClick);
            _contextMenu.Items.Add("Расписание", null, OnScheduleClick);
            _contextMenu.Items.Add("-"); // Разделитель

            // Добавляем опцию автозагрузки с флажком
            ToolStripMenuItem autoStartItem = new ToolStripMenuItem("Запускать при старте Windows");
            autoStartItem.Checked = AutoStartManager.IsAutoStartEnabled();
            autoStartItem.Click += new EventHandler(OnAutoStartClick);
            _contextMenu.Items.Add(autoStartItem);

            _contextMenu.Items.Add("-"); // Разделитель
            _contextMenu.Items.Add("Выйти", null, OnExitClick);

            // ВАЖНО: Получаем ссылку на уже созданный NotifyIcon из MainForm
            // и просто устанавливаем ему новое контекстное меню
            if (_mainForm != null && _mainForm.GetType().GetField("trayIcon",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) != null)
            {
                NotifyIcon trayIcon = (NotifyIcon)_mainForm.GetType().GetField("trayIcon",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(_mainForm);

                if (trayIcon != null)
                {
                    // Устанавливаем новое контекстное меню
                    trayIcon.ContextMenuStrip = _contextMenu;

                    // Убедимся, что иконка установлена и видима
                    if (trayIcon.Icon == null && _mainForm.Icon != null)
                    {
                        trayIcon.Icon = (Icon)_mainForm.Icon.Clone();
                        _logger.Log("Иконка трея установлена из MainForm", LogLevel.Info);
                    }

                    trayIcon.Visible = true;
                    trayIcon.Text = "Backup Manager";

                    // Добавляем обработчик двойного клика, если его еще нет
                    trayIcon.DoubleClick -= OnNotifyIconDoubleClick;  // Сначала удаляем, если уже был
                    trayIcon.DoubleClick += OnNotifyIconDoubleClick;

                    _logger.Log("Использован существующий NotifyIcon из MainForm", LogLevel.Info);
                }
                else
                {
                    _logger.Log("NotifyIcon в MainForm найден, но null", LogLevel.Warning);
                }
            }
            else
            {
                _logger.Log("NotifyIcon в MainForm не найден", LogLevel.Warning);
            }
        }

        // Обработчики событий остаются прежними

        private void OnAutoStartClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                if (item.Checked)
                {
                    // Если опция была включена, отключаем
                    if (AutoStartManager.DisableAutoStart())
                    {
                        item.Checked = false;
                        _logger.Log("Автозапуск при старте Windows отключен");
                    }
                }
                else
                {
                    // Если опция была выключена, включаем
                    if (AutoStartManager.EnableAutoStart())
                    {
                        item.Checked = true;
                        _logger.Log("Автозапуск при старте Windows включен");
                    }
                }
            }
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void OnOpenClick(object sender, EventArgs e)
        {
            ShowMainForm();
        }

        private void OnScheduleClick(object sender, EventArgs e)
        {
            var scheduleForm = new ScheduleListForm(_scheduler, _logger);
            scheduleForm.ShowDialog();
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            // Отписываемся от событий формы
            _mainForm.FormClosing -= OnMainFormClosing;

            // Закрываем приложение
            Application.Exit();
        }

        private void OnBackupCompleted(object sender, BackupJobEventArgs e)
        {
            // Получаем trayIcon из MainForm
            if (_mainForm != null)
            {
                var trayIconField = _mainForm.GetType().GetField("trayIcon",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (trayIconField != null)
                {
                    NotifyIcon trayIcon = (NotifyIcon)trayIconField.GetValue(_mainForm);
                    if (trayIcon != null)
                    {
                        // Показываем уведомление об успешном завершении резервного копирования
                        trayIcon.ShowBalloonTip(
                            3000,
                            "Резервное копирование завершено",
                            $"Задание '{e.Job.SourceDirectory}' -> '{e.Job.BackupDirectory}' успешно выполнено.",
                            ToolTipIcon.Info
                        );
                    }
                }
            }
        }

        private void OnBackupFailed(object sender, BackupJobEventArgs e)
        {
            // Получаем trayIcon из MainForm
            if (_mainForm != null)
            {
                var trayIconField = _mainForm.GetType().GetField("trayIcon",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (trayIconField != null)
                {
                    NotifyIcon trayIcon = (NotifyIcon)trayIconField.GetValue(_mainForm);
                    if (trayIcon != null)
                    {
                        // Показываем уведомление об ошибке резервного копирования
                        trayIcon.ShowBalloonTip(
                            5000,
                            "Ошибка резервного копирования",
                            $"Задание '{e.Job.SourceDirectory}' -> '{e.Job.BackupDirectory}' выполнено с ошибками.",
                            ToolTipIcon.Error
                        );
                    }
                }
            }
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            // Если пользователь закрывает форму, но не выходит из приложения,
            // то просто скрываем форму и оставляем приложение в трее
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Отменяем закрытие формы
                _mainForm.Hide(); // Скрываем форму

                // Получаем trayIcon из MainForm
                if (_mainForm != null)
                {
                    var trayIconField = _mainForm.GetType().GetField("trayIcon",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    if (trayIconField != null)
                    {
                        NotifyIcon trayIcon = (NotifyIcon)trayIconField.GetValue(_mainForm);
                        if (trayIcon != null)
                        {
                            // Показываем уведомление
                            trayIcon.ShowBalloonTip(
                                3000,
                                "Backup Manager",
                                "Приложение продолжает работать в фоновом режиме. Для доступа к приложению щелкните дважды по иконке в трее.",
                                ToolTipIcon.Info
                            );
                        }
                    }
                }
            }
        }

        private void ShowMainForm()
        {
            if (_mainForm != null)
            {
                _mainForm.Show();
                _mainForm.WindowState = FormWindowState.Normal;
                _mainForm.Activate();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_scheduler != null)
                    {
                        _scheduler.BackupCompleted -= OnBackupCompleted;
                        _scheduler.BackupFailed -= OnBackupFailed;
                    }

                    if (_mainForm != null)
                    {
                        _mainForm.FormClosing -= OnMainFormClosing;
                    }

                    // Теперь мы не создаем собственный NotifyIcon, поэтому не нужно его удалять
                    // Удаляем только контекстное меню
                    if (_contextMenu != null)
                    {
                        _contextMenu.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        ~SystemTrayManager()
        {
            Dispose(false);
        }
    }
}