using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TaskReminderApp.Models;
using TaskReminderApp.Services;

namespace TaskReminderApp
{
    public partial class MainWindow : Window
    {
        private readonly TaskManager taskManager;
        private readonly ReminderService reminderService;
        private readonly GlobalKeyboardHook keyboardHook;

        public MainWindow()
        {
            InitializeComponent();

            taskManager = new TaskManager();
            reminderService = new ReminderService(taskManager.Tasks);
            keyboardHook = new GlobalKeyboardHook();

            TaskListBox.ItemsSource = taskManager.Tasks;

            keyboardHook.KeyPressed += KeyboardHook_KeyPressed;
            reminderService.Start();

            TaskDatePicker.SelectedDate = DateTime.Today;
        }

        private void KeyboardHook_KeyPressed(object? sender, Key e)
        {
            if (e == Key.F2)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Автоматически выделяем весь текст (Ctrl+A) и копируем его
                        string? selectedText = ClipboardHelper.GetAllTextWithSelection();

                        if (!string.IsNullOrEmpty(selectedText))
                        {
                            // Конвертируем текст
                            string convertedText = KeyboardLayoutConverter.Convert(selectedText);

                            if (convertedText != selectedText)
                            {
                                // Заменяем выделенный текст на сконвертированный
                                ClipboardHelper.ReplaceSelectedText(convertedText);

                                string preview = selectedText.Length > 20
                                    ? selectedText.Substring(0, 20) + "..."
                                    : selectedText;

                                UpdateStatusBar($"Текст сконвертирован: {preview}",
                                    new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)));
                            }
                            else
                            {
                                UpdateStatusBar("Конвертация не требуется", new SolidColorBrush(Color.FromRgb(0xF9, 0x73, 0x16)));
                            }
                        }
                        else
                        {
                            UpdateStatusBar("Нет текста для конвертации", new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)));
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateStatusBar($"Ошибка: {ex.Message}", new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)));
                    }
                });
            }
        }

        private void StartHook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                keyboardHook.Start();
                HookStatusText.Text = "Активен";
                HookStatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                StartHookBtn.IsEnabled = false;
                StopHookBtn.IsEnabled = true;
                UpdateStatusBar("Хук клавиатуры запущен", new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска хука: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopHook_Click(object sender, RoutedEventArgs e)
        {
            keyboardHook.Stop();
            HookStatusText.Text = "Остановлен";
            HookStatusText.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            StatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            StartHookBtn.IsEnabled = true;
            StopHookBtn.IsEnabled = false;
            UpdateStatusBar("Хук клавиатуры остановлен", new SolidColorBrush(Color.FromRgb(0xF9, 0x73, 0x16)));
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TaskTitleBox.Text))
                {
                    MessageBox.Show("Введите название задачи!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TaskDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату напоминания!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(TaskHourBox.Text, out int hour) || hour < 0 || hour > 23)
                {
                    MessageBox.Show("Введите корректный час (0-23)!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(TaskMinuteBox.Text, out int minute) || minute < 0 || minute > 59)
                {
                    MessageBox.Show("Введите корректную минуту (0-59)!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime reminderDate = TaskDatePicker.SelectedDate.Value.Date
                    .AddHours(hour)
                    .AddMinutes(minute);

                var newTask = new TaskItem
                {
                    Title = TaskTitleBox.Text.Trim(),
                    Description = TaskDescriptionBox.Text.Trim(),
                    ReminderTime = reminderDate,
                    IsCompleted = false,
                    WasNotified = false
                };

                taskManager.AddTask(newTask);

                StatusText.Text = "Задача успешно добавлена!";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                UpdateStatusBar($"Добавлена задача: {newTask.Title}", new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)));

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления задачи: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selectedTask)
            {
                var result = MessageBox.Show(
                    $"Удалить задачу '{selectedTask.Title}'?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    taskManager.RemoveTask(selectedTask);
                    UpdateStatusBar("Задача удалена", new SolidColorBrush(Color.FromRgb(0xF9, 0x73, 0x16)));
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для удаления!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selectedTask)
            {
                selectedTask.IsCompleted = true;
                taskManager.UpdateTask(selectedTask);
                TaskListBox.Items.Refresh();
                UpdateStatusBar("Задача отмечена как выполненная", new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)));
            }
            else
            {
                MessageBox.Show("Выберите задачу!", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshTasks_Click(object sender, RoutedEventArgs e)
        {
            TaskListBox.Items.Refresh();
            UpdateStatusBar("Список обновлен", new SolidColorBrush(Color.FromRgb(0x06, 0xB6, 0xD4)));
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            TaskTitleBox.Clear();
            TaskDescriptionBox.Clear();
            TaskDatePicker.SelectedDate = DateTime.Today;
            TaskHourBox.Text = "12";
            TaskMinuteBox.Text = "00";
            StatusText.Text = "";
        }

        private void TaskListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TaskListBox.SelectedItem is TaskItem selectedTask)
            {
                UpdateStatusBar($"Выбрана: {selectedTask.Title}", new SolidColorBrush(Color.FromRgb(0x06, 0xB6, 0xD4)));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            keyboardHook.Stop();
            reminderService.Stop();
            keyboardHook.Dispose();
        }

        private void UpdateStatusBar(string message, Brush color)
        {
            StatusBarText.Text = message;
            StatusBarText.Foreground = color;
        }
    }
}
