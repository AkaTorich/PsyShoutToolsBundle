using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PsyTrancePlayer.Views;

public partial class VisualizerControl : UserControl
{
    private readonly Rectangle[] _bars;
    private const int BarCount = 64;
    private readonly Color[] _colors;

    public static readonly DependencyProperty SpectrumDataProperty =
        DependencyProperty.Register(nameof(SpectrumData), typeof(float[]), typeof(VisualizerControl),
            new PropertyMetadata(null, OnSpectrumDataChanged));

    public float[]? SpectrumData
    {
        get => (float[]?)GetValue(SpectrumDataProperty);
        set => SetValue(SpectrumDataProperty, value);
    }

    public VisualizerControl()
    {
        InitializeComponent();

        _bars = new Rectangle[BarCount];
        _colors = new[]
        {
            Color.FromRgb(176, 38, 255),   // Purple
            Color.FromRgb(255, 20, 147),   // Pink
            Color.FromRgb(0, 191, 255),    // Blue
            Color.FromRgb(57, 255, 20)     // Green
        };

        Loaded += (s, e) =>
        {
            if (ActualWidth > 0 && ActualHeight > 0)
            {
                InitializeBars();
            }
        };
    }

    private void InitializeBars()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
            return;

        VisualizerCanvas.Children.Clear();

        var barWidth = ActualWidth / BarCount;
        var spacing = 2;

        for (int i = 0; i < BarCount; i++)
        {
            var bar = new Rectangle
            {
                Width = Math.Max(barWidth - spacing, 1),
                Height = 0,
                Fill = new SolidColorBrush(_colors[i % _colors.Length])
                {
                    Opacity = 0.9  // Increased opacity since we removed shadow
                },
                RadiusX = 2,
                RadiusY = 2
            };

            // Removed DropShadowEffect - huge performance improvement!
            // 64 bars × DropShadowEffect was causing severe lag during window dragging

            Canvas.SetLeft(bar, i * barWidth);
            Canvas.SetBottom(bar, 0);

            _bars[i] = bar;
            VisualizerCanvas.Children.Add(bar);
        }
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        
        // Обновляем размер Canvas
        VisualizerCanvas.Width = ActualWidth;
        VisualizerCanvas.Height = ActualHeight;
        
        if (ActualWidth > 0 && ActualHeight > 0 && _bars != null && _bars.Length > 0)
        {
            // Пересчитываем позиции и ширину баров
            var barWidth = ActualWidth / BarCount;
            var spacing = 2;

            for (int i = 0; i < _bars.Length; i++)
            {
                if (_bars[i] != null)
                {
                    _bars[i].Width = Math.Max(barWidth - spacing, 1);
                    Canvas.SetLeft(_bars[i], i * barWidth);
                }
            }
        }
        else if (ActualWidth > 0 && ActualHeight > 0)
        {
            // Если бары еще не созданы, инициализируем их
            InitializeBars();
        }
    }

    private static void OnSpectrumDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VisualizerControl control && e.NewValue is float[] data)
        {
            control.UpdateVisualization(data);
        }
    }

    private void UpdateVisualization(float[] spectrumData)
    {
        if (spectrumData == null || spectrumData.Length == 0 || ActualHeight == 0 || _bars == null)
            return;

        var maxHeight = Math.Max(ActualHeight - 10, 0);

        for (int i = 0; i < Math.Min(_bars.Length, spectrumData.Length); i++)
        {
            if (_bars[i] == null)
                continue;

            var normalizedValue = Math.Min(spectrumData[i], 1.0f);
            var targetHeight = normalizedValue * maxHeight;

            _bars[i].Height = targetHeight;

            var colorIndex = (int)(normalizedValue * (_colors.Length - 1));
            colorIndex = Math.Clamp(colorIndex, 0, _colors.Length - 1);

            if (_bars[i].Fill is SolidColorBrush brush)
            {
                brush.Color = _colors[colorIndex];
            }
        }
    }
}
