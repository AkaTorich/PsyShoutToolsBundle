using System;
using System.Windows.Forms;
using System.Drawing;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Forms
{
    public partial class LogViewForm : Form
    {
        private BackupJob _job;

        public LogViewForm(BackupJob job)
        {
            _job = job;
            InitializeComponent();
            LoadLogs();
        }

        private void LoadLogs()
        {
            this.Text = $"Журнал для {_job.SourceDirectory} -> {_job.BackupDirectory}";

            logTextBox.Clear();
            foreach (var log in _job.Logs)
            {
                logTextBox.SelectionColor = log.Level == LogLevel.Error
                    ? Color.Red
                    : (log.Level == LogLevel.Warning
                        ? Color.Orange
                        : Color.Black);
                logTextBox.AppendText(log.ToString() + Environment.NewLine);
            }
            logTextBox.ScrollToCaret();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}