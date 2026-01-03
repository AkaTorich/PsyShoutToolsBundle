namespace TaskManWPF.Models;

public enum TaskState
{
    Unknown,
    Disabled,
    Queued,
    Ready,
    Running
}

public class ScheduledTaskInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public TaskState State { get; set; }
    public string ActionPath { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string Triggers { get; set; } = string.Empty;
    public DateTime? LastRunTime { get; set; }
    public DateTime? NextRunTime { get; set; }
    public int LastRunResult { get; set; }
    
    public string StateDisplay => State switch
    {
        TaskState.Disabled => "Отключено",
        TaskState.Queued => "В очереди",
        TaskState.Ready => "Готово",
        TaskState.Running => "Выполняется",
        _ => "Неизвестно"
    };
    
    public string LastRunDisplay => LastRunTime?.ToString("dd.MM.yyyy HH:mm") ?? "Никогда";
    public string NextRunDisplay => NextRunTime?.ToString("dd.MM.yyyy HH:mm") ?? "—";
}

