// Program.cs
using System;
using System.IO;
using System.Windows.Forms;

namespace DreamDiary
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Путь к файлу пароля
            string passwordFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "password.dat");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!File.Exists(passwordFilePath))
            {
                // Если файл пароля не существует, запускаем форму установки пароля
                using (SetPasswordForm setPasswordForm = new SetPasswordForm())
                {
                    if (setPasswordForm.ShowDialog() == DialogResult.OK)
                    {
                        string newPassword = setPasswordForm.NewPassword;
                        PasswordManager.SetPassword(newPassword);
                        MessageBox.Show("Пароль успешно установлен. Перезапустите приложение.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return; // Завершаем приложение после установки пароля
                    }
                    else
                    {
                        // Если пользователь отменил установку пароля, закрываем приложение
                        return;
                    }
                }
            }

            // Если пароль уже установлен, запускаем форму авторизации
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    byte[] encryptionKey = loginForm.EncryptionKey;
                    byte[] encryptionIV = loginForm.EncryptionIV;
                    Application.Run(new Form1(encryptionKey, encryptionIV));
                }
                else
                {
                    // Если авторизация не прошла, закрываем приложение
                    return;
                }
            }
        }
    }
}
