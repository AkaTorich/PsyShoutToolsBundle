// DreamDataManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace DreamDiary
{
    public static class DreamDataManager
    {
        private static readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dreams.dat");

        /// <summary>
        /// Получает путь к файлу данных.
        /// </summary>
        /// <returns>Путь к файлу данных.</returns>
        public static string GetDataFilePath()
        {
            return filePath;
        }

        /// <summary>
        /// Загружает список снов из зашифрованного файла.
        /// </summary>
        /// <param name="key">Ключ шифрования.</param>
        /// <param name="iv">Инициализационный вектор.</param>
        /// <returns>Список снов.</returns>
        public static List<Dream> LoadDreams(byte[] key, byte[] iv)
        {
            if (!File.Exists(filePath))
                return new List<Dream>();

            try
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string decryptedXml = EncryptionHelper.DecryptStringFromBytes(encryptedData, key, iv);

                XmlSerializer serializer = new XmlSerializer(typeof(List<Dream>));
                using (StringReader sr = new StringReader(decryptedXml))
                {
                    return (List<Dream>)serializer.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<Dream>();
            }
        }

        /// <summary>
        /// Сохраняет список снов в зашифрованный файл.
        /// </summary>
        /// <param name="dreams">Список снов.</param>
        /// <param name="key">Ключ шифрования.</param>
        /// <param name="iv">Инициализационный вектор.</param>
        public static void SaveDreams(List<Dream> dreams, byte[] key, byte[] iv)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Dream>));
                string xmlData;
                using (StringWriter sw = new StringWriter())
                {
                    serializer.Serialize(sw, dreams);
                    xmlData = sw.ToString();
                }

                byte[] encryptedData = EncryptionHelper.EncryptStringToBytes(xmlData, key, iv);
                File.WriteAllBytes(filePath, encryptedData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
