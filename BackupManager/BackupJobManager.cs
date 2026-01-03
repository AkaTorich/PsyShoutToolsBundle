using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackupManager.Models;
using BackupManager.Services;

namespace BackupManager.Services
{
    public class BackupJobManager
    {
        private readonly ILogger _logger;
        private readonly string _jobsFilePath;
        private List<BackupJob> _backupJobs = new List<BackupJob>();

        public BackupJobManager(ILogger logger)
        {
            _logger = logger;
            _jobsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BackupManager",
                "backup-jobs.json");

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_jobsFilePath));

            LoadJobs();
        }

        public void AddJob(BackupJob job)
        {
            _backupJobs.Add(job);
            SaveJobs();
            _logger.Log($"Добавлено новое задание резервного копирования: {job.SourceDirectory} -> {job.BackupDirectory}");
        }

        public void RemoveJob(BackupJob job)
        {
            _backupJobs.Remove(job);
            SaveJobs();
            _logger.Log($"Удалено задание резервного копирования: {job.SourceDirectory} -> {job.BackupDirectory}");
        }

        public List<BackupJob> GetAllJobs()
        {
            return _backupJobs.ToList();
        }

        public List<BackupJob> GetCompletedJobs()
        {
            return _backupJobs.Where(j => j.CompletedAt.HasValue).ToList();
        }

        public List<BackupJob> GetSuccessfulJobs()
        {
            return _backupJobs.Where(j => j.CompletedAt.HasValue && j.IsSuccessful).ToList();
        }

        public List<BackupJob> GetFailedJobs()
        {
            return _backupJobs.Where(j => j.CompletedAt.HasValue && !j.IsSuccessful).ToList();
        }

        private void LoadJobs()
        {
            try
            {
                if (File.Exists(_jobsFilePath))
                {
                    string json = File.ReadAllText(_jobsFilePath);
                    _backupJobs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BackupJob>>(json) ?? new List<BackupJob>();
                    _logger.Log($"Загружено {_backupJobs.Count} заданий резервного копирования из истории");
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при загрузке истории заданий: {ex.Message}", LogLevel.Error);
                _backupJobs = new List<BackupJob>();
            }
        }

        private void SaveJobs()
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(_backupJobs, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_jobsFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка при сохранении истории заданий: {ex.Message}", LogLevel.Error);
            }
        }
    }
}