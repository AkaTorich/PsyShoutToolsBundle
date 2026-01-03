using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TaskManWPF.Models;

public class ProcessInfo : INotifyPropertyChanged
{
    private double _cpuUsage;
    private long _memoryMB;

    public int Pid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    
    public double CpuUsage
    {
        get => _cpuUsage;
        set { _cpuUsage = value; OnPropertyChanged(); }
    }
    
    public long MemoryMB
    {
        get => _memoryMB;
        set { _memoryMB = value; OnPropertyChanged(); }
    }
    
    public bool IsSigned { get; set; }
    public DateTime StartTime { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

