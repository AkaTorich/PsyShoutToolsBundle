//DifferenceType.cs
namespace BackupManager.Models
{
    public enum DifferenceType
    {
        ContentDifferent,    // Содержимое отличается
        MissingInSource,     // Отсутствует в исходной директории
        MissingInBackup      // Отсутствует в резервной копии
    }
}