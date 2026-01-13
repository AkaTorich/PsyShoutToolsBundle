using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WavetableGenerator
{
    /// <summary>
    /// Форма для создания и редактирования волновых таблиц
    /// УЛУЧШЕННАЯ ВЕРСИЯ с поддержкой Brightness и Warmth
    /// </summary>
    public partial class WavetableForm : Form
    {
        private readonly WavetableGenerator _generator = new WavetableGenerator();
        private Wavetable? _originalWavetable;
        
        // Элементы управления
        private ComboBox _tableSizeComboBox;
        private NumericUpDown _frameCountNumeric;
        private ComboBox _waveTypeComboBox;
        private NumericUpDown _harmonicsNumeric;
        private TrackBar _brightnessTrackBar;
        private TrackBar _warmthTrackBar;
        private Label _brightnessLabel;
        private Label _warmthLabel;
        private Panel _previewPanel;
        private Panel _spectrogramPanel;
        private Button _generateButton;
        private Button _okButton;
        private Button _cancelButton;
        private ProgressBar _progressBar;
        private Bitmap? _spectrogramImage;
        
        public Wavetable? Wavetable { get; private set; }
        
        public WavetableForm(Wavetable? existingWavetable = null)
        {
            _originalWavetable = existingWavetable;
            InitializeComponent();
            InitializeControls();
            LoadWavetableData();
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Настройки формы
            this.Text = _originalWavetable == null ? "Новая волновая таблица" : "Редактирование волновой таблицы";
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            
            this.ResumeLayout(false);
        }
        
        private void InitializeControls()
        {
            int y = 20;
            int spacing = 35;
            int labelWidth = 140;
            int controlWidth = 200;
            
            // Размер таблицы
            var tableSizeLabel = new Label
            {
                Text = "Размер таблицы:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            _tableSizeComboBox = new ComboBox
            {
                Location = new Point(170, y),
                Size = new Size(controlWidth, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _tableSizeComboBox.Items.AddRange(new object[] { "512", "1024", "2048", "4096" });
            _tableSizeComboBox.SelectedIndex = 2; // 2048 по умолчанию
            
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
                Location = new Point(170, y),
                Size = new Size(controlWidth, 20),
                Minimum = 1,
                Maximum = 256,
                Value = 256,
                Increment = 16
            };
            
            y += spacing;
            
            // Тип волны
            var waveTypeLabel = new Label
            {
                Text = "Тип волны:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            _waveTypeComboBox = new ComboBox
            {
                Location = new Point(170, y),
                Size = new Size(controlWidth, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _waveTypeComboBox.Items.Add("FM");
            _waveTypeComboBox.SelectedIndex = 0;
            _waveTypeComboBox.SelectedIndexChanged += WaveTypeComboBox_SelectedIndexChanged;
            
            y += spacing;
            
            // Количество гармоник
            var harmonicsLabel = new Label
            {
                Text = "Гармоники:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            _harmonicsNumeric = new NumericUpDown
            {
                Location = new Point(170, y),
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
                Location = new Point(170, y - 5),
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
                Location = new Point(170, y - 5),
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
            
            // Панель предпросмотра
            var previewLabel = new Label
            {
                Text = "Предпросмотр:",
                Location = new Point(20, y),
                Size = new Size(labelWidth, 20)
            };
            
            y += 25;
            
            _previewPanel = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(440, 120),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.Fixed3D
            };
            _previewPanel.Paint += PreviewPanel_Paint;
            
            // Спектрограмма
            var spectrogramLabel = new Label
            {
                Text = "Спектрограмма:",
                Location = new Point(490, 20),
                Size = new Size(150, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            
            _spectrogramPanel = new Panel
            {
                Location = new Point(490, 45),
                Size = new Size(380, 280),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.Fixed3D
            };
            _spectrogramPanel.Paint += SpectrogramPanel_Paint;
            
            y += 130;
            
            // Прогресс-бар
            _progressBar = new ProgressBar
            {
                Location = new Point(20, y),
                Size = new Size(440, 20),
                Visible = false
            };
            
            y += 30;
            
            // Кнопка генерации
            _generateButton = new Button
            {
                Text = "Генерировать",
                Location = new Point(20, y),
                Size = new Size(120, 30)
            };
            _generateButton.Click += GenerateButton_Click;
            
            // Кнопки OK и Отмена
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(260, y),
                Size = new Size(90, 30),
                DialogResult = DialogResult.OK,
                Enabled = false
            };
            
            _cancelButton = new Button
            {
                Text = "Отмена",
                Location = new Point(370, y),
                Size = new Size(90, 30),
                DialogResult = DialogResult.Cancel
            };
            
            // Добавляем все элементы на форму
            this.Controls.AddRange(new Control[] {
                tableSizeLabel, _tableSizeComboBox,
                frameCountLabel, _frameCountNumeric,
                waveTypeLabel, _waveTypeComboBox,
                harmonicsLabel, _harmonicsNumeric,
                _brightnessLabel, _brightnessTrackBar,
                _warmthLabel, _warmthTrackBar,
                previewLabel, _previewPanel,
                spectrogramLabel, _spectrogramPanel,
                _progressBar,
                _generateButton, _okButton, _cancelButton
            });
        }
        
        private void LoadWavetableData()
        {
            if (_originalWavetable != null)
            {
                // Загружаем данные из существующей волновой таблицы
                _tableSizeComboBox.Text = _originalWavetable.TableSize.ToString();
                _frameCountNumeric.Value = _originalWavetable.FrameCount;
                _waveTypeComboBox.SelectedIndex = 0;
                _harmonicsNumeric.Value = _originalWavetable.Harmonics;
                _brightnessTrackBar.Value = (int)(_originalWavetable.Brightness * 100);
                _warmthTrackBar.Value = (int)(_originalWavetable.Warmth * 100);
                
                Wavetable = _originalWavetable;
                _okButton.Enabled = true;
                _previewPanel.Invalidate();
            }
        }
        
        private void WaveTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _harmonicsNumeric.Enabled = true;
        }
        
        private async void GenerateButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Блокируем элементы управления
                SetControlsEnabled(false);
                _progressBar.Visible = true;
                _progressBar.Style = ProgressBarStyle.Marquee;
                
                // Получаем параметры
                int tableSize = int.Parse(_tableSizeComboBox.Text);
                int frameCount = (int)_frameCountNumeric.Value;
                var waveType = WaveType.FM;
                int harmonics = (int)_harmonicsNumeric.Value;
                double brightness = _brightnessTrackBar.Value / 100.0;
                double warmth = _warmthTrackBar.Value / 100.0;
                
                string name = _originalWavetable?.Name ?? $"WT_{waveType}_{DateTime.Now:HHmmss}";
                
                // Генерируем волновую таблицу асинхронно с новыми параметрами
                Wavetable = await Task.Run(() =>
                {
                    return _generator.GenerateWavetable(
                        name, tableSize, waveType, 
                        440.0, 1.0, harmonics, frameCount,
                        brightness, warmth);
                });
                
                // Обновляем предпросмотр
                _previewPanel.Invalidate();
                
                // Генерируем спектрограмму
                GenerateSpectrogram();
                _spectrogramPanel.Invalidate();
                
                _okButton.Enabled = true;
                
                MessageBox.Show("Волновая таблица успешно сгенерирована!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Разблокируем элементы управления
                SetControlsEnabled(true);
                _progressBar.Visible = false;
            }
        }
        
        private void SetControlsEnabled(bool enabled)
        {
            _tableSizeComboBox.Enabled = enabled;
            _frameCountNumeric.Enabled = enabled;
            _waveTypeComboBox.Enabled = enabled;
            _harmonicsNumeric.Enabled = enabled;
            _brightnessTrackBar.Enabled = enabled;
            _warmthTrackBar.Enabled = enabled;
            _generateButton.Enabled = enabled;
        }
        
        private void PreviewPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            
            if (Wavetable == null || Wavetable.Samples.Length == 0)
            {
                // Рисуем заглушку
                using (var font = new Font("Segoe UI", 9))
                using (var brush = new SolidBrush(Color.Gray))
                {
                    var text = "Нажмите 'Генерировать' для создания";
                    var textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, brush,
                        (_previewPanel.Width - textSize.Width) / 2,
                        (_previewPanel.Height - textSize.Height) / 2);
                }
                return;
            }
            
            // Рисуем первый фрейм волновой таблицы
            int width = _previewPanel.Width;
            int height = _previewPanel.Height;
            int centerY = height / 2;
            
            using (var pen = new Pen(Color.Lime, 1))
            {
                int samplesToShow = Math.Min(Wavetable.TableSize, Wavetable.Samples.Length);
                
                for (int x = 1; x < width; x++)
                {
                    double pos1 = (double)(x - 1) / width;
                    double pos2 = (double)x / width;
                    
                    int index1 = (int)(pos1 * samplesToShow);
                    int index2 = (int)(pos2 * samplesToShow);
                    
                    if (index1 >= Wavetable.Samples.Length || index2 >= Wavetable.Samples.Length)
                        break;
                        
                    float sample1 = Wavetable.Samples[index1];
                    float sample2 = Wavetable.Samples[index2];
                    
                    int y1 = centerY - (int)(sample1 * centerY * 0.8f);
                    int y2 = centerY - (int)(sample2 * centerY * 0.8f);
                    
                    g.DrawLine(pen, x - 1, y1, x, y2);
                }
            }
            
            // Рисуем центральную линию
            using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1))
            {
                g.DrawLine(pen, 0, centerY, width, centerY);
            }
        }
        
        private void SpectrogramPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_spectrogramImage == null)
            {
                using (var font = new Font("Segoe UI", 9))
                using (var brush = new SolidBrush(Color.Gray))
                {
                    string text = "Спектрограмма появится после генерации";
                    var textSize = e.Graphics.MeasureString(text, font);
                    e.Graphics.DrawString(text, font, brush,
                        (_spectrogramPanel.Width - textSize.Width) / 2,
                        (_spectrogramPanel.Height - textSize.Height) / 2);
                }
                return;
            }
            
            e.Graphics.DrawImage(_spectrogramImage, 0, 0, _spectrogramPanel.Width, _spectrogramPanel.Height);
        }
        
        private void GenerateSpectrogram()
        {
            if (Wavetable == null) return;
            
            int width = _spectrogramPanel.Width;
            int height = _spectrogramPanel.Height;
            int frameCount = Wavetable.FrameCount;
            int tableSize = Wavetable.TableSize;
            
            _spectrogramImage = new Bitmap(width, height);
            
            using (var g = Graphics.FromImage(_spectrogramImage))
            {
                g.Clear(Color.Black);
                
                // Анализируем каждый фрейм
                for (int frame = 0; frame < frameCount; frame++)
                {
                    int offset = frame * tableSize;
                    
                    // Рисуем спектр как вертикальную полосу
                    int x = (int)((double)frame / frameCount * width);
                    int columnWidth = Math.Max(1, width / frameCount);
                    
                    // Анализируем гармоники (простой метод без FFT)
                    int numHarmonics = 64;
                    for (int harmonic = 0; harmonic < numHarmonics; harmonic++)
                    {
                        double magnitude = 0;
                        
                        // Вычисляем амплитуду гармоники
                        for (int i = 0; i < tableSize; i++)
                        {
                            double phase = (double)i / tableSize * Math.PI * 2 * (harmonic + 1);
                            magnitude += Math.Abs(Wavetable.Samples[offset + i] * Math.Sin(phase));
                        }
                        
                        magnitude /= tableSize;
                        magnitude = Math.Log10(1 + magnitude * 100) * 70;
                        magnitude = Math.Min(255, magnitude);
                        
                        // Y координата (низкие частоты внизу)
                        int y = height - (int)((double)harmonic / numHarmonics * height);
                        
                        // Цвет спектра
                        Color color;
                        if (magnitude < 50)
                            color = Color.FromArgb((int)(magnitude * 1.5), 0, (int)(magnitude * 2.5));
                        else if (magnitude < 100)
                            color = Color.FromArgb(75, (int)(magnitude - 50) * 2, 125 - (int)(magnitude - 50));
                        else if (magnitude < 150)
                            color = Color.FromArgb((int)(magnitude - 25), (int)(magnitude - 25), 0);
                        else
                            color = Color.FromArgb(255, (int)(255 - (magnitude - 150) * 2), 0);
                        
                        using (var brush = new SolidBrush(color))
                        {
                            g.FillRectangle(brush, x, y, columnWidth, Math.Max(1, height / numHarmonics));
                        }
                    }
                }
            }
        }
    }
}