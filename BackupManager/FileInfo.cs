//FileInfo
using System;

namespace BackupManager.Models
{
    public class BackupFileInfo
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }
}