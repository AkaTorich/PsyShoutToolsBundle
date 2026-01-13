using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WavetableGenerator
{
    /// <summary>
    /// Главная программа генератора волновых таблиц
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Настройка кодировки для корректного отображения русского текста
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Application.Run(new MainForm());
        }
    }
}
