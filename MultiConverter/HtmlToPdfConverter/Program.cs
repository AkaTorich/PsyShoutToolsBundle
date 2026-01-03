using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HtmlToPdfConverter
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Создание файла лога в рабочей директории приложения
            string logPath = Path.Combine(Application.StartupPath, "userapp.txt");
            File.WriteAllText(logPath, $"Приложение запущено: {DateTime.Now}\r\n", Encoding.UTF8);
            
            Application.Run(new MainForm());
        }
    }
}