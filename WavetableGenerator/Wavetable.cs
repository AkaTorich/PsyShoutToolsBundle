using System;
using System.Runtime.CompilerServices;

namespace WavetableGenerator
{
    /// <summary>
    /// Оптимизированная волновая таблица с улучшенной производительностью
    /// УЛУЧШЕННАЯ ВЕРСИЯ с поддержкой Brightness и Warmth
    /// </summary>
    public class Wavetable
    {
        // Основные свойства
        public string Name { get; set; }
        public int SampleCount { get; set; }
        public int FrameCount { get; set; }
        public int TableSize { get; private set; }
        public WaveType WaveType { get; set; }
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public double Phase { get; set; }
        public int Harmonics { get; set; }
        public double ModulationDepth { get; set; }
        public double ModulationFrequency { get; set; }
        
        // Параметры звука
        public double Brightness { get; set; }  // 0.0 - 1.0, яркость звука
        public double Warmth { get; set; }      // 0.0 - 1.0, теплота звука
        
        // Seed для уникальной генерации
        public int GenerationSeed { get; set; }
        
        // Основной массив данных
        public float[] Samples { get; set; }
        
        // Кэшированные значения для быстрого доступа
        private float _peakValue = 0.0f;
        private bool _peakCalculated = false;
        
        /// <summary>
        /// Создаёт новую волновую таблицу (Serum-совместимый формат: 2048 сэмплов/фрейм)
        /// </summary>
        public Wavetable(string name, int totalSamples, WaveType waveType, double frequency = 440.0, double amplitude = 1.0, int frameCount = 8)
        {
            Name = name;
            FrameCount = frameCount;
            TableSize = 2048; // Стандарт индустрии (Serum/Vital)
            SampleCount = TableSize * frameCount;
            WaveType = waveType;
            Frequency = frequency;
            Amplitude = amplitude;
            Phase = 0.0;
            Harmonics = 16;
            ModulationDepth = 0.0;
            ModulationFrequency = 0.0;
            Brightness = 0.5;
            Warmth = 0.5;
            GenerationSeed = HashCode.Combine(name, DateTime.Now.Ticks, Environment.TickCount, Guid.NewGuid());
            Samples = new float[SampleCount];
        }
        
        /// <summary>
        /// Получает сэмпл по индексу с проверкой границ
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSample(int index)
        {
            if (index < 0 || index >= Samples.Length)
                return 0.0f;
            return Samples[index];
        }
        
        /// <summary>
        /// Получает сэмпл из конкретного фрейма
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFrameSample(int frame, int sampleIndex)
        {
            if (frame < 0 || frame >= FrameCount || sampleIndex < 0 || sampleIndex >= TableSize)
                return 0.0f;
                
            int index = frame * TableSize + sampleIndex;
            return Samples[index];
        }
        
        /// <summary>
        /// Получает интерполированное значение с кубической интерполяцией
        /// </summary>
        public float GetInterpolatedSample(double position)
        {
            if (Samples.Length == 0) return 0.0f;
            
            // Нормализуем позицию
            position = position % 1.0;
            if (position < 0) position += 1.0;
            
            double exactIndex = position * (Samples.Length - 1);
            int index0 = (int)Math.Floor(exactIndex);
            double fraction = exactIndex - index0;
            
            // Получаем 4 точки для кубической интерполяции
            int index1 = Math.Min(index0 + 1, Samples.Length - 1);
            int indexM1 = Math.Max(index0 - 1, 0);
            int index2 = Math.Min(index0 + 2, Samples.Length - 1);
            
            float y0 = Samples[indexM1];
            float y1 = Samples[index0];
            float y2 = Samples[index1];
            float y3 = Samples[index2];
            
            // Кубическая интерполяция Hermite
            float a0 = y3 - y2 - y0 + y1;
            float a1 = y0 - y1 - a0;
            float a2 = y2 - y0;
            float a3 = y1;
            
            float fraction2 = (float)(fraction * fraction);
            float fraction3 = fraction2 * (float)fraction;
            
            return a0 * fraction3 + a1 * fraction2 + a2 * (float)fraction + a3;
        }
        
        /// <summary>
        /// Получает интерполированный сэмпл из конкретного фрейма с морфингом
        /// </summary>
        public float GetMorphedSample(double framePosition, double samplePosition)
        {
            if (Samples.Length == 0 || FrameCount == 0) return 0.0f;
            
            // Определяем фреймы для морфинга
            double exactFrame = framePosition * (FrameCount - 1);
            int frame1 = (int)Math.Floor(exactFrame);
            int frame2 = Math.Min(frame1 + 1, FrameCount - 1);
            double frameMix = exactFrame - frame1;
            
            // Применяем smootherstep для плавного морфинга
            frameMix = frameMix * frameMix * frameMix * (frameMix * (frameMix * 6 - 15) + 10);
            
            // Получаем сэмплы из обоих фреймов с кубической интерполяцией
            double exactSample = samplePosition * (TableSize - 1);
            int sampleIndex = (int)Math.Floor(exactSample);
            double sampleFraction = exactSample - sampleIndex;
            
            // Применяем smootherstep для сэмпла тоже
            sampleFraction = sampleFraction * sampleFraction * sampleFraction * (sampleFraction * (sampleFraction * 6 - 15) + 10);
            
            // Кубическая интерполяция внутри каждого фрейма
            float sample1 = InterpolateSampleInFrameCubic(frame1, sampleIndex, sampleFraction);
            float sample2 = InterpolateSampleInFrameCubic(frame2, sampleIndex, sampleFraction);
            
            // Морфинг между фреймами
            return sample1 + (sample2 - sample1) * (float)frameMix;
        }
        
        /// <summary>
        /// Интерполирует сэмпл внутри фрейма (линейная)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InterpolateSampleInFrame(int frame, int sampleIndex, double fraction)
        {
            int baseIndex = frame * TableSize + sampleIndex;
            int nextIndex = Math.Min(baseIndex + 1, frame * TableSize + TableSize - 1);
            
            float sample1 = Samples[baseIndex];
            float sample2 = Samples[nextIndex];
            
            return sample1 + (sample2 - sample1) * (float)fraction;
        }
        
        /// <summary>
        /// Интерполирует сэмпл внутри фрейма (кубическая Hermite)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InterpolateSampleInFrameCubic(int frame, int sampleIndex, double fraction)
        {
            int frameStart = frame * TableSize;
            int frameEnd = frameStart + TableSize - 1;
            
            // Получаем 4 точки для кубической интерполяции
            int idx0 = frameStart + sampleIndex;
            int idxM1 = Math.Max(idx0 - 1, frameStart);
            int idx1 = Math.Min(idx0 + 1, frameEnd);
            int idx2 = Math.Min(idx0 + 2, frameEnd);
            
            float y0 = Samples[idxM1];
            float y1 = Samples[idx0];
            float y2 = Samples[idx1];
            float y3 = Samples[idx2];
            
            // Кубическая интерполяция Hermite
            float a0 = y3 - y2 - y0 + y1;
            float a1 = y0 - y1 - a0;
            float a2 = y2 - y0;
            float a3 = y1;
            
            float fraction2 = (float)(fraction * fraction);
            float fraction3 = fraction2 * (float)fraction;
            
            return a0 * fraction3 + a1 * fraction2 + a2 * (float)fraction + a3;
        }
        
        /// <summary>
        /// Нормализует волновую таблицу
        /// </summary>
        public void Normalize()
        {
            if (Samples.Length == 0) return;
            
            // Находим пиковое значение
            float peak = GetPeakValue();
            
            if (peak > 0.0f && peak != 1.0f)
            {
                float scale = 1.0f / peak; // Полная нормализация без headroom
                
                // Применяем масштабирование
                for (int i = 0; i < Samples.Length; i++)
                {
                    Samples[i] *= scale;
                }
                
                // Сбрасываем кэш пикового значения
                _peakCalculated = false;
            }
        }
        
        /// <summary>
        /// Получает пиковое значение волновой таблицы
        /// </summary>
        public float GetPeakValue()
        {
            if (_peakCalculated)
                return _peakValue;
                
            _peakValue = 0.0f;
            for (int i = 0; i < Samples.Length; i++)
            {
                float abs = Math.Abs(Samples[i]);
                if (abs > _peakValue)
                    _peakValue = abs;
            }
            
            _peakCalculated = true;
            return _peakValue;
        }
        
        /// <summary>
        /// Применяет фейд-ин/фейд-аут между фреймами
        /// </summary>
        public void ApplyFrameFades(int fadeSamples = 64)
        {
            if (fadeSamples <= 0 || fadeSamples > TableSize / 2)
                return;
                
            for (int frame = 0; frame < FrameCount; frame++)
            {
                int frameStart = frame * TableSize;
                int frameEnd = frameStart + TableSize - 1;
                
                // Fade-in
                for (int i = 0; i < fadeSamples; i++)
                {
                    float fade = (float)i / fadeSamples;
                    Samples[frameStart + i] *= fade;
                }
                
                // Fade-out
                for (int i = 0; i < fadeSamples; i++)
                {
                    float fade = (float)(fadeSamples - i) / fadeSamples;
                    Samples[frameEnd - i] *= fade;
                }
            }
        }
        
        /// <summary>
        /// Клонирует волновую таблицу
        /// </summary>
        public Wavetable Clone()
        {
            var clone = new Wavetable(Name + "_copy", SampleCount, WaveType, Frequency, Amplitude, FrameCount)
            {
                Phase = Phase,
                Harmonics = Harmonics,
                ModulationDepth = ModulationDepth,
                ModulationFrequency = ModulationFrequency,
                Brightness = Brightness,
                Warmth = Warmth,
                GenerationSeed = GenerationSeed
            };
            
            Array.Copy(Samples, clone.Samples, Samples.Length);
            return clone;
        }
        
        /// <summary>
        /// Получает статистику волновой таблицы
        /// </summary>
        public WavetableStats GetStatistics()
        {
            var stats = new WavetableStats();
            
            if (Samples.Length == 0)
                return stats;
                
            double sum = 0;
            double sumSquares = 0;
            float min = float.MaxValue;
            float max = float.MinValue;
            
            for (int i = 0; i < Samples.Length; i++)
            {
                float sample = Samples[i];
                sum += sample;
                sumSquares += sample * sample;
                if (sample < min) min = sample;
                if (sample > max) max = sample;
            }
            
            stats.Peak = GetPeakValue();
            stats.RMS = (float)Math.Sqrt(sumSquares / Samples.Length);
            stats.Average = (float)(sum / Samples.Length);
            stats.Min = min;
            stats.Max = max;
            stats.CrestFactor = stats.RMS > 0 ? stats.Peak / stats.RMS : 0;
            
            return stats;
        }
        
        /// <summary>
        /// Возвращает информацию о волновой таблице
        /// </summary>
        public override string ToString()
        {
            return $"{Name} | {WaveType} | Frames:{FrameCount} | Table:{TableSize} | {Frequency:F1}Hz | B:{Brightness:F2} W:{Warmth:F2}";
        }
    }
    
    /// <summary>
    /// Типы волн для генерации
    /// </summary>
    public enum WaveType
    {
        Noise,      // Фильтрованный шум
        Complex,    // Сложная спектральная волна
        FM,         // FM-синтез
        HardSync,   // Hard sync осциллятор
        PWM,        // Pulse Width Modulation
        Formant,    // Формантный синтез
        Metallic,   // Металлический звук
        Custom      // Пользовательская волна
    }
    
    /// <summary>
    /// Статистика волновой таблицы
    /// </summary>
    public struct WavetableStats
    {
        public float Peak;
        public float RMS;
        public float Average;
        public float Min;
        public float Max;
        public float CrestFactor;
    }
}