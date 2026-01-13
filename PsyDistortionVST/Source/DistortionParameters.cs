using System;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;

namespace VSTDistortion
{
    public enum DistortionType
    {
        SoftClip,
        HardClip,
        Tube,
        Fuzz
    }

    public class DistortionParameters : IVstPluginParameters
    {
        private VstParameterCollection _parameters;

        public DistortionParameters()
        {
            _parameters = new VstParameterCollection();

            // Параметр Drive
            var driveInfo = new VstParameterInfo
            {
                Name = "Drive",
                Label = "Drive",
                DefaultValue = 0.5f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 100,
                LargeStepFloat = 1.0f,
                SmallStepFloat = 0.1f,
                StepFloat = 0.1f
            };
            var driveParam = new VstParameter(driveInfo);
            driveParam.Value = driveInfo.DefaultValue; // Устанавливаем начальное значение
            _parameters.Add(driveParam);

            // Параметр Output
            var outputInfo = new VstParameterInfo
            {
                Name = "Output",
                Label = "Output",
                DefaultValue = 0.5f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 200,
                LargeStepFloat = 1.0f,
                SmallStepFloat = 0.1f,
                StepFloat = 0.1f
            };
            var outputParam = new VstParameter(outputInfo);
            outputParam.Value = outputInfo.DefaultValue; // Устанавливаем начальное значение
            _parameters.Add(outputParam);

            // Параметр Type
            var typeInfo = new VstParameterInfo
            {
                Name = "Type",
                Label = "Type",
                DefaultValue = 0.0f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 3,
                LargeStepFloat = 1.0f,
                SmallStepFloat = 1.0f,
                StepFloat = 1.0f
            };
            var typeParam = new VstParameter(typeInfo);
            typeParam.Value = typeInfo.DefaultValue; // Устанавливаем начальное значение
            _parameters.Add(typeParam);

            // Параметр Mix
            var mixInfo = new VstParameterInfo
            {
                Name = "Mix",
                Label = "Mix",
                DefaultValue = 1.0f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 100,
                LargeStepFloat = 1.0f,
                SmallStepFloat = 0.1f,
                StepFloat = 0.1f
            };
            var mixParam = new VstParameter(mixInfo);
            mixParam.Value = mixInfo.DefaultValue; // Устанавливаем начальное значение
            _parameters.Add(mixParam);

            // Параметр Tone
            var toneInfo = new VstParameterInfo
            {
                Name = "Tone",
                Label = "Tone",
                DefaultValue = 0.5f,
                CanBeAutomated = true,
                MinInteger = -100,
                MaxInteger = 100,
                LargeStepFloat = 1.0f,
                SmallStepFloat = 0.1f,
                StepFloat = 0.1f
            };
            var toneParam = new VstParameter(toneInfo);
            toneParam.Value = toneInfo.DefaultValue; // Устанавливаем начальное значение
            _parameters.Add(toneParam);
        }

        public VstParameterCollection Parameters => _parameters;

        public int Count => _parameters.Count;

        public VstParameterInfo this[int index] => _parameters[index].Info;

        public VstParameterCategoryCollection Categories => new VstParameterCategoryCollection();

        // Свойства для совместимости с существующим кодом
        public float Drive 
        { 
            get => _parameters[0].Value; 
            set => _parameters[0].Value = value; 
        }

        public float Output 
        { 
            get => _parameters[1].Value; 
            set => _parameters[1].Value = value; 
        }

        public float Type 
        { 
            get => _parameters[2].Value; 
            set => _parameters[2].Value = value; 
        }

        public float Mix 
        { 
            get => _parameters[3].Value; 
            set => _parameters[3].Value = value; 
        }

        public float Tone 
        { 
            get => _parameters[4].Value; 
            set => _parameters[4].Value = value; 
        }

        // Методы для совместимости с существующим кодом
        public string GetParameterDisplay(int index)
        {
            if (index >= 0 && index < _parameters.Count)
            {
                var param = _parameters[index];
                if (param == _parameters[2]) // Type parameter
                {
                    // ИСПРАВЛЕНИЕ: правильное преобразование для отображения типа
                    int typeIndex = (int)(param.Value * 3.0f);
                    typeIndex = Math.Max(0, Math.Min(3, typeIndex)); // Ограничиваем диапазон
                    DistortionType distType = (DistortionType)typeIndex;
                    return distType.ToString();
                }
                return param.Value.ToString("F1");
            }
            return "";
        }

        public string GetParameterName(int index)
        {
            if (index >= 0 && index < _parameters.Count)
            {
                return _parameters[index].Info.Name;
            }
            return "";
        }

        public string GetParameterLabel(int index)
        {
            if (index >= 0 && index < _parameters.Count)
            {
                return _parameters[index].Info.Label;
            }
            return "";
        }

        public void SetParameterValue(int index, float value)
        {
            if (index >= 0 && index < _parameters.Count)
            {
                _parameters[index].Value = value;
            }
        }

        public float GetParameterValue(int index)
        {
            if (index >= 0 && index < _parameters.Count)
            {
                return _parameters[index].Value;
            }
            return 0.0f;
        }
    }
}