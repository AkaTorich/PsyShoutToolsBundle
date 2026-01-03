//FileHasher
using System;
using System.IO;
using System.Security.Cryptography;

namespace BackupManager.Services
{
    public class FileHasher
    {
        // Вычисление хеша файла
        public string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}