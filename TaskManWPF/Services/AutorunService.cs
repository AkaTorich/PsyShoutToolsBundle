using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using TaskManWPF.Models;

namespace TaskManWPF.Services;

public class AutorunService
{
    private readonly string[] _runKeys = 
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run"
    };

    public List<AutorunInfo> GetAllAutoruns()
    {
        var result = new List<AutorunInfo>();
        
        // Registry HKCU
        foreach (var keyPath in _runKeys)
        {
            ReadRegistryRun(Registry.CurrentUser, keyPath, AutorunSource.RegistryHKCU, result);
        }
        
        // Registry HKLM
        foreach (var keyPath in _runKeys)
        {
            ReadRegistryRun(Registry.LocalMachine, keyPath, AutorunSource.RegistryHKLM, result);
        }
        
        // Startup folders
        ReadStartupFolder(Environment.SpecialFolder.Startup, result);
        ReadStartupFolder(Environment.SpecialFolder.CommonStartup, result);
        
        // Services
        ReadServices(result);
        
        return result;
    }
    
    private void ReadRegistryRun(RegistryKey root, string keyPath, AutorunSource source, List<AutorunInfo> result)
    {
        try
        {
            using var key = root.OpenSubKey(keyPath);
            if (key == null) return;
            
            foreach (var valueName in key.GetValueNames())
            {
                try
                {
                    var value = key.GetValue(valueName)?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(value)) continue;
                    
                    var (path, args) = ParseCommandLine(value);
                    
                    var info = new AutorunInfo
                    {
                        Name = valueName,
                        Path = path,
                        Arguments = args,
                        Source = source,
                        SourceDetails = $"{(root == Registry.CurrentUser ? "HKCU" : "HKLM")}\\{keyPath}",
                        RegistryKey = keyPath,
                        RegistryValue = valueName,
                        Enabled = true
                    };
                    
                    FillFileInfo(info);
                    result.Add(info);
                }
                catch { }
            }
        }
        catch { }
    }
    
    private void ReadStartupFolder(Environment.SpecialFolder folder, List<AutorunInfo> result)
    {
        try
        {
            var path = Environment.GetFolderPath(folder);
            if (!Directory.Exists(path)) return;
            
            foreach (var file in Directory.GetFiles(path))
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    
                    var info = new AutorunInfo
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Path = file,
                        Source = AutorunSource.StartupFolder,
                        SourceDetails = path,
                        Enabled = true,
                        LastModified = fileInfo.LastWriteTime,
                        FileSize = fileInfo.Length
                    };
                    
                    // Resolve .lnk files
                    if (file.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        info.Path = ResolveShortcut(file) ?? file;
                    }
                    
                    FillFileInfo(info);
                    result.Add(info);
                }
                catch { }
            }
        }
        catch { }
    }
    
    private void ReadServices(List<AutorunInfo> result)
    {
        try
        {
            foreach (var service in ServiceController.GetServices())
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(
                        $@"SYSTEM\CurrentControlSet\Services\{service.ServiceName}");
                    
                    if (key == null) continue;
                    
                    var startType = (int)(key.GetValue("Start") ?? 4);
                    if (startType > 2) continue; // Only auto-start services
                    
                    var imagePath = key.GetValue("ImagePath")?.ToString() ?? string.Empty;
                    var (path, args) = ParseCommandLine(imagePath);
                    
                    var info = new AutorunInfo
                    {
                        Name = service.ServiceName,
                        Description = service.DisplayName,
                        Path = path,
                        Arguments = args,
                        Source = AutorunSource.Service,
                        SourceDetails = "Windows Service",
                        Enabled = service.Status == ServiceControllerStatus.Running
                    };
                    
                    FillFileInfo(info);
                    result.Add(info);
                }
                catch { }
            }
        }
        catch { }
    }
    
    public bool RemoveAutorun(AutorunInfo item)
    {
        try
        {
            switch (item.Source)
            {
                case AutorunSource.RegistryHKCU:
                    using (var key = Registry.CurrentUser.OpenSubKey(item.RegistryKey, true))
                    {
                        key?.DeleteValue(item.RegistryValue);
                    }
                    return true;
                    
                case AutorunSource.RegistryHKLM:
                    using (var key = Registry.LocalMachine.OpenSubKey(item.RegistryKey, true))
                    {
                        key?.DeleteValue(item.RegistryValue);
                    }
                    return true;
                    
                case AutorunSource.StartupFolder:
                    if (File.Exists(item.Path))
                    {
                        File.Delete(item.Path);
                        return true;
                    }
                    return false;
                    
                case AutorunSource.Service:
                    return RemoveService(item.Name);
                    
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
    
    public bool DisableAutorun(AutorunInfo item)
    {
        try
        {
            switch (item.Source)
            {
                case AutorunSource.RegistryHKCU:
                case AutorunSource.RegistryHKLM:
                    // Move to disabled key
                    var root = item.Source == AutorunSource.RegistryHKCU 
                        ? Registry.CurrentUser : Registry.LocalMachine;
                    
                    using (var srcKey = root.OpenSubKey(item.RegistryKey, true))
                    {
                        if (srcKey == null) return false;
                        
                        var value = srcKey.GetValue(item.RegistryValue);
                        srcKey.DeleteValue(item.RegistryValue);
                        
                        // Save to disabled key
                        var disabledKey = item.RegistryKey.Replace("Run", "RunDisabled");
                        using var dstKey = root.CreateSubKey(disabledKey);
                        dstKey?.SetValue(item.RegistryValue, value ?? string.Empty);
                    }
                    return true;
                    
                case AutorunSource.StartupFolder:
                    var newPath = item.Path + ".disabled";
                    File.Move(item.Path, newPath);
                    return true;
                    
                case AutorunSource.Service:
                    return SetServiceStartType(item.Name, false);
                    
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
    
    private bool RemoveService(string serviceName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"delete \"{serviceName}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(psi);
            process?.WaitForExit(5000);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    
    private bool SetServiceStartType(string serviceName, bool enable)
    {
        try
        {
            var startType = enable ? "auto" : "disabled";
            var psi = new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = $"config \"{serviceName}\" start= {startType}",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(psi);
            process?.WaitForExit(5000);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
    
    private (string path, string args) ParseCommandLine(string cmdLine)
    {
        if (string.IsNullOrEmpty(cmdLine)) return (string.Empty, string.Empty);
        
        cmdLine = cmdLine.Trim();
        
        if (cmdLine.StartsWith("\""))
        {
            var endQuote = cmdLine.IndexOf('"', 1);
            if (endQuote > 0)
            {
                var path = cmdLine[1..endQuote];
                var args = endQuote + 1 < cmdLine.Length 
                    ? cmdLine[(endQuote + 1)..].Trim() 
                    : string.Empty;
                return (path, args);
            }
        }
        
        var spaceIndex = cmdLine.IndexOf(' ');
        if (spaceIndex > 0)
        {
            return (cmdLine[..spaceIndex], cmdLine[(spaceIndex + 1)..]);
        }
        
        return (cmdLine, string.Empty);
    }
    
    private void FillFileInfo(AutorunInfo info)
    {
        if (string.IsNullOrEmpty(info.Path) || !File.Exists(info.Path)) return;
        
        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(info.Path);
            info.Description = string.IsNullOrEmpty(info.Description) 
                ? versionInfo.FileDescription ?? string.Empty 
                : info.Description;
            info.Company = versionInfo.CompanyName ?? string.Empty;
            info.IsSigned = IsFileSigned(info.Path);
            
            var fileInfo = new FileInfo(info.Path);
            info.LastModified = fileInfo.LastWriteTime;
            info.FileSize = fileInfo.Length;
        }
        catch { }
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
    
    private string? ResolveShortcut(string shortcutPath)
    {
        // Простое чтение .lnk не требуется для базовой функциональности
        // Возвращаем путь к ярлыку как есть
        return shortcutPath;
    }
}

