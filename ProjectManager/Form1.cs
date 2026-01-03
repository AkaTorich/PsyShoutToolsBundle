// Form1.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectManager
{
    public partial class Form1 : Form
    {
        // Глобальные переменные
        private ContextMenuStrip contextMenu;
        private string chosenFolder = string.Empty;
        private List<string> projectFiles = new List<string>();
        private int fileIndexOnContextClick = -1;

        // Словарь: расширение -> название DAW
        private static readonly Dictionary<string, string> extToDaw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    {".cpr", "Cubase"},
    {".npr", "Nuendo"},
    {".mon", "WaveLab"},
    {".bwproject", "Bitwig Studio"},
    {".flp", "FL Studio"},
    {".flm", "FL Studio Mobile"},
    {".rpp", "Reaper"},
    {".ptx", "Pro Tools"},
    {".ptf", "Pro Tools (old)"},
    {".pts", "Pro Tools Session"},
    {".cwp", "Cakewalk by BandLab"},
    {".sng", "Sonar (old)"},
    {".song", "Sonar (old)"},
    {".wrk", "Cakewalk (older)"},
    {".logicx", "Logic Pro"},
    {".dpdoc", "Digital Performer"},
    {".ardour", "Ardour"},
    {".reason", "Reason"},
    {".alp", "Ableton Live"},
    {".als", "Ableton Live"},
    {".adv", "Ableton Device"},
    {".ejay", "eJay"},
    {".mmp", "LMMS"},
    {".mmpz", "LMMS"},
    {".studioone", "Studio One"},
    {".s1p", "Studio One"},
    {".mas", "Maschine"},
    {".mcproject", "Mixcraft"},
    {".mixbus", "Harrison Mixbus"},
    {".sequoia", "Magix Sequoia"},
    {".samplitude", "Samplitude"},
    {".waveform", "Tracktion Waveform"},
    {".t7", "Tracktion (older)"},
    {".ntrk", "n-Track Studio"},
    {".pod", "Podium by Zynewave"},
    {".rns", "Renoise"},
    {".xrns", "Renoise"},
    {".qtr", "Qtractor"},
    {".band", "GarageBand"},
    {".aup", "Audacity"},
    {".aup3", "Audacity"},
    {".sad", "SADiE"},
    {".ses", "Sound Forge"},
    {".sfk", "Sound Forge"},
    {".frg", "Sound Forge"},
    {".saw", "SAWStudio"},
    {".mpro", "MultitrackStudio"},
    {".mts", "MultitrackStudio"},
    {".omf", "Open Media Framework (used by multiple DAWs)"},
    {".rpp-bak", "Reaper Backup"},
    {".rsn", "Reason Project"},
    {".z3ta", "z3ta+ (used with Cakewalk)"},
    {".acd", "ACID Pro"},
    {".acd-zip", "ACID Pro"},
    {".mxt", "Max/MSP"},
    {".mxb", "Max/MSP"},
    {".ohm", "Ohm Studio"},
    {".rosegarden", "Rosegarden"},
    {".mmpj", "MuLab"},
    {".mut", "MuLab"},
    {".trproj", "Traversa"},
    {".sseq", "SunVox"},
    {".sunvox", "SunVox"},
    {".caustic", "Caustic"},
    {".mio", "Marienberg Modular"},
    {".aif", "Audio Interchange File (used by multiple DAWs)"},
    {".vpr", "Vegas Pro"},
    {".vf", "Vegas Pro"},
    {".dar", "DART"},
    {".zyz", "ZynAddSubFX"},
    {".trr", "T-RackS"},
    {".sxt", "Soundation"},
    {".pxp", "Pyramix"},
    {".ppr", "Personal Composer"},
    {".pcs", "Personal Composer System"},
    {".mid", "MIDI (used by multiple DAWs)"},
    {".midi", "MIDI (used by multiple DAWs)"}
};

        // ID пунктов контекстного меню
        private enum ContextMenuIds
        {
            Open = 1001,
            ZipFile,
            OpenFolder,
            ZipFolder
        }

        public Form1()
        {
            InitializeComponent();
            InitializeContextMenu();
            this.Resize += Form1_Resize;
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Открыть", null, ContextMenu_Open_Click).Name = ContextMenuIds.Open.ToString();
            contextMenu.Items.Add("Создать ZIP (файл)", null, ContextMenu_ZipFile_Click).Name = ContextMenuIds.ZipFile.ToString();
            contextMenu.Items.Add("Открыть папку проекта", null, ContextMenu_OpenFolder_Click).Name = ContextMenuIds.OpenFolder.ToString();
            contextMenu.Items.Add("Создать ZIP папки проекта", null, ContextMenu_ZipFolder_Click).Name = ContextMenuIds.ZipFolder.ToString();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Диалог выбора папки
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Выберите папку, где искать проекты";
                fbd.ShowNewFolderButton = false;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    chosenFolder = fbd.SelectedPath;
                    // Сбор проектов
                    await Task.Run(() => CollectProjectFiles(chosenFolder));
                    // Сортировка
                    SortProjectFiles();
                    // Заполнение ListView
                    PopulateListView();
                    // Настройка колонок под размер
                    AdjustListViewColumns();
                }
                else
                {
                    // Если пользователь отменил выбор папки, закрываем приложение
                    this.Close();
                }
            }
        }

        private void CollectProjectFiles(string folder)
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
                {
                    if (IsProjectFile(file))
                    {
                        projectFiles.Add(file);
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки доступа
            }
        }

        private bool IsProjectFile(string path)
        {
            string ext = Path.GetExtension(path);
            return extToDaw.ContainsKey(ext);
        }

        private void SortProjectFiles()
        {
            projectFiles = projectFiles.OrderByDescending(file =>
            {
                try
                {
                    return File.GetLastWriteTime(file);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }).ToList();
        }

        private void PopulateListView()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(PopulateListView));
                return;
            }

            listViewProjects.BeginUpdate();
            listViewProjects.Clear();

            // Добавляем столбцы с начальными ширинами
            listViewProjects.Columns.Add("Проекты (DAW)", 400, HorizontalAlignment.Left);
            listViewProjects.Columns.Add("Размер папки", 150, HorizontalAlignment.Right); // Фиксированная ширина
            listViewProjects.Columns.Add("Дата последнего изменения", 200, HorizontalAlignment.Left); // Фиксированная ширина

            // Инициализация ImageList с системными иконками
            var smallImageList = new ImageList();
            smallImageList.ImageSize = SystemInformation.SmallIconSize;
            listViewProjects.SmallImageList = smallImageList;

            // Добавление элементов в ListView
            foreach (var filePath in projectFiles)
            {
                string fileName = Path.GetFileName(filePath);
                string ext = Path.GetExtension(filePath);
                string dawName = GetDawName(ext);

                string displayName = !string.IsNullOrEmpty(dawName) ? $"({dawName}) {fileName}" : fileName;

                // Получение системной иконки
                Icon icon = Icon.ExtractAssociatedIcon(filePath);
                if (icon != null)
                {
                    // Используем путь файла в качестве ключа для избежания дублирования
                    if (!smallImageList.Images.ContainsKey(filePath))
                    {
                        smallImageList.Images.Add(filePath, icon);
                    }
                }
                else
                {
                    if (!smallImageList.Images.ContainsKey(filePath))
                    {
                        smallImageList.Images.Add(filePath, SystemIcons.WinLogo);
                    }
                }

                // Получение размера папки
                string folderPath = Path.GetDirectoryName(filePath);
                ulong folderSize = GetFolderSize(folderPath);
                string formattedSize = FormatSize(folderSize);

                // Получение даты последнего изменения
                string formattedDate = "Неизвестно";
                try
                {
                    DateTime lastWrite = File.GetLastWriteTime(filePath);
                    formattedDate = lastWrite.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch
                {
                    // Оставляем "Неизвестно"
                }

                var lvi = new ListViewItem(displayName)
                {
                    ImageKey = filePath,
                    Tag = filePath
                };
                lvi.SubItems.Add(formattedSize);
                lvi.SubItems.Add(formattedDate);

                listViewProjects.Items.Add(lvi);
            }

            // Настройка ширины столбцов
            AdjustListViewColumns();

            listViewProjects.EndUpdate();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            AdjustListViewColumns();
        }

        private void AdjustListViewColumns()
        {
            if (listViewProjects.Columns.Count < 3)
                return;

            int listViewWidth = listViewProjects.ClientSize.Width;

            // Фиксированные ширины для столбцов "Размер папки" и "Дата изменения"
            int fixedWidthSize = 150;
            int fixedWidthDate = 200;

            // Проверка, что общий фиксированный размер не превышает ширину ListView
            if (fixedWidthSize + fixedWidthDate > listViewWidth)
            {
                // Если превышает, уменьшаем фиксированные ширины пропорционально
                double ratio = (double)listViewWidth / (fixedWidthSize + fixedWidthDate);
                fixedWidthSize = (int)(fixedWidthSize * ratio);
                fixedWidthDate = (int)(fixedWidthDate * ratio);
            }

            // Установка фиксированных ширин
            listViewProjects.Columns[1].Width = fixedWidthSize;
            listViewProjects.Columns[2].Width = fixedWidthDate;

            // Установка ширины первого столбца на оставшееся место
            int col0Width = listViewWidth - fixedWidthSize - fixedWidthDate;
            if (col0Width < 100) // Минимальная ширина для первого столбца
            {
                col0Width = 100;
            }
            listViewProjects.Columns[0].Width = col0Width;
        }

        private string GetDawName(string ext)
        {
            if (extToDaw.TryGetValue(ext, out string dawName))
            {
                return dawName;
            }
            return string.Empty;
        }

        private ulong GetFolderSize(string folderPath)
        {
            ulong size = 0;
            try
            {
                foreach (var file in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        size += (ulong)fi.Length;
                    }
                    catch
                    {
                        // Игнорируем ошибки доступа
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки доступа
            }
            return size;
        }

        private string FormatSize(ulong size)
        {
            double dblSize = size;
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            while (dblSize >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                dblSize /= 1024;
                suffixIndex++;
            }
            return $"{dblSize:0.##} {suffixes[suffixIndex]}";
        }

        private void listViewProjects_DoubleClick(object sender, EventArgs e)
        {
            if (listViewProjects.SelectedItems.Count > 0)
            {
                var selectedItem = listViewProjects.SelectedItems[0];
                string filePath = selectedItem.Tag as string;
                OpenProject(filePath);
            }
        }

        private void listViewProjects_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var focusedItem = listViewProjects.FocusedItem;
                if (focusedItem != null && focusedItem.Bounds.Contains(e.Location))
                {
                    fileIndexOnContextClick = projectFiles.IndexOf(focusedItem.Tag as string);
                    contextMenu.Show(Cursor.Position);
                }
                else
                {
                    // Клик по пустой области
                    fileIndexOnContextClick = -1;
                    contextMenu.Show(Cursor.Position);
                }
            }
        }

        private void listViewProjects_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ListViewHitTestInfo info = listViewProjects.HitTest(e.X, e.Y);
                if (info.Item != null)
                {
                    // Устанавливаем выделение на весь элемент
                    info.Item.Selected = true;
                }
            }
        }

        private void ContextMenu_Open_Click(object sender, EventArgs e)
        {
            if (fileIndexOnContextClick >= 0 && fileIndexOnContextClick < projectFiles.Count)
            {
                OpenProject(projectFiles[fileIndexOnContextClick]);
            }
        }

        private void ContextMenu_ZipFile_Click(object sender, EventArgs e)
        {
            if (fileIndexOnContextClick >= 0 && fileIndexOnContextClick < projectFiles.Count)
            {
                CreateZipArchiveFile(fileIndexOnContextClick);
            }
        }

        private void ContextMenu_OpenFolder_Click(object sender, EventArgs e)
        {
            if (fileIndexOnContextClick >= 0 && fileIndexOnContextClick < projectFiles.Count)
            {
                OpenProjectFolder(projectFiles[fileIndexOnContextClick]);
            }
        }

        private void ContextMenu_ZipFolder_Click(object sender, EventArgs e)
        {
            if (fileIndexOnContextClick >= 0 && fileIndexOnContextClick < projectFiles.Count)
            {
                CreateZipArchiveProjectFolder(fileIndexOnContextClick);
            }
        }

        private void OpenProject(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть проект.\nОшибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenProjectFolder(string filePath)
        {
            try
            {
                string folderPath = Path.GetDirectoryName(filePath);
                Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть папку проекта.\nОшибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void CreateZipArchiveFile(int index)
        {
            if (index < 0 || index >= projectFiles.Count)
                return;

            string filePath = projectFiles[index];
            // Определяем путь к рабочему столу
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Имя файла без расширения
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // Собираем путь к будущему архиву на рабочем столе
            string destZip = Path.Combine(desktopPath, $"{fileNameWithoutExt}.zip");

            if (File.Exists(destZip))
            {
                var result = MessageBox.Show($"ZIP-архив уже существует: {destZip}\nПерезаписать?",
                                             "Подтверждение",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return;
            }

            // Формируем PowerShell-команду
            string command = $"Compress-Archive -Path '{filePath}' -DestinationPath '{destZip}' -CompressionLevel Optimal -Force";
            var (success, output) = await RunPowerShellCommandAsync(command);

            if (success)
            {
                MessageBox.Show($"ZIP-архив успешно создан на рабочем столе.\n\nВывод PowerShell:\n{output}",
                                "Успех",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Ошибка при создании ZIP-архива.\n\nВывод PowerShell:\n{output}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private async void CreateZipArchiveProjectFolder(int index)
        {
            if (index < 0 || index >= projectFiles.Count)
                return;

            string filePath = projectFiles[index];
            string projectFolder = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(projectFolder))
            {
                MessageBox.Show($"Папка проекта не найдена: {projectFolder}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            // Определяем путь к рабочему столу
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Имя папки для архива
            string folderName = new DirectoryInfo(projectFolder).Name;
            // Путь архива на рабочем столе
            string destZip = Path.Combine(desktopPath, $"{folderName}.zip");

            if (File.Exists(destZip))
            {
                var result = MessageBox.Show($"ZIP-архив уже существует: {destZip}\nПерезаписать?",
                                             "Подтверждение",
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                    return;
            }

            // Формируем PowerShell-команду
            string command = $"Compress-Archive -Path '{projectFolder}\\*' -DestinationPath '{destZip}' -CompressionLevel Optimal -Force";
            var (success, output) = await RunPowerShellCommandAsync(command);

            if (success)
            {
                MessageBox.Show($"ZIP-архив папки проекта успешно создан на рабочем столе.\n\nВывод PowerShell:\n{output}",
                                "Успех",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Ошибка при создании ZIP-архива папки проекта.\n\nВывод PowerShell:\n{output}",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }


        private async Task<(bool Success, string Output)> RunPowerShellCommandAsync(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -Command \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    // Ждем завершения процесса
                    await Task.Run(() => process.WaitForExit());

                    bool success = process.ExitCode == 0;
                    string combinedOutput = string.IsNullOrEmpty(output) ? error : output + Environment.NewLine + error;
                    return (success, combinedOutput);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
