using System;
using System.IO;
using System.Text;

namespace SpirePresetsGenerator.Services
{
    public enum LoggerMode { UserApp, Service }

    public static class Logger
    {
        private static string _logPath;
        private static readonly object _sync = new object();

        public static void Initialize(LoggerMode mode)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = "userapp.txt";
            _logPath = Path.Combine(baseDir, fileName);
            try
            {
                WriteLine("=== start " + DateTime.Now.ToString("u") + " ===");
            }
            catch { }
        }

        public static void Info(string message) { Write("INFO", message); }
        public static void Error(string message) { Write("ERROR", message); }

        private static void Write(string level, string message)
        {
            WriteLine("[" + DateTime.Now.ToString("u") + "] [" + level + "] " + message);
        }

        private static void WriteLine(string line)
        {
            if (string.IsNullOrEmpty(_logPath)) return;
            lock (_sync)
            {
                File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
} 