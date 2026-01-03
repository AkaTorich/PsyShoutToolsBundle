using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace TaskCalendar
{
    public partial class Form1 : Form
    {
        // Constants
        private const string BASE_WINDOW_TITLE = "Calendar with Tasks";
        private const string TASKS_FILE_PATH = "tasks.dat";

        // Task class (with [Serializable] attribute for binary serialization)
        [Serializable]
        public class Task
        {
            public string Name { get; set; }
            public DateTime Timestamp { get; set; }
            public int RepeatDays { get; set; } // 0 means no repeat
        }

        // List of all tasks
        private List<Task> tasks = new List<Task>();

        // The index of the task being edited, -1 means new task
        private int editIndex = -1;

        // TaskForm reference
        private TaskForm taskForm;

        // Processing flag to prevent overlapping task checks
        private bool isProcessingTasks = false;

        public Form1()
        {
            InitializeComponent();

            // Настраиваем иконку в трее из ресурсов формы
            try
            {
                // Используем иконку из ресурсов формы
                System.ComponentModel.ComponentResourceManager resources =
                    new System.ComponentModel.ComponentResourceManager(typeof(Form1));

                // Пытаемся получить иконку из ресурсов формы
                notifyIcon.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");

                // Если иконка не найдена в ресурсах формы, используем иконку окна
                if (notifyIcon.Icon == null && this.Icon != null)
                {
                    notifyIcon.Icon = this.Icon;
                }
            }
            catch (Exception)
            {
                // Если возникла ошибка, используем стандартную иконку
                notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            notifyIcon.Text = "Calendar with Tasks (Tray)";
            notifyIcon.Visible = true;

            // Load tasks before showing the form
            LoadTasks();

            // Create the task form
            taskForm = new TaskForm();
            taskForm.TaskSaved += TaskForm_TaskSaved;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Refresh the task list on form load
            RefreshTaskList();
        }

        private void BtnAddTask_Click(object sender, EventArgs e)
        {
            // Set edit index to -1 to indicate adding a new task
            editIndex = -1;

            // Set form title and show it
            taskForm.Text = "Add Task";
            taskForm.SetTask(null);
            taskForm.ShowDialog(this);
        }

        private void BtnEditTask_Click(object sender, EventArgs e)
        {
            if (taskListBox.SelectedIndex >= 0 && taskListBox.SelectedIndex < tasks.Count)
            {
                // Set the edit index
                editIndex = taskListBox.SelectedIndex;

                // Set form title and task data, then show it
                taskForm.Text = "Edit Task";
                taskForm.SetTask(tasks[editIndex]);
                taskForm.ShowDialog(this);
            }
            else
            {
                MessageBox.Show("Select a task to edit!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteTask_Click(object sender, EventArgs e)
        {
            if (taskListBox.SelectedIndex >= 0 && taskListBox.SelectedIndex < tasks.Count)
            {
                tasks.RemoveAt(taskListBox.SelectedIndex);
                RefreshTaskList();
                SaveTasks();
            }
            else
            {
                MessageBox.Show("Select a task to delete!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TaskForm_TaskSaved(object sender, TaskForm.TaskEventArgs e)
        {
            if (editIndex >= 0 && editIndex < tasks.Count)
            {
                // Edit existing task
                tasks[editIndex] = e.Task;
            }
            else
            {
                // Add new task
                tasks.Add(e.Task);
            }

            // Refresh list and save
            RefreshTaskList();
            SaveTasks();
        }

        private void TaskTimer_Tick(object sender, EventArgs e)
        {
            // Update window title with current time
            Text = $"{BASE_WINDOW_TITLE} - {DateTime.Now:HH:mm:ss}";

            // Check for tasks that need notification
            CheckTasks();
        }

        private void CheckTasks()
        {
            // Prevent overlapping task checks
            if (isProcessingTasks)
                return;

            isProcessingTasks = true;
            DateTime now = DateTime.Now;

            for (int i = 0; i < tasks.Count; i++)
            {
                Task task = tasks[i];
                // Проверяем, не выполнена ли уже задача (DateTime.MaxValue) и не в будущем ли
                if (task.Timestamp != DateTime.MaxValue && task.Timestamp <= now)
                {
                    // Show notification
                    MessageBox.Show($"Пора выполнить задачу: {task.Name}", "Напоминание",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (task.RepeatDays > 0)
                    {
                        // Update timestamp for recurring task
                        task.Timestamp = task.Timestamp.AddDays(task.RepeatDays);
                    }
                    else
                    {
                        // Reset timestamp for non-recurring task (mark as done)
                        task.Timestamp = DateTime.MaxValue;
                    }

                    // Save tasks after modification
                    SaveTasks();
                    RefreshTaskList(); // Обновляем список задач
                    break; // Process only one task at a time
                }
            }

            isProcessingTasks = false;
        }

        // Метод отображения списка задач с индикацией состояния
        private void RefreshTaskList()
        {
            taskListBox.Items.Clear();
            foreach (Task task in tasks)
            {
                string status = "";
                if (task.Timestamp == DateTime.MaxValue)
                {
                    status = " [Выполнено]";
                }
                else if (task.RepeatDays > 0)
                {
                    status = " [Повтор]";
                }

                taskListBox.Items.Add(task.Name + status);
            }
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists(TASKS_FILE_PATH))
                {
                    using (FileStream fs = new FileStream(TASKS_FILE_PATH, FileMode.Open))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        tasks = (List<Task>)formatter.Deserialize(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                tasks = new List<Task>();
            }
        }

        private void SaveTasks()
        {
            try
            {
                using (FileStream fs = new FileStream(TASKS_FILE_PATH, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, tasks);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving tasks: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save tasks before closing
            SaveTasks();

            // Confirm exit
            if (MessageBox.Show("Are you sure to exit?", "Exit",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                e.Cancel = true;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // При минимизации окна скрываем его и показываем только иконку в трее
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                WindowState = FormWindowState.Normal;
            }
        }

        // Tray Icon Handling

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Toggle form visibility
                if (Visible)
                {
                    Hide();
                }
                else
                {
                    Show();
                    WindowState = FormWindowState.Normal;
                    Activate();
                }
            }
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}