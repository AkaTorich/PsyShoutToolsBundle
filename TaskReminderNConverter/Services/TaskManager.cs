using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using TaskReminderApp.Models;

namespace TaskReminderApp.Services
{
    public class TaskManager
    {
        private readonly string dataFilePath;
        public ObservableCollection<TaskItem> Tasks { get; private set; }

        public TaskManager()
        {
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TaskReminderApp");

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            dataFilePath = Path.Combine(appDataFolder, "tasks.json");
            Tasks = new ObservableCollection<TaskItem>();
            LoadTasks();
        }

        public void AddTask(TaskItem task)
        {
            Tasks.Add(task);
            SaveTasks();
        }

        public void RemoveTask(TaskItem task)
        {
            Tasks.Remove(task);
            SaveTasks();
        }

        public void UpdateTask(TaskItem task)
        {
            SaveTasks();
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    var tasks = JsonSerializer.Deserialize<ObservableCollection<TaskItem>>(json);

                    if (tasks != null)
                    {
                        Tasks = tasks;
                    }
                }
            }
            catch
            {
            }
        }

        private void SaveTasks()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(Tasks, options);
                File.WriteAllText(dataFilePath, json);
            }
            catch
            {
            }
        }
    }
}
