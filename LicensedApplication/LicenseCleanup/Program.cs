using System;
using System.IO;
using Microsoft.Win32;

namespace LicenseCleanup
{
    /// <summary>
    /// Консольное приложение для полной очистки всех данных лицензирования из реестра Windows
    /// </summary>
    class Program
    {
        // Константы из LicenseManager (дублируем для независимости)
        private const string RegistryKeyPath = @"SOFTWARE\PsyShout\YourProduct";
        private const string LicenseFileName = "license.dat";

        static int Main(string[] args)
        {
            Console.Title = "License Cleanup Tool v1.0";
            Console.ForegroundColor = ConsoleColor.White;
            
            ShowHeader();

            try
            {
                // Проверяем права администратора
                if (!IsRunningAsAdministrator())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️  ВНИМАНИЕ: Для полной очистки реестра рекомендуется запуск от имени администратора.");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("   Продолжить? (y/n): ");
                    
                    string input = Console.ReadLine();
                    if (input?.ToLower() != "y" && input?.ToLower() != "yes")
                    {
                        Console.WriteLine("Операция отменена пользователем.");
                        return 1;
                    }
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("🔍 Начинаю сканирование системы...\n");

                // Показываем что будет очищено
                ShowWhatWillBeDeleted();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n❓ Продолжить очистку? (y/n): ");
                string confirm = Console.ReadLine();
                
                if (confirm?.ToLower() != "y" && confirm?.ToLower() != "yes")
                {
                    Console.WriteLine("Операция отменена пользователем.");
                    return 1;
                }

                Console.WriteLine();

                // Выполняем очистку
                bool success = PerformCleanup();

                if (success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ ОЧИСТКА ЗАВЕРШЕНА УСПЕШНО!");
                    Console.WriteLine("   Все данные лицензирования удалены из системы.");
                    Console.WriteLine("   Приложение можно запускать заново с чистого листа.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ ОШИБКА ПРИ ОЧИСТКЕ!");
                    Console.WriteLine("   Некоторые данные могли остаться в системе.");
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();

                return success ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                return 1;
            }
        }

        /// <summary>
        /// Показывает заголовок приложения
        /// </summary>
        private static void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    🧹 LICENSE CLEANUP TOOL                      ║");
            Console.WriteLine("║                    Утилита очистки лицензий                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("📋 НАЗНАЧЕНИЕ:");
            Console.WriteLine("   • Удаление всех данных лицензирования из реестра Windows");
            Console.WriteLine("   • Очистка флагов отладки и черного списка");
            Console.WriteLine("   • Удаление лицензионных файлов");
            Console.WriteLine("   • Сброс пробного периода");
            Console.WriteLine();
        }

        /// <summary>
        /// Показывает что именно будет удалено
        /// </summary>
        private static void ShowWhatWillBeDeleted()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🗂️  ДАННЫЕ ДЛЯ УДАЛЕНИЯ:");
            
            int itemCount = 0;

            // Проверяем реестр
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"   📁 Раздел реестра: HKEY_CURRENT_USER\\{RegistryKeyPath}");
                        
                        string[] valueNames = key.GetValueNames();
                        foreach (string valueName in valueNames)
                        {
                            object value = key.GetValue(valueName);
                            Console.WriteLine($"      • {valueName}: {GetValueDescription(valueName, value)}");
                            itemCount++;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("   📁 Раздел реестра: не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   📁 Ошибка доступа к реестру: {ex.Message}");
            }

            // Проверяем лицензионный файл
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string licensePath = Path.Combine(appDir, LicenseFileName);
                
                if (File.Exists(licensePath))
                {
                    FileInfo fileInfo = new FileInfo(licensePath);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"   📄 Лицензионный файл: {licensePath}");
                    Console.WriteLine($"      • Размер: {fileInfo.Length} байт");
                    Console.WriteLine($"      • Создан: {fileInfo.CreationTime}");
                    itemCount++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("   📄 Лицензионный файл: не найден");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   📄 Ошибка проверки файла: {ex.Message}");
            }

            // Проверяем логи
            CheckLogFiles(ref itemCount);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n📊 ИТОГО НАЙДЕНО ЭЛЕМЕНТОВ: {itemCount}");
        }

        /// <summary>
        /// Проверяет наличие лог-файлов
        /// </summary>
        private static void CheckLogFiles(ref int itemCount)
        {
            string[] logFiles = { 
                "license_revoked.log", 
                "license_revoke_error.log", 
                "admin_actions.log",
                "debug_telegram_error.log",
                "debug_telegram_success.log",
                "antidebug_error.log"
            };

            foreach (string logFile in logFiles)
            {
                try
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFile);
                    if (File.Exists(logPath))
                    {
                        FileInfo fileInfo = new FileInfo(logPath);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"   📜 Лог-файл: {logFile} ({fileInfo.Length} байт)");
                        itemCount++;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Получает описание значения реестра
        /// </summary>
        private static string GetValueDescription(string valueName, object value)
        {
            switch (valueName)
            {
                case "FirstRun":
                    return $"Первый запуск ({value})";
                case "InstallDate":
                    return $"Дата установки ({value})";
                case "SystemInfo":
                    return "Системная информация (зашифровано)";
                case "DebuggerDetected":
                    return "🚫 ФЛАГ ЧЕРНОГО СПИСКА (зашифровано)";
                case "NoAutoTrial":
                    return "Запрет пробного периода";
                default:
                    return value?.ToString() ?? "null";
            }
        }

        /// <summary>
        /// Выполняет полную очистку системы
        /// </summary>
        private static bool PerformCleanup()
        {
            bool allSuccess = true;
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🧹 Выполняю очистку...\n");

            // 1. Очистка реестра
            Console.Write("   📁 Очистка реестра... ");
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyPath, false);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ OK");
            }
            catch (ArgumentException)
            {
                // Ключ не существует - это нормально
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("⚪ Ключ не найден");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ ОШИБКА: {ex.Message}");
                allSuccess = false;
            }

            // 2. Удаление лицензионного файла
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   📄 Удаление лицензии... ");
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string licensePath = Path.Combine(appDir, LicenseFileName);
                
                if (File.Exists(licensePath))
                {
                    File.Delete(licensePath);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Удален");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("⚪ Файл не найден");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ ОШИБКА: {ex.Message}");
                allSuccess = false;
            }

            // 3. Удаление лог-файлов
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("   📜 Очистка логов... ");
            int deletedLogs = DeleteLogFiles();
            if (deletedLogs > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Удалено {deletedLogs} файлов");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("⚪ Логи не найдены");
            }

            return allSuccess;
        }

        /// <summary>
        /// Удаляет все лог-файлы
        /// </summary>
        private static int DeleteLogFiles()
        {
            string[] logFiles = { 
                "license_revoked.log", 
                "license_revoke_error.log", 
                "admin_actions.log",
                "debug_telegram_error.log",
                "debug_telegram_success.log",
                "antidebug_error.log"
            };

            int deletedCount = 0;
            foreach (string logFile in logFiles)
            {
                try
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFile);
                    if (File.Exists(logPath))
                    {
                        File.Delete(logPath);
                        deletedCount++;
                    }
                }
                catch { }
            }

            return deletedCount;
        }

        /// <summary>
        /// Проверяет запущено ли приложение от имени администратора
        /// </summary>
        private static bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
} 