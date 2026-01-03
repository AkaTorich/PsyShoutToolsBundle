using System;
using System.Drawing;
using System.Windows.Forms;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Forms
{
    public partial class HistoryForm : Form
    {
        private readonly BackupJobManager _jobManager;
        private readonly ILogger _logger;

        public HistoryForm(BackupJobManager jobManager, ILogger logger)
        {
            _jobManager = jobManager;
            _logger = logger;
            InitializeComponent();
            LoadJobHistory();
        }

        private void LoadJobHistory()
        {
            historyListView.Items.Clear();

            var jobs = _jobManager.GetAllJobs();
            foreach (var job in jobs)
            {
                var item = new ListViewItem(job.SourceDirectory);
                item.SubItems.Add(job.BackupDirectory);
                item.SubItems.Add(job.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss"));
                item.SubItems.Add(job.CompletedAt.HasValue ? job.CompletedAt.Value.ToString("dd.MM.yyyy HH:mm:ss") : "-");
                item.SubItems.Add(job.IsSuccessful ? "Успешно" : "Ошибка");

                // Устанавливаем цвет в зависимости от результата
                if (job.CompletedAt.HasValue)
                {
                    item.BackColor = job.IsSuccessful ? Color.LightGreen : Color.LightPink;
                }

                // Сохраняем ссылку на задание
                item.Tag = job;

                historyListView.Items.Add(item);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadJobHistory();
        }

        private void ClearHistoryButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите очистить историю заданий?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var jobs = _jobManager.GetAllJobs();
                foreach (var job in jobs.ToArray())
                {
                    _jobManager.RemoveJob(job);
                }

                LoadJobHistory();
                _logger.Log("История заданий очищена");
            }
        }

        private void ViewLogsButton_Click(object sender, EventArgs e)
        {
            if (historyListView.SelectedItems.Count > 0)
            {
                var job = historyListView.SelectedItems[0].Tag as BackupJob;
                if (job != null && job.Logs != null && job.Logs.Count > 0)
                {
                    var logForm = new LogViewForm(job);
                    logForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Нет журналов для выбранного задания.", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите задание для просмотра журналов.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}