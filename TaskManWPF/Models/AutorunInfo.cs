namespace TaskManWPF.Models;

public enum AutorunSource
{
    RegistryHKCU,
    RegistryHKLM,
    StartupFolder,
    Service,
    ScheduledTask
}

public class AutorunInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public AutorunSource Source { get; set; }
    public string SourceDetails { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool IsSigned { get; set; }
    public string RegistryKey { get; set; } = string.Empty;
    public string RegistryValue { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public long FileSize { get; set; }
    
    public string SourceDisplay => Source switch
    {
        AutorunSource.RegistryHKCU => "Реестр (HKCU)",
        AutorunSource.RegistryHKLM => "Реестр (HKLM)",
        AutorunSource.StartupFolder => "Папка автозагрузки",
        AutorunSource.Service => "Служба Windows",
        AutorunSource.ScheduledTask => "Планировщик задач",
        _ => "Неизвестно"
    };
    
    public string StatusDisplay => Enabled ? "✓ Включено" : "✗ Отключено";
    public string SignedDisplay => IsSigned ? "✓ Подписан" : "✗ Не подписан";
}

