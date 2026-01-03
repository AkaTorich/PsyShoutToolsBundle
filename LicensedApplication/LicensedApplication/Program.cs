using System;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;

namespace LicensedApplication
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Инициализация системы защиты от отладки
            AntiDebug.Initialize();

            // Запускаем фоновую проверку в отдельном потоке
            Thread antiDebugThread = new Thread(new ThreadStart(AntiDebug.BackgroundAntiDebugCheck));
            antiDebugThread.IsBackground = true;
            antiDebugThread.Start();

            // Инициализация лицензии
            LicenseManager.Initialize();

            // Проверяем лицензию перед запуском основной формы
            if (!LicenseManager.IsLicenseValid())
            {
                using (var activationForm = new LicenseActivationForm())
                {
                    // Если пользователь отменил активацию или не смог активировать
                    if (activationForm.ShowDialog() != DialogResult.OK)
                    {
                        return; // Завершаем приложение
                    }
                    // Если активация прошла успешно, продолжаем запуск
                }
            }

            // Запуск основной формы
            Application.Run(new MainForm());
        }
    }
}