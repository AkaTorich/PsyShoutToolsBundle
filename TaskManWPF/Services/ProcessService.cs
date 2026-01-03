using System.Diagnostics;
using System.IO;
using System.Management;
using System.Security.Cryptography.X509Certificates;
using TaskManWPF.Models;

namespace TaskManWPF.Services;

public class ProcessService
{
    private readonly Dictionary<int, PerformanceCounter> _cpuCounters = new();
    
    public List<ProcessInfo> GetAllProcesses()
    {
        var result = new List<ProcessInfo>();
        
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var info = new ProcessInfo
                {
                    Pid = process.Id,
                    Name = process.ProcessName,
                    MemoryMB = process.WorkingSet64 / (1024 * 1024)
                };
                
                try
                {
                    info.Path = process.MainModule?.FileName ?? string.Empty;
                    info.StartTime = process.StartTime;
                    
                    if (!string.IsNullOrEmpty(info.Path))
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(info.Path);
                        info.Description = versionInfo.FileDescription ?? string.Empty;
                        info.Company = versionInfo.CompanyName ?? string.Empty;
                        info.IsSigned = IsFileSigned(info.Path);
                    }
                }
                catch { }
                
                result.Add(info);
            }
            catch { }
        }
        
        return result.OrderBy(p => p.Name).ToList();
    }
    
    public bool TerminateProcess(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            process.Kill();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public bool TerminateProcessTree(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            KillProcessTree(process);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private void KillProcessTree(Process process)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT * FROM Win32_Process WHERE ParentProcessId = {process.Id}");
            
            foreach (ManagementObject obj in searcher.Get())
            {
                try
                {
                    var childProcess = Process.GetProcessById(Convert.ToInt32(obj["ProcessId"]));
                    KillProcessTree(childProcess);
                }
                catch { }
            }
            
            process.Kill();
        }
        catch { }
    }
    
    public void OpenFileLocation(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
        
        Process.Start("explorer.exe", $"/select,\"{path}\"");
    }
    
    public void SearchGoogle(string processName)
    {
        var url = $"https://www.google.com/search?q={Uri.EscapeDataString(processName + " process")}";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    
    private bool IsFileSigned(string filePath)
    {
        try
        {
            var cert = X509Certificate.CreateFromSignedFile(filePath);
            return cert != null;
        }
        catch
        {
            return false;
        }
    }
}

