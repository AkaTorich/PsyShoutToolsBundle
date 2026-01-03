using Microsoft.Win32;
using System;
using System.Windows.Forms;
using System.IO;

namespace BackupManager.Services
{
    public static class AutoStartManager
    {
        private const string RunRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "BackupManager";

        /// <summary>
        /// Проверяет, добавлено ли приложение в автозагрузку
        /// </summary>
        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKeyPath))
                {
                    if (key == null) return false;
                    return key.GetValue(AppName) != null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке автозагрузки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Добавляет приложение в автозагрузку
        /// </summary>
        public static bool EnableAutoStart()
        {
            try
            {
                string appPath = Application.ExecutablePath;

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKeyPath, true))
                {
                    if (key == null) return false;
                    key.SetValue(AppName, appPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в автозагрузку: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Удаляет приложение из автозагрузки
        /// </summary>
        public static bool DisableAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKeyPath, true))
                {
                    if (key == null) return false;
                    if (key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении из автозагрузки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}