using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WavetableGenerator
{
    /// <summary>
    /// Форма для создания набора волновых таблиц
    /// УЛУЧШЕННАЯ ВЕРСИЯ с поддержкой Brightness и Warmth
    /// </summary>
    public partial class WavetableSetForm : Form
    {
        // Свойства для получения параметров
        public int Count { get; private set; }
        public int TableSize { get; private set; }
        public int FrameCount { get; private set; }
        public int Harmonics { get; private set; }
        public WaveType[] SelectedPresets { get; private set; }
        public double Brightness { get; private set; }
        public double Warmth { get; private set; }
        
        // Элементы управления
        private NumericUpDown _countNumeric;
        private NumericUpDown _frameCountNumeric;
        private NumericUpDown _harmonicsNumeric;
        private ComboBox _tableSizeComboBox;
        private CheckBox _varyFrequencyCheckBox;
        private CheckBox _varyHarmonicsCheckBox;
        private CheckBox _randomizeCheckBox;
        private GroupBox _optionsGroupBox;
        private TrackBar _brightnessTrackBar;
        private TrackBar _warmthTrackBar;
        private Label _brightnessLabel;
        private Label _warmthLabel;
        private Button _okButton;
        private Button _cancelButton;
        private Label _infoLabel;
        
        public WavetableSetForm()
        {
            InitializeComponent();
            InitializeControls();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Настройки формы
            this.Text = "Создание набора FM-таблиц";
            this.Size = new Size(500, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            this.ResumeLayout(false);
        }
        
        private void InitializeControls()
        {
            int y = 20;
            int spacing = 35;
            int labelWidth = 150;
            int controlWidth = 150;
            
            // Количество таблиц
            var countLabel = new Label
            {
                Text = "Количество таблиц:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            
            _countNumeric = new NumericUpDown
            {
                Location = new Point(180, y),
                Size = new Size(controlWidth, 20),
                Minimum = 1,
                Maximum = 256,
                Value = 16,
                Increment = 1
            };
            _countNumeric.ValueChanged += (s, e) => UpdateInfo();
            
            y += spacing;
            
            // Размер таблицы
            var sizeLabel = new Label
            {
                Text = "Размер таблицы:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            
            _tableSizeComboBox = new ComboBox
            {
                Location = new Point(180, y),
                Size = new Size(controlWidth, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _tableSizeComboBox.Items.AddRange(new object[] { "512", "1024", "2048", "4096" });
            _tableSizeComboBox.SelectedIndex = 2; // 2048 по умолчанию
            _tableSizeComboBox.SelectedIndexChanged += (s, e) => UpdateInfo();
            
            y += spacing;
            
            // Количество фреймов
            var frameCountLabel = new Label
            {
                Text = "Количество фреймов:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            
            _frameCountNumeric = new NumericUpDown
            {
                Location = new Point(180, y),
                Size = new Size(controlWidth, 20),
                Minimum = 1,
                Maximum = 512,
                Value = 256,
                Increment = 1
            };
            _frameCountNumeric.ValueChanged += (s, e) => UpdateInfo();
            
            y += spacing;
            
            // Количество гармоник
            var harmonicsLabel = new Label
            {
                Text = "Количество гармоник:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            
            _harmonicsNumeric = new NumericUpDown
            {
                Location = new Point(180, y),
                Size = new Size(controlWidth, 20),
                Minimum = 1,
                Maximum = 64,
                Value = 16,
                Increment = 1
            };
            
            y += spacing + 10;
            
            // Яркость (Brightness)
            _brightnessLabel = new Label
            {
                Text = "Яркость: 50%",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            _brightnessTrackBar = new TrackBar
            {
                Location = new Point(180, y - 5),
                Size = new Size(controlWidth, 45),
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                TickFrequency = 10
            };
            _brightnessTrackBar.ValueChanged += (s, e) => 
            {
                _brightnessLabel.Text = $"Яркость: {_brightnessTrackBar.Value}%";
            };
            
            y += 45;
            
            // Теплота (Warmth)
            _warmthLabel = new Label
            {
                Text = "Теплота: 50%",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            _warmthTrackBar = new TrackBar
            {
                Location = new Point(180, y - 5),
                Size = new Size(controlWidth, 45),
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                TickFrequency = 10
            };
            _warmthTrackBar.ValueChanged += (s, e) =>
            {
                _warmthLabel.Text = $"Теплота: {_warmthTrackBar.Value}%";
            };
            
            y += 50;
            
            // Группа опций
            _optionsGroupBox = new GroupBox
            {
                Text = "Параметры генерации",
                Location = new Point(20, y),
                Size = new Size(450, 100)
            };
            
            _varyFrequencyCheckBox = new CheckBox
            {
                Text = "Вариации частоты",
                Location = new Point(15, 25),
                Size = new Size(150, 20),
                Checked = true
            };
            
            _varyHarmonicsCheckBox = new CheckBox
            {
                Text = "Вариации гармоник",
                Location = new Point(15, 50),
                Size = new Size(150, 20),
                Checked = true
            };
            
            _randomizeCheckBox = new CheckBox
            {
                Text = "Случайные параметры",
                Location = new Point(200, 25),
                Size = new Size(150, 20),
                Checked = false
            };
            
            var uniqueEachTimeLabel = new Label
            {
                Text = "✓ Каждый раз уникальные",
                Location = new Point(200, 50),
                Size = new Size(220, 20),
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            
            _optionsGroupBox.Controls.Add(_varyFrequencyCheckBox);
            _optionsGroupBox.Controls.Add(_varyHarmonicsCheckBox);
            _optionsGroupBox.Controls.Add(_randomizeCheckBox);
            _optionsGroupBox.Controls.Add(uniqueEachTimeLabel);
            
            y += 110;
            
            // Информационная метка
            _infoLabel = new Label
            {
                Location = new Point(20, y),
                Size = new Size(450, 40),
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };
            UpdateInfo();
            
            y += 50;
            
            // Кнопки
            _okButton = new Button
            {
                Text = "Создать",
                Location = new Point(300, y),
                Size = new Size(85, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            
            _cancelButton = new Button
            {
                Text = "Отмена",
                Location = new Point(395, y),
                Size = new Size(85, 30),
                DialogResult = DialogResult.Cancel
            };
            
            // Добавляем элементы на форму
            this.Controls.AddRange(new Control[] {
                countLabel, _countNumeric,
                sizeLabel, _tableSizeComboBox,
                frameCountLabel, _frameCountNumeric,
                harmonicsLabel, _harmonicsNumeric,
                _brightnessLabel, _brightnessTrackBar,
                _warmthLabel, _warmthTrackBar,
                _optionsGroupBox,
                _infoLabel,
                _okButton, _cancelButton
            });
        }
        
        private void UpdateInfo()
        {
            if (_countNumeric != null && _tableSizeComboBox != null && _frameCountNumeric != null && _infoLabel != null)
            {
                int count = (int)_countNumeric.Value;
                int tableSize = int.Parse(_tableSizeComboBox.Text ?? "2048");
                int frameCount = (int)_frameCountNumeric.Value;
                int totalSamples = count * tableSize * frameCount;
                double sizeMB = totalSamples * 4.0 / (1024 * 1024); // 4 байта на float
                
                _infoLabel.Text = $"Будет создано {count} волновых таблиц\n" +
                                 $"Примерный размер в памяти: {sizeMB:F1} МБ";
            }
        }
        
        private void OkButton_Click(object? sender, EventArgs e)
        {
            Count = (int)_countNumeric.Value;
            TableSize = int.Parse(_tableSizeComboBox.Text);
            FrameCount = (int)_frameCountNumeric.Value;
            Harmonics = (int)_harmonicsNumeric.Value;
            SelectedPresets = new[] { WaveType.FM };
            Brightness = _brightnessTrackBar.Value / 100.0;
            Warmth = _warmthTrackBar.Value / 100.0;
            
            // Проверка на слишком большой объём
            int totalSamples = Count * TableSize * FrameCount;
            double sizeMB = totalSamples * 4.0 / (1024 * 1024);
            
            if (sizeMB > 100)
            {
                var result = MessageBox.Show(
                    $"Генерация займёт примерно {sizeMB:F1} МБ памяти.\n" +
                    "Это может занять некоторое время. Продолжить?",
                    "Предупреждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                    
                if (result != DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.None;
                }
            }
        }
    }
}