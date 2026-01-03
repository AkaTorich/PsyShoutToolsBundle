using System;
using System.ComponentModel;

namespace TaskReminderApp.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string title = string.Empty;
        private string description = string.Empty;
        private DateTime reminderTime;
        private bool isCompleted;
        private bool wasNotified;

        public Guid Id { get; set; }

        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Status));
            }
        }

        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public DateTime ReminderTime
        {
            get => reminderTime;
            set
            {
                reminderTime = value;
                OnPropertyChanged(nameof(ReminderTime));
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool IsCompleted
        {
            get => isCompleted;
            set
            {
                isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool WasNotified
        {
            get => wasNotified;
            set
            {
                wasNotified = value;
                OnPropertyChanged(nameof(WasNotified));
            }
        }

        public string Status
        {
            get
            {
                if (IsCompleted)
                    return "Выполнена";
                if (DateTime.Now > ReminderTime)
                    return "Просрочена";
                return "Активна";
            }
        }

        public TaskItem()
        {
            Id = Guid.NewGuid();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
