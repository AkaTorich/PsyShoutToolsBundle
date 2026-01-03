using System;
using System.IO;
using System.Windows.Forms;
using TagLib;
using System.Linq;
using System.Drawing;

namespace WavTagEditor
{
    public partial class MainForm : Form
    {
        private string selectedFolder;

        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите папку с WAV файлами";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFolder = folderDialog.SelectedPath;
                    lblStatus.Text = $"Выбрана папка: {selectedFolder}";
                    LoadFiles(selectedFolder);
                    btnUpdateTags.Enabled = lstFiles.Items.Count > 0;
                }
            }
        }

        private void LoadFiles(string folderPath)
        {
            lstFiles.Items.Clear();

            try
            {
                string[] wavFiles = Directory.GetFiles(folderPath, "*.wav", SearchOption.TopDirectoryOnly);

                foreach (string filePath in wavFiles)
                {
                    FileInfo fileInfo = new FileInfo(filePath);

                    ListViewItem item = new ListViewItem(fileInfo.Name);
                    item.SubItems.Add(FormatFileSize(fileInfo.Length));

                    // Добавляем текущие теги
                    try
                    {
                        using (TagLib.File file = TagLib.File.Create(filePath))
                        {
                            string artist = file.Tag.Performers.Length > 0 ? file.Tag.Performers[0] : "";
                            string title = file.Tag.Title ?? "";
                            string album = file.Tag.Album ?? "";
                            string year = file.Tag.Year > 0 ? file.Tag.Year.ToString() : "";
                            string genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "";
                            string comment = file.Tag.Comment ?? "";

                            item.SubItems.Add(artist);
                            item.SubItems.Add(title);
                            item.SubItems.Add(album);
                            item.SubItems.Add(year);
                            item.SubItems.Add(genre);
                            item.SubItems.Add(comment);
                        }
                    }
                    catch
                    {
                        // Если теги не удалось прочитать, добавляем пустые значения
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                    }

                    item.Tag = filePath; // Сохраняем полный путь для дальнейшего использования

                    lstFiles.Items.Add(item);
                }

                lblStatus.Text = $"Найдено WAV файлов: {wavFiles.Length}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файлов: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:0.##} {suffixes[suffixIndex]}";
        }

        private void BtnUpdateTags_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFolder) || lstFiles.Items.Count == 0)
            {
                MessageBox.Show("Сначала выберите папку с WAV файлами!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Проверяем, заполнены ли поля для новых тегов
                if (string.IsNullOrWhiteSpace(txtArtist.Text) &&
                    string.IsNullOrWhiteSpace(txtTitle.Text) &&
                    string.IsNullOrWhiteSpace(txtAlbum.Text) &&
                    string.IsNullOrWhiteSpace(txtYear.Text) &&
                    string.IsNullOrWhiteSpace(txtGenre.Text) &&
                    string.IsNullOrWhiteSpace(txtComment.Text))
                {
                    MessageBox.Show("Введите хотя бы одно значение тега для обновления!",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Спрашиваем, обновлять все файлы или только выбранные
                bool updateAll = true;
                if (lstFiles.SelectedItems.Count > 0 && lstFiles.SelectedItems.Count < lstFiles.Items.Count)
                {
                    DialogResult result = MessageBox.Show(
                        "Обновить теги только для выбранных файлов?\n\n" +
                        "Да - только выбранные файлы\n" +
                        "Нет - все файлы в папке",
                        "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel)
                        return;

                    updateAll = (result == DialogResult.No);
                }

                // Определяем список файлов для обновления
                var itemsToUpdate = updateAll ?
                    lstFiles.Items.Cast<ListViewItem>().ToList() :
                    lstFiles.SelectedItems.Cast<ListViewItem>().ToList();

                // Выделяем цветом ячейки, которые будут изменены
                PreviewChanges(itemsToUpdate);

                // Спрашиваем подтверждение обновления после предпросмотра
                DialogResult confirmResult = MessageBox.Show(
                    "Теги, которые будут изменены, выделены зеленым цветом в списке файлов.\n\n" +
                    "Применить указанные изменения?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirmResult == DialogResult.No)
                {
                    // Сбрасываем выделение цветом, если отменяем обновление
                    ResetFileListColors();
                    return;
                }

                // Обновляем теги
                int successCount = 0;
                foreach (ListViewItem item in itemsToUpdate)
                {
                    string filePath = item.Tag.ToString();
                    try
                    {
                        UpdateTags(filePath);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обработке файла {item.Text}: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                lblStatus.Text = $"Обновлено тегов: {successCount} из {itemsToUpdate.Count}";
                MessageBox.Show($"Обработка завершена! Обновлено {successCount} из {itemsToUpdate.Count} файлов.",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Обновляем список файлов, чтобы отразить изменения
                LoadFiles(selectedFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PreviewChanges(System.Collections.Generic.List<ListViewItem> items)
        {
            // Сначала сбрасываем все цвета
            ResetFileListColors();

            // Помечаем ячейки, которые будут изменены
            foreach (ListViewItem item in items)
            {
                string filePath = item.Tag.ToString();

                try
                {
                    using (TagLib.File file = TagLib.File.Create(filePath))
                    {
                        // Получаем текущие значения
                        string currentArtist = file.Tag.Performers.Length > 0 ? file.Tag.Performers[0] : "";
                        string currentTitle = file.Tag.Title ?? "";
                        string currentAlbum = file.Tag.Album ?? "";
                        string currentYear = file.Tag.Year > 0 ? file.Tag.Year.ToString() : "";
                        string currentGenre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "";
                        string currentComment = file.Tag.Comment ?? "";

                        // Новые значения
                        string newArtist = txtArtist.Text;
                        string newTitle = txtTitle.Text;
                        string newAlbum = txtAlbum.Text;
                        string newYear = txtYear.Text;
                        string newGenre = txtGenre.Text;
                        string newComment = txtComment.Text;

                        // Выделяем ячейки, которые будут изменены
                        if (!string.IsNullOrEmpty(newArtist) && newArtist != currentArtist)
                            item.SubItems[2].BackColor = Color.LightGreen;

                        if (!string.IsNullOrEmpty(newTitle) && newTitle != currentTitle)
                            item.SubItems[3].BackColor = Color.LightGreen;

                        if (!string.IsNullOrEmpty(newAlbum) && newAlbum != currentAlbum)
                            item.SubItems[4].BackColor = Color.LightGreen;

                        if (!string.IsNullOrEmpty(newYear) && newYear != currentYear)
                            item.SubItems[5].BackColor = Color.LightGreen;

                        if (!string.IsNullOrEmpty(newGenre) && newGenre != currentGenre)
                            item.SubItems[6].BackColor = Color.LightGreen;

                        if (!string.IsNullOrEmpty(newComment) && newComment != currentComment)
                            item.SubItems[7].BackColor = Color.LightGreen;
                    }
                }
                catch (Exception)
                {
                    // Если не удалось прочитать теги, предполагаем, что все поля будут изменены
                    for (int i = 2; i <= 7; i++)
                    {
                        if (!string.IsNullOrEmpty(GetNewValueForColumn(i)))
                            item.SubItems[i].BackColor = Color.LightGreen;
                    }
                }
            }
        }

        private string GetNewValueForColumn(int columnIndex)
        {
            switch (columnIndex)
            {
                case 2: return txtArtist.Text;
                case 3: return txtTitle.Text;
                case 4: return txtAlbum.Text;
                case 5: return txtYear.Text;
                case 6: return txtGenre.Text;
                case 7: return txtComment.Text;
                default: return "";
            }
        }

        private void ResetFileListColors()
        {
            foreach (ListViewItem item in lstFiles.Items)
            {
                for (int i = 0; i < item.SubItems.Count; i++)
                {
                    item.SubItems[i].BackColor = Color.White;
                }
            }
        }

        private void UpdateTags(string filePath)
        {
            using (TagLib.File file = TagLib.File.Create(filePath))
            {
                if (!string.IsNullOrEmpty(txtArtist.Text))
                    file.Tag.Performers = new string[] { txtArtist.Text };

                if (!string.IsNullOrEmpty(txtTitle.Text))
                    file.Tag.Title = txtTitle.Text;

                if (!string.IsNullOrEmpty(txtAlbum.Text))
                    file.Tag.Album = txtAlbum.Text;

                // Преобразование года из строки в uint
                if (!string.IsNullOrEmpty(txtYear.Text) && uint.TryParse(txtYear.Text, out uint year))
                {
                    file.Tag.Year = year;
                }

                if (!string.IsNullOrEmpty(txtGenre.Text))
                    file.Tag.Genres = new string[] { txtGenre.Text };

                if (!string.IsNullOrEmpty(txtComment.Text))
                    file.Tag.Comment = txtComment.Text;

                file.Save();
            }
        }

        private void LstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count > 0)
            {
                // При выборе файла в списке можно загрузить его теги в поля ввода
                ListViewItem item = lstFiles.SelectedItems[0];

                // Заполняем поля текущими значениями тегов выбранного файла
                txtArtist.Text = item.SubItems[2].Text;
                txtTitle.Text = item.SubItems[3].Text;
                txtAlbum.Text = item.SubItems[4].Text;
                txtYear.Text = item.SubItems[5].Text;
                txtGenre.Text = item.SubItems[6].Text;
                txtComment.Text = item.SubItems[7].Text;
            }
        }
    }
}