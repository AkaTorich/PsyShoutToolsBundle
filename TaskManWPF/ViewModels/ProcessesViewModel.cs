using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskManWPF.Models;
using TaskManWPF.Services;

namespace TaskManWPF.ViewModels;

public partial class ProcessesViewModel : ObservableObject
{
    private readonly ProcessService _service = new();
    
    [ObservableProperty]
    private ObservableCollection<ProcessInfo> _processes = new();
    
    [ObservableProperty]
    private ProcessInfo? _selectedProcess;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string _statusText = "Готово";
    
    [ObservableProperty]
    private int _processCount;
    
    private List<ProcessInfo> _allProcesses = new();
    
    public ProcessesViewModel()
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
            ? _allProcesses
            : _allProcesses.Where(p => 
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Path.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        
        Processes = new ObservableCollection<ProcessInfo>(filtered);
        ProcessCount = Processes.Count;
    }
    
    [RelayCommand]
    private void Refresh()
    {
        StatusText = "Обновление...";
        
        Task.Run(() =>
        {
            var processes = _service.GetAllProcesses();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                _allProcesses = processes;
                ApplyFilter();
                StatusText = $"Загружено {_allProcesses.Count} процессов";
            });
        });
    }
    
    [RelayCommand]
    private void TerminateProcess()
    {
        if (SelectedProcess == null) return;
        
        var result = MessageBox.Show(
            $"Завершить процесс \"{SelectedProcess.Name}\" (PID: {SelectedProcess.Pid})?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            if (_service.TerminateProcess(SelectedProcess.Pid))
            {
                StatusText = $"Процесс {SelectedProcess.Name} завершён";
                Refresh();
            }
            else
            {
                MessageBox.Show("Не удалось завершить процесс", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    [RelayCommand]
    private void TerminateProcessTree()
    {
        if (SelectedProcess == null) return;
        
        var result = MessageBox.Show(
            $"Завершить дерево процессов \"{SelectedProcess.Name}\"?\n\nЭто завершит процесс и все его дочерние процессы.",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        
        if (result == MessageBoxResult.Yes)
        {
            if (_service.TerminateProcessTree(SelectedProcess.Pid))
            {
                StatusText = $"Дерево процессов {SelectedProcess.Name} завершено";
                Refresh();
            }
            else
            {
                MessageBox.Show("Не удалось завершить дерево процессов", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    [RelayCommand]
    private void OpenFileLocation()
    {
        if (SelectedProcess == null || string.IsNullOrEmpty(SelectedProcess.Path)) return;
        _service.OpenFileLocation(SelectedProcess.Path);
    }
    
    [RelayCommand]
    private void SearchGoogle()
    {
        if (SelectedProcess == null) return;
        _service.SearchGoogle(SelectedProcess.Name);
    }
}

