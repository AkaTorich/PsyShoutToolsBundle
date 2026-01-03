//BackupJob
using System;
using System.Collections.Generic;

namespace BackupManager.Models
{
    public class BackupJob
    {
        public string SourceDirectory { get; set; }
        public string BackupDirectory { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsSuccessful { get; set; }
        public List<LogEntry> Logs { get; set; }

        public BackupJob(string sourceDirectory, string backupDirectory)
        {
            SourceDirectory = sourceDirectory;
            BackupDirectory = backupDirectory;
            CreatedAt = DateTime.Now;
            Logs = new List<LogEntry>();
        }
    }
}