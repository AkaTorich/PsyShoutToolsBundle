using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskManWPF.Models;
using TaskManWPF.Services;

namespace TaskManWPF.ViewModels;

public partial class TaskSchedulerViewModel : ObservableObject
{
    private readonly TaskSchedulerService _service = new();
    
    [ObservableProperty]
    private ObservableCollection<ScheduledTaskInfo> _tasks = new();
    
    [ObservableProperty]
    private ScheduledTaskInfo? _selectedTask;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string _statusText = "Готово";
    
    [ObservableProperty]
    private int _taskCount;
    
    private List<ScheduledTaskInfo> _allTasks = new();
    
    public TaskSchedulerViewModel()
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
            ? _allTasks
            : _allTasks.Where(t => 
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Path.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                t.Author.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        
        Tasks = new ObservableCollection<ScheduledTaskInfo>(filtered);
        TaskCount = Tasks.Count;
    }
    
    [RelayCommand]
    private void Refresh()
    {
        StatusText = "Загрузка задач...";
        
        Task.Run(() =>
        {
            var tasks = _service.GetAllTasks();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                _allTasks = tasks;
                ApplyFilter();
                StatusText = $"Найдено {_allTasks.Count} задач";
            });
        });
    }
    
    [RelayCommand]
    private void EnableTask()
    {
        if (SelectedTask == null) return;
        
        if (_service.EnableTask(SelectedTask.Path, true))
        {
            StatusText = $"Задача {SelectedTask.Name} включена";
            Refresh();
        }
        else
        {
            MessageBox.Show("Не удалось включить задачу", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void DisableTask()
    {
        if (SelectedTask == null) return;
        
        if (_service.EnableTask(SelectedTask.Path, false))
        {
            StatusText = $"Задача {SelectedTask.Name} отключена";
            Refresh();
        }
        else
        {
            MessageBox.Show("Не удалось отключить задачу", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void DeleteTask()
    {
        if (SelectedTask == null) return;
        
        var result = MessageBox.Show(
            $"Удалить задачу \"{SelectedTask.Name}\"?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            if (_service.DeleteTask(SelectedTask.Path))
            {
                StatusText = $"Задача {SelectedTask.Name} удалена";
                Refresh();
            }
            else
            {
                MessageBox.Show("Не удалось удалить задачу", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    [RelayCommand]
    private void RunTask()
    {
        if (SelectedTask == null) return;
        
        if (_service.RunTask(SelectedTask.Path))
        {
            StatusText = $"Задача {SelectedTask.Name} запущена";
        }
        else
        {
            MessageBox.Show("Не удалось запустить задачу", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void StopTask()
    {
        if (SelectedTask == null) return;
        
        if (_service.StopTask(SelectedTask.Path))
        {
            StatusText = $"Задача {SelectedTask.Name} остановлена";
            Refresh();
        }
        else
        {
            MessageBox.Show("Не удалось остановить задачу", "Ошибка", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void ShowProperties()
    {
        if (SelectedTask == null) return;
        
        var info = $"Имя: {SelectedTask.Name}\n" +
                   $"Путь: {SelectedTask.Path}\n" +
                   $"Описание: {SelectedTask.Description}\n" +
                   $"Автор: {SelectedTask.Author}\n" +
                   $"Состояние: {SelectedTask.StateDisplay}\n" +
                   $"Действие: {SelectedTask.ActionPath} {SelectedTask.Arguments}\n" +
                   $"Триггеры: {SelectedTask.Triggers}\n" +
                   $"Последний запуск: {SelectedTask.LastRunDisplay}\n" +
                   $"Следующий запуск: {SelectedTask.NextRunDisplay}\n" +
                   $"Результат: 0x{SelectedTask.LastRunResult:X8}";
        
        MessageBox.Show(info, "Свойства задачи", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

