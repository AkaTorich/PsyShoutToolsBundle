using System;
using System.Drawing;
using System.Windows.Forms;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Forms
{
    public partial class ScheduleListForm : Form
    {
        private readonly BackupScheduler _scheduler;
        private readonly ILogger _logger;

        public ScheduleListForm(BackupScheduler scheduler, ILogger logger)
        {
            _scheduler = scheduler;
            _logger = logger;

            InitializeComponent();
            LoadScheduledJobs();
        }

        private void LoadScheduledJobs()
        {
            scheduledJobsListView.Items.Clear();

            var jobs = _scheduler.GetScheduledJobs();
            foreach (var job in jobs)
            {
                var item = new ListViewItem(job.Name);
                item.SubItems.Add(job.GetScheduleDescription());
                item.SubItems.Add(job.GetSyncTypeDescription());
                item.SubItems.Add(job.SourceDirectory);
                item.SubItems.Add(job.BackupDirectory);
                item.SubItems.Add(job.Enabled ? "Да" : "Нет");
                item.SubItems.Add(job.LastRunTime.HasValue ? job.LastRunTime.Value.ToString("dd.MM.yyyy HH:mm") : "-");
                item.SubItems.Add(job.LastRunSuccessful ? "Успешно" : (job.LastRunTime.HasValue ? "Ошибка" : "-"));

                // Устанавливаем цвет в зависимости от активности и результата последнего запуска
                if (!job.Enabled)
                {
                    item.BackColor = Color.LightGray;
                }
                else if (job.LastRunTime.HasValue && !job.LastRunSuccessful)
                {
                    item.BackColor = Color.LightPink;
                }
                else if (job.LastRunTime.HasValue && job.LastRunSuccessful)
                {
                    item.BackColor = Color.LightGreen;
                }

                // Сохраняем идентификатор задания
                item.Tag = job.Id;

                scheduledJobsListView.Items.Add(item);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var scheduleForm = new ScheduleForm(_logger);
            if (scheduleForm.ShowDialog() == DialogResult.OK)
            {
                _scheduler.AddScheduledJob(scheduleForm.Job);
                LoadScheduledJobs();
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (scheduledJobsListView.SelectedItems.Count > 0)
            {
                string jobId = scheduledJobsListView.SelectedItems[0].Tag.ToString();
                var job = _scheduler.GetScheduledJobs().Find(j => j.Id == jobId);

                if (job != null)
                {
                    var scheduleForm = new ScheduleForm(_logger, job);
                    if (scheduleForm.ShowDialog() == DialogResult.OK)
                    {
                        _scheduler.UpdateScheduledJob(scheduleForm.Job);
                        LoadScheduledJobs();
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите задание для редактирования.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (scheduledJobsListView.SelectedItems.Count > 0)
            {
                string jobId = scheduledJobsListView.SelectedItems[0].Tag.ToString();
                var job = _scheduler.GetScheduledJobs().Find(j => j.Id == jobId);

                if (job != null)
                {
                    var result = MessageBox.Show($"Вы действительно хотите удалить задание \"{job.Name}\"?",
                        "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _scheduler.RemoveScheduledJob(jobId);
                        LoadScheduledJobs();
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите задание для удаления.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RunNowButton_Click(object sender, EventArgs e)
        {
            if (scheduledJobsListView.SelectedItems.Count > 0)
            {
                string jobId = scheduledJobsListView.SelectedItems[0].Tag.ToString();
                var job = _scheduler.GetScheduledJobs().Find(j => j.Id == jobId);

                if (job != null)
                {
                    var result = MessageBox.Show($"Запустить задание \"{job.Name}\" прямо сейчас?",
                        "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _scheduler.RunJobNow(jobId);
                        // После запуска обновляем список
                        System.Threading.Thread.Sleep(500); // Небольшая задержка для обновления статуса
                        LoadScheduledJobs();
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите задание для запуска.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadScheduledJobs();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ScheduledJobsListView_DoubleClick(object sender, EventArgs e)
        {
            EditButton_Click(sender, e);
        }
    }
}