//FileDifference.cs
using System;

namespace BackupManager.Models
{
    public class FileDifference
    {
        public string RelativePath { get; set; }
        public DifferenceType DifferenceType { get; set; }
        public BackupFileInfo SourceFile { get; set; }
        public BackupFileInfo BackupFile { get; set; }

        public override string ToString()
        {
            switch (DifferenceType)
            {
                case DifferenceType.ContentDifferent:
                    return $"Различается: {RelativePath}";
                case DifferenceType.MissingInSource:
                    return $"Отсутствует в источнике: {RelativePath}";
                case DifferenceType.MissingInBackup:
                    return $"Отсутствует в резервной копии: {RelativePath}";
                default:
                    return RelativePath;
            }
        }
    }
}