using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackupManager.Models;

namespace BackupManager.Services
{
    public class BackupComparer
    {
        private FileHasher _hasher = new FileHasher();
        private FileHashDatabase _sourceDb = new FileHashDatabase();
        private FileHashDatabase _backupDb = new FileHashDatabase();
        private ILogger _logger;

        private static readonly HashSet<string> ExcludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "$RECYCLE.BIN",
            "System Volume Information",
            "Recovery",
            "ProgramData\\Microsoft\\Windows\\WER",
            "Windows\\CSC",
            "boot",
            "Config.Msi"
        };

        public BackupComparer(ILogger logger)
        {
            _logger = logger;
        }

        // Сканирование директории и создание базы хешей
        public void ScanDirectory(string directory, FileHashDatabase database, string basePath = "")
        {
            if (string.IsNullOrEmpty(basePath))
                basePath = directory;

            try
            {
                foreach (string file in Directory.GetFiles(directory))
                {
                    try
                    {
                        string relativePath = file.Substring(basePath.Length).TrimStart('\\', '/');
                        string hash = _hasher.CalculateFileHash(file);
                        var fileSystemInfo = new System.IO.FileInfo(file);

                        var fileInfo = new BackupFileInfo
                        {
                            Path = file,
                            RelativePath = relativePath,
                            Hash = hash,
                            Size = fileSystemInfo.Length,
                            LastModified = fileSystemInfo.LastWriteTime
                        };

                        database.AddFileInfo(fileInfo);
                        _logger.Log($"Проанализирован файл: {relativePath}, хеш: {hash.Substring(0, 8)}...");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger.Log($"Нет доступа к файлу: {file}", LogLevel.Warning);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Ошибка при анализе файла {file}: {ex.Message}", LogLevel.Error);
                    }
                }

                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    string folderName = Path.GetFileName(subDir);

                    // Проверяем, не исключена ли папка
                    if (ExcludedFolders.Contains(folderName))
                    {
                        _logger.Log($"Пропущена системная папка: {subDir}", LogLevel.Warning);
                        continue;
                    }

                    try
                    {
                        ScanDirectory(subDir, database, basePath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        _logger.Log($"Нет доступа к папке: {subDir}", LogLevel.Warning);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Ошибка при сканировании папки {subDir}: {ex.Message}", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при сканировании директории {directory}: {ex.Message}", LogLevel.Error);
            }
        }

        // Сканирование исходной директории
        public void ScanSourceDirectory(string directory)
        {
            _sourceDb.Clear();
            _logger.Log($"Начало сканирования исходной директории: {directory}");
            ScanDirectory(directory, _sourceDb);
            _logger.Log($"Завершено сканирование исходной директории. Проанализировано файлов: {_sourceDb.GetAllFiles().Count}");
        }

        // Сканирование резервной копии
        public void ScanBackupDirectory(string directory)
        {
            _backupDb.Clear();
            _logger.Log($"Начало сканирования резервной копии: {directory}");
            ScanDirectory(directory, _backupDb);
            _logger.Log($"Завершено сканирование резервной копии. Проанализировано файлов: {_backupDb.GetAllFiles().Count}");
        }

        // Сравнение директорий и получение различий
        public List<FileDifference> CompareDirectories()
        {
            var differences = new List<FileDifference>();
            var sourceFiles = _sourceDb.GetAllFiles();
            var backupFiles = _backupDb.GetAllFiles();

            // Проверяем файлы в исходной директории
            foreach (var sourceFile in sourceFiles)
            {
                var backupFile = _backupDb.GetFileInfo(sourceFile.RelativePath);

                if (backupFile == null)
                {
                    // Файл отсутствует в резервной копии
                    differences.Add(new FileDifference
                    {
                        RelativePath = sourceFile.RelativePath,
                        DifferenceType = DifferenceType.MissingInBackup,
                        SourceFile = sourceFile,
                        BackupFile = null
                    });
                }
                else if (sourceFile.Hash != backupFile.Hash)
                {
                    // Файл отличается
                    differences.Add(new FileDifference
                    {
                        RelativePath = sourceFile.RelativePath,
                        DifferenceType = DifferenceType.ContentDifferent,
                        SourceFile = sourceFile,
                        BackupFile = backupFile
                    });
                }
            }

            // Проверяем файлы в резервной копии, которых нет в исходной директории
            foreach (var backupFile in backupFiles)
            {
                var sourceFile = _sourceDb.GetFileInfo(backupFile.RelativePath);

                if (sourceFile == null)
                {
                    // Файл отсутствует в исходной директории
                    differences.Add(new FileDifference
                    {
                        RelativePath = backupFile.RelativePath,
                        DifferenceType = DifferenceType.MissingInSource,
                        SourceFile = null,
                        BackupFile = backupFile
                    });
                }
            }

            _logger.Log($"Сравнение завершено. Найдено различий: {differences.Count}");
            return differences;
        }
    }
}