using System.Windows;

namespace TaskManWPF;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Глобальная обработка ошибок
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"Произошла ошибка: {args.Exception.Message}", 
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
    }
}

