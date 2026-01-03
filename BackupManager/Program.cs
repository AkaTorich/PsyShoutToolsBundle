using System;
using System.Windows.Forms;
using System.Threading;
using BackupManager.Forms;

namespace BackupManager
{
    static class Program
    {
        // Мьютекс для проверки уже запущенного экземпляра приложения
        private static Mutex _mutex = null;

        [STAThread]
        static void Main()
        {
            const string appName = "BackupManager_Instance";
            bool createdNew;

            // Проверяем, запущен ли уже экземпляр приложения
            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // Если экземпляр уже запущен, выходим
                MessageBox.Show("Приложение Backup Manager уже запущено.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Добавляем обработчик необработанных исключений
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // Запускаем приложение
            Application.Run(new MainForm());

            // Освобождаем мьютекс при закрытии приложения
            GC.KeepAlive(_mutex);
        }

        // Обработка исключений в потоке пользовательского интерфейса
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        // Обработка необработанных исключений из других потоков
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        // Общий метод обработки исключений
        private static void HandleException(Exception ex)
        {
            string errorMessage = "Произошла непредвиденная ошибка в приложении.\n\n";
            errorMessage += $"Ошибка: {ex.Message}\n\n";
            if (ex.InnerException != null)
            {
                errorMessage += $"Внутренняя ошибка: {ex.InnerException.Message}\n\n";
            }
            errorMessage += $"Стек вызовов: {ex.StackTrace}";

            // Записываем ошибку в лог-файл
            try
            {
                string logFilePath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BackupManager",
                    "error_log.txt");

                // Убедимся, что директория существует
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logFilePath));

                string logEntry = $"[{DateTime.Now}] {errorMessage}\n\n";
                System.IO.File.AppendAllText(logFilePath, logEntry);
            }
            catch
            {
                // Не обрабатываем ошибки записи в лог
            }

            // Показываем сообщение об ошибке
            MessageBox.Show(errorMessage, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}