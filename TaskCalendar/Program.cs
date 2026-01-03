using System;
using System.Windows.Forms;

namespace TaskCalendar
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Для .NET Framework достаточно этих двух строк
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Запуск главной формы
            Application.Run(new Form1());
        }
    }
}