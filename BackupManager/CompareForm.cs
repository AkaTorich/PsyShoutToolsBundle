using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Forms
{
    public partial class CompareForm : Form
    {
        private List<FileDifference> _differences;
        private string _sourceDirectory;
        private string _backupDirectory;
        private ILogger _logger;

        public CompareForm(List<FileDifference> differences, string sourceDirectory, string backupDirectory, ILogger logger)
        {
            _differences = differences;
            _sourceDirectory = sourceDirectory;
            _backupDirectory = backupDirectory;
            _logger = logger;
            InitializeComponent();
            LoadDifferences();
        }

        private void LoadDifferences()
        {
            differencesListView.Items.Clear();
            foreach (var diff in _differences)
            {
                var item = new ListViewItem(diff.RelativePath);
                // Добавление статуса
                switch (diff.DifferenceType)
                {
                    case DifferenceType.ContentDifferent:
                        item.SubItems.Add("Содержимое отличается");
                        break;
                    case DifferenceType.MissingInSource:
                        item.SubItems.Add("Отсутствует в источнике");
                        break;
                    case DifferenceType.MissingInBackup:
                        item.SubItems.Add("Отсутствует в копии");
                        break;
                }
                // Добавление размеров
                item.SubItems.Add(diff.SourceFile != null ? FormatFileSize(diff.SourceFile.Size) : "-");
                item.SubItems.Add(diff.BackupFile != null ? FormatFileSize(diff.BackupFile.Size) : "-");
                // Задаем цвет в зависимости от типа различия
                switch (diff.DifferenceType)
                {
                    case DifferenceType.ContentDifferent:
                        item.BackColor = Color.LightYellow;
                        break;
                    case DifferenceType.MissingInSource:
                        item.BackColor = Color.LightBlue;
                        break;
                    case DifferenceType.MissingInBackup:
                        item.BackColor = Color.LightPink;
                        break;
                }
                differencesListView.Items.Add(item);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "Б", "КБ", "МБ", "ГБ", "ТБ" };
            int i = 0;
            double dblBytes = bytes;
            while (dblBytes >= 1024 && i < suffixes.Length - 1)
            {
                dblBytes /= 1024;
                i++;
            }
            return $"{dblBytes:0.##} {suffixes[i]}";
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}