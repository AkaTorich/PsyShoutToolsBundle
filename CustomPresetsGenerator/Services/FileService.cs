using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using SpirePresetsGenerator.Models;

namespace SpirePresetsGenerator.Services
{
    public static class FileService
    {
        public static void SaveAllPresets(List<SpirePresetsGenerator.Logic.PresetFile> presets, string directory)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string genDir = Path.Combine(baseDir, "Generated");
            if (!Directory.Exists(genDir)) Directory.CreateDirectory(genDir);

            var serializer = new JavaScriptSerializer();
            foreach (var pf in presets)
            {
                try
                {
                    string json = serializer.Serialize(pf.Preset);
                    string pretty = PrettyPrintJson(json);
                    string path = Path.Combine(genDir, pf.Name);
                    File.WriteAllText(path, pretty, Encoding.UTF8);
                    Logger.Info("Сохранено: " + path);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ошибка сохранения '" + pf.Name + "': " + ex.Message);
                }
            }
        }

        private static string PrettyPrintJson(string json)
        {
            var sb = new StringBuilder();
            bool quoted = false;
            int indent = 0;
            for (int i = 0; i < json.Length; i++)
            {
                char ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.Append('\n');
                            indent++;
                            sb.Append(new string(' ', indent * 2));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.Append('\n');
                            indent--;
                            sb.Append(new string(' ', indent * 2));
                            sb.Append(ch);
                        }
                        else sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        int index = i;
                        while (index > 0 && json[--index] == '\\') escaped = !escaped;
                        if (!escaped) quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.Append('\n');
                            sb.Append(new string(' ', indent * 2));
                        }
                        break;
                    case ':':
                        sb.Append(quoted ? ":" : ": ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
} 