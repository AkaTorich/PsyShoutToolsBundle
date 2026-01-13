using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;

namespace VSTDistortion
{
    /// <summary>
    /// Управление программами VST плагина
    /// </summary>
    public class DistortionPrograms : IVstPluginPrograms
    {
        private VstProgramCollection _programs;

        public DistortionPrograms()
        {
            _programs = new VstProgramCollection();
            
            // Создаем программу по умолчанию
            var defaultProgram = new VstProgram();
            defaultProgram.Name = "Default";
            
            // Добавляем программу в коллекцию
            _programs.Add(defaultProgram);
        }

        public VstProgramCollection Programs => _programs;

        public int Count => _programs.Count;

        public VstProgram this[int index] => _programs[index];

        public VstProgram ActiveProgram 
        { 
            get => _programs[0]; 
            set { /* Игнорируем установку активной программы */ } 
        }

        public void BeginSetProgram()
        {
            // Начало установки программы
        }

        public void EndSetProgram()
        {
            // Конец установки программы
        }
    }
} 