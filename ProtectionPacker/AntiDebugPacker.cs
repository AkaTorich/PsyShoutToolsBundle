using System;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace ProtectionPacker
{
    /// <summary>
    /// Генератор анти-отладочного кода для упакованных приложений
    /// </summary>
    public class AntiDebugPacker
    {
        private readonly AntiDumpLevel _level;
        private readonly Random _random;

        public AntiDebugPacker(AntiDumpLevel level)
        {
            _level = level;
            _random = new Random();
        }

        /// <summary>
        /// Генерация анти-отладочного кода в зависимости от уровня защиты
        /// </summary>
        public string GenerateAntiDebugCode()
        {
            var code = new StringBuilder();
            
            // Импорты Windows API
            code.AppendLine(GenerateApiImports());
            
            // Основные методы проверки
            code.AppendLine(GenerateSecurityChecksMethod());
            
            // Уровень защиты определяет количество и сложность проверок
            switch (_level)
            {
                case AntiDumpLevel.Light:
                    code.AppendLine(GenerateLightProtection());
                    break;
                case AntiDumpLevel.Medium:
                    code.AppendLine(GenerateLightProtection());
                    code.AppendLine(GenerateMediumProtection());
                    break;
                case AntiDumpLevel.Maximum:
                    code.AppendLine(GenerateLightProtection());
                    code.AppendLine(GenerateMediumProtection());
                    code.AppendLine(GenerateMaximumProtection());
                    break;
            }
            
            return code.ToString();
        }

        /// <summary>
        /// Импорты Windows API для анти-отладочных проверок
        /// </summary>
        private string GenerateApiImports()
        {
            return @"
        [DllImport(""kernel32.dll"", SetLastError = true)]
        private static extern bool IsDebuggerPresent();

        [DllImport(""kernel32.dll"", SetLastError = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        [DllImport(""kernel32.dll"", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport(""ntdll.dll"", SetLastError = true)]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
            ref int processInformation, int processInformationLength, out int returnLength);

        [DllImport(""kernel32.dll"", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport(""user32.dll"", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport(""kernel32.dll"", SetLastError = true)]
        private static extern uint GetTickCount();

        [DllImport(""kernel32.dll"", SetLastError = true)]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);";
        }

        /// <summary>
        /// Главный метод проверки безопасности
        /// </summary>
        private string GenerateSecurityChecksMethod()
        {
            var checks = new StringBuilder();
            checks.AppendLine("                if (CheckDebuggerPresent_Method1()) return false;");
            checks.AppendLine("                if (CheckDebuggerPresent_Method2()) return false;");
            
            // Method3 доступен только с Medium уровня
            if (_level >= AntiDumpLevel.Medium)
            {
                checks.AppendLine("                if (CheckDebuggerPresent_Method3()) return false;");
            }
            
            // Method4 доступен только на Maximum уровне
            if (_level >= AntiDumpLevel.Maximum)
            {
                checks.AppendLine("                if (CheckDebuggerPresent_Method4()) return false;");
            }

            return $@"
        private static bool PerformSecurityChecks()
        {{
            try
            {{
                // Множественные проверки с различными методами
{checks.ToString().TrimEnd()}
                
                return true;
            }}
            catch
            {{
                // Если произошла ошибка в проверках - блокируем выполнение
                return false;
            }}
        }}";
        }

        /// <summary>
        /// Легкий уровень защиты - базовые проверки
        /// </summary>
        private string GenerateLightProtection()
        {
            return @"
        private static bool CheckDebuggerPresent_Method1()
        {
            // Проверка через .NET API
            if (Debugger.IsAttached) return true;
            
            // Проверка через Win32 API
            if (IsDebuggerPresent()) return true;
            
            return false;
        }

        private static bool CheckDebuggerPresent_Method2()
        {
            try
            {
                bool isRemoteDebuggerPresent = false;
                CheckRemoteDebuggerPresent(GetCurrentProcess(), ref isRemoteDebuggerPresent);
                return isRemoteDebuggerPresent;
            }
            catch
            {
                return true; // Ошибка считается признаком отладчика
            }
        }";
        }

        /// <summary>
        /// Средний уровень защиты - дополнительные проверки
        /// </summary>
        private string GenerateMediumProtection()
        {
            return $@"
        private static bool CheckDebuggerPresent_Method3()
        {{
            // Проверка через NtQueryInformationProcess
            try
            {{
                int debugPort = 0;
                int returnLength;
                int status = NtQueryInformationProcess(GetCurrentProcess(), 7, // ProcessDebugPort
                    ref debugPort, sizeof(int), out returnLength);
                
                if (status == 0 && debugPort != 0) return true;
            }}
            catch {{ return true; }}
            
            // Проверка времени выполнения (анти-степпинг)
            return CheckTimingAttack();
        }}

        private static bool CheckTimingAttack()
        {{
            uint start = GetTickCount();
            
            // Простая операция, которая должна выполниться быстро
            int dummy = 0;
            for (int i = 0; i < 1000; i++)
            {{
                dummy += i * 2;
            }}
            
            uint elapsed = GetTickCount() - start;
            
            // Если операция заняла слишком много времени - возможно степпинг
            return elapsed > 50; // {_random.Next(30, 100)} мс
        }}

        private static bool CheckDebuggerWindows()
        {{
            // Проверка окон отладчиков
            string[] debuggerWindows = {{
                ""OLLYDBG"", ""WinDbgFrameClass"", ""ID"", ""Zeta Debugger"",
                ""IDAViewClass"", ""IDA View"", ""Rock Debugger"", ""ObsidianGUI"",
                ""x32dbg"", ""x64dbg"", ""Immunity Debugger"", ""Import REConstructor""
            }};
            
            foreach (string window in debuggerWindows)
            {{
                if (FindWindow(window, null) != IntPtr.Zero) return true;
                if (FindWindow(null, window) != IntPtr.Zero) return true;
            }}
            
            return false;
        }}";
        }

        /// <summary>
        /// Максимальный уровень защиты - продвинутые техники
        /// </summary>
        private string GenerateMaximumProtection()
        {
            return $@"
        private static bool CheckDebuggerPresent_Method4()
        {{
            // Комбинированная проверка для максимального уровня
            if (CheckDebuggerProcesses()) return true;
            if (CheckVirtualMachine()) return true;
            if (CheckSandbox()) return true;
            if (CheckMemoryProtection()) return true;
            
            return false;
        }}

        private static bool CheckDebuggerProcesses()
        {{
            try
            {{
                string[] debuggerProcesses = {{
                    ""ollydbg"", ""idaq"", ""idaq64"", ""idaw"", ""idaw64"", ""idag"", ""idag64"",
                    ""windbg"", ""x32dbg"", ""x64dbg"", ""immunity"", ""importrec"", ""lordpe"",
                    ""syser"", ""cheatengine"", ""httpanalyzer"", ""httpdebugger"", ""wireshark"",
                    ""filemon"", ""procmon"", ""regmon"", ""procexp"", ""tcpview"", ""autoruns"",
                    ""autorunsc"", ""shellcode"", ""dbgview"", ""processexplorer""
                }};
                
                Process[] processes = Process.GetProcesses();
                
                foreach (Process process in processes)
                {{
                    try
                    {{
                        string processName = process.ProcessName.ToLower();
                        foreach (string debugger in debuggerProcesses)
                        {{
                            if (processName.Contains(debugger)) return true;
                        }}
                    }}
                    catch {{ continue; }}
                }}
            }}
            catch {{ return true; }}
            
            return false;
        }}

        private static bool CheckHardwareBreakpoints()
        {{
            try
            {{
                // Проверка аппаратных точек останова через GetThreadContext
                // Упрощенная реализация
                IntPtr currentThread = GetCurrentThread();
                // В реальной реализации здесь должна быть проверка CONTEXT структуры
                return false; // Заглушка
            }}
            catch
            {{
                return true;
            }}
        }}

        [DllImport(""kernel32.dll"")]
        private static extern IntPtr GetCurrentThread();

        private static bool CheckMemoryProtection()
        {{
            try
            {{
                // Проверка на модификацию памяти процесса
                byte[] testData = {{ {string.Join(", ", Enumerable.Range(0, 16).Select(i => $"0x{_random.Next(256):X2}"))} }};
                
                // Вычисляем контрольную сумму
                int checksum = 0;
                foreach (byte b in testData)
                {{
                    checksum ^= b;
                }}
                
                // Имитируем задержку
                Thread.Sleep(1);
                
                // Повторно вычисляем контрольную сумму
                int newChecksum = 0;
                foreach (byte b in testData)
                {{
                    newChecksum ^= b;
                }}
                
                // Если контрольная сумма изменилась - возможна модификация памяти
                return checksum != newChecksum;
            }}
            catch
            {{
                return true;
            }}
        }}

        private static bool CheckVirtualMachine()
        {{
            try
            {{
                // Проверка на виртуальную машину
                string[] vmIndicators = {{
                    ""VMware"", ""VirtualBox"", ""VBOX"", ""Virtual"", ""HVM domU"",
                    ""Bochs"", ""QEmu"", ""QEMU"", ""Plex86"", ""Microsoft Corporation"",
                    ""Hyper-V"", ""VMXh"", ""innotek GmbH"", ""Parallels""
                }};
                
                string computerName = Environment.MachineName.ToUpper();
                string userName = Environment.UserName.ToUpper();
                
                foreach (string indicator in vmIndicators)
                {{
                    if (computerName.Contains(indicator.ToUpper()) || 
                        userName.Contains(indicator.ToUpper()))
                    {{
                        return true;
                    }}
                }}
                
                // Проверка через WMI (упрощенная)
                try
                {{
                    using (var searcher = new ManagementObjectSearcher(""SELECT * FROM Win32_ComputerSystem""))
                    {{
                        using (var items = searcher.Get())
                        {{
                            foreach (var item in items)
                            {{
                                string manufacturer = item[""Manufacturer""].ToString().ToLower();
                                string model = item[""Model""].ToString().ToLower();
                                
                                if (manufacturer.Contains(""microsoft"") && model.Contains(""virtual"") ||
                                    manufacturer.Contains(""vmware"") ||
                                    manufacturer.Contains(""oracle""))
                                {{
                                    return true;
                                }}
                            }}
                        }}
                    }}
                }}
                catch {{ }}
                
                return false;
            }}
            catch
            {{
                return true;
            }}
        }}

        private static bool CheckSandbox()
        {{
            try
            {{
                // Проверка на анализирующие системы и песочницы
                
                // 1. Проверка количества файлов в системных папках
                int fileCount = 0;
                try
                {{
                    fileCount = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.System)).Length;
                    if (fileCount < 100) return true; // Слишком мало файлов - возможно песочница
                }}
                catch {{ }}
                
                // 2. Проверка времени работы системы
                int tickCount = Environment.TickCount;
                if (tickCount < 300000) return true; // Система работает менее 5 минут
                
                // 3. Проверка размера экрана
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int screenHeight = Screen.PrimaryScreen.Bounds.Height;
                if (screenWidth < 800 || screenHeight < 600) return true; // Подозрительно малое разрешение
                
                // 4. Проверка количества процессоров
                if (Environment.ProcessorCount < 2) return true; // Менее 2 ядер - подозрительно
                
                return false;
            }}
            catch
            {{
                return true;
            }}
        }}";
        }
    }
} 