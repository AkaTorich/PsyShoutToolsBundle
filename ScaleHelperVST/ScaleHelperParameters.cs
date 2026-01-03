// ScaleHelperParameters.cs - Обновленные параметры с сохранением состояния
using System;
using System.Collections.Generic;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;

namespace ScaleHelperVST
{
    internal sealed class ScaleHelperParameters : IVstPluginParameters
    {
        private VstParameterCollection _parameters;
        private VstParameterCategoryCollection _categories;

        // Константы для индексов параметров
        public const int CategoryParameterIndex = 0;
        public const int ScaleParameterIndex = 1;
        public const int TonicParameterIndex = 2;

        public ScaleHelperParameters()
        {
            // Инициализируем категории
            _categories = new VstParameterCategoryCollection();
            var mainCategory = new VstParameterCategory();
            mainCategory.Name = "Main";
            _categories.Add(mainCategory);

            // Инициализируем параметры
            _parameters = new VstParameterCollection();

            // Параметр категории
            var categoryInfo = new VstParameterInfo
            {
                Name = "Category",
                Label = "Категория",
                DefaultValue = 0.0f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 10, // Примерное количество категорий
                LargeStepFloat = 1.0f,
                SmallStepFloat = 1.0f,
                StepFloat = 1.0f
            };
            var categoryParam = new VstParameter(categoryInfo);
            _parameters.Add(categoryParam);

            // Параметр гаммы
            var scaleInfo = new VstParameterInfo
            {
                Name = "Scale",
                Label = "Гамма",
                DefaultValue = 0.0f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 100, // Примерное количество гамм
                LargeStepFloat = 1.0f,
                SmallStepFloat = 1.0f,
                StepFloat = 1.0f
            };
            var scaleParam = new VstParameter(scaleInfo);
            _parameters.Add(scaleParam);

            // Параметр тоники
            var tonicInfo = new VstParameterInfo
            {
                Name = "Tonic",
                Label = "Тоника",
                DefaultValue = 0.0f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 11, // 12 тоник (0-11)
                LargeStepFloat = 1.0f,
                SmallStepFloat = 1.0f,
                StepFloat = 1.0f
            };
            var tonicParam = new VstParameter(tonicInfo);
            _parameters.Add(tonicParam);
        }

        public VstParameterCategoryCollection Categories
        {
            get { return _categories; }
        }

        public VstParameterCollection Parameters
        {
            get { return _parameters; }
        }

        public int Count
        {
            get { return _parameters.Count; }
        }

        public VstParameterInfo this[int index]
        {
            get { return _parameters[index].Info; }
        }

        // Методы для получения и установки значений параметров
        public int GetCategoryIndex()
        {
            return (int)Math.Round(_parameters[CategoryParameterIndex].Value * _parameters[CategoryParameterIndex].Info.MaxInteger);
        }

        public void SetCategoryIndex(int value)
        {
            if (value >= 0 && value <= _parameters[CategoryParameterIndex].Info.MaxInteger)
            {
                _parameters[CategoryParameterIndex].Value = (float)value / _parameters[CategoryParameterIndex].Info.MaxInteger;
            }
        }

        public int GetScaleIndex()
        {
            return (int)Math.Round(_parameters[ScaleParameterIndex].Value * _parameters[ScaleParameterIndex].Info.MaxInteger);
        }

        public void SetScaleIndex(int value)
        {
            if (value >= 0 && value <= _parameters[ScaleParameterIndex].Info.MaxInteger)
            {
                _parameters[ScaleParameterIndex].Value = (float)value / _parameters[ScaleParameterIndex].Info.MaxInteger;
            }
        }

        public int GetTonicIndex()
        {
            return (int)Math.Round(_parameters[TonicParameterIndex].Value * _parameters[TonicParameterIndex].Info.MaxInteger);
        }

        public void SetTonicIndex(int value)
        {
            if (value >= 0 && value <= _parameters[TonicParameterIndex].Info.MaxInteger)
            {
                _parameters[TonicParameterIndex].Value = (float)value / _parameters[TonicParameterIndex].Info.MaxInteger;
            }
        }
    }
}