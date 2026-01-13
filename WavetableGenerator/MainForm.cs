using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace WavetableGenerator
{
    /// <summary>
    /// Главная форма приложения с оптимизированным интерфейсом
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly WavetableGenerator _generator = new WavetableGenerator();
        private List<Wavetable> _wavetables = new List<Wavetable>();
        private Wavetable? _selectedWavetable;
        private WaveOutEvent? _waveOut;
        private WavetableWaveProvider? _waveProvider;
        private System.Windows.Forms.Timer? _playbackTimer;
        
        // UI элементы
        private Panel _wavePanel;
        private ListBox _wavetableListBox;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripProgressBar _progressBar;
        private MenuStrip _menuStrip;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private TrackBar _frameSlider;
        private Label _frameLabel;
        private Button _playButton;
        private Button _stopButton;
        private GroupBox _infoGroupBox;
        private Label _infoLabel;
        
        // Настройки визуализации
        private bool _antialiasing = true;
        private Color _waveColor = Color.Lime;
        private Color _gridColor = Color.FromArgb(40, 40, 40);
        
        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            UpdateWavetableList();
            
            // Добавляем обработчик закрытия формы
            this.FormClosing += MainForm_FormClosing;
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Настройки формы
            this.Text = "Wavetable Generator Pro v2.0";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.Icon = SystemIcons.Application;
            this.DoubleBuffered = true; // Уменьшаем мерцание
            
            // Создание меню
            CreateMenuStrip();
            
            // Создание панели инструментов
            CreateToolStrip();
            
            // Создание статусной строки
            CreateStatusStrip();
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private void CreateMenuStrip()
        {
            _menuStrip = new MenuStrip();
            
            // Файл
            var fileMenu = new ToolStripMenuItem("&Файл");
            
            var newWavetableItem = new ToolStripMenuItem("&Генератор волновых таблиц");
            newWavetableItem.ShortcutKeys = Keys.Control | Keys.N;
            newWavetableItem.Click += (s, e) => { var form = new WavetableForm(); form.Show(); };
            fileMenu.DropDownItems.Add(newWavetableItem);
            
            var customGeneratorItem = new ToolStripMenuItem("&Нарисовать свою волну");
            customGeneratorItem.ShortcutKeys = Keys.Control | Keys.D;
            customGeneratorItem.Click += (s, e) => { var form = new CustomWavetableForm(); form.Show(); };
            fileMenu.DropDownItems.Add(customGeneratorItem);
            
            fileMenu.DropDownItems.Add("-");
            
            var saveAllItem = new ToolStripMenuItem("&Сохранить все", null, OnSaveAll);
            saveAllItem.ShortcutKeys = Keys.Control | Keys.S;
            fileMenu.DropDownItems.Add(saveAllItem);
            
            var exportWavItem = new ToolStripMenuItem("&Экспорт в WAV", null, OnExportWav);
            exportWavItem.ShortcutKeys = Keys.Control | Keys.E;
            fileMenu.DropDownItems.Add(exportWavItem);
            
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("&Выход", null, OnExit);
            
            // Редактирование
            var editMenu = new ToolStripMenuItem("&Редактирование");
            editMenu.DropDownItems.Add("&Изменить таблицу", null, OnEditWavetable);
            editMenu.DropDownItems.Add("&Удалить таблицу", null, OnDeleteWavetable);
            editMenu.DropDownItems.Add("-");
            editMenu.DropDownItems.Add("&Очистить все", null, OnClearAll);
            
            // Вид
            var viewMenu = new ToolStripMenuItem("&Вид");
            var antialiasItem = new ToolStripMenuItem("&Антиалиасинг");
            antialiasItem.Checked = _antialiasing;
            antialiasItem.Click += (s, e) => {
                _antialiasing = !_antialiasing;
                antialiasItem.Checked = _antialiasing;
                _wavePanel?.Invalidate();
            };
            viewMenu.DropDownItems.Add(antialiasItem);
            viewMenu.DropDownItems.Add("-");
            viewMenu.DropDownItems.Add("&Обновить", null, OnRefreshView);
            
            // Инструменты
            var toolsMenu = new ToolStripMenuItem("&Инструменты");
            toolsMenu.DropDownItems.Add("&Пакетная генерация", null, OnBatchGenerate);
            toolsMenu.DropDownItems.Add("&Анализ спектра", null, OnSpectrumAnalysis);
            
            // Справка
            var helpMenu = new ToolStripMenuItem("&Справка");
            helpMenu.DropDownItems.Add("&О программе", null, OnAbout);
            
            _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, toolsMenu, helpMenu });
            this.Controls.Add(_menuStrip);
            this.MainMenuStrip = _menuStrip;
        }
        
        private void CreateToolStrip()
        {
            _toolStrip = new ToolStrip();
            _toolStrip.ImageScalingSize = new Size(24, 24);
            
            var newButton = new ToolStripButton("Новая", null, OnNewWavetable);
            newButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            _toolStrip.Items.Add(newButton);
            
            var setButton = new ToolStripButton("Набор", null, OnNewWavetableSet);
            _toolStrip.Items.Add(setButton);
            
            _toolStrip.Items.Add(new ToolStripSeparator());
            
            var saveButton = new ToolStripButton("Сохранить", null, OnSaveAll);
            _toolStrip.Items.Add(saveButton);
            
            var exportButton = new ToolStripButton("Экспорт", null, OnExportWav);
            _toolStrip.Items.Add(exportButton);
            
            _toolStrip.Items.Add(new ToolStripSeparator());
            
            var playButton = new ToolStripButton("Воспроизвести", null, OnPlaySelected);
            _toolStrip.Items.Add(playButton);
            
            var stopButton = new ToolStripButton("Стоп", null, OnStopPlayback);
            _toolStrip.Items.Add(stopButton);
            
            _toolStrip.Items.Add(new ToolStripSeparator());
            
            var clearButton = new ToolStripButton("Очистить", null, OnClearAll);
            _toolStrip.Items.Add(clearButton);
            
            this.Controls.Add(_toolStrip);
        }
        
        private void CreateStatusStrip()
        {
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Готов к работе");
            _progressBar = new ToolStripProgressBar();
            _progressBar.Visible = false;
            
            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(new ToolStripStatusLabel() { Spring = true });
            _statusStrip.Items.Add(_progressBar);
            
            this.Controls.Add(_statusStrip);
        }
        
        private void InitializeCustomComponents()
        {
            // Создаём сплиттер для разделения
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 350,
                FixedPanel = FixedPanel.Panel1
            };
            
            // Левая панель - список волновых таблиц
            var leftPanel = new Panel { Dock = DockStyle.Fill };
            
            var listLabel = new Label
            {
                Text = "Волновые таблицы:",
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            
            _wavetableListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                SelectionMode = SelectionMode.One,
                IntegralHeight = false
            };
            _wavetableListBox.SelectedIndexChanged += WavetableListBox_SelectedIndexChanged;
            _wavetableListBox.DoubleClick += (s, e) => OnEditWavetable(s, e);
            
            leftPanel.Controls.Add(_wavetableListBox);
            leftPanel.Controls.Add(listLabel);
            
            splitContainer.Panel1.Controls.Add(leftPanel);
            
            // Правая панель - визуализация и управление
            var rightPanel = new Panel { Dock = DockStyle.Fill };
            
            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150
            };
            
            // Слайдер фреймов
            _frameSlider = new TrackBar
            {
                Dock = DockStyle.Top,
                Height = 45,
                Minimum = 0,
                Maximum = 255,
                TickFrequency = 16,
                Enabled = false
            };
            _frameSlider.ValueChanged += FrameSlider_ValueChanged;
            
            _frameLabel = new Label
            {
                Text = "Фрейм: 0 / 0",
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            // Кнопки управления воспроизведением
            var playPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 35,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };
            
            _playButton = new Button
            {
                Text = "▶ Воспроизвести",
                Width = 120,
                Height = 25
            };
            _playButton.Click += OnPlaySelected;
            
            _stopButton = new Button
            {
                Text = "■ Стоп",
                Width = 80,
                Height = 25,
                Enabled = false
            };
            _stopButton.Click += OnStopPlayback;
            
            playPanel.Controls.Add(_playButton);
            playPanel.Controls.Add(_stopButton);
            
            // Информационная панель
            _infoGroupBox = new GroupBox
            {
                Text = "Информация",
                Dock = DockStyle.Fill
            };
            
            _infoLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8),
                Text = "Выберите волновую таблицу"
            };
            
            _infoGroupBox.Controls.Add(_infoLabel);
            
            controlPanel.Controls.Add(_infoGroupBox);
            controlPanel.Controls.Add(playPanel);
            controlPanel.Controls.Add(_frameLabel);
            controlPanel.Controls.Add(_frameSlider);
            
            // Панель для отображения волны
            _wavePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.Fixed3D
            };
            _wavePanel.Paint += WavePanel_Paint;
            _wavePanel.Resize += (s, e) => _wavePanel.Invalidate();
            
            rightPanel.Controls.Add(_wavePanel);
            rightPanel.Controls.Add(controlPanel);
            
            splitContainer.Panel2.Controls.Add(rightPanel);
            
            this.Controls.Add(splitContainer);
            
            // Устанавливаем правильный порядок элементов
            this.Controls.SetChildIndex(_menuStrip, 0);
            this.Controls.SetChildIndex(_toolStrip, 1);
            this.Controls.SetChildIndex(splitContainer, 2);
            this.Controls.SetChildIndex(_statusStrip, 3);
        }
        
        private void WavePanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            
            if (_antialiasing)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            }
            
            // Рисуем сетку
            DrawGrid(g);
            
            // Рисуем волну
            if (_selectedWavetable != null && _selectedWavetable.Samples.Length > 0)
            {
                DrawWaveform(g);
            }
            else
            {
                // Рисуем заглушку
                using (var font = new Font("Segoe UI", 12))
                using (var brush = new SolidBrush(Color.FromArgb(80, 255, 255, 255)))
                {
                    var text = "Выберите волновую таблицу для отображения";
                    var textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, brush, 
                        (_wavePanel.Width - textSize.Width) / 2,
                        (_wavePanel.Height - textSize.Height) / 2);
                }
            }
        }
        
        private void DrawGrid(Graphics g)
        {
            int width = _wavePanel.Width;
            int height = _wavePanel.Height;
            
            using (var pen = new Pen(_gridColor, 1))
            {
                pen.DashStyle = DashStyle.Dot;
                
                // Горизонтальные линии
                for (int i = 1; i < 4; i++)
                {
                    int y = height * i / 4;
                    g.DrawLine(pen, 0, y, width, y);
                }
                
                // Вертикальные линии
                for (int i = 1; i < 8; i++)
                {
                    int x = width * i / 8;
                    g.DrawLine(pen, x, 0, x, height);
                }
                
                // Центральная линия
                using (var centerPen = new Pen(Color.FromArgb(60, 255, 255, 255), 1))
                {
                    g.DrawLine(centerPen, 0, height / 2, width, height / 2);
                }
            }
        }
        
        private void DrawWaveform(Graphics g)
        {
            if (_selectedWavetable == null || _selectedWavetable.Samples.Length == 0)
                return;
                
            int width = _wavePanel.Width;
            int height = _wavePanel.Height;
            int centerY = height / 2;
            
            // Определяем, какую часть волновой таблицы отображать
            int frameToShow = _frameSlider.Value;
            int samplesPerFrame = _selectedWavetable.TableSize;
            int startSample = frameToShow * samplesPerFrame;
            int endSample = Math.Min(startSample + samplesPerFrame, _selectedWavetable.Samples.Length);
            
            if (endSample <= startSample)
                return;
                
            int samplesToShow = endSample - startSample;
            
            using (var pen = new Pen(_waveColor, 1))
            {
                // Рисуем waveform используя min/max для каждого пикселя
                for (int x = 0; x < width; x++)
                {
                    // Диапазон сэмплов для этого пикселя
                    double startPos = (double)x / width;
                    double endPos = (double)(x + 1) / width;
                    
                    int startIdx = startSample + (int)(startPos * samplesToShow);
                    int endIdx = startSample + (int)(endPos * samplesToShow);
                    
                    if (startIdx >= _selectedWavetable.Samples.Length)
                        break;
                    
                    endIdx = Math.Min(endIdx, _selectedWavetable.Samples.Length);
                    
                    // Находим min и max в этом диапазоне
                    float minSample = 0;
                    float maxSample = 0;
                    
                    for (int i = startIdx; i < endIdx; i++)
                    {
                        float sample = _selectedWavetable.Samples[i];
                        
                        if (float.IsNaN(sample) || float.IsInfinity(sample))
                            sample = 0.0f;
                        
                        sample = Math.Max(-1.0f, Math.Min(1.0f, sample));
                        
                        if (sample < minSample) minSample = sample;
                        if (sample > maxSample) maxSample = sample;
                    }
                    
                    // Рисуем вертикальную линию от min до max
                    int yMin = centerY - (int)(maxSample * centerY * 0.9f);
                    int yMax = centerY - (int)(minSample * centerY * 0.9f);
                    
                    if (yMax > yMin)
                        g.DrawLine(pen, x, yMin, x, yMax);
                }
            }
        }
        
        private void UpdateWavetableList()
        {
            _wavetableListBox.Items.Clear();
            
            foreach (var wt in _wavetables)
            {
                _wavetableListBox.Items.Add(wt.ToString());
            }
            
            UpdateStatus($"Загружено таблиц: {_wavetables.Count}");
        }
        
        private void UpdateStatus(string message)
        {
            _statusLabel.Text = message;
        }
        
        private void ShowProgress(bool show, int max = 100)
        {
            _progressBar.Visible = show;
            if (show)
            {
                _progressBar.Value = 0;
                _progressBar.Maximum = max;
            }
        }
        
        private void UpdateProgress(int value)
        {
            if (_progressBar.Visible)
            {
                _progressBar.Value = Math.Min(value, _progressBar.Maximum);
            }
        }
        
        private void WavetableListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_wavetableListBox.SelectedIndex >= 0 && _wavetableListBox.SelectedIndex < _wavetables.Count)
            {
                _selectedWavetable = _wavetables[_wavetableListBox.SelectedIndex];
                
                // Обновляем слайдер фреймов
                _frameSlider.Maximum = Math.Max(0, _selectedWavetable.FrameCount - 1);
                _frameSlider.Value = 0;
                _frameSlider.Enabled = _selectedWavetable.FrameCount > 1;
                _frameLabel.Text = $"Фрейм: 1 / {_selectedWavetable.FrameCount}";
                
                // Обновляем информацию
                UpdateWavetableInfo();
                
                // Перерисовываем
                _wavePanel.Invalidate();
            }
        }
        
        private void FrameSlider_ValueChanged(object? sender, EventArgs e)
        {
            if (_selectedWavetable != null)
            {
                _frameLabel.Text = $"Фрейм: {_frameSlider.Value + 1} / {_selectedWavetable.FrameCount}";
                _wavePanel.Invalidate();
                
                // Обновляем воспроизведение если играет
                if (_waveProvider != null && _waveOut != null)
                {
                    _waveProvider.SetFrame(_frameSlider.Value);
                    _waveOut.Stop();
                    _waveOut.Play();
                }
            }
        }
        
        private void UpdateWavetableInfo()
        {
            if (_selectedWavetable == null)
            {
                _infoLabel.Text = "Выберите волновую таблицу";
                return;
            }
            
            var stats = _selectedWavetable.GetStatistics();
            
            _infoLabel.Text = $@"Название: {_selectedWavetable.Name}
Тип: {_selectedWavetable.WaveType}
Фреймов: {_selectedWavetable.FrameCount}
Размер таблицы: {_selectedWavetable.TableSize}
Всего сэмплов: {_selectedWavetable.SampleCount}

Статистика:
Пиковое значение: {stats.Peak:F3}
RMS: {stats.RMS:F3}
Crest Factor: {stats.CrestFactor:F2}
DC Offset: {stats.Average:F4}";
        }
        
        // Обработчики событий меню
        private async void OnNewWavetable(object? sender, EventArgs e)
        {
            using (var dialog = new WavetableForm())
            {
                if (dialog.ShowDialog() == DialogResult.OK && dialog.Wavetable != null)
                {
                    UpdateStatus("Генерация волновой таблицы...");
                    ShowProgress(true);
                    
                    await Task.Run(() => {
                        _wavetables.Add(dialog.Wavetable);
                    });
                    
                    ShowProgress(false);
                    UpdateWavetableList();
                    _wavetableListBox.SelectedIndex = _wavetables.Count - 1;
                    UpdateStatus("Волновая таблица создана");
                }
            }
        }
        
        private async void OnNewWavetableSet(object? sender, EventArgs e)
        {
            using (var dialog = new WavetableSetForm())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateStatus("Генерация набора волновых таблиц...");
                    ShowProgress(true, dialog.Count);
                    
                    // ИСПОЛЬЗУЕМ УЛУЧШЕННЫЙ метод GenerateWavetableSet с поддержкой Brightness и Warmth
                    var newTables = await Task.Run(() => {
                        return _generator.GenerateWavetableSet(
                            dialog.Count, 
                            dialog.TableSize, 
                            dialog.SelectedPresets,
                            dialog.FrameCount,
                            dialog.Harmonics,
                            dialog.Brightness,
                            dialog.Warmth);
                    });
                    
                    // Обновляем прогресс после завершения
                    this.Invoke((Action)(() => {
                        _wavetables.AddRange(newTables);
                        ShowProgress(false);
                        UpdateWavetableList();
                        UpdateStatus($"Создано {newTables.Count} волновых таблиц");
                    }));
                }
            }
        }
        
        private void OnEditWavetable(object? sender, EventArgs e)
        {
            if (_selectedWavetable != null)
            {
                using (var dialog = new WavetableForm(_selectedWavetable))
                {
                    if (dialog.ShowDialog() == DialogResult.OK && dialog.Wavetable != null)
                    {
                        int index = _wavetables.IndexOf(_selectedWavetable);
                        _wavetables[index] = dialog.Wavetable;
                        _selectedWavetable = dialog.Wavetable;
                        UpdateWavetableList();
                        _wavetableListBox.SelectedIndex = index;
                        UpdateStatus("Волновая таблица изменена");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите волновую таблицу для редактирования", "Информация", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void OnDeleteWavetable(object? sender, EventArgs e)
        {
            if (_selectedWavetable != null)
            {
                var result = MessageBox.Show($"Удалить волновую таблицу '{_selectedWavetable.Name}'?", 
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    _wavetables.Remove(_selectedWavetable);
                    _selectedWavetable = null;
                    UpdateWavetableList();
                    _wavePanel.Invalidate();
                    UpdateStatus("Волновая таблица удалена");
                }
            }
        }
        
        private void OnClearAll(object? sender, EventArgs e)
        {
            if (_wavetables.Count > 0)
            {
                var result = MessageBox.Show("Удалить все волновые таблицы?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    _wavetables.Clear();
                    _selectedWavetable = null;
                    UpdateWavetableList();
                    _wavePanel.Invalidate();
                    UpdateStatus("Все таблицы удалены");
                }
            }
        }
        
        private async void OnSaveAll(object? sender, EventArgs e)
        {
            if (_wavetables.Count == 0)
            {
                MessageBox.Show("Нет волновых таблиц для сохранения", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку для сохранения волновых таблиц";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateStatus("Сохранение волновых таблиц...");
                    ShowProgress(true, _wavetables.Count);
                    
                    await Task.Run(() => {
                        for (int i = 0; i < _wavetables.Count; i++)
                        {
                            var wt = _wavetables[i];
                            string filename = Path.Combine(dialog.SelectedPath, $"{wt.Name}.wav");
                            _generator.SaveWavetableToWav(wt, filename);
                            
                            this.Invoke((Action)(() => UpdateProgress(i + 1)));
                        }
                    });
                    
                    ShowProgress(false);
                    UpdateStatus($"Сохранено {_wavetables.Count} файлов");
                    MessageBox.Show($"Сохранено {_wavetables.Count} волновых таблиц", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        
        private void OnExportWav(object? sender, EventArgs e)
        {
            if (_selectedWavetable == null)
            {
                MessageBox.Show("Выберите волновую таблицу для экспорта", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "WAV файлы (*.wav)|*.wav|Все файлы (*.*)|*.*";
                dialog.FileName = $"{_selectedWavetable.Name}.wav";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _generator.SaveWavetableToWav(_selectedWavetable, dialog.FileName);
                        UpdateStatus($"Экспортировано: {Path.GetFileName(dialog.FileName)}");
                        MessageBox.Show("Волновая таблица успешно экспортирована", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void OnPlaySelected(object? sender, EventArgs e)
        {
            if (_selectedWavetable == null)
            {
                MessageBox.Show("Выберите волновую таблицу для воспроизведения", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            try
            {
                // Останавливаем предыдущее воспроизведение
                OnStopPlayback(sender, e);
                
                // Создаём провайдер для воспроизведения ВСЕЙ таблицы
                _waveProvider = new WavetableWaveProvider(_selectedWavetable);
                _waveProvider.Reset(); // Сбрасываем на начало
                
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_waveProvider);
                _waveOut.Play();
                
                // Запускаем таймер для отслеживания завершения
                _playbackTimer = new System.Windows.Forms.Timer();
                _playbackTimer.Interval = 50; // Проверяем каждые 50ms
                _playbackTimer.Tick += (s, args) =>
                {
                    if (_waveProvider != null && _waveProvider.IsFinished)
                    {
                        OnStopPlayback(s, args);
                    }
                };
                _playbackTimer.Start();
                
                _playButton.Enabled = false;
                _stopButton.Enabled = true;
                UpdateStatus($"Воспроизведение полного цикла ({_selectedWavetable.FrameCount} фреймов, ~10 сек)...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void OnStopPlayback(object? sender, EventArgs e)
        {
            if (_playbackTimer != null)
            {
                _playbackTimer.Stop();
                _playbackTimer.Dispose();
                _playbackTimer = null;
            }
            
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }
            
            _waveProvider = null;
            _playButton.Enabled = true;
            _stopButton.Enabled = false;
            UpdateStatus("Готов");
        }
        
        private void OnBatchGenerate(object? sender, EventArgs e)
        {
            MessageBox.Show("Функция пакетной генерации в разработке", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OnSpectrumAnalysis(object? sender, EventArgs e)
        {
            MessageBox.Show("Анализатор спектра в разработке", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OnRefreshView(object? sender, EventArgs e)
        {
            _wavePanel.Invalidate();
            UpdateStatus("Вид обновлён");
        }
        
        private void OnAbout(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Wavetable Generator Pro v2.0\n\n" +
                "Профессиональный генератор волновых таблиц\n" +
                "с оптимизированными алгоритмами синтеза\n\n" +
                "© 2024 - Оптимизированная версия",
                "О программе",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void OnExit(object? sender, EventArgs e)
        {
            this.Close();
        }
        
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            OnStopPlayback(sender, e);
        }
    }
    
    /// <summary>
    /// Провайдер для воспроизведения волновой таблицы
    /// </summary>
    public class WavetableWaveProvider : IWaveProvider
    {
        private readonly Wavetable _wavetable;
        private int _position = 0;
        private int _currentFrame = 0;
        private int _samplesInCurrentFrame = 0;
        private int _samplesPerFrameDuration; // Длительность каждого фрейма
        private bool _isFinished = false;
        
        public WaveFormat WaveFormat { get; }
        public bool IsFinished => _isFinished;
        
        public WavetableWaveProvider(Wavetable wavetable)
        {
            _wavetable = wavetable;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);
            
            // Весь цикл за 10 секунд: 44100 samples/sec * 10 sec = 441000 samples
            // Делим на количество фреймов
            int totalSamplesFor10sec = 441000;
            _samplesPerFrameDuration = totalSamplesFor10sec / _wavetable.FrameCount;
        }
        
        public void SetFrame(int frameIndex)
        {
            _currentFrame = frameIndex;
            _samplesInCurrentFrame = 0;
            int samplesPerFrame = _wavetable.Samples.Length / _wavetable.FrameCount;
            _position = frameIndex * samplesPerFrame;
        }
        
        public void Reset()
        {
            _currentFrame = 0;
            _position = 0;
            _samplesInCurrentFrame = 0;
            _isFinished = false;
        }
        
        public int Read(byte[] buffer, int offset, int count)
        {
            if (_isFinished)
                return 0;
            
            int samplesRequested = count / 4; // 4 байта на float
            int samplesRead = 0;
            
            int samplesPerFrame = _wavetable.Samples.Length / _wavetable.FrameCount;
            int freqMultiplier = 2; // Частота ~55 Hz (-3 октавы)
            
            for (int i = 0; i < samplesRequested; i++)
            {
                // Переключаемся на следующий фрейм по таймингу
                if (_samplesInCurrentFrame >= _samplesPerFrameDuration)
                {
                    _currentFrame++;
                    _samplesInCurrentFrame = 0;
                    
                    // Если прошли все фреймы - останавливаемся
                    if (_currentFrame >= _wavetable.FrameCount)
                    {
                        _isFinished = true;
                        return samplesRead * 4;
                    }
                }
                
                int frameStart = _currentFrame * samplesPerFrame;
                int wrappedPos = _position - frameStart;
                
                // Цикличное воспроизведение внутри фрейма
                if (wrappedPos >= samplesPerFrame)
                {
                    _position = frameStart;
                    wrappedPos = 0;
                }
                
                // Ускоряем чтение для повышения частоты
                int readPos = frameStart + (wrappedPos * freqMultiplier) % samplesPerFrame;
                float sample = _wavetable.Samples[readPos];
                
                _position++;
                _samplesInCurrentFrame++;
                
                byte[] bytes = BitConverter.GetBytes(sample);
                
                buffer[offset + i * 4] = bytes[0];
                buffer[offset + i * 4 + 1] = bytes[1];
                buffer[offset + i * 4 + 2] = bytes[2];
                buffer[offset + i * 4 + 3] = bytes[3];
                
                samplesRead++;
            }
            
            return samplesRead * 4;
        }
    }
}