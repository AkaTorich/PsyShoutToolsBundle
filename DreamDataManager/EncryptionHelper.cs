// EncryptionHelper.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DreamDiary
{
    public static class EncryptionHelper
    {
        /// <summary>
        /// Генерирует ключ и IV на основе пароля с использованием Rfc2898DeriveBytes.
        /// </summary>
        /// <param name="password">Пароль пользователя.</param>
        /// <param name="key">Сгенерированный ключ.</param>
        /// <param name="iv">Сгенерированный IV.</param>
        public static void GenerateKeyAndIV(string password, out byte[] key, out byte[] iv)
        {
            // Используем фиксированную соль. Для лучшей безопасности рекомендуется использовать случайную соль и хранить её вместе с зашифрованными данными.
            byte[] salt = Encoding.UTF8.GetBytes("SaltIsGood1234"); // 16 байт

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                key = deriveBytes.GetBytes(32); // 256 бит для AES-256
                iv = deriveBytes.GetBytes(16);  // 128 бит для IV
            }
        }

        /// <summary>
        /// Шифрует строку в массив байтов.
        /// </summary>
        /// <param name="plainText">Чистый текст для шифрования.</param>
        /// <param name="key">Ключ шифрования.</param>
        /// <param name="iv">Инициализационный вектор.</param>
        /// <returns>Зашифрованные данные.</returns>
        public static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt =
                        new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt =
                            new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        /// <summary>
        /// Расшифровывает массив байтов в строку.
        /// </summary>
        /// <param name="cipherText">Зашифрованные данные.</param>
        /// <param name="key">Ключ шифрования.</param>
        /// <param name="iv">Инициализационный вектор.</param>
        /// <returns>Расшифрованный текст.</returns>
        public static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt =
                        new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt =
                            new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
