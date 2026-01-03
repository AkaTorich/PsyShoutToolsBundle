using System.Diagnostics;
using TaskManWPF.Models;
using Microsoft.Win32.TaskScheduler;
using TaskState = TaskManWPF.Models.TaskState;

namespace TaskManWPF.Services;

public class TaskSchedulerService
{
    public List<ScheduledTaskInfo> GetAllTasks()
    {
        var result = new List<ScheduledTaskInfo>();
        
        try
        {
            using var ts = new Microsoft.Win32.TaskScheduler.TaskService();
            EnumerateTasks(ts.RootFolder, result);
        }
        catch { }
        
        return result.OrderBy(t => t.Name).ToList();
    }
    
    private void EnumerateTasks(TaskFolder folder, List<ScheduledTaskInfo> result)
    {
        try
        {
            foreach (var task in folder.Tasks)
            {
                try
                {
                    var info = new ScheduledTaskInfo
                    {
                        Name = task.Name,
                        Path = task.Path,
                        Description = task.Definition.RegistrationInfo.Description ?? string.Empty,
                        Author = task.Definition.RegistrationInfo.Author ?? string.Empty,
                        State = ConvertState(task.State),
                        LastRunTime = task.LastRunTime == DateTime.MinValue ? null : task.LastRunTime,
                        NextRunTime = task.NextRunTime == DateTime.MinValue ? null : task.NextRunTime,
                        LastRunResult = task.LastTaskResult
                    };
                    
                    // Get action info
                    if (task.Definition.Actions.Count > 0)
                    {
                        var action = task.Definition.Actions[0];
                        if (action is ExecAction execAction)
                        {
                            info.ActionPath = execAction.Path ?? string.Empty;
                            info.Arguments = execAction.Arguments ?? string.Empty;
                        }
                    }
                    
                    // Get trigger info
                    var triggers = task.Definition.Triggers
                        .Select(GetTriggerDescription)
                        .Where(t => !string.IsNullOrEmpty(t));
                    info.Triggers = string.Join(", ", triggers);
                    
                    result.Add(info);
                }
                catch { }
            }
            
            foreach (var subFolder in folder.SubFolders)
            {
                EnumerateTasks(subFolder, result);
            }
        }
        catch { }
    }
    
    private TaskState ConvertState(Microsoft.Win32.TaskScheduler.TaskState state)
    {
        return state switch
        {
            Microsoft.Win32.TaskScheduler.TaskState.Disabled => TaskState.Disabled,
            Microsoft.Win32.TaskScheduler.TaskState.Queued => TaskState.Queued,
            Microsoft.Win32.TaskScheduler.TaskState.Ready => TaskState.Ready,
            Microsoft.Win32.TaskScheduler.TaskState.Running => TaskState.Running,
            _ => TaskState.Unknown
        };
    }
    
    private string GetTriggerDescription(Trigger trigger)
    {
        return trigger switch
        {
            DailyTrigger => "Ежедневно",
            WeeklyTrigger => "Еженедельно",
            MonthlyTrigger => "Ежемесячно",
            LogonTrigger => "При входе",
            BootTrigger => "При загрузке",
            IdleTrigger => "При простое",
            TimeTrigger t => $"Однократно: {t.StartBoundary:dd.MM.yyyy HH:mm}",
            EventTrigger => "По событию",
            _ => trigger.TriggerType.ToString()
        };
    }
    
    public bool EnableTask(string taskPath, bool enable)
    {
        try
        {
            using var ts = new Microsoft.Win32.TaskScheduler.TaskService();
            var task = ts.GetTask(taskPath);
            if (task == null) return false;
            
            task.Enabled = enable;
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public bool DeleteTask(string taskPath)
    {
        try
        {
            using var ts = new Microsoft.Win32.TaskScheduler.TaskService();
            ts.RootFolder.DeleteTask(taskPath, false);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public bool RunTask(string taskPath)
    {
        try
        {
            using var ts = new Microsoft.Win32.TaskScheduler.TaskService();
            var task = ts.GetTask(taskPath);
            task?.Run();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public bool StopTask(string taskPath)
    {
        try
        {
            using var ts = new Microsoft.Win32.TaskScheduler.TaskService();
            var task = ts.GetTask(taskPath);
            task?.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

