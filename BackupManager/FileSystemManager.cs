using System;
using System.IO;
using System.Collections.Generic;
using BackupManager.Services;

namespace BackupManager.Services
{
    public class FileSystemManager
    {
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

        // Копирование файлов и папок (только добавление/обновление)
        public bool CopyDirectory(string sourceDir, string targetDir, ILogger logger)
        {
            try
            {
                // Создаем директорию назначения, если она не существует
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                    logger.Log($"Создана директория: {targetDir}");
                }

                // Копируем все файлы
                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(targetDir, fileName);
                        File.Copy(file, destFile, true);
                        logger.Log($"Скопирован файл: {fileName}");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        logger.Log($"Нет доступа к файлу: {file}", LogLevel.Warning);
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Ошибка при копировании файла {file}: {ex.Message}", LogLevel.Error);
                    }
                }

                // Рекурсивно копируем поддиректории
                foreach (string subDir in Directory.GetDirectories(sourceDir))
                {
                    string folderName = Path.GetFileName(subDir);

                    // Проверяем, не исключена ли папка
                    if (ExcludedFolders.Contains(folderName))
                    {
                        logger.Log($"Пропущена системная папка: {subDir}", LogLevel.Warning);
                        continue;
                    }

                    try
                    {
                        string subDirName = Path.GetFileName(subDir);
                        string destSubDir = Path.Combine(targetDir, subDirName);
                        CopyDirectory(subDir, destSubDir, logger);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        logger.Log($"Нет доступа к папке: {subDir}", LogLevel.Warning);
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Ошибка при копировании папки {subDir}: {ex.Message}", LogLevel.Error);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Log($"Ошибка при копировании: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        // Полная синхронизация директорий (копирование + удаление лишних файлов)
        public bool SyncDirectories(string sourceDir, string targetDir, ILogger logger)
        {
            try
            {
                // Сначала копируем/обновляем файлы
                bool copySuccess = CopyDirectory(sourceDir, targetDir, logger);
                if (!copySuccess)
                {
                    return false;
                }

                // Теперь удаляем файлы и папки, которых нет в источнике
                DeleteObsoleteItems(sourceDir, targetDir, logger);

                return true;
            }
            catch (Exception ex)
            {
                logger.Log($"Ошибка при синхронизации: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        // Удаление файлов и папок, которых нет в источнике
        private void DeleteObsoleteItems(string sourceDir, string targetDir, ILogger logger)
        {
            try
            {
                // Удаляем лишние файлы
                foreach (string targetFile in Directory.GetFiles(targetDir))
                {
                    string fileName = Path.GetFileName(targetFile);
                    string sourceFile = Path.Combine(sourceDir, fileName);

                    if (!File.Exists(sourceFile))
                    {
                        try
                        {
                            File.Delete(targetFile);
                            logger.Log($"Удален устаревший файл: {fileName}");
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Ошибка при удалении файла {fileName}: {ex.Message}", LogLevel.Error);
                        }
                    }
                }

                // Удаляем лишние папки
                foreach (string targetSubDir in Directory.GetDirectories(targetDir))
                {
                    string folderName = Path.GetFileName(targetSubDir);
                    string sourceSubDir = Path.Combine(sourceDir, folderName);

                    // Проверяем исключенные папки
                    if (ExcludedFolders.Contains(folderName))
                    {
                        continue;
                    }

                    if (!Directory.Exists(sourceSubDir))
                    {
                        try
                        {
                            Directory.Delete(targetSubDir, true);
                            logger.Log($"Удалена устаревшая папка: {folderName}");
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Ошибка при удалении папки {folderName}: {ex.Message}", LogLevel.Error);
                        }
                    }
                    else
                    {
                        // Рекурсивно обрабатываем подпапки
                        DeleteObsoleteItems(sourceSubDir, targetSubDir, logger);
                    }
                }

                // Удаляем пустые папки в target
                DeleteEmptyDirectories(targetDir, logger);
            }
            catch (Exception ex)
            {
                logger.Log($"Ошибка при очистке устаревших элементов: {ex.Message}", LogLevel.Error);
            }
        }

        // Удаление пустых папок
        private void DeleteEmptyDirectories(string directory, ILogger logger)
        {
            try
            {
                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    string folderName = Path.GetFileName(subDir);

                    // Не трогаем исключенные папки
                    if (ExcludedFolders.Contains(folderName))
                    {
                        continue;
                    }

                    // Рекурсивно обрабатываем подпапки
                    DeleteEmptyDirectories(subDir, logger);

                    // Если папка пустая, удаляем её
                    if (Directory.GetFiles(subDir).Length == 0 && Directory.GetDirectories(subDir).Length == 0)
                    {
                        try
                        {
                            Directory.Delete(subDir);
                            logger.Log($"Удалена пустая папка: {folderName}");
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Ошибка при удалении пустой папки {folderName}: {ex.Message}", LogLevel.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Ошибка при удалении пустых папок: {ex.Message}", LogLevel.Error);
            }
        }

        // Восстановление отдельного файла
        public bool RestoreFile(string sourcePath, string destinationPath, ILogger logger)
        {
            try
            {
                // Создаем директорию назначения, если она не существует
                string destDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                    logger.Log($"Создана директория: {destDir}");
                }

                // Копируем файл
                File.Copy(sourcePath, destinationPath, true);
                logger.Log($"Восстановлен файл: {Path.GetFileName(destinationPath)}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Log($"Ошибка при восстановлении файла: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
    }
}