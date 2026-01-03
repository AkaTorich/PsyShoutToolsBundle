using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace ProtectionPacker
{
    /// <summary>
    /// Главная форма графического интерфейса упаковщика
    /// </summary>
    public partial class MainForm : Form
    {
        private ProtectionOptions _currentOptions;

        public MainForm()
        {
            InitializeComponent();
            InitializeDefaultOptions();
            UpdateStatusLabel("Готов к работе");
        }

        /// <summary>
        /// Инициализация параметров защиты по умолчанию
        /// </summary>
        private void InitializeDefaultOptions()
        {
            _currentOptions = new ProtectionOptions
            {
                EnableCompression = true,
                EnableEncryption = true,
                EnableAntiDebug = true,
                EnableObfuscation = true,
                EnableStringEncryption = true,
                EnableResourceProtection = true,
                EnableVirtualization = true,
                EnableFakeAPI = true,
                EnableDebugOutput = false,
                EnablePackerDebug = false,
                ApplicationType = ApplicationType.WindowsApp,
                OutputFileType = OutputFileType.Executable,
                AntiDumpLevel = AntiDumpLevel.Maximum,
                CompressionLevel = CompressionLevel.Maximum
            };

            // Синхронизируем с UI
            SyncOptionsToUI();
        }

        /// <summary>
        /// Синхронизация опций с элементами UI
        /// </summary>
        private void SyncOptionsToUI()
        {
            chkCompression.Checked = _currentOptions.EnableCompression;
            chkEncryption.Checked = _currentOptions.EnableEncryption;
            chkAntiDebug.Checked = _currentOptions.EnableAntiDebug;
            chkObfuscation.Checked = _currentOptions.EnableObfuscation;
            chkStringEncryption.Checked = _currentOptions.EnableStringEncryption;
            chkResourceProtection.Checked = _currentOptions.EnableResourceProtection;
            chkVirtualization.Checked = _currentOptions.EnableVirtualization;
            chkFakeAPI.Checked = _currentOptions.EnableFakeAPI;
            chkDebugOutput.Checked = _currentOptions.EnableDebugOutput;
            chkPackerDebug.Checked = _currentOptions.EnablePackerDebug;

            // Уровень анти-дампа
            switch (_currentOptions.AntiDumpLevel)
            {
                case AntiDumpLevel.Light:
                    rbAntiDumpLight.Checked = true;
                    break;
                case AntiDumpLevel.Medium:
                    rbAntiDumpMedium.Checked = true;
                    break;
                case AntiDumpLevel.Maximum:
                    rbAntiDumpMaximum.Checked = true;
                    break;
                default:
                    rbAntiDumpNone.Checked = true;
                    break;
            }

            // Уровень сжатия
            switch (_currentOptions.CompressionLevel)
            {
                case CompressionLevel.Fast:
                    rbCompressionFast.Checked = true;
                    break;
                case CompressionLevel.Optimal:
                    rbCompressionOptimal.Checked = true;
                    break;
                case CompressionLevel.Maximum:
                    rbCompressionMaximum.Checked = true;
                    break;
                default:
                    rbCompressionNone.Checked = true;
                    break;
            }

            // Тип приложения
            if (_currentOptions.ApplicationType == ApplicationType.ConsoleApp)
                rbConsoleApp.Checked = true;
            else
                rbWindowsApp.Checked = true;

            // Тип выходного файла
            if (_currentOptions.OutputFileType == OutputFileType.Library)
                rbOutputDll.Checked = true;
            else
                rbOutputExe.Checked = true;
        }

        /// <summary>
        /// Синхронизация UI с опциями
        /// </summary>
        private void SyncUIToOptions()
        {
            _currentOptions.EnableCompression = chkCompression.Checked;
            _currentOptions.EnableEncryption = chkEncryption.Checked;
            _currentOptions.EnableAntiDebug = chkAntiDebug.Checked;
            _currentOptions.EnableObfuscation = chkObfuscation.Checked;
            _currentOptions.EnableStringEncryption = chkStringEncryption.Checked;
            _currentOptions.EnableResourceProtection = chkResourceProtection.Checked;
            _currentOptions.EnableVirtualization = chkVirtualization.Checked;
            _currentOptions.EnableFakeAPI = chkFakeAPI.Checked;
            _currentOptions.EnableDebugOutput = chkDebugOutput.Checked;
            _currentOptions.EnablePackerDebug = chkPackerDebug.Checked;

            // Уровень анти-дампа
            if (rbAntiDumpNone.Checked)
                _currentOptions.AntiDumpLevel = AntiDumpLevel.None;
            else if (rbAntiDumpLight.Checked)
                _currentOptions.AntiDumpLevel = AntiDumpLevel.Light;
            else if (rbAntiDumpMedium.Checked)
                _currentOptions.AntiDumpLevel = AntiDumpLevel.Medium;
            else if (rbAntiDumpMaximum.Checked)
                _currentOptions.AntiDumpLevel = AntiDumpLevel.Maximum;

            // Уровень сжатия
            if (rbCompressionNone.Checked)
                _currentOptions.CompressionLevel = CompressionLevel.None;
            else if (rbCompressionFast.Checked)
                _currentOptions.CompressionLevel = CompressionLevel.Fast;
            else if (rbCompressionOptimal.Checked)
                _currentOptions.CompressionLevel = CompressionLevel.Optimal;
            else if (rbCompressionMaximum.Checked)
                _currentOptions.CompressionLevel = CompressionLevel.Maximum;

            // Тип приложения
            _currentOptions.ApplicationType = rbConsoleApp.Checked ?
                ApplicationType.ConsoleApp : ApplicationType.WindowsApp;

            // Тип выходного файла
            _currentOptions.OutputFileType = rbOutputDll.Checked ?
                OutputFileType.Library : OutputFileType.Executable;
        }

        /// <summary>
        /// Обработчик кнопки выбора входного файла
        /// </summary>
        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Исполняемые файлы (*.exe)|*.exe|Библиотеки (*.dll)|*.dll|Все файлы (*.*)|*.*";
                openFileDialog.Title = "Выберите файл для упаковки";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtInputFile.Text = openFileDialog.FileName;
                    
                    // Автоматически предлагаем имя выходного файла
                    if (string.IsNullOrEmpty(txtOutputFile.Text))
                    {
                        string directory = Path.GetDirectoryName(openFileDialog.FileName);
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                        string extension = Path.GetExtension(openFileDialog.FileName);
                        txtOutputFile.Text = Path.Combine(directory, $"{fileNameWithoutExt}_protected{extension}");
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки выбора выходного файла
        /// </summary>
        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Исполняемые файлы (*.exe)|*.exe|Библиотеки (*.dll)|*.dll|Все файлы (*.*)|*.*";
                saveFileDialog.Title = "Выберите место сохранения защищенного файла";

                if (!string.IsNullOrEmpty(txtInputFile.Text))
                {
                    string directory = Path.GetDirectoryName(txtInputFile.Text);
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(txtInputFile.Text);
                    string extension = Path.GetExtension(txtInputFile.Text);
                    saveFileDialog.FileName = $"{fileNameWithoutExt}_protected{extension}";
                    saveFileDialog.InitialDirectory = directory;
                }

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFile.Text = saveFileDialog.FileName;
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки начала упаковки
        /// </summary>
        private async void btnPack_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtInputFile.Text))
            {
                MessageBox.Show("Пожалуйста, выберите входной файл!", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(txtInputFile.Text))
            {
                MessageBox.Show("Входной файл не найден!", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtOutputFile.Text))
            {
                MessageBox.Show("Пожалуйста, укажите выходной файл!", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Синхронизируем опции
            SyncUIToOptions();

            // Блокируем UI во время работы
            SetUIEnabled(false);
            progressBar.Style = ProgressBarStyle.Marquee;
            txtLog.Clear();
            UpdateStatusLabel("Упаковка в процессе...");

            // Сохраняем оригинальный вывод консоли
            var originalOut = Console.Out;

            try
            {
                // Перенаправляем вывод консоли в лог
                var writer = new TextBoxWriter(txtLog);
                Console.SetOut(writer);

                string inputFile = txtInputFile.Text;
                string outputFile = txtOutputFile.Text;

                // Запускаем упаковку в фоновом потоке
                bool success = await Task.Run(() =>
                {
                    try
                    {
                        var packer = new ProtectionPacker(_currentOptions);
                        return packer.PackAndProtect(inputFile, outputFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Критическая ошибка: {ex.Message}");
                        Console.WriteLine($"Детали: {ex.StackTrace}");
                        return false;
                    }
                });

                if (success)
                {
                    UpdateStatusLabel("Упаковка завершена успешно!");
                    MessageBox.Show("Файл успешно упакован и защищен!", "Успех", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    UpdateStatusLabel("Ошибка упаковки!");
                    MessageBox.Show("Произошла ошибка при упаковке файла. Проверьте лог.", "Ошибка", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                UpdateStatusLabel("Критическая ошибка!");
                MessageBox.Show($"Критическая ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine($"Детали: {ex.StackTrace}");
            }
            finally
            {
                // Восстанавливаем оригинальный вывод консоли
                Console.SetOut(originalOut);
                
                // Разблокируем UI
                SetUIEnabled(true);
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
            }
        }

        /// <summary>
        /// Блокировка/разблокировка UI
        /// </summary>
        private void SetUIEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetUIEnabled(enabled)));
                return;
            }

            txtInputFile.Enabled = enabled;
            txtOutputFile.Enabled = enabled;
            btnBrowseInput.Enabled = enabled;
            btnBrowseOutput.Enabled = enabled;
            btnPack.Enabled = enabled;
            grpProtectionOptions.Enabled = enabled;
            grpCompressionLevel.Enabled = enabled;
            grpAntiDumpLevel.Enabled = enabled;
            grpApplicationType.Enabled = enabled;
            grpOutputFileType.Enabled = enabled;
            grpDebugOptions.Enabled = enabled;
        }

        /// <summary>
        /// Обновление статусной строки
        /// </summary>
        private void UpdateStatusLabel(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatusLabel(text)));
                return;
            }

            lblStatus.Text = text;
        }

        /// <summary>
        /// Установка максимальной защиты
        /// </summary>
        private void btnMaximumProtection_Click(object sender, EventArgs e)
        {
            chkCompression.Checked = true;
            chkEncryption.Checked = true;
            chkAntiDebug.Checked = true;
            chkObfuscation.Checked = true;
            chkStringEncryption.Checked = true;
            chkResourceProtection.Checked = true;
            chkVirtualization.Checked = true;
            chkFakeAPI.Checked = true;
            rbAntiDumpMaximum.Checked = true;
            rbCompressionMaximum.Checked = true;
            
            MessageBox.Show("Установлены максимальные параметры защиты.", "Информация", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Установка быстрой защиты
        /// </summary>
        private void btnLightProtection_Click(object sender, EventArgs e)
        {
            chkCompression.Checked = true;
            chkEncryption.Checked = true;
            chkAntiDebug.Checked = false;
            chkObfuscation.Checked = false;
            chkStringEncryption.Checked = false;
            chkResourceProtection.Checked = false;
            chkVirtualization.Checked = false;
            chkFakeAPI.Checked = false;
            rbAntiDumpLight.Checked = true;
            rbCompressionFast.Checked = true;
            
            MessageBox.Show("Установлены параметры быстрой защиты.", "Информация", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Очистка лога
        /// </summary>
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        /// <summary>
        /// Обработчик закрытия формы
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Дополнительная логика при закрытии, если нужно
        }
    }

    /// <summary>
    /// Класс для перенаправления вывода Console в TextBox
    /// </summary>
    public class TextBoxWriter : System.IO.TextWriter
    {
        private TextBox _textBox;

        public TextBoxWriter(TextBox textBox)
        {
            _textBox = textBox;
        }

        public override void Write(char value)
        {
            AppendText(value.ToString());
        }

        public override void Write(string value)
        {
            if (value != null)
                AppendText(value);
        }

        public override void WriteLine(string value)
        {
            AppendText(value + Environment.NewLine);
        }

        public override void WriteLine()
        {
            AppendText(Environment.NewLine);
        }

        private void AppendText(string text)
        {
            if (_textBox == null || _textBox.IsDisposed)
                return;

            if (_textBox.InvokeRequired)
            {
                try
                {
                    _textBox.BeginInvoke(new Action(() => AppendText(text)));
                }
                catch { }
            }
            else
            {
                _textBox.AppendText(text);
                // Прокрутка к концу
                _textBox.SelectionStart = _textBox.TextLength;
                _textBox.ScrollToCaret();
            }
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}

