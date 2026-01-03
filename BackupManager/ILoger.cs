//ILoger
using System.Collections.Generic;
using BackupManager.Models;

namespace BackupManager.Services
{
    public interface ILogger
    {
        void Log(string message, LogLevel level = LogLevel.Info);
        List<LogEntry> GetLogs();
    }
}