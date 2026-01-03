using System;
using System.Windows.Forms;

namespace TaskCalendar
{
    public partial class TaskForm : Form
    {
        // Event for task saving
        public class TaskEventArgs : EventArgs
        {
            public Form1.Task Task { get; set; }
        }

        public event EventHandler<TaskEventArgs> TaskSaved;

        public TaskForm()
        {
            InitializeComponent();
        }

        // Set task data to form fields
        public void SetTask(Form1.Task task)
        {
            if (task != null)
            {
                // Set values for existing task
                txtName.Text = task.Name;

                // Проверяем, что время задачи не выходит за пределы допустимого диапазона DateTimePicker
                DateTime timestamp = task.Timestamp;

                // Если задача уже выполнена или её время далеко в будущем
                if (timestamp == DateTime.MaxValue || timestamp.Year > 9980)
                {
                    // Установим текущее время вместо недопустимого значения
                    timestamp = DateTime.Now;
                }

                // Теперь безопасно устанавливаем значения
                datePicker.Value = timestamp.Date;
                numHour.Value = timestamp.Hour;
                numMinute.Value = timestamp.Minute;
            }
            else
            {
                // Initialize with default values for new task
                txtName.Text = string.Empty;
                datePicker.Value = DateTime.Today;
                numHour.Value = 0;
                numMinute.Value = 0;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Task name cannot be empty!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create task timestamp from date and time values
            DateTime taskTime = datePicker.Value.Date
                .AddHours((double)numHour.Value)
                .AddMinutes((double)numMinute.Value);

            // Create task
            Form1.Task task = new Form1.Task
            {
                Name = txtName.Text,
                Timestamp = taskTime,
                RepeatDays = 0  // Default to no repeat
            };

            // Raise event with the new/edited task
            TaskSaved?.Invoke(this, new TaskEventArgs { Task = task });

            // Close form
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}