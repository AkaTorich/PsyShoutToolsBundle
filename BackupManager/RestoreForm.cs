//RestoreForm
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Forms
{
    public partial class RestoreForm : Form
    {
        private List<FileDifference> _differences;
        private string _sourceDirectory;
        private string _backupDirectory;
        private FileSystemManager _fileManager;
        private ILogger _logger;

        public RestoreForm(List<FileDifference> differences, string sourceDirectory, string backupDirectory,
                          FileSystemManager fileManager, ILogger logger)
        {
            _differences = differences;
            _sourceDirectory = sourceDirectory;
            _backupDirectory = backupDirectory;
            _fileManager = fileManager;
            _logger = logger;

            InitializeComponent();
            LoadDifferences();
        }

        private void LoadDifferences()
        {
            differencesListView.Items.Clear();

            // Отфильтруем список различий - покажем только те файлы, которые можно восстановить
            var restorableDifferences = _differences.Where(d =>
                d.DifferenceType == DifferenceType.ContentDifferent ||
                d.DifferenceType == DifferenceType.MissingInSource).ToList();

            foreach (var diff in restorableDifferences)
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
                }

                // Добавление размеров
                item.SubItems.Add(diff.SourceFile != null ? FormatFileSize(diff.SourceFile.Size) : "-");
                item.SubItems.Add(diff.BackupFile != null ? FormatFileSize(diff.BackupFile.Size) : "-");

                // Задаем цвет в зависимости от типа различия
                switch (diff.DifferenceType)
                {
                    case DifferenceType.ContentDifferent:
                        item.BackColor = System.Drawing.Color.LightYellow;
                        break;
                    case DifferenceType.MissingInSource:
                        item.BackColor = System.Drawing.Color.LightBlue;
                        break;
                }

                // Сохраняем индекс различия в теге элемента
                item.Tag = _differences.IndexOf(diff);

                differencesListView.Items.Add(item);
            }

            if (restorableDifferences.Count == 0)
            {
                MessageBox.Show("Нет файлов, которые можно восстановить.", "Восстановление",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        private void SelectAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < differencesListView.Items.Count; i++)
            {
                differencesListView.Items[i].Checked = true;
            }
        }

        private void RestoreButton_Click(object sender, EventArgs e)
        {
            var selectedItems = new List<ListViewItem>();

            // Собираем выбранные элементы
            for (int i = 0; i < differencesListView.Items.Count; i++)
            {
                if (differencesListView.Items[i].Checked)
                {
                    selectedItems.Add(differencesListView.Items[i]);
                }
            }

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Не выбрано ни одного файла для восстановления.", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Восстановить выбранные файлы ({selectedItems.Count} шт.)?",
                                       "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _logger.Log($"Начало восстановления {selectedItems.Count} файлов.");

                    foreach (var item in selectedItems)
                    {
                        int diffIndex = (int)item.Tag;
                        var diff = _differences[diffIndex];
                        string sourcePath = Path.Combine(_backupDirectory, diff.RelativePath);
                        string destinationPath = Path.Combine(_sourceDirectory, diff.RelativePath);

                        // Восстанавливаем файл
                        _fileManager.RestoreFile(sourcePath, destinationPath, _logger);
                    }

                    _logger.Log("Восстановление файлов успешно завершено.");
                    MessageBox.Show("Файлы успешно восстановлены.", "Восстановление",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    _logger.Log($"Ошибка при восстановлении файлов: {ex.Message}", LogLevel.Error);
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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