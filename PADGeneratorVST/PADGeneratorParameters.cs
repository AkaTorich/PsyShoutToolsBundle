// PADGeneratorParameters.cs - Параметры плагина
using System;
using System.Collections.Generic;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;

namespace PADGeneratorVST
{
    internal sealed class PADGeneratorParameters : IVstPluginParameters
    {
        private VstParameterCollection _parameters;
        private VstParameterCategoryCollection _categories;

        // Константы для индексов параметров
        public const int CategoryParameterIndex = 0;
        public const int ScaleParameterIndex = 1;
        public const int TonicParameterIndex = 2;
        public const int PadNotesCountIndex = 3;
        public const int TactsNumberIndex = 4;
        public const int RepeatsNumberIndex = 5;

        public PADGeneratorParameters()
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
                MaxInteger = 10,
                LargeStepFloat = 1.0f,
                SmallStepFloat = 1.0f,
                StepFloat = 1.0f
            };
            _parameters.Add(new VstParameter(categoryInfo));

            // Параметр гаммы
            var scaleInfo = new VstParameterInfo
            {
                Name = "Scale",
                Label = "Гамма",
                DefaultValue = 0.0f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 100,
                LargeStepFloat = 1.0f,
                SmallStepFloat = 1.0f,
                StepFloat = 1.0f
            };
            _parameters.Add(new VstParameter(scaleInfo));

            // Параметр тоники
            var tonicInfo = new VstParameterInfo
            {
                Name = "Tonic",
                Label = "Тоника",
                DefaultValue = 0.0f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 11,
                LargeStepFloat = 1.0f,
                SmallStepFloat = 1.0f,
                StepFloat = 1.0f
            };
            _parameters.Add(new VstParameter(tonicInfo));

            // Параметр количества нот
            var padNotesInfo = new VstParameterInfo
            {
                Name = "PadNotes",
                Label = "Кол-во нот",
                DefaultValue = 0.25f, // 4 ноты из 16
                CanBeAutomated = true,
                MinInteger = 1,
                MaxInteger = 16,
                LargeStepFloat = 1.0f / 16.0f,
                SmallStepFloat = 1.0f / 16.0f,
                StepFloat = 1.0f / 16.0f
            };
            _parameters.Add(new VstParameter(padNotesInfo));

            // Параметр количества тактов
            var tactsInfo = new VstParameterInfo
            {
                Name = "Tacts",
                Label = "Кол-во тактов",
                DefaultValue = 0.0625f, // 1 такт из 16
                CanBeAutomated = true,
                MinInteger = 1,
                MaxInteger = 16,
                LargeStepFloat = 1.0f / 16.0f,
                SmallStepFloat = 1.0f / 16.0f,
                StepFloat = 1.0f / 16.0f
            };
            _parameters.Add(new VstParameter(tactsInfo));

            // Параметр количества повторов
            var repeatsInfo = new VstParameterInfo
            {
                Name = "Repeats",
                Label = "Кол-во повторов",
                DefaultValue = 0.0f,
                CanBeAutomated = true,
                MinInteger = 0,
                MaxInteger = 16,
                LargeStepFloat = 1.0f / 16.0f,
                SmallStepFloat = 1.0f / 16.0f,
                StepFloat = 1.0f / 16.0f
            };
            _parameters.Add(new VstParameter(repeatsInfo));
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
    }
}

