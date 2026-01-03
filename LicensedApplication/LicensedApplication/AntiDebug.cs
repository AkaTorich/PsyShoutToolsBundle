using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Management;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;

namespace LicensedApplication
{
    /// <summary>
    /// Класс для обнаружения и противодействия отладке приложения
    /// </summary>
    public static class AntiDebug
    {
        // Импорт Windows API функций
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool IsDebuggerPresent();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport("ntdll.dll", SetLastError = true)]
        static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
            ref int processInformation, int processInformationLength, out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Глобальный флаг для хранения результата проверки
        private static bool debuggerDetected = false;

        // Таймер для периодической проверки отладчика
        private static System.Windows.Forms.Timer antiDebugTimer;

        // Telegram Bot настройки (БЕЗ ПАРОЛЕЙ!)
        private static readonly string TELEGRAM_BOT_TOKEN = "7682403083:AAFbljuyyXVk4hjNEzcvzBy7xeba_kkJz8k";
        private static readonly string TELEGRAM_CHAT_ID = "974063951";
        private static readonly bool TELEGRAM_NOTIFICATIONS_ENABLED = true;
        
        // Флаг для предотвращения множественных отправок уведомлений
        private static bool notificationSent = false;
        
        // Флаг для предотвращения множественного применения мер безопасности
        private static bool antiDebugMeasuresApplied = false;
        
        // Объект для синхронизации потоков
        private static readonly object lockObject = new object();
        
        // Время запуска приложения для "разогрева"
        private static DateTime applicationStartTime = DateTime.Now;

        /// <summary>
        /// Инициализирует систему защиты от отладки
        /// </summary>
        public static void Initialize()
        {
            // КРИТИЧЕСКИ ВАЖНО: Настраиваем TLS протоколы для всего приложения
            try
            {
                System.Net.ServicePointManager.Expect100Continue = true;
                System.Net.ServicePointManager.SecurityProtocol = 
                    System.Net.SecurityProtocolType.Tls12 | 
                    System.Net.SecurityProtocolType.Tls11 | 
                    System.Net.SecurityProtocolType.Tls;
                System.Net.ServicePointManager.ServerCertificateValidationCallback = 
                    (sender, certificate, chain, sslPolicyErrors) => true;
                
                System.IO.File.AppendAllText("debug_telegram_info.log", 
                    $"{DateTime.Now}: TLS протоколы инициализированы: {System.Net.ServicePointManager.SecurityProtocol}\r\n");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("debug_telegram_error.log", 
                    $"{DateTime.Now}: Ошибка инициализации TLS: {ex.Message}\r\n");
            }
            
            // Инициализируем и настраиваем таймер антиотладки
            antiDebugTimer = new System.Windows.Forms.Timer();
            antiDebugTimer.Interval = 2000; // Интервал проверки 2 секунды для уменьшения агрессивности
            antiDebugTimer.Tick += AntiDebugTimer_Tick;

            // Запускаем таймер
            antiDebugTimer.Start();

            // Запускаем фоновый поток для обнаружения attach отладчиков
            Thread backgroundThread = new Thread(() => BackgroundAntiDebugCheck())
            {
                IsBackground = true,
                Name = "AntiDebugBackground"
            };
            backgroundThread.Start();

            // Начальная проверка при запуске
            if (CheckForDebugger())
            {
                debuggerDetected = true;
                // Применяем меры безопасности только один раз
                ApplyAntiDebugMeasuresOnce();
            }
        }

        /// <summary>
        /// Обработчик события таймера для периодической проверки
        /// </summary>
        private static void AntiDebugTimer_Tick(object sender, EventArgs e)
        {
            if (CheckForDebugger())
            {
                debuggerDetected = true;
                antiDebugTimer?.Stop();
                
                // Применяем меры безопасности только один раз
                ApplyAntiDebugMeasuresOnce();
            }
        }

        /// <summary>
        /// Фоновая проверка на отладчик в отдельном потоке
        /// </summary>
        public static void BackgroundAntiDebugCheck()
        {
            // Активная фоновая проверка для обнаружения attach отладчиков
            while (true)
            {
                try
                {
                    // Комплексная проверка отладчика включая IDA Pro attach
                    if (CheckForDebugger())
                    {
                        debuggerDetected = true;
                        // Применяем меры безопасности только один раз
                        ApplyAntiDebugMeasuresOnce();
                        break; // Выходим из цикла после обнаружения
                    }

                    // Пауза между проверками
                    Thread.Sleep(3000); // Проверка каждые 3 секунды для уменьшения агрессивности
                }
                catch
                {
                    // В случае ошибки продолжаем проверки
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Комплексная проверка на наличие отладчика
        /// </summary>
        public static bool CheckForDebugger()
        {
            try
            {
                // "Разогрев" - не проверяем отладчик первые 5 секунд после запуска
                if ((DateTime.Now - applicationStartTime).TotalSeconds < 5)
                {
                    return false;
                }
                
                // Проверяем все виды отладчиков с логированием
                bool managedPresent = CheckDebuggerManagedPresent();
                bool unmanagedPresent = CheckDebuggerUnmanagedPresent();
                bool debugPort = CheckDebugPortInformation();
                bool debugProcesses = CheckDebuggerProcesses();
                bool debugWindows = CheckDebuggerWindows();
                bool timingCheck = CheckDebuggerTracerTiming();
                bool modulesCheck = CheckDebuggerModules();
                bool idaAttach = CheckIdaProAttach();

                // Логируем результаты для диагностики
                if (managedPresent || unmanagedPresent || debugPort || debugProcesses || 
                    debugWindows || timingCheck || modulesCheck || idaAttach)
                {
                    string logEntry = $"{DateTime.Now}: ОТЛАДЧИК ОБНАРУЖЕН! " +
                        $"Managed={managedPresent}, Unmanaged={unmanagedPresent}, " +
                        $"DebugPort={debugPort}, Processes={debugProcesses}, " +
                        $"Windows={debugWindows}, Timing={timingCheck}, " +
                        $"Modules={modulesCheck}, IdaAttach={idaAttach}";
                    
                    try
                    {
                        System.IO.File.AppendAllText("antidebug_detection.log", logEntry + "\r\n");
                    }
                    catch { }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.File.AppendAllText("antidebug_error.log", 
                        $"{DateTime.Now}: Ошибка в CheckForDebugger: {ex.Message}\r\n");
                }
                catch { }
            }
            
            return false;
        }

        /// <summary>
        /// 1. Проверка на отладчик через Managed API
        /// </summary>
        private static bool CheckDebuggerManagedPresent()
        {
            try
            {
                // Проверка через .NET API
                if (Debugger.IsAttached)
                    return true;

                // Проверка через Environment
                if (System.Environment.GetEnvironmentVariable("COMPLUS_ZapDisable") == "1" ||
                    System.Environment.GetEnvironmentVariable("COMPlus_JITDebug") == "1")
                    return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 2. Проверка на отладчик через Windows API
        /// </summary>
        private static bool CheckDebuggerUnmanagedPresent()
        {
            try
            {
                // Проверка через IsDebuggerPresent
                if (IsDebuggerPresent())
                    return true;

                // Проверка через CheckRemoteDebuggerPresent
                bool isDebuggerPresent = false;
                if (CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent))
                {
                    if (isDebuggerPresent)
                        return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 3. Проверка на DebugPort через NtQueryInformationProcess
        /// </summary>
        private static bool CheckDebugPortInformation()
        {
            try
            {
                const int ProcessDebugPort = 7;
                int debugPort = 0;
                int returnLength;

                if (NtQueryInformationProcess(Process.GetCurrentProcess().Handle,
                    ProcessDebugPort, ref debugPort, sizeof(int), out returnLength) >= 0)
                {
                    if (debugPort != 0)
                        return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 4. Проверка на запущенные процессы отладчиков
        /// </summary>
        private static bool CheckDebuggerProcesses()
        {
            try
            {
                string[] debuggerProcessNames = new string[]
                {
                    "dnspy", "ida", "ida64", "idag", "idag64", "idaw", "idaw64", "idaq", "idaq64",
                    "idapro", "hexrays", "decompiler", "ollydbg", "x32dbg", "x64dbg", "windbg", 
                    "ilspy", "reflector", "dotpeek", "cheatengine", "immunity", "fiddler",
                    "procmon", "procexp", "wireshark", "apimonitor", "detours", "easyhook",
                    "ghidra", "radare2", "r2", "cutter", "binaryninja", "hopper"
                };

                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    try
                    {
                        string processName = process.ProcessName.ToLower();
                        foreach (string debuggerProcess in debuggerProcessNames)
                        {
                            if (processName.Contains(debuggerProcess))
                                return true;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 5. Проверка на наличие окон отладчиков
        /// </summary>
        private static bool CheckDebuggerWindows()
        {
            try
            {
                string[] debuggerWindows = new string[]
                {
                    "OLLYDBG", "Zeta Debugger", "Rock Debugger", "ObsidianGUI",
                    "WinDbgFrameClass", "idafront", "x32dbg", "x64dbg", "DNSpy",
                    "ILSpy", ".NET Reflector", "JetBrains dotPeek", "Fiddler",
                    "IDA Pro", "Hex-Rays", "DisAsm", "TIdaApplication", "Qt5QWindowIcon",
                    "Ghidra", "Binary Ninja", "Radare2", "Cutter", "Hopper Disassembler"
                };

                foreach (string windowName in debuggerWindows)
                {
                    if (FindWindow(windowName, null) != IntPtr.Zero)
                        return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 6. Проверка времени выполнения (отладчик замедляет выполнение)
        /// </summary>
        private static bool CheckDebuggerTracerTiming()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Более легкая операция для уменьшения ложных срабатываний
                int count = 0;
                for (int i = 0; i < 100000; i++) // Уменьшили нагрузку в 10 раз
                {
                    count += i;
                }

                stopwatch.Stop();

                // Увеличили порог до 1000мс для уменьшения ложных срабатываний
                bool timingAnomaly = stopwatch.ElapsedMilliseconds > 1000; 
                
                if (timingAnomaly)
                {
                    try
                    {
                        System.IO.File.AppendAllText("antidebug_timing.log", 
                            $"{DateTime.Now}: Timing аномалия - {stopwatch.ElapsedMilliseconds}ms\r\n");
                    }
                    catch { }
                }
                
                return timingAnomaly;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 7. Проверка на загруженные модули отладки
        /// </summary>
        private static bool CheckDebuggerModules()
        {
            try
            {
                string[] debuggerModules = new string[]
                {
                    "ntice.dll", "sice.dll", "sicev.dll",
                    "syserdbgmsg.dll", "dbghelp.dll", "msvsmon.dll"
                };

                Process currentProcess = Process.GetCurrentProcess();

                foreach (ProcessModule module in currentProcess.Modules)
                {
                    foreach (string debuggerModule in debuggerModules)
                    {
                        if (module.ModuleName.ToLower() == debuggerModule.ToLower())
                            return true;
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 8. Специальная проверка на IDA Pro attach и другие профессиональные отладчики
        /// </summary>
        private static bool CheckIdaProAttach()
        {
            try
            {
                // Проверяем специфичные для IDA Pro признаки attach
                
                // 1. Проверка через NtQueryInformationProcess с другими флагами
                if (CheckDebugObjectHandle() || CheckDebugFlags())
                    return true;

                // 2. Проверка контрольных сумм критических функций (IDA может модифицировать код)
                if (CheckCodeIntegrity())
                    return true;

                // 3. Проверка времени выполнения критических участков
                if (CheckExecutionTiming())
                    return true;

                // 4. Проверка на подозрительные DLL загруженные в процесс
                if (CheckSuspiciousDlls())
                    return true;

                // 5. Проверка Hardware Breakpoints
                if (CheckHardwareBreakpoints())
                    return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Проверка Debug Object Handle
        /// </summary>
        private static bool CheckDebugObjectHandle()
        {
            try
            {
                const int ProcessDebugObjectHandle = 30;
                int debugObjectHandle = 0;
                int returnLength;

                if (NtQueryInformationProcess(Process.GetCurrentProcess().Handle,
                    ProcessDebugObjectHandle, ref debugObjectHandle, sizeof(int), out returnLength) >= 0)
                {
                    if (debugObjectHandle != 0)
                        return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Проверка Debug Flags
        /// </summary>
        private static bool CheckDebugFlags()
        {
            try
            {
                const int ProcessDebugFlags = 31;
                int debugFlags = 0;
                int returnLength;

                if (NtQueryInformationProcess(Process.GetCurrentProcess().Handle,
                    ProcessDebugFlags, ref debugFlags, sizeof(int), out returnLength) >= 0)
                {
                    // Если флаги отладки не равны 1, возможно присутствует отладчик
                    if (debugFlags != 1)
                        return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Проверка целостности кода (IDA может модифицировать инструкции)
        /// </summary>
        private static bool CheckCodeIntegrity()
        {
            try
            {
                // ВРЕМЕННО ОТКЛЮЧЕНО: эта проверка может быть нестабильной
                // Включать только если действительно нужно
                return false;
                
                /*
                // Вычисляем контрольную сумму критического участка кода
                byte[] currentCode = new byte[16];
                IntPtr methodPtr = typeof(AntiDebug).GetMethod("CheckCodeIntegrity").MethodHandle.GetFunctionPointer();
                Marshal.Copy(methodPtr, currentCode, 0, 16);
                
                // Если первые байты изменены (например, установлены breakpoints), это подозрительно
                // Проверяем на наличие инструкций int3 (0xCC) или других breakpoint опкодов
                foreach (byte b in currentCode)
                {
                    if (b == 0xCC || b == 0xCD) // int3 или int
                    {
                        try
                        {
                            System.IO.File.AppendAllText("antidebug_code_integrity.log", 
                                $"{DateTime.Now}: Код изменен - найден breakpoint опкод 0x{b:X2}\r\n");
                        }
                        catch { }
                        return true;
                    }
                }
                */
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Проверка времени выполнения для обнаружения пошагового выполнения
        /// </summary>
        private static bool CheckExecutionTiming()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                // Выполняем простую операцию
                int result = 0;
                for (int i = 0; i < 1000; i++)
                {
                    result += i * 2;
                }
                
                stopwatch.Stop();
                
                // Увеличили порог до 500мс для уменьшения ложных срабатываний
                bool timingIssue = stopwatch.ElapsedMilliseconds > 500;
                
                if (timingIssue)
                {
                    try
                    {
                        System.IO.File.AppendAllText("antidebug_execution_timing.log", 
                            $"{DateTime.Now}: Execution timing аномалия - {stopwatch.ElapsedMilliseconds}ms\r\n");
                    }
                    catch { }
                }
                
                return timingIssue;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Проверка на подозрительные DLL в процессе
        /// </summary>
        private static bool CheckSuspiciousDlls()
        {
            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                foreach (ProcessModule module in currentProcess.Modules)
                {
                    string moduleName = module.ModuleName.ToLower();
                    
                    // Проверяем только на явно подозрительные DLL отладчиков (убрали системные)
                    if (moduleName.Contains("dbghelp") && !moduleName.Equals("dbghelp.dll") ||
                        moduleName.Contains("dbgcore") ||
                        moduleName.Contains("detours") ||
                        moduleName.Contains("easyhook") ||
                        moduleName.Contains("minhook") ||
                        moduleName.Contains("apimonitor"))
                    {
                        try
                        {
                            System.IO.File.AppendAllText("antidebug_suspicious_dll.log", 
                                $"{DateTime.Now}: Подозрительная DLL - {module.ModuleName}\r\n");
                        }
                        catch { }
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Проверка Hardware Breakpoints через контекст потока
        /// </summary>
        private static bool CheckHardwareBreakpoints()
        {
            try
            {
                // Эта проверка требует более сложной реализации с GetThreadContext
                // Пока используем упрощенную версию
                return false;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Отправка уведомления через Telegram Bot (БЕЗ ПАРОЛЕЙ!)
        /// </summary>
        private static async Task SendTelegramNotificationAsync(string licenseInfo = null)
        {
            // Thread-safe проверка и установка флага
            bool shouldSend = false;
            lock (lockObject)
            {
                if (!notificationSent && TELEGRAM_NOTIFICATIONS_ENABLED)
                {
                    notificationSent = true; // Устанавливаем флаг сразу, чтобы другие потоки не прошли
                    shouldSend = true;
                }
            }
            
            if (!shouldSend)
            {
                System.IO.File.AppendAllText("debug_telegram_info.log", 
                    $"{DateTime.Now}: Пропуск отправки - уведомление уже отправлено или отключено\r\n");
                return;
            }

            try
            {
                // Логируем попытку отправки
                System.IO.File.AppendAllText("debug_telegram_info.log", 
                    $"{DateTime.Now}: Начинаем отправку Telegram уведомления. Токен: {TELEGRAM_BOT_TOKEN.Substring(0, Math.Min(10, TELEGRAM_BOT_TOKEN.Length))}..., Chat ID: {TELEGRAM_CHAT_ID}\r\n");

                // Токен и Chat ID теперь захардкожены в коде - проверка не нужна

                // Собираем информацию о системе
                string systemInfo = GetSystemInformation();
                
                // Получаем серийный номер аппаратуры
                string hardwareInfo = LicenseManager.GetHardwareId();
                
                // Используем переданную информацию о лицензии или получаем текущую
                if (string.IsNullOrEmpty(licenseInfo))
                {
                    licenseInfo = LicenseManager.GetLicenseInfo();
                }
                
                // Формируем сообщение для Telegram
                string message = $@"🚨 *ОБНАРУЖЕНА ПОПЫТКА ОТЛАДКИ*

📅 *Время:* {DateTime.Now:dd.MM.yyyy HH:mm:ss}
💻 *Компьютер:* {Environment.MachineName}
👤 *Пользователь:* {Environment.UserName}
🏠 *Домен:* {Environment.UserDomainName}
⚙️ *ОС:* {Environment.OSVersion}
🔧 *Архитектура:* {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}
🆔 *Hardware ID:* {hardwareInfo}

📄 *Информация о лицензии:*
```{licenseInfo}```

📊 *СИСТЕМА:*
{systemInfo.Replace("•", "▪")}

🔍 *УГРОЗЫ:*
▪ Debugger.IsAttached: {(Debugger.IsAttached ? "✅ ДА" : "❌ НЕТ")}
▪ IsDebuggerPresent API: {(CheckDebuggerUnmanagedPresent() ? "🔴 ДА" : "🟢 НЕТ")}
▪ Debug Port: {(CheckDebugPortInformation() ? "🔴 ДА" : "🟢 НЕТ")}
▪ Процессы отладчиков: {(CheckDebuggerProcesses() ? "🔴 НАЙДЕНЫ" : "🟢 НЕТ")}
▪ Окна отладчиков: {(CheckDebuggerWindows() ? "🔴 НАЙДЕНЫ" : "🟢 НЕТ")}
▪ Модули отладки: {(CheckDebuggerModules() ? "🔴 НАЙДЕНЫ" : "🟢 НЕТ")}
▪ IDA Pro Attach: {(CheckIdaProAttach() ? "🔴 ОБНАРУЖЕН" : "🟢 НЕТ")}
▪ Timing аномалии: {(CheckDebuggerTracerTiming() ? "🔴 ДА" : "🟢 НЕТ")}

⚠️ *ЛИЦЕНЗИЯ ОТОЗВАНА! КОМПЬЮТЕР ЗАБЛОКИРОВАН!*";

                await SendTelegramMessage(message);
                // Флаг notificationSent уже установлен в начале функции
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("debug_telegram_error.log", 
                    $"{DateTime.Now}: Ошибка отправки Telegram: {ex.Message}\r\n");
            }
        }

        /// <summary>
        /// Отправка сообщения через Telegram Bot API
        /// </summary>
        private static async Task SendTelegramMessage(string message)
        {
            const int MAX_ATTEMPTS = 3;
            const int TIMEOUT_SECONDS = 30;
            
            // КРИТИЧЕСКИ ВАЖНО: Настраиваем TLS протоколы для работы с Telegram API
            System.Net.ServicePointManager.Expect100Continue = true;
            System.Net.ServicePointManager.SecurityProtocol = 
                System.Net.SecurityProtocolType.Tls12 | 
                System.Net.SecurityProtocolType.Tls11 | 
                System.Net.SecurityProtocolType.Tls;
            
            // Отключаем проверку SSL сертификатов (для совместимости)
            System.Net.ServicePointManager.ServerCertificateValidationCallback = 
                (sender, certificate, chain, sslPolicyErrors) => true;
            
            System.IO.File.AppendAllText("debug_telegram_info.log", 
                $"{DateTime.Now}: Настроены TLS протоколы: {System.Net.ServicePointManager.SecurityProtocol}\r\n");
            
            // Сначала пробуем с HttpClient, потом с WebClient как fallback
            Exception lastHttpClientException = null;
            
            for (int attempt = 1; attempt <= MAX_ATTEMPTS; attempt++)
            {
                try
                {
                    System.IO.File.AppendAllText("debug_telegram_info.log", 
                        $"{DateTime.Now}: Попытка отправки #{attempt} из {MAX_ATTEMPTS} (HttpClient)\r\n");
                    
                    // Проверяем подключение к интернету
                    if (!await CheckInternetConnection())
                    {
                        System.IO.File.AppendAllText("debug_telegram_error.log", 
                            $"{DateTime.Now}: Нет подключения к интернету (попытка #{attempt})\r\n");
                        
                        if (attempt == MAX_ATTEMPTS)
                        {
                            throw new Exception("Нет подключения к интернету после проверки");
                        }
                        
                        await Task.Delay(2000);
                        continue;
                    }
                    
                    using (var handler = new HttpClientHandler())
                    {
                        // Настраиваем SSL/TLS 
                        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                        
                        // Настройки прокси (если нужно)
                        handler.UseProxy = false;
                        
                        using (var client = new HttpClient(handler))
                        {
                            // Увеличиваем таймаут
                            client.Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS);
                            
                            // Добавляем заголовки для лучшей совместимости
                            client.DefaultRequestHeaders.Clear();
                            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
                            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                            
                            // URL для Telegram Bot API
                            string url = $"https://api.telegram.org/bot{TELEGRAM_BOT_TOKEN}/sendMessage";
                            
                            System.IO.File.AppendAllText("debug_telegram_info.log", 
                                $"{DateTime.Now}: Отправка на URL: {url.Replace(TELEGRAM_BOT_TOKEN, "***TOKEN***")}\r\n");
                            
                            // Параметры запроса
                            var parameters = new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("chat_id", TELEGRAM_CHAT_ID),
                                new KeyValuePair<string, string>("text", message),
                                new KeyValuePair<string, string>("parse_mode", "Markdown"),
                                new KeyValuePair<string, string>("disable_web_page_preview", "true")
                            };

                            System.IO.File.AppendAllText("debug_telegram_info.log", 
                                $"{DateTime.Now}: Параметры подготовлены. Размер сообщения: {message.Length} символов\r\n");

                            // Отправляем POST запрос
                            var content = new FormUrlEncodedContent(parameters);
                            
                            System.IO.File.AppendAllText("debug_telegram_info.log", 
                                $"{DateTime.Now}: Отправляем POST запрос...\r\n");
                            
                            var response = await client.PostAsync(url, content);
                            
                            System.IO.File.AppendAllText("debug_telegram_info.log", 
                                $"{DateTime.Now}: Получен ответ: {response.StatusCode}\r\n");
                            
                            if (response.IsSuccessStatusCode)
                            {
                                string responseBody = await response.Content.ReadAsStringAsync();
                                System.IO.File.AppendAllText("debug_telegram_success.log", 
                                    $"{DateTime.Now}: Telegram уведомление отправлено успешно (попытка #{attempt}): {responseBody}\r\n");
                                return; // Успешно отправлено, выходим из метода
                            }
                            else
                            {
                                string error = await response.Content.ReadAsStringAsync();
                                System.IO.File.AppendAllText("debug_telegram_error.log", 
                                    $"{DateTime.Now}: Ошибка Telegram API (попытка #{attempt}): {response.StatusCode} - {error}\r\n");
                                
                                // Если это последняя попытка, пробуем WebClient
                                if (attempt == MAX_ATTEMPTS)
                                {
                                    lastHttpClientException = new Exception($"Telegram API вернул ошибку: {response.StatusCode} - {error}");
                                    break; // Выходим из цикла, чтобы попробовать WebClient
                                }
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    string detailedError = $"HTTP ошибка: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        detailedError += $" Inner: {ex.InnerException.Message}";
                    }
                    
                    System.IO.File.AppendAllText("debug_telegram_error.log", 
                        $"{DateTime.Now}: {detailedError} (попытка #{attempt})\r\n");
                    
                    lastHttpClientException = ex;
                    if (attempt == MAX_ATTEMPTS)
                    {
                        break; // Выходим из цикла, чтобы попробовать WebClient
                    }
                }
                catch (TaskCanceledException ex)
                {
                    System.IO.File.AppendAllText("debug_telegram_error.log", 
                        $"{DateTime.Now}: Таймаут запроса (попытка #{attempt}): {ex.Message}\r\n");
                    
                    lastHttpClientException = ex;
                    if (attempt == MAX_ATTEMPTS)
                    {
                        break; // Выходим из цикла, чтобы попробовать WebClient
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText("debug_telegram_error.log", 
                        $"{DateTime.Now}: Общая ошибка (попытка #{attempt}): {ex.GetType().Name} - {ex.Message}\r\n");
                    
                    lastHttpClientException = ex;
                    if (attempt == MAX_ATTEMPTS)
                    {
                        break; // Выходим из цикла, чтобы попробовать WebClient
                    }
                }
                
                // Если это не последняя попытка, ждем перед повтором
                if (attempt < MAX_ATTEMPTS)
                {
                    System.IO.File.AppendAllText("debug_telegram_info.log", 
                        $"{DateTime.Now}: Ожидание 2 секунды перед повтором...\r\n");
                    await Task.Delay(2000);
                }
            }
            
            // Если HttpClient не сработал, пробуем WebClient как fallback
            System.IO.File.AppendAllText("debug_telegram_info.log", 
                $"{DateTime.Now}: HttpClient не сработал, пробуем WebClient...\r\n");
            
            try
            {
                await SendTelegramMessageWebClient(message);
                return; // Успешно отправлено через WebClient
            }
            catch (Exception webClientException)
            {
                System.IO.File.AppendAllText("debug_telegram_error.log", 
                    $"{DateTime.Now}: WebClient также не сработал: {webClientException.Message}\r\n");
                
                // Бросаем исключение с информацией об обеих попытках
                throw new Exception($"HttpClient ошибка: {lastHttpClientException?.Message ?? "Неизвестно"}; WebClient ошибка: {webClientException.Message}", lastHttpClientException ?? webClientException);
            }
        }
        
        /// <summary>
        /// Альтернативная отправка через WebClient
        /// </summary>
        private static async Task SendTelegramMessageWebClient(string message)
        {
            // Убеждаемся, что TLS настройки применены и для WebClient
            System.Net.ServicePointManager.Expect100Continue = true;
            System.Net.ServicePointManager.SecurityProtocol = 
                System.Net.SecurityProtocolType.Tls12 | 
                System.Net.SecurityProtocolType.Tls11 | 
                System.Net.SecurityProtocolType.Tls;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = 
                (sender, certificate, chain, sslPolicyErrors) => true;
            
            using (var client = new System.Net.WebClient())
            {
                try
                {
                    System.IO.File.AppendAllText("debug_telegram_info.log", 
                        $"{DateTime.Now}: Используем WebClient для отправки (TLS: {System.Net.ServicePointManager.SecurityProtocol})...\r\n");
                    
                    // Настраиваем заголовки
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    client.Encoding = System.Text.Encoding.UTF8;
                    
                    // URL для Telegram Bot API
                    string url = $"https://api.telegram.org/bot{TELEGRAM_BOT_TOKEN}/sendMessage";
                    
                    // Формируем данные для POST запроса
                    var postData = $"chat_id={TELEGRAM_CHAT_ID}&text={Uri.EscapeDataString(message)}&parse_mode=Markdown&disable_web_page_preview=true";
                    
                    System.IO.File.AppendAllText("debug_telegram_info.log", 
                        $"{DateTime.Now}: WebClient POST на URL: {url.Replace(TELEGRAM_BOT_TOKEN, "***TOKEN***")}\r\n");
                    
                    // Отправляем запрос
                    string response = await Task.Run(() => client.UploadString(url, "POST", postData));
                    
                    System.IO.File.AppendAllText("debug_telegram_success.log", 
                        $"{DateTime.Now}: Telegram уведомление отправлено успешно через WebClient: {response}\r\n");
                }
                catch (System.Net.WebException webEx)
                {
                    string errorDetails = webEx.Message;
                    if (webEx.Response != null)
                    {
                        using (var reader = new System.IO.StreamReader(webEx.Response.GetResponseStream()))
                        {
                            errorDetails += $" Response: {reader.ReadToEnd()}";
                        }
                    }
                    
                    System.IO.File.AppendAllText("debug_telegram_error.log", 
                        $"{DateTime.Now}: WebClient WebException: {errorDetails}\r\n");
                    throw new Exception($"WebClient ошибка: {errorDetails}", webEx);
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText("debug_telegram_error.log", 
                        $"{DateTime.Now}: WebClient общая ошибка: {ex.Message}\r\n");
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Проверка подключения к интернету
        /// </summary>
        private static async Task<bool> CheckInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    // WebClient не имеет свойства Timeout, используем CancellationToken
                    using (var cts = new CancellationTokenSource(5000)) // 5 секунд таймаут
                    {
                        var task = Task.Run(() => client.DownloadString("https://www.google.com"), cts.Token);
                        var result = await task;
                        return !string.IsNullOrEmpty(result);
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Собирает информацию о системе для отправки в уведомление
        /// </summary>
        private static string GetSystemInformation()
        {
            try
            {
                StringBuilder info = new StringBuilder();
                
                info.AppendLine($"• ОС: {Environment.OSVersion}");
                info.AppendLine($"• Версия .NET: {Environment.Version}");
                info.AppendLine($"• Процессор: {Environment.ProcessorCount} ядер");
                info.AppendLine($"• Память: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
                info.AppendLine($"• Рабочий каталог: {Environment.CurrentDirectory}");
                info.AppendLine($"• Командная строка: {Environment.CommandLine}");
                
                // Получаем текущие процессы
                var processes = Process.GetProcesses()
                    .Where(p => p.ProcessName.ToLower().Contains("debug") || 
                               p.ProcessName.ToLower().Contains("ida") ||
                               p.ProcessName.ToLower().Contains("olly") ||
                               p.ProcessName.ToLower().Contains("x32dbg") ||
                               p.ProcessName.ToLower().Contains("x64dbg"))
                    .Take(5)
                    .Select(p => p.ProcessName);
                
                if (processes.Any())
                {
                    info.AppendLine($"• Подозрительные процессы: {string.Join(", ", processes)}");
                }

                return info.ToString();
            }
            catch
            {
                return "Не удалось собрать информацию о системе";
            }
        }

        /// <summary>
        /// Безопасный вызов мер безопасности (только один раз)
        /// </summary>
        private static void ApplyAntiDebugMeasuresOnce()
        {
            lock (lockObject)
            {
                if (antiDebugMeasuresApplied) return; // Уже применено
                antiDebugMeasuresApplied = true;
            }
            
            // Асинхронный вызов реальных мер
            _ = Task.Run(async () => await ApplyAntiDebugMeasures());
        }

        /// <summary>
        /// Применение различных мер при обнаружении отладчика
        /// </summary>
        private static async Task ApplyAntiDebugMeasures()
        {
            try
            {
                // Дополнительная проверка на всякий случай
                if (antiDebugMeasuresApplied && notificationSent) return;

                // ВАЖНО: Сохраняем информацию о лицензии ДО её отзыва
                string licenseInfoBeforeRevoke = LicenseManager.GetLicenseInfo();

                // КРИТИЧЕСКОЕ ДЕЙСТВИЕ: Отзыв лицензии и добавление в чёрный список
                LicenseManager.RevokeLicenseForDebugging();

                // Отправляем уведомление через Telegram Bot (синхронно) с сохранённой информацией
                await SendTelegramNotificationAsync(licenseInfoBeforeRevoke);

                // Показываем сообщение пользователю
                MessageBox.Show("Обнаружена попытка отладки приложения!\n\nЛицензия отозвана из соображений безопасности.", 
                    "Нарушение безопасности", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                Application.Exit();
            }
            catch (Exception ex)
            {
                // Если что-то пошло не так, всё равно закрываем приложение
                try
                {
                    System.IO.File.AppendAllText("antidebug_error.log", 
                        $"{DateTime.Now}: Ошибка в ApplyAntiDebugMeasures: {ex.Message}\r\n");
                }
                catch { }
                
                Application.Exit();
            }
        }

        /// <summary>
        /// Метод для проверки отладчика перед выполнением защищенных функций
        /// </summary>
        /// <returns>true если безопасно выполнять функцию, false если обнаружен отладчик</returns>
        public static bool IsSecure()
        {
            return !CheckForDebugger() && !debuggerDetected;
        }
        
        /// <summary>
        /// Тестовая отправка Telegram сообщения для проверки соединения
        /// </summary>
        public static async Task TestTelegramConnection()
        {
            try
            {
                System.IO.File.AppendAllText("debug_telegram_info.log", 
                    $"{DateTime.Now}: === ТЕСТ TELEGRAM СОЕДИНЕНИЯ ===\r\n");
                
                string testMessage = $"🧪 *ТЕСТ СОЕДИНЕНИЯ*\n\n📅 Время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n💻 Компьютер: {Environment.MachineName}\n\n✅ Если вы видите это сообщение, Telegram уведомления работают корректно!";
                
                await SendTelegramMessage(testMessage);
                
                System.IO.File.AppendAllText("debug_telegram_info.log", 
                    $"{DateTime.Now}: === ТЕСТ ЗАВЕРШЕН УСПЕШНО ===\r\n");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("debug_telegram_error.log", 
                    $"{DateTime.Now}: ТЕСТ FAILED: {ex.Message}\r\n");
                throw;
            }
        }
    }
}