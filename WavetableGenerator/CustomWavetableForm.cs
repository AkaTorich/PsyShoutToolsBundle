using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace WavetableGenerator
{
    public class CustomWavetableForm : Form
    {
        private Panel _startPanel;
        private Panel _endPanel;
        private Button _generateButton;
        private Button _clearStartButton;
        private Button _clearEndButton;
        private Button _saveButton;
        private NumericUpDown _brightnessNumeric;
        private NumericUpDown _warmthNumeric;
        private Label _statusLabel;
        
        private List<PointF> _startWavePoints = new List<PointF>();
        private List<PointF> _endWavePoints = new List<PointF>();
        private bool _isDrawingStart = false;
        private bool _isDrawingEnd = false;
        
        private Wavetable? _generatedWavetable;
        
        public CustomWavetableForm()
        {
            Text = "Custom Wavetable Generator v4 - Нарисуй волны";
            Size = new Size(1400, 700);
            StartPosition = FormStartPosition.CenterScreen;
            
            CreateUI();
        }
        
        private void CreateUI()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            
            // Заголовки
            var startLabel = new Label
            {
                Text = "ПЕРВАЯ ВОЛНА (фрейм 1)",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White
            };
            
            var endLabel = new Label
            {
                Text = "ПОСЛЕДНЯЯ ВОЛНА (фрейм 256)",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White
            };
            
            // Canvas для рисования
            _startPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            _startPanel.Paint += StartPanel_Paint;
            _startPanel.MouseDown += StartPanel_MouseDown;
            _startPanel.MouseMove += StartPanel_MouseMove;
            _startPanel.MouseUp += StartPanel_MouseUp;
            
            _endPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 30),
                BorderStyle = BorderStyle.FixedSingle
            };
            _endPanel.Paint += EndPanel_Paint;
            _endPanel.MouseDown += EndPanel_MouseDown;
            _endPanel.MouseMove += EndPanel_MouseMove;
            _endPanel.MouseUp += EndPanel_MouseUp;
            
            // Панель управления
            var controlPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(35, 35, 40),
                Padding = new Padding(10)
            };
            
            _clearStartButton = new Button
            {
                Text = "Очистить первую",
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _clearStartButton.Click += (s, e) =>
            {
                _startWavePoints.Clear();
                _startPanel.Invalidate();
            };
            
            _clearEndButton = new Button
            {
                Text = "Очистить последнюю",
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(60, 60, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _clearEndButton.Click += (s, e) =>
            {
                _endWavePoints.Clear();
                _endPanel.Invalidate();
            };
            
            controlPanel.Controls.Add(_clearStartButton);
            controlPanel.Controls.Add(_clearEndButton);
            
            var brightnessLabel = new Label
            {
                Text = "Brightness:",
                AutoSize = true,
                ForeColor = Color.White,
                Margin = new Padding(20, 7, 5, 0)
            };
            
            _brightnessNumeric = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                Width = 60,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White
            };
            
            var warmthLabel = new Label
            {
                Text = "Warmth:",
                AutoSize = true,
                ForeColor = Color.White,
                Margin = new Padding(20, 7, 5, 0)
            };
            
            _warmthNumeric = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                Width = 60,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White
            };
            
            controlPanel.Controls.Add(brightnessLabel);
            controlPanel.Controls.Add(_brightnessNumeric);
            controlPanel.Controls.Add(warmthLabel);
            controlPanel.Controls.Add(_warmthNumeric);
            
            _generateButton = new Button
            {
                Text = "ГЕНЕРИРОВАТЬ ТАБЛИЦУ",
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Margin = new Padding(20, 0, 10, 0)
            };
            _generateButton.Click += GenerateButton_Click;
            
            _saveButton = new Button
            {
                Text = "Сохранить WAV",
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(0, 150, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _saveButton.Click += SaveButton_Click;
            
            controlPanel.Controls.Add(_generateButton);
            controlPanel.Controls.Add(_saveButton);
            
            _statusLabel = new Label
            {
                Text = "Нарисуйте обе волны и нажмите 'ГЕНЕРИРОВАТЬ'",
                AutoSize = true,
                ForeColor = Color.LightGray,
                Margin = new Padding(20, 12, 0, 0)
            };
            controlPanel.Controls.Add(_statusLabel);
            
            // Размещаем элементы
            mainLayout.Controls.Add(startLabel, 0, 0);
            mainLayout.Controls.Add(endLabel, 1, 0);
            mainLayout.Controls.Add(_startPanel, 0, 1);
            mainLayout.Controls.Add(_endPanel, 1, 1);
            mainLayout.SetColumnSpan(controlPanel, 2);
            mainLayout.Controls.Add(controlPanel, 0, 2);
            
            Controls.Add(mainLayout);
        }
        
        // Рисование первой волны
        private void StartPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDrawingStart = true;
                _startWavePoints.Clear();
                _startWavePoints.Add(new PointF(e.X, e.Y));
            }
        }
        
        private void StartPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDrawingStart)
            {
                _startWavePoints.Add(new PointF(e.X, e.Y));
                _startPanel.Invalidate();
            }
        }
        
        private void StartPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            _isDrawingStart = false;
        }
        
        private void StartPanel_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Сетка
            using (var gridPen = new Pen(Color.FromArgb(50, 50, 55)))
            {
                int height = _startPanel.Height;
                int width = _startPanel.Width;
                
                // Горизонтальные линии
                for (int i = 0; i <= 4; i++)
                {
                    int y = height * i / 4;
                    e.Graphics.DrawLine(gridPen, 0, y, width, y);
                }
                
                // Вертикальные линии
                for (int i = 0; i <= 8; i++)
                {
                    int x = width * i / 8;
                    e.Graphics.DrawLine(gridPen, x, 0, x, height);
                }
            }
            
            // Центральная линия
            using (var centerPen = new Pen(Color.FromArgb(80, 80, 85), 2))
            {
                e.Graphics.DrawLine(centerPen, 0, _startPanel.Height / 2, _startPanel.Width, _startPanel.Height / 2);
            }
            
            // Нарисованная волна
            if (_startWavePoints.Count > 1)
            {
                using (var wavePen = new Pen(Color.FromArgb(0, 255, 100), 3))
                {
                    for (int i = 0; i < _startWavePoints.Count - 1; i++)
                    {
                        e.Graphics.DrawLine(wavePen, _startWavePoints[i], _startWavePoints[i + 1]);
                    }
                }
            }
        }
        
        // Рисование последней волны
        private void EndPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDrawingEnd = true;
                _endWavePoints.Clear();
                _endWavePoints.Add(new PointF(e.X, e.Y));
            }
        }
        
        private void EndPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDrawingEnd)
            {
                _endWavePoints.Add(new PointF(e.X, e.Y));
                _endPanel.Invalidate();
            }
        }
        
        private void EndPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            _isDrawingEnd = false;
        }
        
        private void EndPanel_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Сетка
            using (var gridPen = new Pen(Color.FromArgb(50, 50, 55)))
            {
                int height = _endPanel.Height;
                int width = _endPanel.Width;
                
                for (int i = 0; i <= 4; i++)
                {
                    int y = height * i / 4;
                    e.Graphics.DrawLine(gridPen, 0, y, width, y);
                }
                
                for (int i = 0; i <= 8; i++)
                {
                    int x = width * i / 8;
                    e.Graphics.DrawLine(gridPen, x, 0, x, height);
                }
            }
            
            // Центральная линия
            using (var centerPen = new Pen(Color.FromArgb(80, 80, 85), 2))
            {
                e.Graphics.DrawLine(centerPen, 0, _endPanel.Height / 2, _endPanel.Width, _endPanel.Height / 2);
            }
            
            // Нарисованная волна
            if (_endWavePoints.Count > 1)
            {
                using (var wavePen = new Pen(Color.FromArgb(255, 100, 0), 3))
                {
                    for (int i = 0; i < _endWavePoints.Count - 1; i++)
                    {
                        e.Graphics.DrawLine(wavePen, _endWavePoints[i], _endWavePoints[i + 1]);
                    }
                }
            }
        }
        
        private void GenerateButton_Click(object? sender, EventArgs e)
        {
            if (_startWavePoints.Count < 10 || _endWavePoints.Count < 10)
            {
                MessageBox.Show("Нарисуйте обе волны!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                _statusLabel.Text = "Генерация...";
                Application.DoEvents();
                
                // Конвертируем точки canvas в волны
                var startWave = ConvertPointsToWave(_startWavePoints, _startPanel.Height);
                var endWave = ConvertPointsToWave(_endWavePoints, _endPanel.Height);
                
                // Генерируем таблицу
                double brightness = (double)_brightnessNumeric.Value / 100.0;
                double warmth = (double)_warmthNumeric.Value / 100.0;
                
                _generatedWavetable = WavetableGeneratorCustom.GenerateMorphingWavetable(
                    startWave, endWave, 256, 2048, brightness, warmth);
                
                _saveButton.Enabled = true;
                _statusLabel.Text = "Таблица сгенерирована! Сохраните WAV.";
                _statusLabel.ForeColor = Color.LightGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusLabel.Text = "Ошибка генерации";
                _statusLabel.ForeColor = Color.Red;
            }
        }
        
        private float[] ConvertPointsToWave(List<PointF> points, int panelHeight)
        {
            // Сортируем точки по X
            points.Sort((a, b) => a.X.CompareTo(b.X));
            
            // Конвертируем в нормализованные значения
            var wave = new float[points.Count];
            float centerY = panelHeight / 2f;
            
            for (int i = 0; i < points.Count; i++)
            {
                // Инвертируем Y (canvas Y растет вниз)
                wave[i] = (centerY - points[i].Y) / centerY;
                wave[i] = Math.Max(-1f, Math.Min(1f, wave[i]));
            }
            
            // Ресэмплинг до 2048
            return WavetableGeneratorCustom.CanvasToWave(wave, 2048);
        }
        
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (_generatedWavetable == null) return;
            
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "WAV files (*.wav)|*.wav";
                dialog.FileName = $"WT_Custom_{DateTime.Now:HHmmss}.wav";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        WavetableGeneratorCustom.SaveToWav(_generatedWavetable, dialog.FileName);
                        _statusLabel.Text = $"Сохранено: {System.IO.Path.GetFileName(dialog.FileName)}";
                        _statusLabel.ForeColor = Color.LightBlue;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}

