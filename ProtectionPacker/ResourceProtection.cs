using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProtectionPacker
{
    /// <summary>
    /// Класс для генерации runtime защиты ресурсов в stub'е
    /// </summary>
    public class ResourceProtection
    {
        private readonly byte[] _resourceKey;
        private readonly byte[] _resourceIV;
        private readonly Random _random;
        private readonly Dictionary<string, byte[]> _protectedResources;
        private readonly List<string> _resourceNames;

        public ResourceProtection()
        {
            _random = new Random();
            _protectedResources = new Dictionary<string, byte[]>();
            _resourceNames = new List<string>();
            
            // Генерируем ключ шифрования для ресурсов
            using (var rng = new RNGCryptoServiceProvider())
            {
                _resourceKey = new byte[32]; // AES-256
                _resourceIV = new byte[16];  // AES IV
                rng.GetBytes(_resourceKey);
                rng.GetBytes(_resourceIV);
            }
        }

        /// <summary>
        /// НЕ модифицируем оригинальную сборку - просто возвращаем её как есть
        /// Защита будет добавлена в stub через runtime перехват
        /// </summary>
        public byte[] ProtectResources(byte[] assemblyData)
        {
            // Анализируем ресурсы в сборке для последующей защиты
            try
            {
                AnalyzeAssemblyResources(assemblyData);
            }
            catch
            {
                // Если анализ не удался - создаём базовые защищённые ресурсы
                CreateDefaultProtectedResources();
            }

            // Возвращаем оригинальную сборку БЕЗ ИЗМЕНЕНИЙ
            return assemblyData;
        }

        /// <summary>
        /// Анализ ресурсов в сборке для последующей защиты в runtime
        /// </summary>
        private void AnalyzeAssemblyResources(byte[] assemblyData)
        {
            try
            {
                // Загружаем сборку для анализа (НЕ для модификации!)
                var assembly = Assembly.Load(assemblyData);
                
                // Получаем имена манифестных ресурсов
                var resourceNames = assembly.GetManifestResourceNames();
                
                foreach (var resourceName in resourceNames)
                {
                    try
                    {
                        // Загружаем ресурс для анализа
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null && stream.Length > 0 && stream.Length < 10 * 1024 * 1024) // Максимум 10MB
                            {
                                byte[] resourceData = new byte[stream.Length];
                                stream.Read(resourceData, 0, (int)stream.Length);
                                
                                // Защищаем ресурс
                                ProtectResource(resourceName, resourceData);
                            }
                        }
                    }
                    catch
                    {
                        // При ошибке анализа ресурса - пропускаем его
                        _resourceNames.Add(resourceName);
                    }
                }
            }
            catch
            {
                // Если анализ сборки не удался - создаём базовые защищённые ресурсы
                CreateDefaultProtectedResources();
            }
        }

        /// <summary>
        /// Создание базовых защищённых ресурсов
        /// </summary>
        private void CreateDefaultProtectedResources()
        {
            // Создаём защиту для типичных ресурсов
            var commonResources = new Dictionary<string, byte[]>
            {
                ["app.ico"] = GenerateRandomData(1024),
                ["splash.png"] = GenerateRandomData(2048),
                ["config.xml"] = Encoding.UTF8.GetBytes("<configuration><security>protected</security></configuration>"),
                ["license.key"] = Encoding.UTF8.GetBytes("PROTECTED_RESOURCE_DATA"),
            };

            foreach (var resource in commonResources)
            {
                ProtectResource(resource.Key, resource.Value);
            }
        }

        /// <summary>
        /// Генерация случайных данных для тестирования
        /// </summary>
        private byte[] GenerateRandomData(int size)
        {
            byte[] data = new byte[size];
            _random.NextBytes(data);
            return data;
        }

        /// <summary>
        /// Защита ресурса шифрованием
        /// </summary>
        private void ProtectResource(string resourceName, byte[] resourceData)
        {
            try
            {
                // Шифруем ресурс
                byte[] encryptedData = EncryptResource(resourceData);
                
                // Сохраняем зашифрованный ресурс
                _protectedResources[resourceName] = encryptedData;
                _resourceNames.Add(resourceName);
            }
            catch
            {
                // При ошибке шифрования - просто добавляем имя ресурса
                _resourceNames.Add(resourceName);
            }
        }

        /// <summary>
        /// Шифрование ресурса с использованием AES
        /// </summary>
        private byte[] EncryptResource(byte[] resourceData)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _resourceKey;
                aes.IV = _resourceIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var output = new MemoryStream())
                using (var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(resourceData, 0, resourceData.Length);
                    cryptoStream.FlushFinalBlock();
                    return output.ToArray();
                }
            }
        }

        /// <summary>
        /// Генерация кода защиты ресурсов для stub'а
        /// </summary>
        public string GenerateResourceProtectionCode()
        {
            if (_protectedResources.Count == 0 && _resourceNames.Count == 0)
                return string.Empty;

            var code = new StringBuilder();
            
            // Добавляем словарь зашифрованных ресурсов
            if (_protectedResources.Count > 0)
            {
                code.AppendLine("        private static readonly Dictionary<string, byte[]> _encryptedResources = new Dictionary<string, byte[]>");
                code.AppendLine("        {");
                
                foreach (var resource in _protectedResources)
                {
                    var safeKey = resource.Key.Replace("\\", "\\\\").Replace("\"", "\\\"");
                    var base64Data = Convert.ToBase64String(resource.Value);
                    code.AppendLine($"            {{\"{safeKey}\", Convert.FromBase64String(\"{base64Data}\")}},");
                }
                
                code.AppendLine("        };");
                code.AppendLine();
            }

            // Добавляем список имён ресурсов
            code.AppendLine("        private static readonly HashSet<string> _protectedResourceNames = new HashSet<string>");
            code.AppendLine("        {");
            
            foreach (var resourceName in _resourceNames.Distinct())
            {
                var safeName = resourceName.Replace("\\", "\\\\").Replace("\"", "\\\"");
                code.AppendLine($"            \"{safeName}\",");
            }
            
            code.AppendLine("        };");
            code.AppendLine();

            // Добавляем ключи расшифровки
            code.AppendLine($"        private static readonly byte[] _resourceKey = new byte[] {{{string.Join(",", _resourceKey.Select(b => b.ToString()))}}};");
            code.AppendLine($"        private static readonly byte[] _resourceIV = new byte[] {{{string.Join(",", _resourceIV.Select(b => b.ToString()))}}};");
            code.AppendLine();

            // Добавляем методы защиты ресурсов
            code.AppendLine(GenerateResourceProtectionMethods());

            return code.ToString();
        }

        /// <summary>
        /// Генерация методов защиты ресурсов
        /// </summary>
        private string GenerateResourceProtectionMethods()
        {
            var code = new StringBuilder();
            
            code.AppendLine("        /// <summary>");
            code.AppendLine("        /// Перехват загрузки ресурсов для защиты");
            code.AppendLine("        /// </summary>");
            code.AppendLine("        private static byte[] InterceptResourceLoad(string resourceName)");
            code.AppendLine("        {");
            code.AppendLine("            // Проверяем, является ли ресурс защищённым");
            code.AppendLine("            if (!_protectedResourceNames.Contains(resourceName))");
            code.AppendLine("                return null; // Не защищённый ресурс");
            code.AppendLine("            ");
            code.AppendLine("            // Проверяем наличие зашифрованных данных");
            code.AppendLine("            byte[] encryptedData;");
            code.AppendLine("            if (_encryptedResources != null && _encryptedResources.TryGetValue(resourceName, out encryptedData))");
            code.AppendLine("            {");
            code.AppendLine("                try");
            code.AppendLine("                {");
            code.AppendLine("                    return DecryptResourceData(encryptedData);");
            code.AppendLine("                }");
            code.AppendLine("                catch");
            code.AppendLine("                {");
            code.AppendLine("                    return null; // При ошибке расшифровки");
            code.AppendLine("                }");
            code.AppendLine("            }");
            code.AppendLine("            ");
            code.AppendLine("            // Возвращаем заглушку для защищённого ресурса");
            code.AppendLine("            return CreateResourceStub(resourceName);");
            code.AppendLine("        }");
            code.AppendLine();
            
            code.AppendLine("        /// <summary>");
            code.AppendLine("        /// Расшифровка зашифрованных данных ресурса");
            code.AppendLine("        /// </summary>");
            code.AppendLine("        private static byte[] DecryptResourceData(byte[] encryptedData)");
            code.AppendLine("        {");
            code.AppendLine("            try");
            code.AppendLine("            {");
            code.AppendLine("                using (var aes = Aes.Create())");
            code.AppendLine("                {");
            code.AppendLine("                    aes.Key = _resourceKey;");
            code.AppendLine("                    aes.IV = _resourceIV;");
            code.AppendLine("                    aes.Mode = CipherMode.CBC;");
            code.AppendLine("                    aes.Padding = PaddingMode.PKCS7;");
            code.AppendLine();
            code.AppendLine("                    using (var decryptor = aes.CreateDecryptor())");
            code.AppendLine("                    using (var input = new MemoryStream(encryptedData))");
            code.AppendLine("                    using (var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))");
            code.AppendLine("                    using (var output = new MemoryStream())");
            code.AppendLine("                    {");
            code.AppendLine("                        cryptoStream.CopyTo(output);");
            code.AppendLine("                        return output.ToArray();");
            code.AppendLine("                    }");
            code.AppendLine("                }");
            code.AppendLine("            }");
            code.AppendLine("            catch");
            code.AppendLine("            {");
            code.AppendLine("                return null; // При ошибке возвращаем null");
            code.AppendLine("            }");
            code.AppendLine("        }");
            code.AppendLine();
            
            code.AppendLine("        /// <summary>");
            code.AppendLine("        /// Создание заглушки для защищённого ресурса");
            code.AppendLine("        /// </summary>");
            code.AppendLine("        private static byte[] CreateResourceStub(string resourceName)");
            code.AppendLine("        {");
            code.AppendLine("            // Возвращаем минимальную заглушку в зависимости от типа ресурса");
            code.AppendLine("            string extension = Path.GetExtension(resourceName).ToLower();");
            code.AppendLine("            ");
            code.AppendLine("            switch (extension)");
            code.AppendLine("            {");
            code.AppendLine("                case \".png\":");
            code.AppendLine("                    // Минимальный PNG (1x1 прозрачный пиксель)");
            code.AppendLine("                    return Convert.FromBase64String(\"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChAI9nfSSSQAAAABJRU5ErkJggg==\");");
            code.AppendLine("                ");
            code.AppendLine("                case \".jpg\":");
            code.AppendLine("                case \".jpeg\":");
            code.AppendLine("                    // Минимальный JPEG (1x1 чёрный пиксель)");
            code.AppendLine("                    return Convert.FromBase64String(\"/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/fA==\");");
            code.AppendLine("                ");
            code.AppendLine("                case \".ico\":");
            code.AppendLine("                    // Минимальный ICO (16x16 прозрачная иконка)");
            code.AppendLine("                    return Convert.FromBase64String(\"AAABAAEAEBAAAAAAAABoBAAAFgAAACgAAAAQAAAAIAAAAAEAIAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAA==\");");
            code.AppendLine("                ");
            code.AppendLine("                case \".xml\":");
            code.AppendLine("                    return Encoding.UTF8.GetBytes(\"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><root><protected>true</protected></root>\");");
            code.AppendLine("                ");
            code.AppendLine("                case \".txt\":");
            code.AppendLine("                    return Encoding.UTF8.GetBytes(\"[PROTECTED RESOURCE]\");");
            code.AppendLine("                ");
            code.AppendLine("                default:");
            code.AppendLine("                    return Encoding.UTF8.GetBytes(\"PROTECTED_RESOURCE_DATA\");");
            code.AppendLine("            }");
            code.AppendLine("        }");
            
            return code.ToString();
        }
    }
} 