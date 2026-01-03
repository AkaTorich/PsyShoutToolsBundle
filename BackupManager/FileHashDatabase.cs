//FileHashDatabase
using System.Collections.Generic;
using System.Linq;
using BackupManager.Models;

namespace BackupManager.Services
{
    public class FileHashDatabase
    {
        private Dictionary<string, BackupFileInfo> _fileInfos = new Dictionary<string, BackupFileInfo>();

        // Добавление информации о файле
        public void AddFileInfo(BackupFileInfo fileInfo)
        {
            _fileInfos[fileInfo.RelativePath] = fileInfo;
        }

        // Получение информации о файле по относительному пути
        public BackupFileInfo GetFileInfo(string relativePath)
        {
            if (_fileInfos.ContainsKey(relativePath))
                return _fileInfos[relativePath];
            return null;
        }

        // Получение всех файлов
        public List<BackupFileInfo> GetAllFiles()
        {
            return _fileInfos.Values.ToList();
        }

        // Очистка базы
        public void Clear()
        {
            _fileInfos.Clear();
        }
    }
}