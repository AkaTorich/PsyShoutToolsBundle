using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TaskManWPF.Views;

public partial class MainWindow : Window
{
    public static IValueConverter EqualityConverter { get; } = new EqualityToCheckedConverter();
    
    public MainWindow()
    {
        InitializeComponent();
    }
}

public class EqualityToCheckedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return parameter;
    }
}

