//MemoryLogger
using System.Collections.Generic;
using BackupManager.Models;

namespace BackupManager.Services
{
    public class MemoryLogger : ILogger
    {
        private List<LogEntry> _logs = new List<LogEntry>();

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            _logs.Add(new LogEntry(message, level));
        }

        public List<LogEntry> GetLogs()
        {
            return _logs;
        }
    }
}