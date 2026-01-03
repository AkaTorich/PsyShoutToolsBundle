using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using TaskReminderApp.Models;

namespace TaskReminderApp.Services
{
    public class ReminderService
    {
        private readonly DispatcherTimer timer;
        private readonly ObservableCollection<TaskItem> tasks;

        public ReminderService(ObservableCollection<TaskItem> tasks)
        {
            this.tasks = tasks;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            timer.Tick += Timer_Tick;
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            var tasksToNotify = tasks
                .Where(t => !t.IsCompleted && !t.WasNotified && t.ReminderTime <= now)
                .ToList();

            foreach (var task in tasksToNotify)
            {
                ShowNotification(task);
                task.WasNotified = true;
            }
        }

        private void ShowNotification(TaskItem task)
        {
            string message = $"Напоминание о задаче:\n\n{task.Title}\n\n{task.Description}\n\nВремя: {task.ReminderTime:dd.MM.yyyy HH:mm}";

            MessageBox.Show(
                message,
                "Напоминание о задаче",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
