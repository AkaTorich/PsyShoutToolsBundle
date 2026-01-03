using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Services
{
    public class BackupScheduler : IDisposable
    {
        private readonly ILogger _logger;
        private readonly FileSystemManager _fileManager;
        private readonly BackupComparer _backupComparer;
        private readonly List<ScheduledBackupJob> _scheduledJobs = new List<ScheduledBackupJob>();
        private readonly Dictionary<string, System.Timers.Timer> _jobTimers = new Dictionary<string, System.Timers.Timer>();
        private readonly string _scheduleFilePath;
        private bool _disposed = false;

        public event EventHandler<BackupJobEventArgs> BackupCompleted;
        public event EventHandler<BackupJobEventArgs> BackupStarted;
        public event EventHandler<BackupJobEventArgs> BackupFailed;

        public BackupScheduler(ILogger logger)
        {
            _logger = logger;
            _fileManager = new FileSystemManager();
            _backupComparer = new BackupComparer(_logger);
            _scheduleFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BackupManager",
                "schedules.json");

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_scheduleFilePath));

            LoadSchedules();
        }

        public void AddScheduledJob(ScheduledBackupJob job)
        {
            _logger.Log($"Добавление задания резервного копирования: {job.Name} ({job.SourceDirectory} -> {job.BackupDirectory})");

            // Generate a unique ID if none is provided
            if (string.IsNullOrEmpty(job.Id))
            {
                job.Id = Guid.NewGuid().ToString();
            }

            // Remove any existing job with the same ID
            RemoveScheduledJob(job.Id);

            _scheduledJobs.Add(job);
            ScheduleJob(job);
            SaveSchedules();
        }

        public List<ScheduledBackupJob> GetScheduledJobs()
        {
            return _scheduledJobs.ToList();
        }

        public void RemoveScheduledJob(string jobId)
        {
            var job = _scheduledJobs.FirstOrDefault(j => j.Id == jobId);
            if (job != null)
            {
                _logger.Log($"Удаление задания резервного копирования: {job.Name}");
                _scheduledJobs.Remove(job);

                if (_jobTimers.ContainsKey(jobId))
                {
                    var timer = _jobTimers[jobId];
                    timer.Stop();
                    timer.Dispose();
                    _jobTimers.Remove(jobId);
                }

                SaveSchedules();
            }
        }

        public void UpdateScheduledJob(ScheduledBackupJob job)
        {
            RemoveScheduledJob(job.Id);
            AddScheduledJob(job);
        }

        public void RunJobNow(string jobId)
        {
            var job = _scheduledJobs.FirstOrDefault(j => j.Id == jobId);
            if (job != null)
            {
                _logger.Log($"Ручной запуск задания резервного копирования: {job.Name}");
                ExecuteBackupJob(job);
            }
            else
            {
                _logger.Log($"Задание с ID {jobId} не найдено", LogLevel.Warning);
            }
        }

        private void ScheduleJob(ScheduledBackupJob job)
        {
            if (!job.Enabled)
            {
                _logger.Log($"Задание {job.Name} отключено, не планируется");
                return;
            }

            var timer = new System.Timers.Timer();

            // Calculate the initial interval
            DateTime nextRun = CalculateNextRun(job);
            TimeSpan initialInterval = nextRun - DateTime.Now;

            if (initialInterval.TotalMilliseconds <= 0)
            {
                // If next run time is in the past, schedule for the next occurrence
                nextRun = CalculateNextRun(job, DateTime.Now.AddMinutes(1));
                initialInterval = nextRun - DateTime.Now;
            }

            _logger.Log($"Задание {job.Name} запланировано на: {nextRun}");

            timer.Interval = initialInterval.TotalMilliseconds;
            timer.AutoReset = false;
            timer.Elapsed += (sender, e) => OnTimerElapsed(job, timer);
            timer.Start();

            // Store the timer
            if (_jobTimers.ContainsKey(job.Id))
            {
                _jobTimers[job.Id].Dispose();
            }
            _jobTimers[job.Id] = timer;
        }

        private DateTime CalculateNextRun(ScheduledBackupJob job, DateTime? fromTime = null)
        {
            DateTime baseTime = fromTime ?? DateTime.Now;

            switch (job.ScheduleType)
            {
                case ScheduleType.Hourly:
                    return new DateTime(baseTime.Year, baseTime.Month, baseTime.Day,
                        baseTime.Hour, job.Minute, 0).AddHours(
                        baseTime.Minute >= job.Minute ? 1 : 0);

                case ScheduleType.Daily:
                    var dailyTime = new DateTime(baseTime.Year, baseTime.Month, baseTime.Day,
                        job.Hour, job.Minute, 0);
                    return baseTime > dailyTime ? dailyTime.AddDays(1) : dailyTime;

                case ScheduleType.Weekly:
                    var dayOfWeek = (int)job.DayOfWeek;
                    var currentDayOfWeek = (int)baseTime.DayOfWeek;
                    var daysUntilTargetDay = (dayOfWeek - currentDayOfWeek + 7) % 7;

                    var weeklyTime = new DateTime(baseTime.Year, baseTime.Month, baseTime.Day,
                        job.Hour, job.Minute, 0).AddDays(daysUntilTargetDay);

                    if (daysUntilTargetDay == 0 && baseTime > weeklyTime)
                    {
                        weeklyTime = weeklyTime.AddDays(7);
                    }

                    return weeklyTime;

                case ScheduleType.Monthly:
                    var day = Math.Min(job.Day, DateTime.DaysInMonth(baseTime.Year, baseTime.Month));
                    var monthlyTime = new DateTime(baseTime.Year, baseTime.Month, day,
                        job.Hour, job.Minute, 0);

                    if (baseTime > monthlyTime)
                    {
                        var nextMonth = baseTime.AddMonths(1);
                        day = Math.Min(job.Day, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                        monthlyTime = new DateTime(nextMonth.Year, nextMonth.Month, day,
                            job.Hour, job.Minute, 0);
                    }

                    return monthlyTime;

                case ScheduleType.Once:
                    return job.ScheduledTime;

                case ScheduleType.Interval:
                    TimeSpan interval = TimeSpan.FromMinutes(job.IntervalMinutes);
                    return baseTime.Add(interval);

                default:
                    throw new NotImplementedException($"Schedule type {job.ScheduleType} not implemented");
            }
        }

        private void OnTimerElapsed(ScheduledBackupJob job, System.Timers.Timer timer)
        {
            Task.Run(() =>
            {
                ExecuteBackupJob(job);

                // Schedule the next run if it's a recurring job
                if (job.ScheduleType != ScheduleType.Once)
                {
                    DateTime nextRun = CalculateNextRun(job);
                    TimeSpan nextInterval = nextRun - DateTime.Now;

                    // Ensure we don't have a negative interval
                    if (nextInterval.TotalMilliseconds <= 0)
                    {
                        nextRun = CalculateNextRun(job, DateTime.Now.AddMinutes(1));
                        nextInterval = nextRun - DateTime.Now;
                    }

                    _logger.Log($"Следующий запуск задания {job.Name} запланирован на: {nextRun}");

                    timer.Interval = nextInterval.TotalMilliseconds;
                    timer.Start();
                }
                else
                {
                    // For one-time jobs, remove from scheduler after execution
                    job.Enabled = false;
                    SaveSchedules();
                }
            });
        }

        private void ExecuteBackupJob(ScheduledBackupJob job)
        {
            _logger.Log($"Начало выполнения задания резервного копирования: {job.Name}, Тип синхронизации: {job.GetSyncTypeDescription()}");

            var backupJob = new BackupJob(job.SourceDirectory, job.BackupDirectory);
            // Создаем отдельный логгер для этого задания
            var jobLogger = new MemoryLogger();
            jobLogger.Log($"Начато выполнение задания: {job.Name}");

            try
            {
                OnBackupStarted(backupJob);

                // Ensure the directories exist
                if (!Directory.Exists(job.SourceDirectory))
                {
                    throw new DirectoryNotFoundException($"Исходная директория {job.SourceDirectory} не найдена");
                }

                if (!Directory.Exists(job.BackupDirectory))
                {
                    Directory.CreateDirectory(job.BackupDirectory);
                }

                bool success = false;

                switch (job.SyncType)
                {
                    case SyncType.Full:
                        // Полная синхронизация (копирование + удаление устаревших файлов)
                        success = _fileManager.SyncDirectories(job.SourceDirectory, job.BackupDirectory, jobLogger);
                        break;

                    case SyncType.Incremental:
                        // Инкрементальное копирование
                        var incrementalComparer = new BackupComparer(jobLogger);
                        incrementalComparer.ScanSourceDirectory(job.SourceDirectory);
                        incrementalComparer.ScanBackupDirectory(job.BackupDirectory);
                        var differences = incrementalComparer.CompareDirectories();

                        // Копируем только измененные или новые файлы
                        int copiedFiles = 0;
                        int errors = 0;

                        foreach (var diff in differences.Where(d =>
                            d.DifferenceType == DifferenceType.ContentDifferent ||
                            d.DifferenceType == DifferenceType.MissingInBackup))
                        {
                            try
                            {
                                string sourcePath = Path.Combine(job.SourceDirectory, diff.RelativePath);
                                string destPath = Path.Combine(job.BackupDirectory, diff.RelativePath);

                                string destDir = Path.GetDirectoryName(destPath);
                                if (!Directory.Exists(destDir))
                                {
                                    Directory.CreateDirectory(destDir);
                                }

                                File.Copy(sourcePath, destPath, true);
                                jobLogger.Log($"Скопирован файл: {diff.RelativePath}");
                                copiedFiles++;
                            }
                            catch (Exception ex)
                            {
                                jobLogger.Log($"Ошибка при копировании файла {diff.RelativePath}: {ex.Message}", LogLevel.Error);
                                errors++;
                            }
                        }

                        jobLogger.Log($"Инкрементальное копирование выполнено. Скопировано файлов: {copiedFiles}, ошибок: {errors}");
                        success = errors == 0;
                        break;

                    case SyncType.Decremental:
                        // Зеркальная синхронизация (аналогично Full)
                        success = _fileManager.SyncDirectories(job.SourceDirectory, job.BackupDirectory, jobLogger);
                        break;

                    case SyncType.TwoWay:
                        // Односторонняя синхронизация (аналогично Full)
                        success = _fileManager.SyncDirectories(job.SourceDirectory, job.BackupDirectory, jobLogger);
                        break;
                }

                backupJob.IsSuccessful = success;
                backupJob.CompletedAt = DateTime.Now;

                if (success)
                {
                    _logger.Log($"Задание резервного копирования {job.Name} успешно выполнено");
                    jobLogger.Log($"Задание резервного копирования {job.Name} успешно выполнено");
                    job.LastRunSuccessful = true;
                    job.LastRunTime = DateTime.Now;
                    SaveSchedules();

                    // Сохраняем логи этого задания
                    backupJob.Logs = jobLogger.GetLogs();
                    OnBackupCompleted(backupJob);
                }
                else
                {
                    _logger.Log($"Задание резервного копирования {job.Name} завершилось с ошибками", LogLevel.Error);
                    jobLogger.Log($"Задание резервного копирования {job.Name} завершилось с ошибками", LogLevel.Error);
                    job.LastRunSuccessful = false;
                    job.LastRunTime = DateTime.Now;
                    SaveSchedules();

                    // Сохраняем логи этого задания
                    backupJob.Logs = jobLogger.GetLogs();
                    OnBackupFailed(backupJob);
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при выполнении задания {job.Name}: {ex.Message}", LogLevel.Error);
                jobLogger.Log($"Ошибка при выполнении задания {job.Name}: {ex.Message}", LogLevel.Error);

                backupJob.IsSuccessful = false;
                backupJob.CompletedAt = DateTime.Now;

                job.LastRunSuccessful = false;
                job.LastRunTime = DateTime.Now;
                SaveSchedules();

                // Сохраняем логи этого задания
                backupJob.Logs = jobLogger.GetLogs();
                OnBackupFailed(backupJob);
            }
        }
        private void LoadSchedules()
        {
            try
            {
                if (File.Exists(_scheduleFilePath))
                {
                    string json = File.ReadAllText(_scheduleFilePath);
                    var jobs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ScheduledBackupJob>>(json);

                    if (jobs != null)
                    {
                        _scheduledJobs.Clear();
                        _scheduledJobs.AddRange(jobs);

                        foreach (var job in _scheduledJobs)
                        {
                            if (job.Enabled)
                            {
                                ScheduleJob(job);
                            }
                        }

                        _logger.Log($"Загружено {_scheduledJobs.Count} заданий резервного копирования");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при загрузке расписаний: {ex.Message}", LogLevel.Error);
            }
        }

        private void SaveSchedules()
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_scheduledJobs, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_scheduleFilePath, json);
                _logger.Log($"Сохранено {_scheduledJobs.Count} заданий резервного копирования");
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при сохранении расписаний: {ex.Message}", LogLevel.Error);
            }
        }

        protected virtual void OnBackupCompleted(BackupJob job)
        {
            BackupCompleted?.Invoke(this, new BackupJobEventArgs(job));
        }

        protected virtual void OnBackupStarted(BackupJob job)
        {
            BackupStarted?.Invoke(this, new BackupJobEventArgs(job));
        }

        protected virtual void OnBackupFailed(BackupJob job)
        {
            BackupFailed?.Invoke(this, new BackupJobEventArgs(job));
        }

        // Удаление пустых папок
        private void RemoveEmptyDirectories(string directory, ILogger logger)
        {
            try
            {
                var excludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "$RECYCLE.BIN",
                    "System Volume Information",
                    "Recovery",
                    "ProgramData\\Microsoft\\Windows\\WER",
                    "Windows\\CSC",
                    "boot",
                    "Config.Msi"
                };

                foreach (string subDir in Directory.GetDirectories(directory))
                {
                    string folderName = Path.GetFileName(subDir);

                    // Не трогаем исключенные папки
                    if (excludedFolders.Contains(folderName))
                    {
                        continue;
                    }

                    // Рекурсивно обрабатываем подпапки
                    RemoveEmptyDirectories(subDir, logger);

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var timer in _jobTimers.Values)
                    {
                        timer.Stop();
                        timer.Dispose();
                    }
                    _jobTimers.Clear();
                }

                _disposed = true;
            }
        }

        ~BackupScheduler()
        {
            Dispose(false);
        }
    }

    public class BackupJobEventArgs : EventArgs
    {
        public BackupJob Job { get; }

        public BackupJobEventArgs(BackupJob job)
        {
            Job = job;
        }
    }
}