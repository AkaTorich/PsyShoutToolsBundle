using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TaskManWPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;
    
    [ObservableProperty]
    private string _currentPage = "Processes";
    
    public ProcessesViewModel ProcessesVM { get; }
    public AutorunViewModel AutorunVM { get; }
    public TaskSchedulerViewModel TaskSchedulerVM { get; }
    
    public MainViewModel()
    {
        ProcessesVM = new ProcessesViewModel();
        AutorunVM = new AutorunViewModel();
        TaskSchedulerVM = new TaskSchedulerViewModel();
        
        CurrentView = ProcessesVM;
    }
    
    [RelayCommand]
    private void NavigateToProcesses()
    {
        CurrentPage = "Processes";
        CurrentView = ProcessesVM;
        ProcessesVM.RefreshCommand.Execute(null);
    }
    
    [RelayCommand]
    private void NavigateToAutorun()
    {
        CurrentPage = "Autorun";
        CurrentView = AutorunVM;
        AutorunVM.RefreshCommand.Execute(null);
    }
    
    [RelayCommand]
    private void NavigateToTasks()
    {
        CurrentPage = "Tasks";
        CurrentView = TaskSchedulerVM;
        TaskSchedulerVM.RefreshCommand.Execute(null);
    }
}

