using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Management;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using Microsoft.Win32;
using System.Threading;
using System.Security.Principal;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Globalization;

namespace LicensedApplication
{
    /// <summary>
    /// Класс для управления лицензированием программного обеспечения
    /// </summary>
    public class LicenseManager
    {
        #region Константы и приватные поля

        // Ключ шифрования для лицензионных данных (в реальном приложении смените его)
        private static readonly byte[] AesKey = new byte[32]
        {
            0x42, 0x1A, 0x86, 0xE5, 0x3D, 0xC1, 0x5B, 0x9F,
            0x78, 0x2E, 0x4D, 0x8C, 0x36, 0xA7, 0xF9, 0x02,
            0x63, 0xB4, 0x19, 0xD7, 0x51, 0x8F, 0x0A, 0xE3,
            0x74, 0xC5, 0x2B, 0x96, 0x4E, 0x3F, 0xD8, 0x1C
        };

        // Вектор инициализации для AES шифрования
        private static readonly byte[] AesIV = new byte[16]
        {
            0x3A, 0xF1, 0x84, 0x2D, 0xB9, 0x6C, 0x05, 0xE7,
            0x47, 0x98, 0x1F, 0xA2, 0xD5, 0x83, 0x6B, 0xC0
        };

        // Соль для хеширования аппаратного идентификатора
        private static readonly byte[] HashSalt = new byte[16]
        {
            0x59, 0xF3, 0x8A, 0x2C, 0xD1, 0x7B, 0x4E, 0x6F,
            0x92, 0x0D, 0xA4, 0xB8, 0x35, 0xC6, 0xE0, 0x17
        };

        // Путь к файлу лицензии (относительно каталога приложения)
        private const string LicenseFileName = "license.dat";

        // Название продукта
        private const string ProductName = "YourProductName";

        // URL сервера активации (замените на ваш сервер)
        private const string ActivationServerUrl = "https://your-activation-server.com/activate";

        // Ключи реестра
        private const string RegistryKeyPath = @"SOFTWARE\PsyShout\YourProduct";
        private const string RegistryFirstRunValue = "FirstRun";
        private const string RegistryInstallDateValue = "InstallDate";
        private const string RegistrySystemInfo = "SystemInfo";
        private const string RegistryDebuggerDetectedFlag = "DebuggerDetected"; // Флаг обнаружения отладчика

        // Статус лицензии
        private static LicenseStatus _licenseStatus = LicenseStatus.NotInitialized;
        private static LicenseInfo _currentLicense = null;
        private static string _licenseError = string.Empty;
        private static string _hardwareId = string.Empty;

        #endregion

        #region Перечисления и Классы

        /// <summary>
        /// Типы лицензий
        /// </summary>
        public enum LicenseType
        {
            Trial = 0,
            Full = 1
        }

        /// <summary>
        /// Статусы лицензии
        /// </summary>
        public enum LicenseStatus
        {
            NotInitialized = 0,
            Valid = 1,
            Expired = 2,
            Invalid = 3,
            Blacklisted = 4,
            HardwareChanged = 5,
            TrialExpired = 6,
            NoLicense = 7
        }

        /// <summary>
        /// Информация о лицензии
        /// </summary>
        public class LicenseInfo
        {
            public string LicenseKey { get; set; }
            public string UserName { get; set; }
            public string UserEmail { get; set; }
            public string CompanyName { get; set; }
            public LicenseType Type { get; set; }
            public DateTime IssueDate { get; set; }
            public DateTime ExpirationDate { get; set; }
            public string HardwareId { get; set; }
            public bool AllowHardwareChange { get; set; }
            public int MaximumInstances { get; set; }
            public bool AllowUpdates { get; set; }
            public string[] Features { get; set; }
            public string ProductVersion { get; set; }
            public string Notes { get; set; }
            public string ValidationToken { get; set; }
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Инициализирует систему лицензирования
        /// </summary>
        /// <returns>Статус лицензии</returns>
        public static LicenseStatus Initialize()
        {
            try
            {
                // Проверяем защиту от отладки
                if (AntiDebug.CheckForDebugger())
                {
                    _licenseStatus = LicenseStatus.Invalid;
                    _licenseError = "Обнаружена попытка отладки приложения";
                    return _licenseStatus;
                }

                // Проверяем флаг "чёрного списка" - если ранее был обнаружен отладчик
                if (IsBlacklisted())
                {
                    _licenseStatus = LicenseStatus.Blacklisted;
                    _licenseError = "Приложение заблокировано из-за попытки отладки";
                    return _licenseStatus;
                }

                // Создаем/проверяем записи в реестре
                InitializeRegistry();

                // Генерируем аппаратный идентификатор
                _hardwareId = GenerateHardwareId();

                // Проверяем наличие лицензионного файла
                string licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LicenseFileName);

                if (File.Exists(licensePath))
                {
                    try
                    {
                        // Загружаем и проверяем лицензию
                        _currentLicense = LoadLicenseFromFile(licensePath);
                        _licenseStatus = ValidateLicense(_currentLicense);
                    }
                    catch (Exception ex)
                    {
                        _licenseStatus = LicenseStatus.Invalid;
                        _licenseError = "Ошибка чтения лицензии: " + ex.Message;
                    }
                }
                else
                {
                    // Проверяем флаг в реестре, запрещающий автоматическую активацию пробной лицензии
                    bool noAutoTrial = false;
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                    {
                        if (key != null)
                        {
                            noAutoTrial = key.GetValue("NoAutoTrial") != null;
                        }
                    }

                    if (noAutoTrial)
                    {
                        _licenseStatus = LicenseStatus.NoLicense;
                        _licenseError = "Требуется активация лицензии";
                    }
                    // Если флага нет, и пробный период валиден - создаем пробную лицензию
                    else if (IsTrialValid())
                    {
                        _licenseStatus = LicenseStatus.Valid;

                        // Создаем лицензию пробного периода
                        _currentLicense = new LicenseInfo
                        {
                            Type = LicenseType.Trial,
                            IssueDate = GetFirstRunDate(),
                            ExpirationDate = GetFirstRunDate().AddDays(30), // 30-дневный пробный период
                            HardwareId = _hardwareId,
                            UserName = "Trial User",
                            Features = new string[] { "all" }
                        };
                    }
                    else
                    {
                        _licenseStatus = LicenseStatus.TrialExpired;
                        _licenseError = "Пробный период истек";
                    }
                }

                return _licenseStatus;
            }
            catch (Exception ex)
            {
                _licenseStatus = LicenseStatus.Invalid;
                _licenseError = "Ошибка инициализации лицензии: " + ex.Message;
                return _licenseStatus;
            }
        }

        /// <summary>
        /// Активирует лицензию по ключу
        /// </summary>
        /// <param name="licenseKey">Лицензионный ключ</param>
        /// <param name="userName">Имя пользователя</param>
        /// <param name="userEmail">Email пользователя</param>
        /// <returns>Результат активации лицензии</returns>
        public static bool ActivateLicense(string licenseKey, string userName, string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(licenseKey) || licenseKey.Length < 10)
                {
                    _licenseError = "Некорректный лицензионный ключ";
                    return false;
                }

                // Генерируем аппаратный ID
                string hardwareId = GenerateHardwareId();

                // Формируем данные для отправки на сервер активации
                string activationData = string.Format(
                    "key={0}&hwid={1}&user={2}&email={3}&product={4}&version={5}",
                    Uri.EscapeDataString(licenseKey),
                    Uri.EscapeDataString(hardwareId),
                    Uri.EscapeDataString(userName),
                    Uri.EscapeDataString(userEmail),
                    Uri.EscapeDataString(ProductName),
                    Uri.EscapeDataString(Application.ProductVersion)
                );

                // Здесь должен быть запрос к серверу активации
                // В этом примере мы симулируем успешный ответ от сервера

                /* 
                 * В реальном приложении это должно выглядеть примерно так:
                 * 
                 * using (WebClient client = new WebClient())
                 * {
                 *     client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                 *     string responseData = client.UploadString(ActivationServerUrl, activationData);
                 *     // Далее - разбор ответа сервера
                 * }
                 */

                // Для демонстрации создаем тестовую лицензию
                LicenseInfo license = new LicenseInfo
                {
                    LicenseKey = licenseKey,
                    UserName = userName,
                    UserEmail = userEmail,
                    CompanyName = "",
                    Type = LicenseType.Full,
                    IssueDate = DateTime.Now,
                    ExpirationDate = DateTime.Now.AddYears(1), // Лицензия на 1 год
                    HardwareId = hardwareId,
                    AllowHardwareChange = false,
                    MaximumInstances = 1,
                    AllowUpdates = true,
                    Features = new string[] { "all" },
                    ProductVersion = Application.ProductVersion,
                    Notes = "Activated online",
                    ValidationToken = GenerateValidationToken(licenseKey, hardwareId)
                };

                // Сохраняем лицензию в файл
                if (SaveLicenseToFile(license))
                {
                    _currentLicense = license;
                    _licenseStatus = LicenseStatus.Valid;

                    // При успешной активации удаляем флаг NoAutoTrial из реестра
                    try
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                        {
                            if (key != null && key.GetValue("NoAutoTrial") != null)
                            {
                                key.DeleteValue("NoAutoTrial");
                            }
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки при удалении флага
                        // Лицензия все равно активирована успешно
                    }

                    return true;
                }
                else
                {
                    _licenseError = "Ошибка сохранения лицензии";
                    return false;
                }
            }
            catch (Exception ex)
            {
                _licenseError = "Ошибка активации: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Деактивирует текущую лицензию
        /// </summary>
        /// <returns>true, если деактивация прошла успешно</returns>
        public static bool DeactivateLicense()
        {
            try
            {
                // Удаляем файл лицензии
                string licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LicenseFileName);
                if (File.Exists(licensePath))
                {
                    File.Delete(licensePath);
                }

                // Записываем в реестр флаг, запрещающий автоматическую активацию пробной лицензии
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("NoAutoTrial", "1");
                    }
                }

                _currentLicense = null;
                _licenseStatus = LicenseStatus.NoLicense;
                return true;
            }
            catch (Exception ex)
            {
                _licenseError = "Ошибка деактивации: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Проверяет валидность лицензии, включая проверку срока действия
        /// </summary>
        /// <returns>true, если лицензия действительна</returns>
        public static bool IsLicenseValid()
        {
            return GetLicenseStatus() == LicenseStatus.Valid &&
                   _currentLicense != null &&
                   DateTime.Now <= _currentLicense.ExpirationDate;
        }
        /// <summary>
        /// Проверяет, имеет ли пользователь доступ к определенной функции
        /// </summary>
        /// <param name="featureName">Название функции</param>
        /// <returns>true, если функция доступна по лицензии</returns>
        public static bool IsFeatureEnabled(string featureName)
        {
            if (_licenseStatus != LicenseStatus.Valid || _currentLicense == null)
                return false;

            // В пробном режиме доступны все функции
            if (_currentLicense.Type == LicenseType.Trial)
                return true;

            // В полной версии доступны все функции
            if (_currentLicense.Type == LicenseType.Full)
                return true;

            // Проверка конкретных функций по массиву Features
            if (_currentLicense.Features != null &&
                (_currentLicense.Features.Contains(featureName.ToLower()) ||
                 _currentLicense.Features.Contains("all")))
                return true;

            return false;
        }

        /// <summary>
        /// Получает текущую информацию о лицензии для отображения
        /// </summary>
        /// <returns>Информация о лицензии в виде строки</returns>
        public static string GetLicenseInfo()
        {
            if (_currentLicense == null)
                return "Лицензия отсутствует";

            StringBuilder info = new StringBuilder();
            info.AppendLine($"Тип лицензии: {GetLicenseTypeName(_currentLicense.Type)}");
            info.AppendLine($"Пользователь: {_currentLicense.UserName}");

            if (!string.IsNullOrEmpty(_currentLicense.UserEmail))
                info.AppendLine($"Email: {_currentLicense.UserEmail}");

            if (!string.IsNullOrEmpty(_currentLicense.CompanyName))
                info.AppendLine($"Компания: {_currentLicense.CompanyName}");

            if (_currentLicense.Type != LicenseType.Trial)
                info.AppendLine($"Ключ: {MaskLicenseKey(_currentLicense.LicenseKey)}");

            info.AppendLine($"Дата выдачи: {_currentLicense.IssueDate.ToShortDateString()}");
            info.AppendLine($"Действительна до: {_currentLicense.ExpirationDate.ToShortDateString()}");

            int daysLeft = (int)(_currentLicense.ExpirationDate - DateTime.Now).TotalDays;
            if (daysLeft > 0)
                info.AppendLine($"Осталось дней: {daysLeft}");
            else
                info.AppendLine("Лицензия истекла");

            return info.ToString();
        }

        /// <summary>
        /// Возвращает текущий статус лицензии
        /// </summary>
        public static LicenseStatus GetLicenseStatus()
        {
            return _licenseStatus;
        }

        /// <summary>
        /// Возвращает последнюю ошибку лицензирования
        /// </summary>
        public static string GetLastError()
        {
            return _licenseError;
        }

        /// <summary>
        /// Получает оставшееся количество дней действия лицензии/пробного периода
        /// </summary>
        public static int GetRemainingDays()
        {
            if (_currentLicense == null)
                return 0;

            return Math.Max(0, (int)(_currentLicense.ExpirationDate - DateTime.Now).TotalDays);
        }

        /// <summary>
        /// Получает тип текущей лицензии
        /// </summary>
        public static LicenseType GetLicenseType()
        {
            if (_currentLicense == null)
                return LicenseType.Trial;

            return _currentLicense.Type;
        }

        /// <summary>
        /// Получает текстовое описание типа лицензии
        /// </summary>
        public static string GetLicenseTypeName(LicenseType type)
        {
            switch (type)
            {
                case LicenseType.Trial: return "Пробная версия";
                case LicenseType.Full: return "Полная версия";
                default: return "Неизвестно";
            }
        }

        /// <summary>
        /// Получает уникальный идентификатор оборудования
        /// </summary>
        public static string GetHardwareId()
        {
            return _hardwareId;
        }

        #endregion

        #region Система чёрного списка для защиты от отладки

        /// <summary>
        /// Проверяет, находится ли приложение в чёрном списке (был ли обнаружен отладчик ранее)
        /// </summary>
        public static bool IsBlacklisted()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        // Проверяем наличие флага обнаружения отладчика
                        string debuggerFlag = key.GetValue(RegistryDebuggerDetectedFlag) as string;
                        
                        if (!string.IsNullOrEmpty(debuggerFlag))
                        {
                            // Проверяем, что флаг установлен для текущего аппаратного ID
                            string currentHwId = GenerateHardwareId();
                            return debuggerFlag.Contains(currentHwId);
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Отзывает лицензию и добавляет в чёрный список из-за обнаружения отладчика
        /// </summary>
        public static void RevokeLicenseForDebugging()
        {
            try
            {
                // 1. Деактивируем текущую лицензию
                DeactivateLicense();

                // 2. Удаляем лицензионный файл
                string licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LicenseFileName);
                if (File.Exists(licensePath))
                {
                    try
                    {
                        File.Delete(licensePath);
                    }
                    catch { } // Игнорируем ошибки удаления файла
                }

                // 3. Устанавливаем флаг в реестре
                SetBlacklistFlag();

                // 4. Логируем событие
                string logMessage = $"{DateTime.Now}: Лицензия отозвана из-за обнаружения отладчика. Машина: {Environment.MachineName}, Пользователь: {Environment.UserName}\r\n";
                try
                {
                    File.AppendAllText("license_revoked.log", logMessage);
                }
                catch { } // Игнорируем ошибки записи лога

                // 5. Обновляем статус
                _licenseStatus = LicenseStatus.Blacklisted;
                _licenseError = "Лицензия отозвана из-за попытки отладки";
            }
            catch (Exception ex)
            {
                // Логируем ошибку отзыва лицензии
                try
                {
                    File.AppendAllText("license_revoke_error.log", 
                        $"{DateTime.Now}: Ошибка отзыва лицензии: {ex.Message}\r\n");
                }
                catch { }
            }
        }

        /// <summary>
        /// Устанавливает флаг чёрного списка в реестре
        /// </summary>
        private static void SetBlacklistFlag()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        // Создаём уникальный флаг с информацией о системе
                        string hardwareId = GenerateHardwareId();
                        string flagValue = $"{DateTime.Now:yyyy-MM-dd_HH:mm:ss}|{hardwareId}|{Environment.MachineName}|{Environment.UserName}";
                        
                        // Шифруем флаг для усложнения взлома
                        string encryptedFlag = EncryptString(flagValue);
                        
                        key.SetValue(RegistryDebuggerDetectedFlag, encryptedFlag);
                        
                        // Дополнительно устанавливаем флаг запрета автоматической пробной лицензии
                        key.SetValue("NoAutoTrial", "1");
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Очищает флаг чёрного списка (только для административного использования)
        /// </summary>
        public static bool ClearBlacklistFlag(string adminPassword)
        {
            // Простая защита административной функции
            if (adminPassword != "AdminReset2025!")
                return false;

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key != null)
                    {
                        if (key.GetValue(RegistryDebuggerDetectedFlag) != null)
                        {
                            key.DeleteValue(RegistryDebuggerDetectedFlag);
                        }
                        
                        if (key.GetValue("NoAutoTrial") != null)
                        {
                            key.DeleteValue("NoAutoTrial");
                        }
                    }
                }

                // Логируем административное действие
                File.AppendAllText("admin_actions.log", 
                    $"{DateTime.Now}: Флаг чёрного списка очищен администратором\r\n");

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Генерирует уникальный идентификатор оборудования
        /// </summary>
        private static string GenerateHardwareId()
        {
            try
            {
                // Собираем данные об оборудовании
                StringBuilder sb = new StringBuilder();

                // CPU ID
                string cpuId = GetCpuId();
                sb.Append(cpuId);

                // BIOS Serial
                string biosSerial = GetBiosSerial();
                sb.Append(biosSerial);

                // Motherboard Serial
                string mbSerial = GetMotherboardSerial();
                sb.Append(mbSerial);

                // MAC-адрес первой сетевой карты
                string macAddress = GetMacAddress();
                sb.Append(macAddress);

                // Системный диск
                string diskId = GetDiskId();
                sb.Append(diskId);

                // Хешируем собранные данные
                using (SHA256 sha = SHA256.Create())
                {
                    // Добавляем соль к данным
                    byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
                    byte[] dataWithSalt = new byte[data.Length + HashSalt.Length];
                    Buffer.BlockCopy(data, 0, dataWithSalt, 0, data.Length);
                    Buffer.BlockCopy(HashSalt, 0, dataWithSalt, data.Length, HashSalt.Length);

                    // Хешируем
                    byte[] hash = sha.ComputeHash(dataWithSalt);

                    // Преобразуем хеш в строку в формате XXXX-XXXX-XXXX-XXXX-XXXX-XXXX
                    return BitConverter.ToString(hash).Replace("-", "").Substring(0, 24)
                        .Insert(4, "-").Insert(9, "-").Insert(14, "-").Insert(19, "-").Insert(24, "-");
                }
            }
            catch
            {
                // В случае ошибки возвращаем хеш от имени компьютера и текущего пользователя
                using (SHA256 sha = SHA256.Create())
                {
                    string fallbackId = Environment.MachineName + Environment.UserName +
                                        Environment.OSVersion.VersionString;
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(fallbackId));
                    return BitConverter.ToString(hash).Replace("-", "").Substring(0, 24)
                        .Insert(4, "-").Insert(9, "-").Insert(14, "-").Insert(19, "-").Insert(24, "-");
                }
            }
        }

        /// <summary>
        /// Получает серийный номер процессора
        /// </summary>
        private static string GetCpuId()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["ProcessorId"].ToString().Trim();
                    }
                }
            }
            catch { }
            return "UNKNOWN_CPU";
        }

        /// <summary>
        /// Получает серийный номер BIOS
        /// </summary>
        private static string GetBiosSerial()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["SerialNumber"].ToString().Trim();
                    }
                }
            }
            catch { }
            return "UNKNOWN_BIOS";
        }

        /// <summary>
        /// Получает серийный номер материнской платы
        /// </summary>
        private static string GetMotherboardSerial()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["SerialNumber"].ToString().Trim();
                    }
                }
            }
            catch { }
            return "UNKNOWN_MB";
        }

        /// <summary>
        /// Получает MAC-адрес первой физической сетевой карты
        /// </summary>
        private static string GetMacAddress()
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Фильтруем виртуальные адаптеры и адаптеры, не подключенные к сети
                    if (nic.OperationalStatus == OperationalStatus.Up &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        PhysicalAddress mac = nic.GetPhysicalAddress();
                        if (mac != null)
                        {
                            return BitConverter.ToString(mac.GetAddressBytes()).Replace("-", "");
                        }
                    }
                }
            }
            catch { }
            return "UNKNOWN_MAC";
        }

        /// <summary>
        /// Получает идентификатор системного диска
        /// </summary>
        private static string GetDiskId()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive WHERE Index=0"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["SerialNumber"].ToString().Trim();
                    }
                }
            }
            catch { }
            return "UNKNOWN_DISK";
        }

        /// <summary>
        /// Инициализирует записи в реестре при первом запуске
        /// </summary>
        private static void InitializeRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
                {
                    if (key == null) return;

                    // Проверяем запись о первом запуске
                    if (key.GetValue(RegistryFirstRunValue) == null)
                    {
                        key.SetValue(RegistryFirstRunValue, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        key.SetValue(RegistryInstallDateValue, DateTime.Now.ToString("yyyy-MM-dd"));
                    }

                    // Обновляем информацию о системе
                    string systemInfo = $"{Environment.MachineName}|{Environment.UserName}|{Environment.OSVersion.VersionString}";
                    key.SetValue(RegistrySystemInfo, EncryptString(systemInfo));
                }
            }
            catch { }
        }

        /// <summary>
        /// Получает дату первого запуска приложения
        /// </summary>
        private static DateTime GetFirstRunDate()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        string value = key.GetValue(RegistryFirstRunValue) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            return DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            catch { }

            return DateTime.Now;
        }

        /// <summary>
        /// Проверяет действителен ли пробный период
        /// </summary>
        private static bool IsTrialValid()
        {
            try
            {
                DateTime firstRunDate = GetFirstRunDate();

                // Проверяем не прошло ли 30 дней с первого запуска
                return (DateTime.Now - firstRunDate).TotalDays <= 30;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Загружает лицензию из файла
        /// </summary>
        private static LicenseInfo LoadLicenseFromFile(string path)
        {
            try
            {
                // Читаем содержимое файла
                byte[] encryptedData = File.ReadAllBytes(path);

                // Дешифруем данные
                string licenseData = DecryptData(encryptedData);

                // Разбираем данные в формате XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(licenseData);

                // Извлекаем информацию о лицензии
                LicenseInfo license = new LicenseInfo();

                license.LicenseKey = doc.SelectSingleNode("//License/Key")?.InnerText;
                license.UserName = doc.SelectSingleNode("//License/UserName")?.InnerText;
                license.UserEmail = doc.SelectSingleNode("//License/UserEmail")?.InnerText;
                license.CompanyName = doc.SelectSingleNode("//License/CompanyName")?.InnerText;

                string licenseTypeStr = doc.SelectSingleNode("//License/Type")?.InnerText;
                if (Enum.TryParse(licenseTypeStr, out LicenseType licenseType))
                    license.Type = licenseType;

                string issueDateStr = doc.SelectSingleNode("//License/IssueDate")?.InnerText;
                if (DateTime.TryParse(issueDateStr, out DateTime issueDate))
                    license.IssueDate = issueDate;

                string expirationDateStr = doc.SelectSingleNode("//License/ExpirationDate")?.InnerText;
                if (DateTime.TryParse(expirationDateStr, out DateTime expirationDate))
                    license.ExpirationDate = expirationDate;

                license.HardwareId = doc.SelectSingleNode("//License/HardwareId")?.InnerText;

                string allowHardwareChangeStr = doc.SelectSingleNode("//License/AllowHardwareChange")?.InnerText;
                if (bool.TryParse(allowHardwareChangeStr, out bool allowHardwareChange))
                    license.AllowHardwareChange = allowHardwareChange;

                string maxInstancesStr = doc.SelectSingleNode("//License/MaximumInstances")?.InnerText;
                if (int.TryParse(maxInstancesStr, out int maxInstances))
                    license.MaximumInstances = maxInstances;

                string allowUpdatesStr = doc.SelectSingleNode("//License/AllowUpdates")?.InnerText;
                if (bool.TryParse(allowUpdatesStr, out bool allowUpdates))
                    license.AllowUpdates = allowUpdates;

                XmlNodeList featureNodes = doc.SelectNodes("//License/Features/Feature");
                if (featureNodes != null && featureNodes.Count > 0)
                {
                    string[] features = new string[featureNodes.Count];
                    for (int i = 0; i < featureNodes.Count; i++)
                    {
                        features[i] = featureNodes[i].InnerText;
                    }
                    license.Features = features;
                }

                license.ProductVersion = doc.SelectSingleNode("//License/ProductVersion")?.InnerText;
                license.Notes = doc.SelectSingleNode("//License/Notes")?.InnerText;
                license.ValidationToken = doc.SelectSingleNode("//License/ValidationToken")?.InnerText;

                return license;
            }
            catch (Exception ex)
            {
                _licenseError = "Ошибка чтения лицензии: " + ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Сохраняет лицензию в файл
        /// </summary>
        private static bool SaveLicenseToFile(LicenseInfo license)
        {
            try
            {
                // Создаем XML документ
                XmlDocument doc = new XmlDocument();
                XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(xmlDeclaration);

                XmlElement root = doc.CreateElement("License");
                doc.AppendChild(root);

                AddXmlElement(doc, root, "Key", license.LicenseKey);
                AddXmlElement(doc, root, "UserName", license.UserName);
                AddXmlElement(doc, root, "UserEmail", license.UserEmail);
                AddXmlElement(doc, root, "CompanyName", license.CompanyName);
                AddXmlElement(doc, root, "Type", license.Type.ToString());
                AddXmlElement(doc, root, "IssueDate", license.IssueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                AddXmlElement(doc, root, "ExpirationDate", license.ExpirationDate.ToString("yyyy-MM-dd HH:mm:ss"));
                AddXmlElement(doc, root, "HardwareId", license.HardwareId);
                AddXmlElement(doc, root, "AllowHardwareChange", license.AllowHardwareChange.ToString());
                AddXmlElement(doc, root, "MaximumInstances", license.MaximumInstances.ToString());
                AddXmlElement(doc, root, "AllowUpdates", license.AllowUpdates.ToString());

                XmlElement featuresElement = doc.CreateElement("Features");
                root.AppendChild(featuresElement);

                if (license.Features != null)
                {
                    foreach (string feature in license.Features)
                    {
                        AddXmlElement(doc, featuresElement, "Feature", feature);
                    }
                }

                AddXmlElement(doc, root, "ProductVersion", license.ProductVersion);
                AddXmlElement(doc, root, "Notes", license.Notes);
                AddXmlElement(doc, root, "ValidationToken", license.ValidationToken);

                // Получаем XML строку
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlTextWriter xw = new XmlTextWriter(sw))
                    {
                        doc.WriteTo(xw);
                        string xmlString = sw.ToString();

                        // Шифруем и сохраняем в файл
                        byte[] encryptedData = EncryptData(xmlString);
                        File.WriteAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LicenseFileName), encryptedData);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _licenseError = "Ошибка сохранения лицензии: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Добавляет элемент в XML документ
        /// </summary>
        private static void AddXmlElement(XmlDocument doc, XmlElement parent, string name, string value)
        {
            XmlElement element = doc.CreateElement(name);
            element.InnerText = value ?? string.Empty;
            parent.AppendChild(element);
        }

        /// <summary>
        /// Валидирует лицензию
        /// </summary>
        private static LicenseStatus ValidateLicense(LicenseInfo license)
        {
            if (license == null)
                return LicenseStatus.NoLicense;

            // Проверяем дату истечения
            if (DateTime.Now > license.ExpirationDate)
            {
                _licenseError = "Срок действия лицензии истек";
                return license.Type == LicenseType.Trial ? LicenseStatus.TrialExpired : LicenseStatus.Expired;
            }

            // Проверяем аппаратный идентификатор
            if (!string.IsNullOrEmpty(license.HardwareId) && !license.AllowHardwareChange)
            {
                string currentHardwareId = GenerateHardwareId();
                if (license.HardwareId != currentHardwareId)
                {
                    _licenseError = "Лицензия не соответствует данному компьютеру";
                    return LicenseStatus.HardwareChanged;
                }
            }

            // Проверяем валидационный токен
            string expectedToken = GenerateValidationToken(license.LicenseKey, license.HardwareId);
            if (license.ValidationToken != expectedToken)
            {
                _licenseError = "Лицензия повреждена или подделана";
                return LicenseStatus.Invalid;
            }

            return LicenseStatus.Valid;
        }

        /// <summary>
        /// Генерирует токен валидации для лицензии
        /// </summary>
        private static string GenerateValidationToken(string licenseKey, string hardwareId)
        {
            using (HMACSHA256 hmac = new HMACSHA256(AesKey))
            {
                string data = licenseKey + "|" + hardwareId + "|" + ProductName;
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Шифрует данные с помощью AES
        /// </summary>
        private static byte[] EncryptData(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = AesKey;
                aes.IV = AesIV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(plainText);
                        }

                        return memoryStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Дешифрует данные с помощью AES
        /// </summary>
        private static string DecryptData(byte[] cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = AesKey;
                aes.IV = AesIV;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(cipherText))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cryptoStream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Шифрует строку
        /// </summary>
        private static string EncryptString(string text)
        {
            byte[] data = EncryptData(text);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Маскирует лицензионный ключ для показа пользователю
        /// </summary>
        private static string MaskLicenseKey(string licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey) || licenseKey.Length < 8)
                return "****-****-****";

            return licenseKey.Substring(0, 4) + "-****-" + licenseKey.Substring(licenseKey.Length - 4);
        }

        #endregion
    }
}