using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace PsyTrancePlayer;

public partial class SplashScreen : Window
{
    private readonly DispatcherTimer _timer;

    public SplashScreen()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _timer.Tick += Timer_Tick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var pulseStoryboard = (Storyboard)FindResource("PulseStoryboard");
        var rotateStoryboard = (Storyboard)FindResource("RotateStoryboard");
        var rotateReverseStoryboard = (Storyboard)FindResource("RotateReverseStoryboard");

        pulseStoryboard.Begin();
        rotateStoryboard.Begin();
        rotateReverseStoryboard.Begin();

        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _timer.Stop();

        var mainWindow = new MainWindow();
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();

        Close();
    }
}
