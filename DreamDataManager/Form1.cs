// Form1.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO; // Добавлено для работы с файлами

namespace DreamDiary
{
    public partial class Form1 : Form
    {
        private List<Dream> dreams;
        private int sortColumn = -1; // Индекс текущего столбца для сортировки
        private byte[] EncryptionKey;
        private byte[] EncryptionIV;

        public Form1(byte[] key, byte[] iv)
        {
            InitializeComponent();
            EncryptionKey = key;
            EncryptionIV = iv;
            InitializeListView();
            LoadDreams();
        }

        private void InitializeListView()
        {
            listViewDreams.Columns.Add("Заголовок", 150, HorizontalAlignment.Left);
            listViewDreams.Columns.Add("Описание сна", 350, HorizontalAlignment.Left);
            listViewDreams.Columns.Add("Дата", 100, HorizontalAlignment.Right);
            listViewDreams.FullRowSelect = true;
            listViewDreams.View = View.Details;
            listViewDreams.ColumnClick += ListViewDreams_ColumnClick;
            listViewDreams.Dock = DockStyle.Fill;
        }

        private void LoadDreams()
        {
            dreams = DreamDataManager.LoadDreams(EncryptionKey, EncryptionIV);
            RefreshListView();
        }

        private void SaveDreams()
        {
            DreamDataManager.SaveDreams(dreams, EncryptionKey, EncryptionIV);
        }

        private void RefreshListView()
        {
            listViewDreams.Items.Clear();
            foreach (var dream in dreams)
            {
                var item = new ListViewItem(dream.Title);
                item.SubItems.Add(dream.Description);
                item.SubItems.Add(dream.Date.ToShortDateString());
                item.Tag = dream.ID; // Сохраняем ID сна для идентификации
                listViewDreams.Items.Add(item);
            }

            // Если был выбран столбец для сортировки, применяем сортировку
            if (sortColumn != -1)
            {
                listViewDreams.ListViewItemSorter = new ListViewItemComparer(sortColumn, listViewDreams.Sorting);
                listViewDreams.Sort();
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            AddDreamForm addForm = new AddDreamForm(EncryptionKey, EncryptionIV);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                Dream newDream = addForm.UpdatedDream; // Обновлённый сон уже содержит ID
                dreams.Add(newDream);
                SaveDreams();
                RefreshListView();
            }
        }

        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (listViewDreams.SelectedItems.Count > 0)
            {
                var selectedItem = listViewDreams.SelectedItems[0];
                Guid selectedID;

                // Проверяем, содержит ли Tag значение и можно ли его преобразовать в Guid
                if (selectedItem.Tag != null && Guid.TryParse(selectedItem.Tag.ToString(), out selectedID))
                {
                    var dreamToRemove = dreams.FirstOrDefault(d => d.ID == selectedID);
                    if (dreamToRemove != null)
                    {
                        var result = MessageBox.Show("Вы уверены, что хотите удалить выбранный сон?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            dreams.Remove(dreamToRemove);
                            SaveDreams();
                            RefreshListView();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти выбранный сон.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось определить идентификатор выбранного сна.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сон для удаления.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ListViewDreams_DoubleClick(object sender, EventArgs e)
        {
            if (listViewDreams.SelectedItems.Count > 0)
            {
                var selectedItem = listViewDreams.SelectedItems[0];
                Guid selectedID;

                // Проверяем, содержит ли Tag значение и можно ли его преобразовать в Guid
                if (selectedItem.Tag != null && Guid.TryParse(selectedItem.Tag.ToString(), out selectedID))
                {
                    Dream selectedDream = dreams.FirstOrDefault(d => d.ID == selectedID);
                    if (selectedDream != null)
                    {
                        DreamDetailsForm detailsForm = new DreamDetailsForm(selectedDream, EncryptionKey, EncryptionIV);
                        if (detailsForm.ShowDialog() == DialogResult.OK)
                        {
                            // Обновляем сон в списке
                            selectedDream.Title = detailsForm.UpdatedDream.Title;
                            selectedDream.Description = detailsForm.UpdatedDream.Description;
                            // Дата остаётся неизменной

                            SaveDreams();
                            RefreshListView();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти выбранный сон.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось определить идентификатор выбранного сна.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ListViewDreams_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != sortColumn)
            {
                // Если клик на новый столбец, устанавливаем направление сортировки по возрастанию
                sortColumn = e.Column;
                listViewDreams.Sorting = SortOrder.Ascending;
            }
            else
            {
                // Если клик на тот же столбец, переключаем направление сортировки
                if (listViewDreams.Sorting == SortOrder.Ascending)
                    listViewDreams.Sorting = SortOrder.Descending;
                else
                    listViewDreams.Sorting = SortOrder.Ascending;
            }

            // Устанавливаем сортировщик и сортируем
            listViewDreams.ListViewItemSorter = new ListViewItemComparer(e.Column, listViewDreams.Sorting);
            listViewDreams.Sort();
        }

        // Новый метод для обработки клика по кнопке Backup
        private void ButtonBackup_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Сохранить резервную копию базы данных";
                saveFileDialog.Filter = "Файлы базы данных (*.dat)|*.dat|Все файлы (*.*)|*.*";
                saveFileDialog.DefaultExt = "dat";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = "DreamDiaryBackup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".dat";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string sourcePath = DreamDataManager.GetDataFilePath();

                        if (!File.Exists(sourcePath))
                        {
                            MessageBox.Show("Исходный файл данных не найден. Резервное копирование невозможно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        File.Copy(sourcePath, saveFileDialog.FileName, overwrite: true);
                        MessageBox.Show("Резервная копия успешно сохранена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении резервной копии: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
