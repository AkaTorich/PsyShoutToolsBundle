using System;

namespace VSTDistortion
{
    public class DistortionProcessor
    {
        private readonly DistortionParameters _parameters;
        private float _lowPassState = 0.0f;
        private float _highPassState = 0.0f;

        public DistortionProcessor(DistortionParameters parameters)
        {
            _parameters = parameters;
            // Исправление: явно сбрасываем состояния фильтров
            ResetFilterStates();
        }
        
        // Метод для сброса состояния фильтров
        public void ResetFilterStates()
        {
            _lowPassState = 0.0f;
            _highPassState = 0.0f;
        }

        public void Process(float[] inputBuffer, float[] outputBuffer, int sampleCount)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                // Читаем параметры КАЖДЫЙ РАЗ для реального времени с проверкой
                float drive = float.IsNaN(_parameters.Drive) ? 0.5f : Math.Max(0.0f, Math.Min(1.0f, _parameters.Drive));
                float output = float.IsNaN(_parameters.Output) ? 0.5f : Math.Max(0.0f, Math.Min(2.0f, _parameters.Output));
                float mix = float.IsNaN(_parameters.Mix) ? 1.0f : Math.Max(0.0f, Math.Min(1.0f, _parameters.Mix));
                float tone = float.IsNaN(_parameters.Tone) ? 0.0f : Math.Max(-1.0f, Math.Min(1.0f, _parameters.Tone));
                // ИСПРАВЛЕНИЕ: правильное преобразование нормализованного значения Type
                DistortionType distType = (DistortionType)(int)(_parameters.Type * 3.0f);

                float drySample = inputBuffer[i];
                
                // Исправление: проверяем на некорректные значения входного сигнала
                if (float.IsNaN(drySample) || float.IsInfinity(drySample))
                    drySample = 0.0f;
                    
                // Ограничиваем входной сигнал для предотвращения экстремальных значений
                drySample = Math.Max(-2.0f, Math.Min(2.0f, drySample));
                
                float wetSample = drySample;

                // Применяем искажение
                wetSample = ApplyDistortion(wetSample, drive, distType);

                // Применяем тон-контроль (простой фильтр)
                wetSample = ApplyToneControl(wetSample, tone);
                
                // Исправление: проверяем результат тон-контроля на корректность
                if (float.IsNaN(wetSample) || float.IsInfinity(wetSample))
                {
                    wetSample = drySample; // Возвращаем оригинальный сигнал при ошибке
                    ResetFilterStates(); // Сбрасываем состояние фильтров
                }

                // Смешиваем сухой и обработанный сигнал
                float mixedSample = drySample * (1.0f - mix) + wetSample * mix;

                // Применяем выходной уровень
                outputBuffer[i] = mixedSample * output;
                
                // Исправление: проверяем финальный результат
                if (float.IsNaN(outputBuffer[i]) || float.IsInfinity(outputBuffer[i]))
                    outputBuffer[i] = 0.0f;

                // Ограничиваем выходной сигнал
                outputBuffer[i] = Math.Max(-1.0f, Math.Min(1.0f, outputBuffer[i]));
            }
        }

        private float ApplyDistortion(float input, float drive, DistortionType type)
        {
            // Drive теперь в диапазоне 0-1, используем умеренное усиление
            float drivenInput = input * (1.0f + drive * 5.0f); // Уменьшил с 15.0f до 5.0f для менее чувствительных ручек

            switch (type)
            {
                case DistortionType.SoftClip:
                    return SoftClip(drivenInput);

                case DistortionType.HardClip:
                    return HardClip(drivenInput);

                case DistortionType.Tube:
                    return TubeDistortion(drivenInput);

                case DistortionType.Fuzz:
                    return FuzzDistortion(drivenInput);

                default:
                    return SoftClip(drivenInput);
            }
        }

        private float SoftClip(float input)
        {
            // БОЛЕЕ АГРЕССИВНОЕ мягкое искажение
            if (Math.Abs(input) < 0.3f) // Уменьшил с 0.5f до 0.3f
                return input;
            else if (Math.Abs(input) < 0.8f) // Уменьшил с 1.0f до 0.8f
                return Math.Sign(input) * (0.3f + 0.7f * (Math.Abs(input) - 0.3f) * (1.6f - Math.Abs(input)));
            else
                return Math.Sign(input) * 0.6f; // Уменьшил с 0.75f до 0.6f для большего искажения
        }

        private float HardClip(float input)
        {
            // БОЛЕЕ АГРЕССИВНОЕ жесткое ограничение
            return Math.Max(-0.6f, Math.Min(0.6f, input)); // Уменьшил с 0.8f до 0.6f
        }

        private float TubeDistortion(float input)
        {
            float abs_input = Math.Abs(input);
            // БОЛЕЕ АГРЕССИВНОЕ трубное искажение
            float output = Math.Sign(input) * (1.0f - (float)Math.Exp(-abs_input * 4.0f)); // Увеличил с 2.0f до 4.0f
            return output * 0.7f; // Уменьшил компенсацию с 0.8f до 0.7f
        }

        private float FuzzDistortion(float input)
        {
            // МУЗЫКАЛЬНЫЙ FUZZ - как в классических педалях
            float abs_input = Math.Abs(input);
            
            // Noise gate
            if (abs_input < 0.005f) 
            {
                return 0.0f;
            }
            
            // Мягкий fuzz с компрессией - без резких переходов
            if (abs_input > 0.2f)
            {
                // Сильные сигналы - мягкое ограничение
                return Math.Sign(input) * 0.25f;
            }
            else
            {
                // Слабые сигналы - легкая компрессия
                float compressed = input * 1.5f;
                return Math.Max(-0.25f, Math.Min(0.25f, compressed));
            }
        }

        private float ApplyToneControl(float input, float tone)
        {
            // Простой и стабильный тон-контроль
            // tone: -1.0 = мягче/тусклее, +1.0 = ярче/острее
            
            if (Math.Abs(tone) < 0.01f)
                return input;
            
            if (tone < 0.0f)
            {
                // Отрицательные значения - мягкое приглушение (как низкочастотный фильтр)
                float amount = Math.Abs(tone);
                // Мягкая S-образная компрессия для имитации фильтрации
                float compressed = (float)Math.Tanh(input * (1.0 - amount * 0.5));
                return compressed * (1.0f - amount * 0.2f);
            }
            else
            {
                // Положительные значения - легкое подчеркивание деталей
                float amount = tone;
                // Легкое насыщение для яркости
                float enhanced = input * (1.0f + amount * 0.15f);
                return Math.Max(-0.95f, Math.Min(0.95f, enhanced));
            }
        }
    }
}