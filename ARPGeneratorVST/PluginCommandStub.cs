// PluginCommandStub.cs - Точка входа для VST
using System;
using System.Reflection;
using System.IO;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace ARPGeneratorVST
{
    /// <summary>
    /// Точка входа для VST плагина ARPGenerator
    /// </summary>
    public sealed class PluginCommandStub : StdPluginCommandStub
    {
        static PluginCommandStub()
        {
            // Подписываемся на событие загрузки сборок для автоматического разрешения зависимостей
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                // Получаем имя запрошенной сборки
                string assemblyName = new AssemblyName(args.Name).Name;
                
                // Список DLL, которые нужно загружать автоматически
                string[] embeddedDlls = { "NAudio", "NAudio.Core", "NAudio.Midi", "NAudio.Asio", "NAudio.WinForms", "Jacobi.Vst.Core", "Jacobi.Vst.Framework", "Jacobi.Vst.Interop" };
                
                // Проверяем, является ли запрошенная сборка одной из встроенных
                foreach (string dllName in embeddedDlls)
                {
                    if (assemblyName.Equals(dllName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Пытаемся загрузить из встроенных ресурсов (они встроены как ARPGeneratorVST.bin.Release.{dllName}.dll)
                        string resourceName = $"ARPGeneratorVST.bin.Release.{dllName}.dll";
                        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                byte[] assemblyData = new byte[stream.Length];
                                stream.Read(assemblyData, 0, assemblyData.Length);
                                return Assembly.Load(assemblyData);
                            }
                        }
                        
                        // Если не нашли в ресурсах, пытаемся загрузить из той же папки, где находится плагин
                        string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string dllPath = Path.Combine(pluginDir, dllName + ".dll");
                        
                        if (File.Exists(dllPath))
                        {
                            return Assembly.LoadFrom(dllPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resolving assembly: {ex.Message}");
            }
            
            return null;
        }

        /// <summary>
        /// Создаёт экземпляр плагина
        /// </summary>
        protected override IVstPlugin CreatePluginInstance()
        {
            return new ARPGeneratorVstPlugin();
        }
    }
}

