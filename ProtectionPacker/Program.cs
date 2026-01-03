using System;
using System.IO;
using System.Diagnostics;

namespace ProtectionPacker
{
    /// <summary>
    /// Утилита упаковки и обфускации для защиты лицензионного ПО
    /// Создает защищенную версию приложения с множественными уровнями защиты
    /// </summary>
    class Program
    {
        private static readonly string Version = "1.0.0";
        private static readonly string Copyright = "© 2024 PsyShout Protection Suite";
        
        [STAThread]
        static void Main(string[] args)
        {
            // Если нет аргументов командной строки, запускаем GUI
            if (args.Length == 0)
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                System.Windows.Forms.Application.Run(new MainForm());
                return;
            }

            Console.Title = "PsyShout Protection Packer v" + Version;
            PrintBanner();

            if (args.Length < 2)
            {
                PrintUsage();
                Environment.Exit(1);
            }

            string inputFile = args[0];
            string outputFile = args[1];
            
            // Параметры защиты по умолчанию
            var options = new ProtectionOptions
            {
                EnableCompression = true,
                EnableEncryption = true,
                EnableAntiDebug = true,
                EnableObfuscation = true,
                EnableStringEncryption = true,
                EnableResourceProtection = true,
                EnableVirtualization = true,
                EnableFakeAPI = true,
                EnableDebugOutput = true, // По умолчанию отладка включена
                ApplicationType = ApplicationType.WindowsApp, // По умолчанию Windows-приложение без консоли
                OutputFileType = OutputFileType.Executable, // По умолчанию EXE
                AntiDumpLevel = AntiDumpLevel.Maximum,
                CompressionLevel = CompressionLevel.Maximum
            };

            // Парсинг дополнительных параметров
            for (int i = 2; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                switch (arg)
                {
                    case "--no-compression":
                        options.EnableCompression = false;
                        break;
                    case "--no-encryption":
                        options.EnableEncryption = false;
                        break;
                    case "--no-antidebug":
                        options.EnableAntiDebug = false;
                        break;
                    case "--no-obfuscation":
                        options.EnableObfuscation = false;
                        break;
                    case "--no-string-encryption":
                        options.EnableStringEncryption = false;
                        break;
                    case "--no-resource-protection":
                        options.EnableResourceProtection = false;
                        break;
                    case "--no-debug":
                        options.EnableDebugOutput = false;
                        break;
                    case "--debug":
                        options.EnableDebugOutput = true;
                        options.EnablePackerDebug = true;
                        Console.WriteLine("🐛 Режим отладки включен (упаковщик + stub)");
                        break;
                    case "--light":
                        options.AntiDumpLevel = AntiDumpLevel.Light;
                        options.CompressionLevel = CompressionLevel.Fast;
                        break;
                    case "--maximum":
                        options.AntiDumpLevel = AntiDumpLevel.Maximum;
                        options.CompressionLevel = CompressionLevel.Maximum;
                        options.EnableVirtualization = true;
                        break;
                    case "--console":
                        options.ApplicationType = ApplicationType.ConsoleApp;
                        Console.WriteLine("📟 Режим консольного приложения включен (с консолью)");
                        break;
                    case "--winexe":
                        options.ApplicationType = ApplicationType.WindowsApp;
                        Console.WriteLine("🖥️ Режим Windows-приложения включен (без консоли)");
                        break;
                    case "--dll":
                        options.OutputFileType = OutputFileType.Library;
                        Console.WriteLine("📚 Режим DLL библиотеки включен");
                        break;
                    case "--exe":
                        options.OutputFileType = OutputFileType.Executable;
                        Console.WriteLine("📦 Режим EXE файла включен");
                        break;
                    case "--help":
                    case "-h":
                        PrintUsage();
                        Environment.Exit(0);
                        break;
                }
            }

            try
            {
                Console.WriteLine($"🎯 Обработка файла: {inputFile}");
                Console.WriteLine($"📦 Выходной файл: {outputFile}");
                Console.WriteLine();

                if (!File.Exists(inputFile))
                {
                    Console.WriteLine($"❌ Ошибка: Файл '{inputFile}' не найден!");
                    Environment.Exit(1);
                }

                // Создаем главный класс упаковщика
                var packer = new ProtectionPacker(options);
                
                // Запускаем процесс защиты
                bool success = packer.PackAndProtect(inputFile, outputFile);

                if (success)
                {
                    Console.WriteLine();
                    Console.WriteLine("✅ Упаковка и защита завершены успешно!");
                    Console.WriteLine($"📊 Размер исходного файла: {GetFileSize(inputFile)}");
                    Console.WriteLine($"📊 Размер защищенного файла: {GetFileSize(outputFile)}");
                    Console.WriteLine();
                    Console.WriteLine("🔐 Применены защиты:");
                    PrintAppliedProtections(options);
                }
                else
                {
                    Console.WriteLine("❌ Ошибка при упаковке и защите!");
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Критическая ошибка: {ex.Message}");
                Console.WriteLine($"📍 Детали: {ex.StackTrace}");
                Environment.Exit(1);
            }

            Console.WriteLine();
            Console.WriteLine("📱 Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        private static void PrintBanner()
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                 🛡️  PSYSHOUT PROTECTION PACKER             ║");
            Console.WriteLine("║                        Advanced Security Suite             ║");
            Console.WriteLine($"║                           Version {Version}                    ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  🔐 Упаковка и обфускация исполняемых файлов              ║");
            Console.WriteLine("║  🛡️  Защита от отладки и реверс-инжиниринга              ║");
            Console.WriteLine("║  🔧 Шифрование строк и ресурсов                           ║");
            Console.WriteLine("║  ⚡ Виртуализация кода и анти-дамп защиты                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"   {Copyright}");
            Console.WriteLine();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("📖 Использование:");
            Console.WriteLine("   ProtectionPacker.exe <входной_файл> <выходной_файл> [опции]");
            Console.WriteLine();
            Console.WriteLine("📋 Опции:");
            Console.WriteLine("   --no-compression    Отключить сжатие");
            Console.WriteLine("   --no-encryption     Отключить шифрование");
            Console.WriteLine("   --no-antidebug      Отключить анти-отладочные защиты");
            Console.WriteLine("   --no-obfuscation    Отключить обфускацию");
            Console.WriteLine("   --no-string-encryption    Отключить шифрование строк");
            Console.WriteLine("   --no-resource-protection  Отключить защиту ресурсов");
            Console.WriteLine("   --debug             Включить подробную отладку процесса упаковки");
            Console.WriteLine("   --no-debug          Отключить отладочные сообщения в stub'е");
            Console.WriteLine("   --console           Создать консольное приложение (с консолью для отладки)");
            Console.WriteLine("   --winexe            Создать Windows-приложение (без консоли, по умолчанию)");
            Console.WriteLine("   --dll               Создать DLL библиотеку");
            Console.WriteLine("   --exe               Создать EXE файл (по умолчанию)");
            Console.WriteLine("   --light             Легкий уровень защиты (быстрее)");
            Console.WriteLine("   --maximum           Максимальный уровень защиты");
            Console.WriteLine("   --help, -h          Показать эту справку");
            Console.WriteLine();
            Console.WriteLine("💡 Примеры:");
            Console.WriteLine("   ProtectionPacker.exe app.exe protected_app.exe");
            Console.WriteLine("   ProtectionPacker.exe app.exe protected.exe --maximum");
            Console.WriteLine("   ProtectionPacker.exe library.dll protected.dll --dll --maximum");
            Console.WriteLine("   ProtectionPacker.exe app.exe light.exe --light --no-encryption");
            Console.WriteLine("   ProtectionPacker.exe app.exe debug.exe --console --debug");
            Console.WriteLine("   ProtectionPacker.exe app.exe release.exe --winexe --maximum");
        }

        private static void PrintAppliedProtections(ProtectionOptions options)
        {
            // Тип приложения
            string appTypeStr = options.ApplicationType == ApplicationType.ConsoleApp ? 
                "Консольное приложение (с консолью для отладки)" : 
                "Windows-приложение (без консоли)";
            Console.WriteLine($"   🖥️ Тип: {appTypeStr}");

            if (options.EnableCompression)
                Console.WriteLine($"   ✓ Сжатие данных ({options.CompressionLevel})");
            if (options.EnableEncryption)
                Console.WriteLine("   ✓ Шифрование исполняемого кода");
            if (options.EnableAntiDebug)
                Console.WriteLine($"   ✓ Анти-отладочные защиты ({options.AntiDumpLevel})");
            if (options.EnableObfuscation)
                Console.WriteLine("   ✓ Обфускация методов и классов");
            if (options.EnableStringEncryption)
                Console.WriteLine("   ✓ Шифрование строковых констант");
            if (options.EnableResourceProtection)
                Console.WriteLine("   ✓ Защита встроенных ресурсов");
            if (options.EnableVirtualization)
                Console.WriteLine("   ✓ Виртуализация критического кода");
            if (options.EnableFakeAPI)
                Console.WriteLine("   ✓ Ложные API вызовы и обманные функции");
        }

        private static string GetFileSize(string filePath)
        {
            if (!File.Exists(filePath)) return "N/A";
            
            long bytes = new FileInfo(filePath).Length;
            if (bytes >= 1024 * 1024)
                return $"{bytes / (1024.0 * 1024.0):F1} MB";
            else if (bytes >= 1024)
                return $"{bytes / 1024.0:F1} KB";
            else
                return $"{bytes} bytes";
        }
    }

    /// <summary>
    /// Настройки защиты для упаковщика
    /// </summary>
    public class ProtectionOptions
    {
        public bool EnableCompression { get; set; }
        public bool EnableEncryption { get; set; }
        public bool EnableAntiDebug { get; set; }
        public bool EnableObfuscation { get; set; }
        public bool EnableStringEncryption { get; set; }
        public bool EnableResourceProtection { get; set; }
        public bool EnableVirtualization { get; set; }
        public bool EnableFakeAPI { get; set; }
        public bool EnableDebugOutput { get; set; }
        public bool EnablePackerDebug { get; set; } // Отладка процесса упаковки
        public ApplicationType ApplicationType { get; set; } // Тип приложения (консольное/Windows)
        public OutputFileType OutputFileType { get; set; } // Тип выходного файла (EXE/DLL)
        public AntiDumpLevel AntiDumpLevel { get; set; }
        public CompressionLevel CompressionLevel { get; set; }
    }

    public enum AntiDumpLevel
    {
        None,
        Light,
        Medium,
        Maximum
    }

    public enum CompressionLevel
    {
        None,
        Fast,
        Optimal,
        Maximum
    }

    public enum ApplicationType
    {
        WindowsApp,     // winexe - без консоли
        ConsoleApp      // exe - с консолью для отладки
    }

    public enum OutputFileType
    {
        Executable,     // EXE - исполняемый файл
        Library         // DLL - библиотека
    }
} 