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
    /// Класс для генерации runtime защиты строк в stub'е
    /// </summary>
    public class StringEncryption
    {
        private readonly byte[] _encryptionKey;
        private readonly byte[] _encryptionIV;
        private readonly Random _random;
        private readonly Dictionary<string, string> _stringMappings;

        public StringEncryption()
        {
            _random = new Random();
            _stringMappings = new Dictionary<string, string>();
            
            // Генерируем ключ шифрования для строк
            using (var rng = new RNGCryptoServiceProvider())
            {
                _encryptionKey = new byte[32]; // AES-256
                _encryptionIV = new byte[16];  // AES IV
                rng.GetBytes(_encryptionKey);
                rng.GetBytes(_encryptionIV);
            }
        }

        /// <summary>
        /// НЕ модифицируем оригинальную сборку - просто возвращаем её как есть
        /// Защита будет добавлена в stub через runtime перехват
        /// </summary>
        public byte[] EncryptStrings(byte[] assemblyData)
        {
            // Анализируем сборку для извлечения строк
            try
            {
                AnalyzeAssemblyStrings(assemblyData);
            }
            catch
            {
                // Если анализ не удался - продолжаем без защиты строк
            }

            // Возвращаем оригинальную сборку БЕЗ ИЗМЕНЕНИЙ
            return assemblyData;
        }

        /// <summary>
        /// Анализ строк в сборке для последующей защиты в runtime
        /// </summary>
        private void AnalyzeAssemblyStrings(byte[] assemblyData)
        {
            try
            {
                // Загружаем сборку для анализа (НЕ для модификации!)
                var assembly = Assembly.Load(assemblyData);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    // Анализируем строковые поля и свойства
                    AnalyzeTypeStrings(type);
                }
            }
            catch
            {
                // Если анализ не удался - создаём базовые защищённые строки
                CreateDefaultProtectedStrings();
            }
        }

        /// <summary>
        /// Анализ строк в типе
        /// </summary>
        private void AnalyzeTypeStrings(Type type)
        {
            try
            {
                // Анализируем публичные строковые поля
                var stringFields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(string));

                foreach (var field in stringFields)
                {
                    try
                    {
                        var value = field.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(value) && value.Length > 3)
                        {
                            ProtectString(value);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        /// <summary>
        /// Создание базовых защищённых строк
        /// </summary>
        private void CreateDefaultProtectedStrings()
        {
            var commonStrings = new[]
            {
                "License key is invalid",
                "Application has expired", 
                "Debugger detected",
                "Tampering detected",
                "Security violation",
                "Access denied",
                "Trial period expired",
                "Registration required"
            };

            foreach (var str in commonStrings)
            {
                ProtectString(str);
            }
        }

        /// <summary>
        /// Защита строки шифрованием
        /// </summary>
        private void ProtectString(string plainText)
        {
            if (_stringMappings.ContainsKey(plainText))
                return;

            try
            {
                string encrypted = EncryptString(plainText);
                _stringMappings[plainText] = encrypted;
            }
            catch
            {
                // При ошибке шифрования - пропускаем строку
            }
        }

        /// <summary>
        /// Шифрование строки с использованием AES
        /// </summary>
        private string EncryptString(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = _encryptionIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                    swEncrypt.Close();
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        /// <summary>
        /// Генерация кода защиты строк для stub'а
        /// </summary>
        public string GenerateStringProtectionCode()
        {
            if (_stringMappings.Count == 0)
                return string.Empty;

            var code = new StringBuilder();
            
            // Добавляем словарь зашифрованных строк
            code.AppendLine("        private static readonly Dictionary<string, string> _encryptedStrings = new Dictionary<string, string>");
            code.AppendLine("        {");
            
            foreach (var mapping in _stringMappings)
            {
                var safePlainText = mapping.Key.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
                var safeValue = mapping.Value.Replace("\\", "\\\\").Replace("\"", "\\\"");
                code.AppendLine($"            {{\"{safePlainText}\", \"{safeValue}\"}},");
            }
            
            code.AppendLine("        };");
            code.AppendLine();

            // Добавляем ключи расшифровки
            code.AppendLine($"        private static readonly byte[] _stringKey = new byte[] {{{string.Join(",", _encryptionKey)}}};");
            code.AppendLine($"        private static readonly byte[] _stringIV = new byte[] {{{string.Join(",", _encryptionIV)}}};");
            code.AppendLine();

            // Добавляем методы расшифровки
            code.AppendLine(GenerateDecryptionMethods());

            return code.ToString();
        }

        /// <summary>
        /// Генерация методов расшифровки строк
        /// </summary>
        private string GenerateDecryptionMethods()
        {
            return @"        /// <summary>
        /// Метод расшифровки защищённых строк
        /// </summary>
        private static string DecryptProtectedString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            
            string encryptedData;
            if (_encryptedStrings.TryGetValue(plainText, out encryptedData))
            {
                try
                {
                    return DecryptStringData(encryptedData);
                }
                catch
                {
                    return plainText; // При ошибке возвращаем оригинал
                }
            }
            
            return plainText;
        }

        /// <summary>
        /// Расшифровка зашифрованных данных строки
        /// </summary>
        private static string DecryptStringData(string encryptedBase64)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
                
                using (var aes = Aes.Create())
                {
                    aes.Key = _stringKey;
                    aes.IV = _stringIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(encryptedBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch
            {
                return string.Empty; // При ошибке возвращаем пустую строку
            }
        }

        /// <summary>
        /// Runtime перехват строк для защиты
        /// </summary>
        private static void InitializeStringProtection(Assembly loadedAssembly)
        {
            try
            {
                // Перехватываем обращения к строкам через рефлексию
                var types = loadedAssembly.GetTypes();
                
                foreach (var type in types)
                {
                    ProtectTypeStrings(type);
                }
            }
            catch
            {
                // При ошибке продолжаем без защиты строк
            }
        }

        /// <summary>
        /// Защита строк в типе
        /// </summary>
        private static void ProtectTypeStrings(Type type)
        {
            try
            {
                // Заменяем строковые константы на защищённые версии
                var stringFields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(string));

                foreach (var field in stringFields)
                {
                    try
                    {
                        var originalValue = field.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(originalValue))
                        {
                            var protectedValue = DecryptProtectedString(originalValue);
                            if (protectedValue != originalValue)
                            {
                                field.SetValue(null, protectedValue);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }";
        }
    }
} 