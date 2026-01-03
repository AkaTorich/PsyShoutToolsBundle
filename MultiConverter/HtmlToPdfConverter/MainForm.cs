using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace HtmlToPdfConverter
{
    public partial class MainForm : Form
    {
        private List<string> selectedFiles;
        private PdfConverter converter;
        private bool isConverting = false;
        private Point? dragStartPoint = null;
        private int insertIndex = -1;

        public MainForm()
        {
            InitializeComponent();
            selectedFiles = new List<string>();
            converter = new PdfConverter();
            InitializeForm();
            
            // Инициализация уровня сжатия по умолчанию
            if (cmbCompression != null)
            {
                cmbCompression.SelectedIndex = 0; // Рекомендуемая (25%)
                cmbCompression.Enabled = chkCompress.Checked;
                chkCompress.CheckedChanged += (s, e) =>
                {
                    cmbCompression.Enabled = chkCompress.Checked;
                };
            }
        }

        private void InitializeForm()
        {
            // Установка начального пути для выходного файла
            string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "converted_document.pdf");
            txtOutputPath.Text = defaultPath;

            // Обновление счетчика файлов
            UpdateFileCount();
        }

        private void btnSelectFiles_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите HTML, MHTML или PDF файлы";
                openFileDialog.Filter = "HTML/MHTML/PDF|*.html;*.htm;*.mhtml;*.mht;*.pdf|HTML файлы|*.html;*.htm|MHTML файлы|*.mhtml;*.mht|PDF файлы|*.pdf|Все файлы|*.*";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        if (!selectedFiles.Contains(fileName))
                        {
                            selectedFiles.Add(fileName);
                            lstFiles.Items.Add(Path.GetFileName(fileName) + " (" + fileName + ")");
                        }
                    }
                    UpdateFileCount();
                }
            }
        }

        private void btnRemoveFile_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndices.Count > 0)
            {
                // Создаем список индексов для удаления в обратном порядке
                var indicesToRemove = lstFiles.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
                
                foreach (int index in indicesToRemove)
                {
                    selectedFiles.RemoveAt(index);
                    lstFiles.Items.RemoveAt(index);
                }
                
                UpdateFileCount();
            }
            else
            {
                MessageBox.Show("Выберите файлы для удаления из списка.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (selectedFiles.Count > 0)
            {
                var result = MessageBox.Show("Вы уверены, что хотите очистить весь список файлов?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    selectedFiles.Clear();
                    lstFiles.Items.Clear();
                    UpdateFileCount();
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Выберите место для сохранения PDF файла";
                saveFileDialog.Filter = "PDF файлы|*.pdf|Все файлы|*.*";
                saveFileDialog.DefaultExt = "pdf";
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(txtOutputPath.Text);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = saveFileDialog.FileName;
                }
            }
        }

        private async void btnConvert_Click(object sender, EventArgs e)
        {
            if (isConverting)
            {
                MessageBox.Show("Конвертация уже выполняется. Пожалуйста, дождитесь завершения.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один файл для конвертации.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtOutputPath.Text))
            {
                MessageBox.Show("Укажите путь для сохранения PDF файла.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем, что все выбранные файлы существуют
            var missingFiles = selectedFiles.Where(f => !File.Exists(f)).ToList();
            if (missingFiles.Any())
            {
                var message = $"Следующие файлы не найдены:\n{string.Join("\n", missingFiles.Take(5))}";
                if (missingFiles.Count > 5)
                {
                    message += $"\n... и еще {missingFiles.Count - 5} файлов";
                }
                MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                isConverting = true;
                SetControlsEnabled(false);
                
                progressBar.Value = 0;
                lblProgress.Text = "Инициализация...";
                
                converter = new PdfConverter();
                converter.EnableCompression = chkCompress.Checked;
                if (chkCompress.Checked)
                {
                    // 1.0 = 100% (максимальная), 0.5 = 50% (умеренная), 0.25 = 25% (рекомендуемая)
                    double factor = 0.25;
                    if (cmbCompression.SelectedIndex == 1) factor = 0.5;
                    else if (cmbCompression.SelectedIndex == 2) factor = 1.0;
                    converter.CompressionFactor = factor;
                }
                
                // Переносим длительную работу в фоновую задачу, чтобы не блокировать STA/UI поток
                await Task.Run(() => converter.ConvertToPdfAsync(selectedFiles, txtOutputPath.Text, UpdateProgress));
                
                MessageBox.Show($"Конвертация успешно завершена!\nРезультат сохранен в: {txtOutputPath.Text}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                var result = MessageBox.Show("Хотите открыть папку с созданным PDF файлом?", "Открыть папку", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{txtOutputPath.Text}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при конвертации:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblProgress.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                isConverting = false;
                SetControlsEnabled(true);
                progressBar.Value = 0;
                
                if (!lblProgress.Text.StartsWith("Ошибка:"))
                {
                    lblProgress.Text = "Готов к работе";
                }
            }
        }

        private void UpdateProgress(int percentage, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, string>(UpdateProgress), percentage, message);
                return;
            }

            progressBar.Value = Math.Min(percentage, 100);
            lblProgress.Text = message;
            Application.DoEvents();
        }

        private void UpdateFileCount()
        {
            lblFileCount.Text = $"Файлов: {selectedFiles.Count}";
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnSelectFiles.Enabled = enabled;
            btnRemoveFile.Enabled = enabled;
            btnClearAll.Enabled = enabled;
            btnBrowseOutput.Enabled = enabled;
            btnConvert.Enabled = enabled;
            txtOutputPath.Enabled = enabled;
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            if (isConverting)
            {
                var result = MessageBox.Show("Конвертация еще не завершена. Вы уверены, что хотите закрыть приложение?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            try
            {
                if (converter != null)
                {
                    await converter.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не блокируем закрытие приложения
                var logPath = Path.Combine(Application.StartupPath, "userapp.txt");
                try
                {
                    File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Ошибка при закрытии: {ex.Message}\r\n", Encoding.UTF8);
                }
                catch { }
            }

            base.OnFormClosing(e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Дополнительная инициализация при загрузке формы
            lblProgress.Text = "Готов к работе. Выберите HTML или MHTML файлы для конвертации.";
            // Включаем отрисовку для отображения индикатора вставки
            lstFiles.DrawMode = DrawMode.OwnerDrawFixed;
            lstFiles.DrawItem += lstFiles_DrawItem;
        }

        private void lstFiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0 && e.Index < lstFiles.Items.Count)
            {
                TextRenderer.DrawText(e.Graphics, lstFiles.Items[e.Index].ToString(), e.Font, e.Bounds, e.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
            // Рисуем черную линию на позиции вставки
            if (insertIndex >= 0)
            {
                int lineY;
                if (insertIndex >= lstFiles.Items.Count)
                    lineY = lstFiles.GetItemRectangle(lstFiles.Items.Count - 1).Bottom - 1;
                else
                    lineY = lstFiles.GetItemRectangle(Math.Max(insertIndex, 0)).Top;
                using (var pen = new Pen(Color.Black, 2))
                {
                    e.Graphics.DrawLine(pen, 0, lineY, lstFiles.Width, lineY);
                }
            }
            e.DrawFocusRectangle();
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndices.Count == 0) return;
            // Двигаем первые выбранные элементы вверх по одному
            var indices = lstFiles.SelectedIndices.Cast<int>().OrderBy(i => i).ToList();
            bool changed = false;
            for (int k = 0; k < indices.Count; k++)
            {
                int i = indices[k];
                if (i == 0) continue;
                SwapItems(i, i - 1);
                indices[k] = i - 1;
                changed = true;
            }
            if (changed) Reselect(indices);
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndices.Count == 0) return;
            var indices = lstFiles.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            bool changed = false;
            for (int k = 0; k < indices.Count; k++)
            {
                int i = indices[k];
                if (i >= lstFiles.Items.Count - 1) continue;
                SwapItems(i, i + 1);
                indices[k] = i + 1;
                changed = true;
            }
            if (changed) Reselect(indices);
        }

        private void SwapItems(int a, int b)
        {
            // Порядок в визуальном списке
            var tmpItem = lstFiles.Items[a];
            lstFiles.Items[a] = lstFiles.Items[b];
            lstFiles.Items[b] = tmpItem;
            // Порядок в данных
            var tmpPath = selectedFiles[a];
            selectedFiles[a] = selectedFiles[b];
            selectedFiles[b] = tmpPath;
        }

        private void Reselect(IEnumerable<int> newIndices)
        {
            lstFiles.ClearSelected();
            foreach (var i in newIndices)
                lstFiles.SetSelected(i, true);
        }

        private void lstFiles_MouseDown(object sender, MouseEventArgs e)
        {
            dragStartPoint = new Point(e.X, e.Y);
        }

        private void lstFiles_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || dragStartPoint == null) return;
            // Начинаем перетаскивание при смещении курсора
            var delta = new Size(Math.Abs(e.X - dragStartPoint.Value.X), Math.Abs(e.Y - dragStartPoint.Value.Y));
            if (delta.Width + delta.Height >= SystemInformation.DragSize.Width / 4)
            {
                if (lstFiles.SelectedItems.Count > 0)
                    lstFiles.DoDragDrop(lstFiles.SelectedItems, DragDropEffects.Move);
                dragStartPoint = null;
            }
        }

        private void lstFiles_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListBox.SelectedObjectCollection)))
            {
                e.Effect = DragDropEffects.Move;
                var p = lstFiles.PointToClient(new Point(e.X, e.Y));
                int idx = lstFiles.IndexFromPoint(p);
                if (idx < 0) idx = lstFiles.Items.Count; // линия после последнего
                // Обновляем индекс вставки и перерисовываем
                if (insertIndex != idx)
                {
                    insertIndex = idx;
                    lstFiles.Invalidate();
                }
            }
            else e.Effect = DragDropEffects.None;
        }

        private void lstFiles_DragLeave(object sender, EventArgs e)
        {
            insertIndex = -1;
            lstFiles.Invalidate();
        }

        private void lstFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ListBox.SelectedObjectCollection))) return;

            Point clientPoint = lstFiles.PointToClient(new Point(e.X, e.Y));
            int targetIndex = lstFiles.IndexFromPoint(clientPoint);
            if (targetIndex < 0) targetIndex = lstFiles.Items.Count - 1;

            var selected = lstFiles.SelectedIndices.Cast<int>().OrderBy(i => i).ToList();
            if (selected.Count == 0) return;

            if (targetIndex > selected.Last()) targetIndex = targetIndex - selected.Count + 1;
            if (targetIndex < 0) targetIndex = 0;

            var items = lstFiles.Items.Cast<object>().ToList();
            var files = selectedFiles.ToList();
            var movingItems = selected.Select(i => new { item = items[i], file = files[i] }).ToList();

            for (int i = selected.Count - 1; i >= 0; i--)
            {
                items.RemoveAt(selected[i]);
                files.RemoveAt(selected[i]);
            }
            items.InsertRange(targetIndex, movingItems.Select(m => m.item));
            files.InsertRange(targetIndex, movingItems.Select(m => m.file));

            lstFiles.Items.Clear();
            foreach (var it in items) lstFiles.Items.Add(it);
            selectedFiles = files;

            lstFiles.ClearSelected();
            for (int i = 0; i < movingItems.Count; i++)
                lstFiles.SetSelected(targetIndex + i, true);

            insertIndex = -1;
            lstFiles.Invalidate();
        }
    }
}