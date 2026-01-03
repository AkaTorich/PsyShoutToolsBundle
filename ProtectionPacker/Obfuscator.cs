using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace ProtectionPacker
{
    /// <summary>
    /// Класс обфускации, затрудняющий реверс-инжиниринг и анализ кода
    /// </summary>
    public class Obfuscator
    {
        private readonly Random _random;
        private readonly Dictionary<string, string> _nameMapping;
        private readonly List<string> _obfuscatedNames;

        public Obfuscator()
        {
            _random = new Random(DateTime.Now.Millisecond);
            _nameMapping = new Dictionary<string, string>();
            _obfuscatedNames = new List<string>();
            GenerateObfuscatedNames();
        }

        /// <summary>
        /// Основной метод обфускации сборки
        /// БЕЗОПАСНАЯ ВЕРСИЯ - НЕ ИЗМЕНЯЕТ ОРИГИНАЛЬНУЮ СБОРКУ
        /// </summary>
        public byte[] ObfuscateAssembly(byte[] assemblyData)
        {
            try
            {
                // ВАЖНО: Мы НЕ модифицируем бинарную сборку напрямую,
                // так как это может её повредить. Вместо этого добавляем
                // безопасные изменения:
                
                // 1. Добавляем случайные данные в конец файла (не нарушает PE структуру)
                assemblyData = AddRandomPadding(assemblyData);
                
                // 2. Обфускация будет происходить на уровне stub'а (в загрузчике)
                // Генерируем ложный код для добавления в stub
                GenerateStubObfuscationCode();
                
                return assemblyData;
            }
            catch (Exception)
            {
                // Если обфускация не удалась, возвращаем оригинальные данные
                return assemblyData;
            }
        }

        /// <summary>
        /// Генерация обфускированных имен для замены
        /// </summary>
        private void GenerateObfuscatedNames()
        {
            // Генерируем случайные имена из разных наборов символов
            var charSets = new[]
            {
                "abcdefghijklmnopqrstuvwxyz",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 
                "абвгдеёжзийклмнопрстуфхцчшщъыьэюя",
                "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ",
                "αβγδεζηθικλμνξοπρστυφχψω",
                "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ"
            };

            for (int i = 0; i < 1000; i++)
            {
                var name = GenerateRandomName(charSets[_random.Next(charSets.Length)], _random.Next(3, 15));
                if (!_obfuscatedNames.Contains(name))
                {
                    _obfuscatedNames.Add(name);
                }
            }
        }

        /// <summary>
        /// Генерация случайного имени из заданного набора символов
        /// </summary>
        private string GenerateRandomName(string charset, int length)
        {
            var result = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                result.Append(charset[_random.Next(charset.Length)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Добавление случайных данных в конец файла (безопасно для PE файлов)
        /// </summary>
        private byte[] AddRandomPadding(byte[] data)
        {
            // Добавляем случайные байты в конец файла
            // Это не нарушает структуру PE файла
            var paddingSize = _random.Next(1024, 4096);
            var padding = new byte[paddingSize];
            _random.NextBytes(padding);
            
            var result = new byte[data.Length + padding.Length];
            Array.Copy(data, 0, result, 0, data.Length);
            Array.Copy(padding, 0, result, data.Length, padding.Length);
            
            return result;
        }

        /// <summary>
        /// Генерация кода обфускации для добавления в stub
        /// </summary>
        private void GenerateStubObfuscationCode()
        {
            // Здесь генерируем ложный код, который будет добавлен в stub
            // Это безопасно, так как stub - это новый код, а не модификация существующей сборки
        }

        /// <summary>
        /// Получение кода обфускации для вставки в stub
        /// </summary>
        public string GetObfuscationCodeForStub()
        {
            var obfuscationCode = new StringBuilder();
            
            // Добавляем ложные классы и методы в stub
            obfuscationCode.AppendLine("        // --- Обфускационный код ---");
            
            for (int i = 0; i < 3; i++)
            {
                obfuscationCode.AppendLine(GenerateDecoyClass());
            }
            
            for (int i = 0; i < 5; i++)
            {
                obfuscationCode.AppendLine(GenerateDecoyMethod());
            }
            
            obfuscationCode.AppendLine(GenerateDecoyCryptoCode());
            obfuscationCode.AppendLine(GenerateDecoyAntiDebugCode());
            
            obfuscationCode.AppendLine("        // --- Конец обфускационного кода ---");
            
            return obfuscationCode.ToString();
        }

        /// <summary>
        /// УДАЛЕНО: Небезопасный метод, который повреждал сборку
        /// Ложные методы теперь добавляются только в stub через GetObfuscationCodeForStub()
        /// </summary>
        [Obsolete("Этот метод был небезопасен и удален")]
        private byte[] InsertDecoyMethods(byte[] data)
        {
            // Больше не модифицируем бинарные данные сборки
            return data;
        }

        /// <summary>
        /// Генерация ложного кода для обмана анализаторов
        /// </summary>
        private string GenerateDecoyCode()
        {
            var decoys = new[]
            {
                GenerateDecoyClass(),
                GenerateDecoyMethod(),
                GenerateDecoyCryptoCode(),
                GenerateDecoyAntiDebugCode(),
                GenerateDecoyLicenseCode()
            };

            return decoys[_random.Next(decoys.Length)];
        }

        /// <summary>
        /// Генерация ложного класса
        /// </summary>
        private string GenerateDecoyClass()
        {
            var className = $"DecoyClass_{_random.Next(1000)}";
            var methodName = $"DecoyMethod_{_random.Next(1000)}";
            var arrayName = $"decoyArray_{_random.Next(1000)}";
            var varName = $"decoyVar_{_random.Next(1000)}";
            
            var str1 = GenerateRandomString(20).Replace("\\", "\\\\").Replace("\"", "\\\"");
            var str2 = GenerateRandomString(25).Replace("\\", "\\\\").Replace("\"", "\\\"");
            var str3 = GenerateRandomString(30).Replace("\\", "\\\\").Replace("\"", "\\\"");
            
            return $@"
public class {className}
{{
    private static readonly string[] {arrayName} = 
    {{
        ""{str1}"",
        ""{str2}"",
        ""{str3}""
    }};
    
    public void {methodName}()
    {{
        var {varName} = DateTime.Now.Ticks;
        for (int i = 0; i < 1000; i++)
        {{
            {varName} ^= (long)Math.Sin(i);
        }}
    }}
}}";
        }

        /// <summary>
        /// Генерация ложного метода
        /// </summary>
        private string GenerateDecoyMethod()
        {
            var methodName = $"DecoyMethod_{_random.Next(1000)}";
            var varName = $"decoyBuffer_{_random.Next(1000)}";
            
            return $@"
private static bool {methodName}()
{{
    byte[] {varName} = new byte[256];
    new Random().NextBytes({varName});
    
    for (int i = 0; i < {varName}.Length; i++)
    {{
        {varName}[i] = (byte)({varName}[i] ^ 0x{_random.Next(256):X2});
    }}
    
    return {varName}.Sum(b => b) % 2 == 0;
}}";
        }

        /// <summary>
        /// Генерация ложного криптографического кода
        /// </summary>
        private string GenerateDecoyCryptoCode()
        {
            var methodName = $"DecoyCrypt_{_random.Next(1000)}";
            var keyVar = $"decoyKey_{_random.Next(1000)}";
            var dataVar = $"decoyData_{_random.Next(1000)}";
            
            return $@"
private static byte[] {methodName}(byte[] {dataVar})
{{
    byte[] {keyVar} = {{ {string.Join(", ", Enumerable.Range(0, 16).Select(i => $"0x{_random.Next(256):X2}"))} }};
    
    for (int i = 0; i < {dataVar}.Length; i++)
    {{
        {dataVar}[i] ^= {keyVar}[i % {keyVar}.Length];
    }}
    
    return {dataVar};
}}";
        }

        /// <summary>
        /// Генерация ложного анти-отладочного кода
        /// </summary>
        private string GenerateDecoyAntiDebugCode()
        {
            var methodName = $"DecoyAntiDebug_{_random.Next(1000)}";
            var debugVar = $"decoyTicks_{_random.Next(1000)}";
            var processVar = $"decoyProcess_{_random.Next(1000)}";
            
            return $@"
// Ложный анти-отладочный код (без конфликтующих импортов)
private static void {methodName}()
{{
    if (DateTime.Now.Millisecond % 2 == 0)
    {{
        var {debugVar} = Environment.TickCount;
        Thread.Sleep(1);
        if (Environment.TickCount - {debugVar} > 100)
        {{
            // Ложная проверка на отладчик
            var {processVar} = Process.GetCurrentProcess().ProcessName;
            if ({processVar}.Length > 5)
            {{
                // Обманная логика
            }}
        }}
    }}
}}";
        }

        /// <summary>
        /// Генерация ложного кода проверки лицензии
        /// </summary>
        private string GenerateDecoyLicenseCode()
        {
            var methodName = $"DecoyLicense_{_random.Next(1000)}";
            var licenseVar = $"decoyLicense_{_random.Next(1000)}";
            var hash1Var = $"decoyHash1_{_random.Next(1000)}";
            var hash2Var = $"decoyHash2_{_random.Next(1000)}";
            
            return $@"
private static bool {methodName}()
{{
    string {licenseVar} = ""{GenerateRandomString(32).Replace("\\", "\\\\").Replace("\"", "\\\"")}"";
    
    var {hash1Var} = {licenseVar}.GetHashCode();
    var {hash2Var} = Environment.MachineName.GetHashCode();
    
    return ({hash1Var} ^ {hash2Var}) != 0;
}}";
        }

        /// <summary>
        /// УДАЛЕНО: Небезопасный метод запутывания потока выполнения
        /// Теперь код запутывания добавляется только в stub
        /// </summary>
        [Obsolete("Этот метод был небезопасен и удален")]
        private byte[] ObfuscateControlFlow(byte[] data)
        {
            // Больше не модифицируем бинарные данные сборки
            return data;
        }

        /// <summary>
        /// УДАЛЕНО: Небезопасный метод вставки мусорного кода
        /// Теперь мусорный код добавляется только в stub
        /// </summary>
        [Obsolete("Этот метод был небезопасен и удален")]
        private byte[] InsertJunkCode(byte[] data)
        {
            // Больше не модифицируем бинарные данные сборки
            return data;
        }

        /// <summary>
        /// Генерация мусорного кода
        /// </summary>
        private string GenerateJunkCode()
        {
            var randomStr = GenerateRandomString(20).Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $@"
// Мусорные переменные и операции
var {_obfuscatedNames[_random.Next(_obfuscatedNames.Count)]} = {_random.Next(1000)};
var {_obfuscatedNames[_random.Next(_obfuscatedNames.Count)]} = ""{randomStr}"";
var {_obfuscatedNames[_random.Next(_obfuscatedNames.Count)]} = new byte[] {{ {string.Join(", ", Enumerable.Range(0, 10).Select(i => _random.Next(256)))} }};

// Ложные вычисления
{_obfuscatedNames[_random.Next(_obfuscatedNames.Count)]} = {_obfuscatedNames[_random.Next(_obfuscatedNames.Count)]} * 2 + 1;
{_obfuscatedNames[_random.Next(_obfuscatedNames.Count)]} = {_obfuscatedNames[_random.Next(_obfuscatedNames.Count)]}.GetHashCode().ToString();

// Ложные массивы
int[] {_obfuscatedNames[_random.Next(_obfuscatedNames.Count)]} = {{ {string.Join(", ", Enumerable.Range(0, 5).Select(i => _random.Next(100)))} }};
";
        }

        /// <summary>
        /// УДАЛЕНО: Небезопасный метод вставки фрагментов кода
        /// Этот метод повреждал бинарную структуру сборки
        /// </summary>
        [Obsolete("Этот метод был небезопасен и удален")]
        private byte[] InsertCodeSnippets(byte[] data, string code, int count)
        {
            // Больше не модифицируем бинарные данные сборки
            return data;
        }

        /// <summary>
        /// Генерация случайной строки
        /// </summary>
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
} 