// PasswordManager.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DreamDiary
{
    public static class PasswordManager
    {
        private static readonly string passwordFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "password.dat");

        /// <summary>
        /// Устанавливает новый пароль, сохраняя его хэш и соль в файл.
        /// </summary>
        /// <param name="password">Новый пароль.</param>
        public static void SetPassword(string password)
        {
            // Генерируем случайную соль
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Генерируем хэш пароля с использованием соли
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                byte[] key = deriveBytes.GetBytes(32); // 256 бит
                using (var fs = new FileStream(passwordFilePath, FileMode.Create))
                {
                    fs.Write(salt, 0, salt.Length);
                    fs.Write(key, 0, key.Length);
                }
            }
        }

        /// <summary>
        /// Проверяет введённый пароль на соответствие сохранённому хэшу.
        /// </summary>
        /// <param name="enteredPassword">Введённый пароль.</param>
        /// <returns>Истина, если пароль верен; иначе ложь.</returns>
        public static bool ValidatePassword(string enteredPassword)
        {
            if (!File.Exists(passwordFilePath))
                throw new FileNotFoundException("Файл пароля не найден.");

            byte[] salt = new byte[16];
            byte[] storedKey = new byte[32];

            using (var fs = new FileStream(passwordFilePath, FileMode.Open))
            {
                fs.Read(salt, 0, salt.Length);
                fs.Read(storedKey, 0, storedKey.Length);
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(enteredPassword, salt, 10000))
            {
                byte[] enteredKey = deriveBytes.GetBytes(32);
                return CompareBytes(enteredKey, storedKey);
            }
        }

        /// <summary>
        /// Сравнивает два массива байтов.
        /// </summary>
        /// <param name="a">Первый массив.</param>
        /// <param name="b">Второй массив.</param>
        /// <returns>Истина, если массивы равны; иначе ложь.</returns>
        private static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}
