using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpirePresetsGenerator.Logic;
using SpirePresetsGenerator.Models;
using SpirePresetsGenerator.Services;

namespace SpirePresetsGenerator
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Logger.Initialize(LoggerMode.UserApp);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Application.Run(new UI.MainForm());
        }

        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("--"))
                {
                    string key = arg.Substring(2);
                    string value = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[++i] : "true";
                    dict[key] = value;
                }
            }
            return dict;
        }

        private static int GetInt(Dictionary<string, string> opts, string key, int defVal)
        {
            return opts.TryGetValue(key, out var v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : defVal;
        }

        private static double GetDouble(Dictionary<string, string> opts, string key, double defVal)
        {
            return opts.TryGetValue(key, out var v) && double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : defVal;
        }

        private static bool GetBool(Dictionary<string, string> opts, string key, bool defVal)
        {
            return opts.TryGetValue(key, out var v) ? (v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1") : defVal;
        }

        private static string GetString(Dictionary<string, string> opts, string key, string defVal)
        {
            return opts.TryGetValue(key, out var v) ? v : defVal;
        }
    }
} 