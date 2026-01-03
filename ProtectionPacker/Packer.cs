using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Management;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
using System.Reflection;

namespace ProtectionPacker
{
    /// <summary>
    /// Главный класс упаковщика, координирующий все процессы защиты
    /// </summary>
    public class ProtectionPacker
    {
        private readonly ProtectionOptions _options;
        private readonly byte[] _encryptionKey;
        private readonly byte[] _encryptionIV;
        private bool _isDllFile;
        private string _targetPlatform; // x86, x64, anycpu

        public ProtectionPacker(ProtectionOptions options)
        {
            _options = options;

            // Генерируем ключи шифрования
            using (var rng = RandomNumberGenerator.Create())
            {
                _encryptionKey = new byte[32]; // AES-256
                _encryptionIV = new byte[16];  // AES block size
                rng.GetBytes(_encryptionKey);
                rng.GetBytes(_encryptionIV);
            }
        }

        /// <summary>
        /// Определяет, является ли файл DLL-библиотекой
        /// </summary>
        private bool IsDllFile(string filePath)
        {
            try
            {
                // Проверяем по расширению
                string extension = Path.GetExtension(filePath).ToLower();
                if (extension == ".dll")
                    return true;

                // Дополнительная проверка через Assembly
                Assembly asm = Assembly.LoadFrom(filePath);
                // Если EntryPoint == null, это обычно DLL
                return asm.EntryPoint == null;
            }
            catch
            {
                // Если не можем загрузить как .NET assembly, проверяем только расширение
                return Path.GetExtension(filePath).ToLower() == ".dll";
            }
        }

        /// <summary>
        /// Определяет целевую платформу (архитектуру) входного файла
        /// </summary>
        private string DetectTargetPlatform(string filePath)
        {
            try
            {
                // Читаем PE-заголовок для определения архитектуры
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(stream))
                {
                    // Проверяем DOS заголовок (MZ)
                    if (reader.ReadUInt16() != 0x5A4D) // MZ
                        return "anycpu";

                    // Переходим к смещению PE заголовка
                    stream.Seek(0x3C, SeekOrigin.Begin);
                    int peOffset = reader.ReadInt32();

                    // Переходим к PE заголовку
                    stream.Seek(peOffset, SeekOrigin.Begin);

                    // Проверяем PE сигнатуру
                    if (reader.ReadUInt32() != 0x00004550) // PE\0\0
                        return "anycpu";

                    // Читаем Machine тип (2 байта после PE сигнатуры)
                    ushort machine = reader.ReadUInt16();

                    // 0x014c = x86 (I386)
                    // 0x8664 = x64 (AMD64)
                    if (machine == 0x8664)
                        return "x64";
                    else if (machine == 0x014c)
                    {
                        // Для x86 нужно дополнительно проверить, это AnyCPU или чистый x86
                        // Читаем Characteristics и Optional Header
                        reader.ReadUInt16(); // NumberOfSections
                        reader.ReadUInt32(); // TimeDateStamp
                        reader.ReadUInt32(); // PointerToSymbolTable
                        reader.ReadUInt32(); // NumberOfSymbols
                        ushort optionalHeaderSize = reader.ReadUInt16();
                        ushort characteristics = reader.ReadUInt16();

                        if (optionalHeaderSize > 0)
                        {
                            // Magic number: 0x10b = PE32, 0x20b = PE32+
                            ushort magic = reader.ReadUInt16();
                            
                            // Пропускаем до поля DllCharacteristics (байт 70 в Optional Header для PE32)
                            // Или проверяем через .NET заголовок
                            
                            // Для .NET сборок проверяем CorFlags
                            try
                            {
                                Assembly asm = Assembly.ReflectionOnlyLoadFrom(filePath);
                                var name = asm.GetName();
                                
                                switch (name.ProcessorArchitecture)
                                {
                                    case ProcessorArchitecture.Amd64:
                                        return "x64";
                                    case ProcessorArchitecture.X86:
                                        return "x86";
                                    case ProcessorArchitecture.MSIL:
                                        return "anycpu";
                                    default:
                                        return "anycpu";
                                }
                            }
                            catch
                            {
                                // Если не можем загрузить как .NET, возвращаем x86
                                return "x86";
                            }
                        }
                        return "x86";
                    }

                    return "anycpu";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Не удалось определить архитектуру: {ex.Message}, используется AnyCPU");
                return "anycpu";
            }
        }

        /// <summary>
        /// Основной метод упаковки и защиты файла
        /// </summary>
        public bool PackAndProtect(string inputFile, string outputFile)
        {
            try
            {
                if (_options.EnablePackerDebug)
                    Console.WriteLine($"🐛 [DEBUG] Начало упаковки файла: {inputFile}");

                Console.WriteLine($"🔒 Загрузка файла: {Path.GetFileName(inputFile)}");

                if (!File.Exists(inputFile))
                {
                    Console.WriteLine($"❌ Файл не найден: {inputFile}");
                    return false;
                }

                // Определяем тип файла на основе пользовательского выбора
                _isDllFile = (_options.OutputFileType == OutputFileType.Library);

                // Если пользователь не выбрал явно, определяем автоматически
                if (!_isDllFile && IsDllFile(inputFile))
                {
                    _isDllFile = true;
                    Console.WriteLine("ℹ️ Входной файл определен как DLL, автоматически выбран режим библиотеки");
                }

                // Определяем архитектуру входного файла
                _targetPlatform = DetectTargetPlatform(inputFile);
                Console.WriteLine($"🖥️ Архитектура входного файла: {_targetPlatform.ToUpper()}");

                string fileType = _isDllFile ? "DLL" : "EXE";
                Console.WriteLine($"📋 Тип выходного файла: {fileType}");

                if (_options.EnablePackerDebug)
                    Console.WriteLine($"🐛 [DEBUG] Выходной файл будет скомпилирован как {fileType}");

                // Читаем оригинальный файл
                byte[] originalData = File.ReadAllBytes(inputFile);
                var originalSize = originalData.Length;
                
                Console.WriteLine($"📁 Размер оригинала: {originalSize:N0} байт");

                if (_options.EnablePackerDebug)
                {
                    Console.WriteLine($"🐛 [DEBUG] Оригинальный файл загружен: {originalSize} байт");
                    Console.WriteLine($"🐛 [DEBUG] Настройки защиты:");
                    Console.WriteLine($"      - Сжатие: {_options.EnableCompression} ({_options.CompressionLevel})");
                    Console.WriteLine($"      - Шифрование: {_options.EnableEncryption}");
                    Console.WriteLine($"      - Анти-отладка: {_options.EnableAntiDebug} ({_options.AntiDumpLevel})");
                    Console.WriteLine($"      - Обфускация: {_options.EnableObfuscation}");
                    Console.WriteLine($"      - Шифрование строк: {_options.EnableStringEncryption}");
                    Console.WriteLine($"      - Защита ресурсов: {_options.EnableResourceProtection}");
                }

                // Сохраняем оригинальные данные для анализа строк и ресурсов
                byte[] originalAssembly = (byte[])originalData.Clone();

                // Применяем обфускацию (если включена)
                if (_options.EnableObfuscation)
                {
                    Console.WriteLine("🔀 Применение обфускации...");
                    if (_options.EnablePackerDebug)
                        Console.WriteLine("🐛 [DEBUG] Запуск модуля обфускации...");
                    var obfuscator = new Obfuscator();
                    originalData = obfuscator.ObfuscateAssembly(originalData);
                    if (_options.EnablePackerDebug)
                        Console.WriteLine($"🐛 [DEBUG] Обфускация завершена, размер: {originalData.Length} байт");
                }

                // StringEncryption и ResourceProtection теперь обрабатываются в stub'е

                byte[] processedData = originalData;

                // Сжатие (если включено)
                if (_options.EnableCompression)
                {
                    Console.WriteLine($"📦 Сжатие данных (уровень {_options.CompressionLevel})...");
                    processedData = CompressData(processedData);
                    var compressionRatio = (1.0 - (double)processedData.Length / originalData.Length) * 100;
                    Console.WriteLine($"✅ Сжатие завершено: {compressionRatio:F1}% экономии");
                }

                // Шифрование (если включено) 
                if (_options.EnableEncryption)
                {
                    Console.WriteLine("🔐 Шифрование AES-256...");
                    processedData = EncryptData(processedData);
                    Console.WriteLine("✅ Шифрование завершено");
                }

                // Создание защищенного stub'а
                Console.WriteLine("🏗️ Создание защищенного загрузчика...");
                if (_options.EnablePackerDebug)
                    Console.WriteLine($"🐛 [DEBUG] Создание stub'а с полезной нагрузкой {processedData.Length} байт...");
                byte[] protectedStub = CreateProtectedStub(processedData, originalAssembly);

                // Сохранение результата
                Console.WriteLine($"💾 Сохранение: {Path.GetFileName(outputFile)}");
                if (_options.EnablePackerDebug)
                    Console.WriteLine($"🐛 [DEBUG] Сохранение файла размером {protectedStub.Length} байт...");
                File.WriteAllBytes(outputFile, protectedStub);
                if (_options.EnablePackerDebug)
                    Console.WriteLine($"🐛 [DEBUG] Файл успешно сохранен: {outputFile}");

                var finalSize = protectedStub.Length;
                var sizeRatio = (double)finalSize / originalSize * 100;

                Console.WriteLine();
                Console.WriteLine("✅ Упаковка завершена успешно!");
                Console.WriteLine($"📊 Оригинал: {originalSize:N0} байт");
                Console.WriteLine($"📊 Результат: {finalSize:N0} байт ({sizeRatio:F1}%)");
                
                if (_options.EnableCompression)
                {
                    var totalCompression = (1.0 - (double)finalSize / originalSize) * 100;
                    if (totalCompression > 0)
                        Console.WriteLine($"📊 Общее сжатие: {totalCompression:F1}%");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка упаковки: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Сжатие данных с помощью GZip
        /// </summary>
        private byte[] CompressData(byte[] data)
        {
            System.IO.Compression.CompressionLevel compressionLevel;
            switch (_options.CompressionLevel)
            {
                case CompressionLevel.Fast:
                    compressionLevel = System.IO.Compression.CompressionLevel.Fastest;
                    break;
                case CompressionLevel.Optimal:
                    compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                    break;
                case CompressionLevel.Maximum:
                    compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                    break;
                default:
                    compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                    break;
            }

            using (var output = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(output, compressionLevel))
                {
                    gzipStream.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// Шифрование данных с помощью AES-256
        /// </summary>
        private byte[] EncryptData(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = _encryptionIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var output = new MemoryStream())
                using (var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return output.ToArray();
                }
            }
        }

        /// <summary>
        /// Создание защищенного stub-загрузчика
        /// </summary>
        private byte[] CreateProtectedStub(byte[] encryptedPayload, byte[] originalAssembly)
        {
            var stubBuilder = new StringBuilder();
            
            // Генерируем C# код для stub'а
            stubBuilder.AppendLine("using System;");
            stubBuilder.AppendLine("using System.IO;");
            stubBuilder.AppendLine("using System.IO.Compression;");
            stubBuilder.AppendLine("using System.Security.Cryptography;");
            stubBuilder.AppendLine("using System.Reflection;");
            stubBuilder.AppendLine("using System.Runtime.InteropServices;");
            stubBuilder.AppendLine("using System.Diagnostics;");
            stubBuilder.AppendLine("using System.Threading;");
            stubBuilder.AppendLine("using System.Management;");
            stubBuilder.AppendLine("using System.Windows.Forms;");
            stubBuilder.AppendLine("using System.Drawing;");
            stubBuilder.AppendLine("using System.Linq;");
            stubBuilder.AppendLine("using System.Collections.Generic;");
            stubBuilder.AppendLine("using System.Text;");
            stubBuilder.AppendLine();
            
            stubBuilder.AppendLine("namespace ProtectedApplication");
            stubBuilder.AppendLine("{");
            // Для DLL делаем публичный класс, для EXE - internal
            string classAccessModifier = _isDllFile ? "public" : "internal";
            stubBuilder.AppendLine($"    {classAccessModifier} class ProtectedLoader");
            stubBuilder.AppendLine("    {");
            
            // Добавляем анти-отладочные импорты
            if (_options.EnableAntiDebug)
            {
                var antiDebugPacker = new AntiDebugPacker(_options.AntiDumpLevel);
                stubBuilder.AppendLine(antiDebugPacker.GenerateAntiDebugCode());
            }

            // Добавляем защиту строк (если включена)
            if (_options.EnableStringEncryption)
            {
                Console.WriteLine("🔐 Шифрование строк...");
                var stringEncryption = new StringEncryption();
                // Анализируем оригинальную сборку для извлечения строк
                stringEncryption.EncryptStrings(originalAssembly);
                string stringProtectionCode = stringEncryption.GenerateStringProtectionCode();
                if (!string.IsNullOrEmpty(stringProtectionCode))
                {
                    stubBuilder.AppendLine(stringProtectionCode);
                }
            }

            // Добавляем защиту ресурсов (если включена)
            if (_options.EnableResourceProtection)
            {
                Console.WriteLine("🛡️ Защита ресурсов...");
                var resourceProtection = new ResourceProtection();
                // Анализируем оригинальную сборку для извлечения ресурсов
                resourceProtection.ProtectResources(originalAssembly);
                string resourceProtectionCode = resourceProtection.GenerateResourceProtectionCode();
                if (!string.IsNullOrEmpty(resourceProtectionCode))
                {
                    stubBuilder.AppendLine(resourceProtectionCode);
                }
            }

            // Добавляем код обфускации (если включена)
            if (_options.EnableObfuscation)
            {
                Console.WriteLine("🎭 Добавление обфускационного кода...");
                var obfuscator = new Obfuscator();
                string obfuscationCode = obfuscator.GetObfuscationCodeForStub();
                if (!string.IsNullOrEmpty(obfuscationCode))
                {
                    stubBuilder.AppendLine(obfuscationCode);
                }
            }
            
            // Добавляем метод расшифровки
            stubBuilder.AppendLine(GenerateDecryptionMethod());
            
            // Добавляем основной метод загрузки
            stubBuilder.AppendLine(GenerateMainMethod(encryptedPayload));
            
            stubBuilder.AppendLine("    }");
            stubBuilder.AppendLine("}");

            // Компилируем stub в память и возвращаем как исполняемый файл
            return CompileStubToExecutable(stubBuilder.ToString(), encryptedPayload);
        }

        /// <summary>
        /// Генерация метода расшифровки для stub'а
        /// </summary>
        private string GenerateDecryptionMethod()
        {
            var method = new StringBuilder();
            
            method.AppendLine("        private static byte[] DecryptAndDecompress(byte[] encryptedData)");
            method.AppendLine("        {");
            method.AppendLine("            try");
            method.AppendLine("            {");
            
            if (_options.EnableEncryption)
            {
                method.AppendLine("                // Расшифровка AES-256");
                method.AppendLine($"                byte[] key = new byte[] {{{string.Join(",", _encryptionKey)}}};");
                method.AppendLine($"                byte[] iv = new byte[] {{{string.Join(",", _encryptionIV)}}};");
                method.AppendLine();
                method.AppendLine("                using (var aes = Aes.Create())");
                method.AppendLine("                {");
                method.AppendLine("                    aes.Key = key;");
                method.AppendLine("                    aes.IV = iv;");
                method.AppendLine("                    aes.Mode = CipherMode.CBC;");
                method.AppendLine("                    aes.Padding = PaddingMode.PKCS7;");
                method.AppendLine();
                method.AppendLine("                    using (var decryptor = aes.CreateDecryptor())");
                method.AppendLine("                    using (var input = new MemoryStream(encryptedData))");
                method.AppendLine("                    using (var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))");
                method.AppendLine("                    using (var output = new MemoryStream())");
                method.AppendLine("                    {");
                method.AppendLine("                        cryptoStream.CopyTo(output);");
                method.AppendLine("                        encryptedData = output.ToArray();");
                method.AppendLine("                    }");
                method.AppendLine("                }");
            }
            
            if (_options.EnableCompression)
            {
                method.AppendLine();
                method.AppendLine("                // Распаковка GZip");
                method.AppendLine("                using (var input = new MemoryStream(encryptedData))");
                method.AppendLine("                using (var gzip = new GZipStream(input, CompressionMode.Decompress))");
                method.AppendLine("                using (var output = new MemoryStream())");
                method.AppendLine("                {");
                method.AppendLine("                    gzip.CopyTo(output);");
                method.AppendLine("                    return output.ToArray();");
                method.AppendLine("                }");
            }
            else
            {
                method.AppendLine("                return encryptedData;");
            }
            
            method.AppendLine("            }");
            method.AppendLine("            catch");
            method.AppendLine("            {");
            method.AppendLine("                Environment.Exit(-1);");
            method.AppendLine("                return null;");
            method.AppendLine("            }");
            method.AppendLine("        }");
            
            return method.ToString();
        }

        /// <summary>
        /// Генерация основного метода загрузки для stub'а
        /// </summary>
        private string GenerateMainMethod(byte[] encryptedPayload)
        {
            // Для DLL используем другой метод загрузки
            if (_isDllFile)
                return GenerateDllLoaderMethod(encryptedPayload);

            var method = new StringBuilder();

            method.AppendLine("        [STAThread]");
            method.AppendLine("        private static void Main()");
            method.AppendLine("        {");

            // Добавляем консольную отладку (если включена)
            method.AppendLine("            try");
            method.AppendLine("            {");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Starting protected loader...\");");

            if (_options.EnableAntiDebug)
            {
                if (_options.EnableDebugOutput)
                    method.AppendLine("                Console.WriteLine(\"[DEBUG] Performing security checks...\");");
                method.AppendLine("                if (!PerformSecurityChecks()) Environment.Exit(-1);");
                method.AppendLine();
            }

            // Загружаем зашифрованные данные из embedded resource
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Loading payload from embedded resource...\");");

            method.AppendLine("                byte[] encryptedPayload;");
            method.AppendLine("                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(");
            method.AppendLine("                    Assembly.GetExecutingAssembly().GetManifestResourceNames()[0]))");
            method.AppendLine("                {");
            if (_options.EnableDebugOutput)
                method.AppendLine("                    Console.WriteLine(\"[DEBUG] Resource stream opened, size: \" + stream.Length + \" bytes\");");
            method.AppendLine("                    using (var ms = new MemoryStream())");
            method.AppendLine("                    {");
            method.AppendLine("                        stream.CopyTo(ms);");
            method.AppendLine("                        encryptedPayload = ms.ToArray();");
            method.AppendLine("                    }");
            method.AppendLine("                }");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Payload data copied\");");
            method.AppendLine();
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Starting decryption and decompression...\");");
            method.AppendLine("                byte[] originalAssembly = DecryptAndDecompress(encryptedPayload);");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Decrypted assembly: \" + originalAssembly.Length + \" bytes\");");

            // Добавляем обработчик для загрузки зависимостей из текущей папки
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Setting up dependency resolver...\");");
            method.AppendLine("                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>");
            method.AppendLine("                {");
            if (_options.EnableDebugOutput)
                method.AppendLine("                    Console.WriteLine(\"[DEBUG] Resolving dependency: \" + args.Name);");
            method.AppendLine("                    string assemblyName = new AssemblyName(args.Name).Name + \".dll\";");
            method.AppendLine("                    string assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);");
            if (_options.EnableDebugOutput)
                method.AppendLine("                    Console.WriteLine(\"[DEBUG] Looking for: \" + assemblyPath);");
            method.AppendLine("                    if (File.Exists(assemblyPath))");
            method.AppendLine("                    {");
            if (_options.EnableDebugOutput)
                method.AppendLine("                        Console.WriteLine(\"[DEBUG] Loading dependency from: \" + assemblyPath);");
            method.AppendLine("                        return Assembly.LoadFrom(assemblyPath);");
            method.AppendLine("                    }");
            if (_options.EnableDebugOutput)
                method.AppendLine("                    Console.WriteLine(\"[DEBUG] Dependency not found: \" + assemblyName);");
            method.AppendLine("                    return null;");
            method.AppendLine("                };");
            method.AppendLine();

            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Loading assembly...\");");
            method.AppendLine("                Assembly assembly = Assembly.Load(originalAssembly);");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Assembly loaded successfully\");");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Getting entry point...\");");
            method.AppendLine("                MethodInfo entryPoint = assembly.EntryPoint;");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Invoking entry point...\");");
            method.AppendLine("                if (entryPoint.GetParameters().Length > 0)");
            method.AppendLine("                    entryPoint.Invoke(null, new object[] { new string[0] });");
            method.AppendLine("                else");
            method.AppendLine("                    entryPoint.Invoke(null, null);");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] Entry point invoked successfully\");");
            method.AppendLine("            }");
            method.AppendLine("            catch (Exception ex)");
            method.AppendLine("            {");
            if (_options.EnableDebugOutput)
            {
                method.AppendLine("                Console.WriteLine(\"[ERROR] Error in protected loader: \" + ex.Message);");
                method.AppendLine("                Console.WriteLine(\"[ERROR] Stack trace: \" + ex.StackTrace);");
                method.AppendLine("                ");
                method.AppendLine("                // Show inner exception (actual error from packed application)");
                method.AppendLine("                if (ex.InnerException != null)");
                method.AppendLine("                {");
                method.AppendLine("                    Console.WriteLine(\"[ERROR] ===== ACTUAL ERROR FROM APPLICATION =====\");");
                method.AppendLine("                    Console.WriteLine(\"[ERROR] Inner exception: \" + ex.InnerException.Message);");
                method.AppendLine("                    Console.WriteLine(\"[ERROR] Inner exception type: \" + ex.InnerException.GetType().FullName);");
                method.AppendLine("                    Console.WriteLine(\"[ERROR] Inner stack trace: \" + ex.InnerException.StackTrace);");
                method.AppendLine("                    ");
                method.AppendLine("                    // Check for nested inner exceptions");
                method.AppendLine("                    if (ex.InnerException.InnerException != null)");
                method.AppendLine("                    {");
                method.AppendLine("                        Console.WriteLine(\"[ERROR] Nested inner exception: \" + ex.InnerException.InnerException.Message);");
                method.AppendLine("                        Console.WriteLine(\"[ERROR] Nested stack trace: \" + ex.InnerException.InnerException.StackTrace);");
                method.AppendLine("                    }");
                method.AppendLine("                }");
                method.AppendLine("                Console.WriteLine(\"Press any key to exit...\");");
                method.AppendLine("                Console.ReadKey();");
            }
            method.AppendLine("                Environment.Exit(-1);");
            method.AppendLine("            }");
            method.AppendLine("        }");
            
            return method.ToString();
        }

        /// <summary>
        /// Генерация метода загрузки для DLL stub'а
        /// </summary>
        private string GenerateDllLoaderMethod(byte[] encryptedPayload)
        {
            var method = new StringBuilder();

            // Для DLL создаем статический конструктор, который загружается автоматически
            method.AppendLine("        private static Assembly _loadedAssembly = null;");
            method.AppendLine("        private static object _initLock = new object();");
            method.AppendLine("        private static bool _initialized = false;");
            method.AppendLine();

            method.AppendLine("        static ProtectedLoader()");
            method.AppendLine("        {");
            method.AppendLine("            try");
            method.AppendLine("            {");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Static constructor called\");");

            if (_options.EnableAntiDebug)
            {
                if (_options.EnableDebugOutput)
                    method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Performing security checks...\");");
                method.AppendLine("                if (!PerformSecurityChecks())");
                method.AppendLine("                {");
                if (_options.EnableDebugOutput)
                    method.AppendLine("                    Console.WriteLine(\"[DEBUG] DLL Loader: Security check failed\");");
                method.AppendLine("                    Environment.Exit(-1);");
                method.AppendLine("                }");
            }

            // Загружаем зашифрованные данные из embedded resource
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Loading payload from embedded resource...\");");

            method.AppendLine("                byte[] encryptedPayload;");
            method.AppendLine("                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(");
            method.AppendLine("                    Assembly.GetExecutingAssembly().GetManifestResourceNames()[0]))");
            method.AppendLine("                {");
            if (_options.EnableDebugOutput)
                method.AppendLine("                    Console.WriteLine(\"[DEBUG] DLL Loader: Resource stream opened, size: \" + stream.Length + \" bytes\");");
            method.AppendLine("                    using (var ms = new MemoryStream())");
            method.AppendLine("                    {");
            method.AppendLine("                        stream.CopyTo(ms);");
            method.AppendLine("                        encryptedPayload = ms.ToArray();");
            method.AppendLine("                    }");
            method.AppendLine("                }");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Payload decoded\");");
            method.AppendLine();

            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Starting decryption and decompression...\");");
            method.AppendLine("                byte[] originalAssembly = DecryptAndDecompress(encryptedPayload);");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Decrypted assembly: \" + originalAssembly.Length + \" bytes\");");

            // Загружаем оригинальную DLL в память
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Loading original DLL into memory...\");");
            method.AppendLine("                _loadedAssembly = Assembly.Load(originalAssembly);");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Original DLL loaded: \" + _loadedAssembly.FullName);");

            // Регистрируем обработчик разрешения сборок (только для возврата загруженной DLL)
            method.AppendLine();
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Setting up assembly resolver...\");");
            method.AppendLine("                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>");
            method.AppendLine("                {");
            if (_options.EnableDebugOutput)
                method.AppendLine("                    Console.WriteLine(\"[DEBUG] DLL Loader: Resolving assembly: \" + args.Name);");
            method.AppendLine("                    // Если запрашивается наша загруженная DLL, возвращаем её");
            method.AppendLine("                    if (_loadedAssembly != null && args.Name == _loadedAssembly.FullName)");
            method.AppendLine("                    {");
            if (_options.EnableDebugOutput)
                method.AppendLine("                        Console.WriteLine(\"[DEBUG] DLL Loader: Returning loaded assembly\");");
            method.AppendLine("                        return _loadedAssembly;");
            method.AppendLine("                    }");
            method.AppendLine("                    // Для DLL не загружаем сторонние библиотеки");
            method.AppendLine("                    return null;");
            method.AppendLine("                };");
            method.AppendLine();

            method.AppendLine("                _initialized = true;");
            if (_options.EnableDebugOutput)
                method.AppendLine("                Console.WriteLine(\"[DEBUG] DLL Loader: Initialization complete\");");
            method.AppendLine("            }");
            method.AppendLine("            catch (Exception ex)");
            method.AppendLine("            {");
            if (_options.EnableDebugOutput)
            {
                method.AppendLine("                Console.WriteLine(\"[ERROR] DLL Loader error: \" + ex.Message);");
                method.AppendLine("                Console.WriteLine(\"[ERROR] Stack trace: \" + ex.StackTrace);");
                method.AppendLine("                if (ex.InnerException != null)");
                method.AppendLine("                {");
                method.AppendLine("                    Console.WriteLine(\"[ERROR] Inner exception: \" + ex.InnerException.Message);");
                method.AppendLine("                    Console.WriteLine(\"[ERROR] Inner stack trace: \" + ex.InnerException.StackTrace);");
                method.AppendLine("                }");
            }
            method.AppendLine("                Environment.Exit(-1);");
            method.AppendLine("            }");
            method.AppendLine("        }");
            method.AppendLine();

            // Добавляем публичный метод для получения загруженной сборки
            method.AppendLine("        public static Assembly GetLoadedAssembly()");
            method.AppendLine("        {");
            method.AppendLine("            return _loadedAssembly;");
            method.AppendLine("        }");

            return method.ToString();
        }

        /// <summary>
        /// Компиляция stub'а в исполняемый файл с помощью CodeDom
        /// </summary>
        private byte[] CompileStubToExecutable(string sourceCode, byte[] embeddedPayload)
        {
            Console.WriteLine("⚡ Компиляция защищенного загрузчика...");

            if (_options.EnablePackerDebug)
            {
                Console.WriteLine($"🐛 [DEBUG] Код stub'а для компиляции ({sourceCode.Length} символов):");
                Console.WriteLine($"🐛 [DEBUG] Размер встроенного ресурса: {embeddedPayload.Length} байт");
                Console.WriteLine("🐛 [DEBUG] --- Начало исходного кода ---");
                // Показываем первые 500 символов кода
                string preview = sourceCode.Length > 500 ? sourceCode.Substring(0, 500) + "..." : sourceCode;
                Console.WriteLine($"🐛 [DEBUG] {preview}");
                Console.WriteLine("🐛 [DEBUG] --- Конец исходного кода ---");

                // Сохраняем полный код в файл для анализа
                try
                {
                    string debugCodePath = Path.Combine(Path.GetTempPath(), "ProtectionPacker_Debug_Code.cs");
                    File.WriteAllText(debugCodePath, sourceCode);
                    Console.WriteLine($"🐛 [DEBUG] Полный код сохранен в: {debugCodePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"🐛 [DEBUG] Не удалось сохранить код: {ex.Message}");
                }
            }

            // Создаем временный файл для ресурса
            string tempResourcePath = Path.Combine(Path.GetTempPath(), "payload_" + Guid.NewGuid().ToString("N") + ".bin");
            try
            {
                File.WriteAllBytes(tempResourcePath, embeddedPayload);
                if (_options.EnablePackerDebug)
                    Console.WriteLine($"🐛 [DEBUG] Временный ресурс создан: {tempResourcePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка создания временного ресурса: {ex.Message}");
                throw;
            }
            
            // Создаем провайдер компилятора C#
            using (var codeProvider = new CSharpCodeProvider())
            {
                // Выбираем тип приложения на основе настроек и типа файла
                string targetType;
                bool generateExecutable;

                if (_isDllFile)
                {
                    targetType = "library";
                    generateExecutable = false;
                    if (_options.EnablePackerDebug)
                        Console.WriteLine($"🐛 [DEBUG] Компиляция в DLL библиотеку ({_targetPlatform})");
                }
                else
                {
                    targetType = _options.ApplicationType == ApplicationType.ConsoleApp ? "exe" : "winexe";
                    generateExecutable = true;
                    if (_options.EnablePackerDebug)
                        Console.WriteLine($"🐛 [DEBUG] Компиляция в {targetType.ToUpper()} ({_targetPlatform})");
                }

                // Настраиваем параметры компиляции
                var compilerParams = new CompilerParameters
                {
                    GenerateExecutable = generateExecutable,
                    GenerateInMemory = false, // Изменяем на false для создания файла
                    IncludeDebugInformation = false,
                    CompilerOptions = $"/target:{targetType} /optimize+ /platform:{_targetPlatform}", // Выбираем тип приложения и архитектуру
                    TreatWarningsAsErrors = false,
                    WarningLevel = 0
                };

                // Добавляем необходимые сборки
                compilerParams.ReferencedAssemblies.Add("System.dll");
                compilerParams.ReferencedAssemblies.Add("System.Core.dll");
                compilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                compilerParams.ReferencedAssemblies.Add("System.Management.dll");
                compilerParams.ReferencedAssemblies.Add("System.Drawing.dll");
                compilerParams.ReferencedAssemblies.Add("mscorlib.dll");

                // Добавляем встроенный ресурс с полезной нагрузкой
                compilerParams.EmbeddedResources.Add(tempResourcePath);

                if (_options.EnablePackerDebug)
                {
                    Console.WriteLine("🐛 [DEBUG] Параметры компиляции:");
                    Console.WriteLine($"      - GenerateExecutable: {compilerParams.GenerateExecutable}");
                    Console.WriteLine($"      - GenerateInMemory: {compilerParams.GenerateInMemory}");
                    Console.WriteLine($"      - CompilerOptions: {compilerParams.CompilerOptions}");
                    Console.WriteLine($"      - Ссылки на сборки: {string.Join(", ", compilerParams.ReferencedAssemblies.Cast<string>())}");
                }

                // Компилируем код
                if (_options.EnablePackerDebug)
                    Console.WriteLine("🐛 [DEBUG] Запуск компилятора C#...");
                CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParams, sourceCode);

                // Проверяем результат компиляции
                if (_options.EnablePackerDebug)
                    Console.WriteLine($"🐛 [DEBUG] Компиляция завершена, ошибок: {results.Errors.Count}, предупреждений: {results.Errors.Cast<CompilerError>().Count(e => e.IsWarning)}");
                    
                if (results.Errors.HasErrors)
                {
                    Console.WriteLine("❌ Ошибки компиляции stub'а:");
                    foreach (CompilerError error in results.Errors)
                    {
                        Console.WriteLine($"   Строка {error.Line}: {error.ErrorText}");
                        if (_options.EnablePackerDebug)
                            Console.WriteLine($"      🐛 [DEBUG] Файл: {error.FileName}, Столбец: {error.Column}");
                    }
                    throw new Exception("Не удалось скомпилировать защищенный загрузчик");
                }

                Console.WriteLine("✅ Компиляция успешна");
                if (_options.EnablePackerDebug)
                    Console.WriteLine($"🐛 [DEBUG] Путь к скомпилированной сборке: {results.PathToAssembly}");

                // Получаем байты скомпилированной сборки
                string compiledPath = results.PathToAssembly;
                if (string.IsNullOrEmpty(compiledPath))
                {
                    throw new Exception("Не удалось получить путь к скомпилированной сборке");
                }

                byte[] compiledBytes = File.ReadAllBytes(compiledPath);

                // Удаляем временные файлы
                try
                {
                    File.Delete(compiledPath);
                    File.Delete(tempResourcePath);
                    if (_options.EnablePackerDebug)
                        Console.WriteLine($"🐛 [DEBUG] Временные файлы удалены");
                }
                catch (Exception ex)
                {
                    if (_options.EnablePackerDebug)
                        Console.WriteLine($"🐛 [DEBUG] Не удалось удалить временные файлы: {ex.Message}");
                }

                return compiledBytes;
            }
        }
    }
} 