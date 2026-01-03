using System;

namespace BackupManager.Models
{
    public enum ScheduleType
    {
        Once,       // Однократное выполнение
        Hourly,     // Ежечасно
        Daily,      // Ежедневно
        Weekly,     // Еженедельно
        Monthly,    // Ежемесячно
        Interval    // С интервалом в минутах
    }

    public class ScheduledBackupJob
    {
        // Идентификатор задания
        public string Id { get; set; }

        // Название задания
        public string Name { get; set; }

        // Исходная директория
        public string SourceDirectory { get; set; }

        // Директория резервной копии
        public string BackupDirectory { get; set; }

        // Активно ли задание
        public bool Enabled { get; set; } = true;

        // Тип расписания
        public ScheduleType ScheduleType { get; set; }

        // Для однократного запуска и общих настроек времени
        public DateTime ScheduledTime { get; set; }

        // Для ежемесячного расписания - день месяца (1-31)
        public int Day { get; set; } = 1;

        // Для еженедельного расписания - день недели
        public DayOfWeek DayOfWeek { get; set; } = System.DayOfWeek.Monday;

        // Час (0-23)
        public int Hour { get; set; }

        // Минута (0-59)
        public int Minute { get; set; }

        // Для интервального расписания - интервал в минутах
        public int IntervalMinutes { get; set; } = 60;

        // Время последнего запуска
        public DateTime? LastRunTime { get; set; }

        // Успешность последнего запуска
        public bool LastRunSuccessful { get; set; }

        // Тип синхронизации
        public SyncType SyncType { get; set; } = SyncType.Incremental;

        public ScheduledBackupJob()
        {
            Id = Guid.NewGuid().ToString();
            Name = "Новое задание";
            ScheduledTime = DateTime.Now.AddHours(1);
        }

        public string GetScheduleDescription()
        {
            switch (ScheduleType)
            {
                case ScheduleType.Once:
                    return $"Однократно {ScheduledTime:dd.MM.yyyy HH:mm}";

                case ScheduleType.Hourly:
                    return $"Ежечасно в {Minute:D2} минут";

                case ScheduleType.Daily:
                    return $"Ежедневно в {Hour:D2}:{Minute:D2}";

                case ScheduleType.Weekly:
                    return $"{GetDayOfWeekName(DayOfWeek)} в {Hour:D2}:{Minute:D2}";

                case ScheduleType.Monthly:
                    return $"{Day} числа каждого месяца в {Hour:D2}:{Minute:D2}";

                case ScheduleType.Interval:
                    if (IntervalMinutes < 60)
                        return $"Каждые {IntervalMinutes} мин.";
                    else if (IntervalMinutes % 60 == 0)
                        return $"Каждые {IntervalMinutes / 60} ч.";
                    else
                        return $"Каждые {IntervalMinutes / 60} ч. {IntervalMinutes % 60} мин.";

                default:
                    return "Неизвестное расписание";
            }
        }

        public string GetSyncTypeDescription()
        {
            switch (SyncType)
            {
                case SyncType.Full:
                    return "Полная синхронизация";
                case SyncType.Incremental:
                    return "Инкрементальное копирование";
                case SyncType.Decremental:
                    return "Зеркальная синхронизация";
                case SyncType.TwoWay:
                    return "Односторонняя синхронизация";
                default:
                    return "Неизвестный тип";
            }
        }

        private string GetDayOfWeekName(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return "Понедельник";
                case DayOfWeek.Tuesday: return "Вторник";
                case DayOfWeek.Wednesday: return "Среда";
                case DayOfWeek.Thursday: return "Четверг";
                case DayOfWeek.Friday: return "Пятница";
                case DayOfWeek.Saturday: return "Суббота";
                case DayOfWeek.Sunday: return "Воскресенье";
                default: return day.ToString();
            }
        }
    }
}