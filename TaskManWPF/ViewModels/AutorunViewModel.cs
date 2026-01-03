using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskManWPF.Models;
using TaskManWPF.Services;

namespace TaskManWPF.ViewModels;

public partial class AutorunViewModel : ObservableObject
{
    private readonly AutorunService _service = new();
    
    [ObservableProperty]
    private ObservableCollection<AutorunInfo> _autoruns = new();
    
    [ObservableProperty]
    private AutorunInfo? _selectedAutorun;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string _statusText = "Готово";
    
    [ObservableProperty]
    private int _autorunCount;
    
    private List<AutorunInfo> _allAutoruns = new();
    
    public AutorunViewModel()
    {
        Refresh();
    }
    
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }
    
    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allAutoruns
            : _allAutoruns.Where(a => 
                a.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                a.Path.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                a.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                a.Company.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        
        Autoruns = new ObservableCollection<AutorunInfo>(filtered);
        AutorunCount = Autoruns.Count;
    }
    
    [RelayCommand]
    private void Refresh()
    {
        StatusText = "Сканирование автозапуска...";
        
        Task.Run(() =>
        {
            var autoruns = _service.GetAllAutoruns();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                _allAutoruns = autoruns;
                ApplyFilter();
                StatusText = $"Найдено {_allAutoruns.Count} записей автозапуска";
            });
        });
    }
    
    [RelayCommand]
    private void RemoveAutorun()
    {
        if (SelectedAutorun == null) return;
        
        var result = MessageBox.Show(
            $"Удалить запись \"{SelectedAutorun.Name}\"?\n\nИсточник: {SelectedAutorun.SourceDisplay}",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            if (_service.RemoveAutorun(SelectedAutorun))
            {
                StatusText = $"Запись {SelectedAutorun.Name} удалена";
                Refresh();
            }
            else
            {
                MessageBox.Show(
                    "Не удалось удалить запись.\n\nВозможные причины:\n• Недостаточно прав\n• Запись защищена системой",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    [RelayCommand]
    private void DisableAutorun()
    {
        if (SelectedAutorun == null) return;
        
        if (_service.DisableAutorun(SelectedAutorun))
        {
            StatusText = $"Запись {SelectedAutorun.Name} отключена";
            Refresh();
        }
        else
        {
            MessageBox.Show("Не удалось отключить запись", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void OpenFileLocation()
    {
        if (SelectedAutorun == null || string.IsNullOrEmpty(SelectedAutorun.Path)) return;
        
        try
        {
            if (System.IO.File.Exists(SelectedAutorun.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{SelectedAutorun.Path}\"");
            }
        }
        catch { }
    }
    
    [RelayCommand]
    private void ShowProperties()
    {
        if (SelectedAutorun == null) return;
        
        var info = $"Имя: {SelectedAutorun.Name}\n" +
                   $"Путь: {SelectedAutorun.Path}\n" +
                   $"Аргументы: {SelectedAutorun.Arguments}\n" +
                   $"Описание: {SelectedAutorun.Description}\n" +
                   $"Компания: {SelectedAutorun.Company}\n" +
                   $"Источник: {SelectedAutorun.SourceDisplay}\n" +
                   $"Детали: {SelectedAutorun.SourceDetails}\n" +
                   $"Состояние: {SelectedAutorun.StatusDisplay}\n" +
                   $"Подпись: {SelectedAutorun.SignedDisplay}\n" +
                   $"Размер: {SelectedAutorun.FileSize:N0} байт\n" +
                   $"Изменён: {SelectedAutorun.LastModified:dd.MM.yyyy HH:mm}";
        
        MessageBox.Show(info, "Свойства", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

