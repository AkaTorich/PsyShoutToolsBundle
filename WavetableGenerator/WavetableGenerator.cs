using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WavetableGenerator
{
    /// <summary>
    /// Оптимизированный генератор волновых таблиц с профессиональными алгоритмами
    /// УЛУЧШЕННАЯ ВЕРСИЯ с поддержкой Brightness/Warmth и улучшенной уникальностью
    /// </summary>
    public class WavetableGenerator
    {
        // Константы для оптимизации
        private const int CACHE_SIZE = 4096;
        private const double TWO_PI = 2.0 * Math.PI;
        private const int MAX_HARMONICS = 128;
        
        // Кэши для ускорения вычислений
        private readonly double[] _sineCache;
        private readonly double[] _cosineCache;
        private readonly Random _globalRandom;
        
        // Предварительно вычисленные таблицы
        private static readonly double[] HarmonicDecayTable = new double[MAX_HARMONICS];
        
        // Музыкальные соотношения для более гармоничных звуков
        private static readonly double[] MusicalRatios = new double[]
        {
            1.0,      // Унисон
            2.0,      // Октава
            3.0/2.0,  // Квинта
            4.0/3.0,  // Кварта
            5.0/4.0,  // Большая терция
            6.0/5.0,  // Малая терция
            9.0/8.0,  // Большая секунда
            16.0/15.0 // Малая секунда
        };
        
        static WavetableGenerator()
        {
            // Инициализация таблицы спада гармоник с более мелодичным профилем
            for (int i = 0; i < MAX_HARMONICS; i++)
            {
                // Используем более мягкий спад для теплого звучания
                HarmonicDecayTable[i] = Math.Pow(1.0 / (i + 1), 0.85);
            }
        }
        
        public WavetableGenerator()
        {
            // Инициализация кэшей синуса и косинуса
            _sineCache = new double[CACHE_SIZE];
            _cosineCache = new double[CACHE_SIZE];
            
            // УЛУЧШЕННАЯ рандомизация: комбинируем несколько источников энтропии для максимальной уникальности
            int seed = unchecked(
                Environment.TickCount * 31 + 
                Guid.NewGuid().GetHashCode() * 17 + 
                DateTime.Now.Millisecond * 13 +
                System.Diagnostics.Process.GetCurrentProcess().Id * 7
            );
            _globalRandom = new Random(seed);
            
            double step = TWO_PI / CACHE_SIZE;
            for (int i = 0; i < CACHE_SIZE; i++)
            {
                double angle = i * step;
                _sineCache[i] = Math.Sin(angle);
                _cosineCache[i] = Math.Cos(angle);
            }
        }
        
        /// <summary>
        /// Быстрое получение значения синуса из кэша
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double FastSin(double angle)
        {
            // Нормализуем угол в диапазон [0, 2π]
            angle = angle % TWO_PI;
            if (angle < 0) angle += TWO_PI;
            
            int index = (int)((angle / TWO_PI) * CACHE_SIZE);
            if (index >= CACHE_SIZE) index = CACHE_SIZE - 1;
            
            return _sineCache[index];
        }
        
        /// <summary>
        /// Быстрое получение значения косинуса из кэша
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double FastCos(double angle)
        {
            angle = angle % TWO_PI;
            if (angle < 0) angle += TWO_PI;
            
            int index = (int)((angle / TWO_PI) * CACHE_SIZE);
            if (index >= CACHE_SIZE) index = CACHE_SIZE - 1;
            
            return _cosineCache[index];
        }
        
        /// <summary>
        /// Генерирует оптимизированную волновую таблицу с поддержкой Brightness и Warmth
        /// </summary>
        public Wavetable GenerateWavetable(string name, int tableSize, WaveType waveType, 
            double frequency = 440.0, double amplitude = 1.0, int harmonics = 10, int frameCount = 8,
            double brightness = 0.5, double warmth = 0.5)
        {
            // Создаём волновую таблицу с уникальным seed (tableSize игнорируется - используется 2048)
            var uniqueSeed = Guid.NewGuid().GetHashCode() + Environment.TickCount;
            var wavetable = new Wavetable(name, 0, waveType, frequency, amplitude, frameCount)
            {
                Harmonics = harmonics,
                Brightness = brightness,
                Warmth = warmth,
                GenerationSeed = uniqueSeed
            };
            
            // Генерируем волновую таблицу параллельно для ускорения
            GenerateOptimizedWavetable(wavetable, wavetable.TableSize);
            
            // Применяем финальную обработку
            ApplyFinalProcessing(wavetable);
            
            return wavetable;
        }
        
        /// <summary>
        /// Оптимизированная генерация волновой таблицы
        /// </summary>
        private void GenerateOptimizedWavetable(Wavetable wavetable, int tableSize)
        {
            int frameCount = wavetable.FrameCount;
            int totalSamples = tableSize * frameCount;
            wavetable.Samples = new float[totalSamples];
            
            const int keyFrameCount = 64;
            var keyFrames = new float[keyFrameCount][];
            var random = new Random(wavetable.GenerationSeed);
            
            for (int keyFrame = 0; keyFrame < keyFrameCount; keyFrame++)
            {
                keyFrames[keyFrame] = new float[tableSize];
                double progress = (double)keyFrame / (keyFrameCount - 1);
                var parameters = GenerateFrameParameters(wavetable.WaveType, progress, keyFrame, wavetable.GenerationSeed, wavetable.Brightness, wavetable.Warmth, keyFrameCount);
                GenerateOptimizedFrame(keyFrames[keyFrame], wavetable.WaveType, parameters);
            }
            
            var frames = new float[frameCount][];
            
            for (int frame = 0; frame < frameCount; frame++)
            {
                frames[frame] = new float[tableSize];
                
                double keyFramePosition = (double)frame / (frameCount - 1) * (keyFrameCount - 1);
                int keyFrame1 = (int)keyFramePosition;
                int keyFrame2 = Math.Min(keyFrame1 + 1, keyFrameCount - 1);
                double blend = keyFramePosition - keyFrame1;
                
                for (int i = 0; i < tableSize; i++)
                {
                    frames[frame][i] = (float)(keyFrames[keyFrame1][i] * (1 - blend) + keyFrames[keyFrame2][i] * blend);
                }
            }
            
            AssembleWavetableWithMorphing(wavetable, frames, tableSize);
        }
        
        /// <summary>
        /// Генерирует оптимизированный фрейм
        /// </summary>
        private void GenerateOptimizedFrame(float[] samples, WaveType waveType, FrameParameters parameters)
        {
            switch (waveType)
            {
                case WaveType.Noise:
                    GenerateFilteredNoise(samples, parameters);
                    break;
                    
                case WaveType.Complex:
                    GenerateSpectralFrame(samples, parameters);
                    break;
                    
                case WaveType.FM:
                    GenerateFMFrame(samples, parameters);
                    break;
                    
                case WaveType.HardSync:
                    GenerateHardSyncFrame(samples, parameters);
                    break;
                    
                case WaveType.PWM:
                    GeneratePWMFrame(samples, parameters);
                    break;
                    
                case WaveType.Formant:
                    GenerateFormantFrame(samples, parameters);
                    break;
                    
                case WaveType.Metallic:
                    GenerateMetallicFrame(samples, parameters);
                    break;
                    
                default:
                    GenerateSpectralFrame(samples, parameters);
                    break;
            }
        }
        
        /// <summary>
        /// Генерирует спектральный фрейм с богатым музыкальным тембром
        /// УПРОЩЁННАЯ ВЕРСИЯ: простые формы волн для плавного морфинга
        /// </summary>
        private void GenerateSpectralFrame(float[] samples, FrameParameters parameters)
        {
            int tableSize = samples.Length;
            double phaseIncrement = TWO_PI / tableSize;
            
            var random = new Random(unchecked(
                parameters.GetHashCode() * 31 + 
                Environment.TickCount * 17
            ));
            
            // РАДИКАЛЬНО увеличенное количество гармоник
            int harmonics = 4 + (int)(parameters.Harmonics % 28); // От 4 до 31 гармоник
            double mainAmp = 0.3 + random.NextDouble() * 0.7; // БОЛЬШАЯ вариация
            
            // ЭКСТРЕМАЛЬНАЯ Phase distortion
            double phaseDistortion = 0.05 + random.NextDouble() * 0.35;
            double progress = parameters.Phase / TWO_PI;
            
            // КОМПЛЕКСНЫЕ модуляторы
            double mod1 = Math.Sin(progress * TWO_PI * 2.1 + parameters.Harmonics * 0.1);
            double mod2 = Math.Cos(progress * TWO_PI * 3.7 + parameters.Brightness * TWO_PI);
            double mod3 = Math.Sin(progress * TWO_PI * 5.3 + parameters.Warmth * TWO_PI);
            double mod4 = Math.Cos(progress * TWO_PI * 7.9 + parameters.ModulationDepth * 0.5);
            
            for (int i = 0; i < tableSize; i++)
            {
                // Phase distortion + УСИЛЕННАЯ FM-модуляция между фреймами
                double basePhase = (i + Math.Sin(i * 0.12) * phaseDistortion) * phaseIncrement;
                double phaseModulation = FastSin(basePhase * 0.6 + progress * TWO_PI) * 0.25 +
                                        FastCos(basePhase * 1.3 + progress * TWO_PI * 1.8) * 0.18;
                double phase = basePhase + phaseModulation;
                double value = 0.0;
                
                // Основной тон с переменной амплитудой
                value += FastSin(phase + mod1 * 0.1) * mainAmp;
                
                // ДРАМАТИЧНЫЕ гармоники
                for (int h = 1; h < harmonics && h < 32; h++)
                {
                    double amplitude = Math.Pow(1.0 / (h + 1), 0.6); // Еще медленнее спад
                    
                    // РАДИКАЛЬНОЕ влияние brightness
                    amplitude *= (0.2 + parameters.Brightness * 1.2);
                    
                    // ЭКСТРЕМАЛЬНОЕ влияние warmth
                    if (h < 5)
                        amplitude *= (0.4 + parameters.Warmth * 0.9);
                    if (h > 15)
                        amplitude *= (0.3 + parameters.Brightness * 1.0);
                    
                    // КОМПЛЕКСНОЕ вибрато
                    double vibrato = Math.Sin(phase * 2.0 + progress * TWO_PI * 3 + mod4 * 0.5) * 0.3 +
                                    Math.Cos(phase * 1.5 + mod2 * TWO_PI) * 0.2 +
                                    Math.Sin(phase * 3.2 + mod1 * TWO_PI) * 0.15;
                    
                    // НЕЛИНЕЙНЫЕ гармонические соотношения
                    double harmonicRatio = (h + 1) * (1.0 + mod3 * 0.4 + mod4 * 0.3);
                    
                    value += amplitude * FastSin(phase * harmonicRatio + vibrato);
                }
                
                // Wave folding с переменным порогом
                double foldThreshold = 1.3 + mod1 * 0.3;
                if (Math.Abs(value) > foldThreshold)
                {
                    value = Math.Sign(value) * (2.6 - Math.Abs(value));
                }
                
                samples[i] = (float)value;
            }
            
            NormalizeInPlace(samples);
        }
        
        /// <summary>
        /// Генерирует FM-синтезированный фрейм с уникальными металлическими звуками
        /// УЛУЧШЕННАЯ ВЕРСИЯ: гладкая форма волны как в профессиональных плагинах
        /// </summary>
        private void GenerateFMFrame(float[] samples, FrameParameters parameters)
        {
            int tableSize = samples.Length;
            double phaseIncrement = TWO_PI / tableSize;
            
            var random = new Random(unchecked(
                parameters.GetHashCode() * 31 + 
                Environment.TickCount * 17 + 
                Guid.NewGuid().GetHashCode() * 13 +
                DateTime.Now.Ticks.GetHashCode() * 7
            ));
            
            // ДРАМАТИЧНЫЙ индекс модуляции
            double modIndex = parameters.ModulationDepth * (0.5 + random.NextDouble() * 3.5);
            
            // ШИРОКИЙ диапазон соотношений (гармонические и негармонические)
            double modRatio = parameters.ModulationFrequency * (0.8 + random.NextDouble() * 0.6);
            
            // МНОЖЕСТВО модуляторов для сложности
            double modRatio2 = modRatio * (1.5 + random.NextDouble() * 2.5);
            double modRatio3 = modRatio * (0.5 + random.NextDouble() * 1.0);
            
            // БОЛЬШИЕ амплитуды для драматичности
            double cascadeAmplitude = 0.4 + random.NextDouble() * 1.2;
            double parallelAmplitude = 0.3 + random.NextDouble() * 0.9;
            double tertiaryAmplitude = 0.2 + random.NextDouble() * 0.6;
            
            cascadeAmplitude *= (0.3 + parameters.Brightness * 1.0);
            parallelAmplitude *= (0.3 + parameters.Warmth * 1.0);
            modIndex *= (0.5 + parameters.Brightness * 0.8);
            
            // ЭКСТРЕМАЛЬНАЯ Phase distortion
            double phaseDistortion = 0.05 + random.NextDouble() * 0.4;
            
            for (int i = 0; i < tableSize; i++)
            {
                // Phase distortion
                double phase = (i + Math.Sin(i * 0.1) * phaseDistortion + Math.Cos(i * 0.07) * phaseDistortion * 0.5) * phaseIncrement;
                
                // ТРИ модулятора для КОМПЛЕКСНОЙ текстуры
                double mod1 = FastSin(phase * modRatio + parameters.Phase) * modIndex * cascadeAmplitude;
                double mod2 = FastSin(phase * modRatio2 + parameters.Phase * 1.5) * modIndex * parallelAmplitude;
                double mod3 = FastSin(phase * modRatio3 + parameters.Phase * 2.0) * modIndex * tertiaryAmplitude;
                
                // КОМПЛЕКСНАЯ каскадная модуляция
                double cascade = FastSin(phase * modRatio + mod2 * 0.5) * modIndex * cascadeAmplitude * 0.7;
                
                // Суммируем модуляцию
                double totalModulation = mod1 + mod2 * 0.8 + mod3 * 0.6 + cascade;
                
                // Генерируем несущую с модуляцией
                double carrierPhase = phase + totalModulation;
                float carrier = (float)FastSin(carrierPhase);
                
                // БОЛЬШЕ гармоник для богатства
                float harmonic2 = (float)FastSin(carrierPhase * 2.0) * 0.25f * (float)parameters.Brightness;
                float harmonic3 = (float)FastSin(carrierPhase * 3.0) * 0.18f * (float)parameters.Warmth;
                float harmonic5 = (float)FastSin(carrierPhase * 5.0) * 0.12f * (float)parameters.Brightness;
                
                double value = carrier * 0.7f + harmonic2 + harmonic3 + harmonic5;
                
                // Wave folding для остроты
                if (Math.Abs(value) > 1.5)
                {
                    value = Math.Sign(value) * (3.0 - Math.Abs(value));
                }
                
                samples[i] = (float)value;
            }
            
            // FM - цифровой пресет, убираем сглаживание для острых пиков
            NormalizeInPlace(samples);
        }
        
        /// <summary>
        /// Генерирует отфильтрованный шум с уникальными характеристиками
        /// </summary>
        private void GenerateFilteredNoise(float[] samples, FrameParameters parameters)
        {
            // Уникальный seed (улучшенный)
            var random = new Random(unchecked(
                parameters.GetHashCode() * 31 + 
                Environment.TickCount * 17 + 
                Guid.NewGuid().GetHashCode() * 13
            ));
            
            // Вариация типа шума между фреймами
            double progress = parameters.Phase / TWO_PI;
            double noiseMix = progress * 0.3 + 0.7;
            
            // Генерируем шум с случайными характеристиками
            for (int i = 0; i < samples.Length; i++)
            {
                double noiseType = random.NextDouble() * noiseMix;
                
                float whiteNoise = (float)((_globalRandom.NextDouble() * 2.0 - 1.0) * 0.3);
                
                if (noiseType < 0.3)
                {
                    samples[i] = whiteNoise;
                }
                else if (noiseType < 0.6)
                {
                    // Розовый шум (1/f)
                    double pinkNoise = whiteNoise;
                    for (int j = 0; j < 4; j++)
                    {
                        pinkNoise += (_globalRandom.NextDouble() * 2.0 - 1.0) / Math.Sqrt(j + 1) * 0.15;
                    }
                    samples[i] = (float)pinkNoise;
                }
                else
                {
                    // Коричневый шум (1/f^2)
                    double brownNoise = whiteNoise;
                    for (int j = 0; j < 2; j++)
                    {
                        brownNoise += (_globalRandom.NextDouble() * 2.0 - 1.0) / (j + 1) * 0.1;
                    }
                    samples[i] = (float)brownNoise;
                }
            }
            
            // Применяем слабый фильтр - оставляем шум видимым
            double cutoff = 0.02 + random.NextDouble() * 0.15;
            cutoff *= (0.6 + parameters.Brightness * 0.5);
            double resonance = 0.5 + random.NextDouble() * 1.5;
            resonance *= (0.8 + parameters.Warmth * 0.4);
            
            ApplyLightFilter(samples, cutoff, resonance);
            
            // Нормализуем чтобы шум был виден
            NormalizeInPlace(samples);
        }
        
        /// <summary>
        /// Применяет резонансный фильтр к сэмплам
        /// </summary>
        private void ApplyResonantFilter(float[] samples, double cutoff, double resonance)
        {
            double f = 2.0 * Math.Sin(Math.PI * cutoff);
            double q = resonance;
            double scale = resonance;
            
            double low = 0, band = 0, high = 0;
            
            for (int i = 0; i < samples.Length; i++)
            {
                low += f * band;
                high = scale * samples[i] - low - q * band;
                band += f * high;
                
                samples[i] = (float)low;
            }
        }
        
        /// <summary>
        /// Применяет лёгкий фильтр для шума - оставляет его видимым
        /// </summary>
        private void ApplyLightFilter(float[] samples, double cutoff, double resonance)
        {
            float alpha = 1.0f - (float)cutoff;
            float filtered = samples[0];
            
            for (int i = 0; i < samples.Length; i++)
            {
                filtered = alpha * filtered + (1.0f - alpha) * samples[i];
                samples[i] = samples[i] * 0.3f + filtered * 0.7f;
            }
        }
        

        
        /// <summary>
        /// Получает амплитуды гармоник для синусоиды
        /// </summary>
        private double[] GetSineHarmonics(int count)
        {
            var harmonics = new double[1];
            harmonics[0] = 1.0;
            return harmonics;
        }
        
        /// <summary>
        /// Получает амплитуды гармоник для прямоугольной волны
        /// </summary>
        private double[] GetSquareHarmonics(int count)
        {
            var harmonics = new double[Math.Min(count, MAX_HARMONICS)];
            for (int i = 0; i < harmonics.Length; i++)
            {
                if (i % 2 == 0) // Только нечётные гармоники
                {
                    // Делаем более резкие переходы для цифровых звуков
                    harmonics[i] = 1.0 / (i + 1) * 1.2;
                }
            }
            return harmonics;
        }
        
        /// <summary>
        /// Получает амплитуды гармоник для пилообразной волны
        /// </summary>
        private double[] GetSawtoothHarmonics(int count)
        {
            var harmonics = new double[Math.Min(count, MAX_HARMONICS)];
            for (int i = 0; i < harmonics.Length; i++)
            {
                // Делаем более резкие переходы для цифровых звуков
                harmonics[i] = 1.0 / (i + 1) * 1.1;
            }
            return harmonics;
        }
        
        /// <summary>
        /// Получает амплитуды гармоник для треугольной волны
        /// </summary>
        private double[] GetTriangleHarmonics(int count)
        {
            var harmonics = new double[Math.Min(count, MAX_HARMONICS)];
            for (int i = 0; i < harmonics.Length; i++)
            {
                if (i % 2 == 0) // Только нечётные гармоники
                {
                    int n = i + 1;
                    harmonics[i] = 1.0 / (n * n);
                    if ((n - 1) / 2 % 2 == 1) harmonics[i] *= -1;
                }
            }
            return harmonics;
        }
        
        /// <summary>
        /// Собирает волновую таблицу с морфингом между фреймами (без кликов)
        /// </summary>
        private void AssembleWavetableWithMorphing(Wavetable wavetable, float[][] frames, int tableSize)
        {
            int frameCount = frames.Length;
            int totalSamples = wavetable.Samples.Length;
            
            // Используем Hann window для anti-click crossfade (как в профессиональных синтезаторах)
            for (int i = 0; i < totalSamples; i++)
            {
                int sampleInTable = i % tableSize;
                int currentFrame = (i / tableSize);
                
                if (currentFrame >= frameCount - 1)
                {
                    wavetable.Samples[i] = frames[frameCount - 1][sampleInTable];
                }
                else
                {
                    // Нормализуем позицию от 0 до 1 в рамках одного фрейма
                    float t = (float)(i % tableSize) / tableSize;
                    
                    // РАСШИРЕННАЯ область crossfade для СУПЕР ПЛАВНОГО перехода
                    float crossfadeStart = 0.0f; // Начинаем crossfade с самого начала
                    float crossfadeEnd = 1.0f;   // Заканчиваем crossfade в конце
                    float fadeT = MathF.Max(0.0f, MathF.Min(1.0f, (t - crossfadeStart) / (crossfadeEnd - crossfadeStart)));
                    
                    // Используем smootherstep (Ken Perlin's improved smoothstep) для еще более плавного перехода
                    // smootherstep: 6t^5 - 15t^4 + 10t^3
                    float fadeT2 = fadeT * fadeT;
                    float fadeT3 = fadeT2 * fadeT;
                    float fadeT4 = fadeT3 * fadeT;
                    float fadeT5 = fadeT4 * fadeT;
                    float smoothFadeT = 6.0f * fadeT5 - 15.0f * fadeT4 + 10.0f * fadeT3;
                    
                    // Hann window функция для anti-click морфинга
                    float hannIn = MathF.Sin((1.0f - smoothFadeT) * MathF.PI * 0.5f);
                    hannIn = hannIn * hannIn;
                    
                    float hannOut = MathF.Sin(smoothFadeT * MathF.PI * 0.5f);
                    hannOut = hannOut * hannOut;
                    
                    // Нормализуем коэффициенты
                    float sum = hannIn + hannOut;
                    if (sum > 0.0001f)
                    {
                        hannIn /= sum;
                        hannOut /= sum;
                    }
                    
                    float sample1 = frames[currentFrame][sampleInTable];
                    float sample2 = frames[currentFrame + 1][sampleInTable];
                    
                    // Плавный crossfade с улучшенным Hann window
                    wavetable.Samples[i] = sample1 * hannIn + sample2 * hannOut;
                }
            }
        }
        
        /// <summary>
        /// Применяет расширенный crossfade между всеми фреймами
        /// </summary>
        private void ApplyExtendedCrossfade(float[][] frames)
        {
            int tableSize = frames[0].Length;
            int crossfadeLength = Math.Min(256, tableSize / 2);
            
            for (int i = 0; i < frames.Length - 1; i++)
            {
                // Плавно смешиваем конец текущего фрейма с началом следующего
                for (int j = 0; j < crossfadeLength; j++)
                {
                    float t = (float)j / crossfadeLength;
                    
                    // Используем smoothstep для более плавного перехода
                    t = t * t * (3.0f - 2.0f * t);
                    
                    int endIdx = tableSize - crossfadeLength + j;
                    int startIdx = j;
                    
                    if (endIdx >= 0 && endIdx < tableSize && startIdx < tableSize)
                    {
                        float endSample = frames[i][endIdx];
                        float startSample = frames[i + 1][startIdx];
                        
                        // Плавное смешивание
                        frames[i][endIdx] = endSample + (startSample - endSample) * t;
                        frames[i + 1][startIdx] = startSample + (endSample - startSample) * (1.0f - t);
                    }
                }
            }
        }
        
        /// <summary>
        /// Синхронизирует фазу между фреймами для плавных переходов без кликов
        /// </summary>
        private void SynchronizeFramesPhase(float[][] frames)
        {
            for (int i = 0; i < frames.Length - 1; i++)
            {
                int tableSize = frames[i].Length;
                
                // Берём последние и первые сэмплы для проверки фазы
                float endValue = frames[i][tableSize - 1];
                float startValue = frames[i + 1][0];
                
                // Вычисляем разницу значений
                float valueDiff = startValue - endValue;
                
                // Плавно устраняем разрыв (используем 128 сэмплов для плавности)
                int fadeLength = Math.Min(128, tableSize);
                for (int j = 0; j < fadeLength; j++)
                {
                    float fadeIn = (float)j / fadeLength;
                    float fadeOut = 1.0f - fadeIn;
                    
                    // Hann window для более плавного crossfade
                    float hannIn = MathF.Sin(fadeOut * MathF.PI * 0.5f);
                    hannIn = hannIn * hannIn;
                    float hannOut = MathF.Sin(fadeIn * MathF.PI * 0.5f);
                    hannOut = hannOut * hannOut;
                    
                    float sum = hannIn + hannOut;
                    if (sum > 0.0001f)
                    {
                        hannIn /= sum;
                        hannOut /= sum;
                    }
                    
                    // Корректируем конец текущего фрейма
                    int endIdx = tableSize - fadeLength + j;
                    if (endIdx >= 0 && endIdx < tableSize)
                        frames[i][endIdx] = frames[i][endIdx] * hannIn + endValue * (1.0f - hannIn);
                    
                    // Корректируем начало следующего фрейма
                    if (j < tableSize)
                        frames[i + 1][j] = frames[i + 1][j] * hannOut + endValue * (1.0f - hannOut);
                }
            }
        }
        
        /// <summary>
        /// Генерирует параметры фрейма с улучшенными характеристиками и поддержкой Brightness/Warmth
        /// </summary>
        private FrameParameters GenerateFrameParameters(WaveType waveType, double progress, int frameIndex, int seed,
            double brightnessOverride = 0.5, double warmthOverride = 0.5, int totalKeyFrames = 8)
        {
            var uniqueSeed = unchecked(seed + frameIndex * 1000);
            var random = new Random(uniqueSeed);
            
            // Плавное изменение параметров от начала к концу
            double smoothProgress = progress; // 0.0 -> 1.0
            
            // Вариация с синусоидой для естественности
            double wave = Math.Sin(smoothProgress * Math.PI); // 0 -> 1 -> 0
            
            // Brightness и Warmth с небольшими вариациями
            double variedBrightness = brightnessOverride + wave * 0.2 - 0.1;
            double variedWarmth = warmthOverride + Math.Cos(smoothProgress * Math.PI) * 0.2 - 0.1;
            variedBrightness = Math.Max(0.0, Math.Min(1.0, variedBrightness));
            variedWarmth = Math.Max(0.0, Math.Min(1.0, variedWarmth));
            
            // Параметры меняются плавно
            double harmonics = 8 + smoothProgress * 24; // 8 -> 32
            double modDepth = 0.5 + smoothProgress * 2.5; // 0.5 -> 3.0
            double modFreq = 1.0 + smoothProgress * 4.0; // 1.0 -> 5.0
            
            var parameters = new FrameParameters
            {
                Frequency = 440.0,
                Phase = 0,
                Harmonics = Math.Max(4, Math.Min(64, (int)harmonics)),
                ModulationDepth = Math.Max(0.1, Math.Min(5.0, modDepth)),
                ModulationFrequency = Math.Max(0.5, Math.Min(8.0, modFreq)),
                Brightness = variedBrightness,
                Warmth = variedWarmth
            };
            
            // Специфичные настройки для типов волн
            switch (waveType)
            {
                case WaveType.Complex:
                    parameters.Harmonics = Math.Max(8, Math.Min(48, (int)(12 + smoothProgress * 36)));
                    break;
                    
                case WaveType.FM:
                    double[] ratios = { 1.0, 1.5, 2.0, 3.0, 4.0 };
                    int ratioIndex = Math.Min((int)(smoothProgress * ratios.Length), ratios.Length - 1);
                    parameters.ModulationFrequency = ratios[ratioIndex] * (1.0 + wave * 0.5);
                    parameters.ModulationDepth = 1.0 + smoothProgress * 3.0;
                    break;
            }
            
            return parameters;
        }
        
        /// <summary>
        /// Применяет финальную обработку к волновой таблице
        /// </summary>
        private void ApplyFinalProcessing(Wavetable wavetable)
        {
            // Применяем DC-фильтр для удаления постоянной составляющей
            RemoveDCOffset(wavetable.Samples);
            
            // Финальная нормализация
            wavetable.Normalize();
        }
        

        
        /// <summary>
        /// Удаляет DC-смещение
        /// </summary>
        private void RemoveDCOffset(float[] samples)
        {
            double sum = 0;
            foreach (float sample in samples)
            {
                sum += sample;
            }
            
            float dcOffset = (float)(sum / samples.Length);
            
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] -= dcOffset;
            }
        }
        
        /// <summary>
        /// Применяет лёгкое low-pass сглаживание для уменьшения пиков и артефактов
        /// Используется простой moving average фильтр как в профессиональных синтезаторах
        /// </summary>
        private void ApplyLowpassSmoothing(float[] samples)
        {
            if (samples.Length < 3) return;
            
            // Используем очень лёгкое сглаживание (3-точечный moving average)
            // Это уменьшает высокочастотные пики без потери характера звука
            float[] temp = new float[samples.Length];
            Array.Copy(samples, temp, samples.Length);
            
            // Первый сэмпл остаётся без изменений
            samples[0] = temp[0];
            
            // Применяем сглаживание к средним сэмплам
            for (int i = 1; i < samples.Length - 1; i++)
            {
                // Взвешенное среднее: 25% предыдущий + 50% текущий + 25% следующий
                samples[i] = temp[i - 1] * 0.25f + temp[i] * 0.5f + temp[i + 1] * 0.25f;
            }
            
            // Последний сэмпл остаётся без изменений
            samples[samples.Length - 1] = temp[samples.Length - 1];
        }
        
        /// <summary>
        /// Быстрая нормализация in-place
        /// </summary>
        private void NormalizeInPlace(float[] samples)
        {
            float max = 0.0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float abs = Math.Abs(samples[i]);
                if (abs > max) max = abs;
            }
            
            if (max > 0.0f && max != 1.0f)
            {
                float scale = 1.0f / max; // Полная нормализация без headroom
                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] *= scale;
                }
            }
        }
        
        /// <summary>
        /// Создаёт набор волновых таблиц с уникальными параметрами
        /// УЛУЧШЕННАЯ ВЕРСИЯ: каждый раз генерируются новые уникальные таблицы
        /// </summary>
        public List<Wavetable> GenerateWavetableSet(int count, int tableSize, WaveType[] presets,
            int frameCount = 256, int harmonics = 16,
            double brightness = 0.5, double warmth = 0.5)
        {
            var wavetables = new List<Wavetable>(count);
            
            // Генерируем уникальные волновые таблицы
            for (int i = 0; i < count; i++)
            {
                // Выбираем пресет циклически, но с вариациями
                var waveType = presets[i % presets.Length];
                
                // УНИКАЛЬНОЕ имя: комбинируем несколько источников энтропии
                var uniqueId = unchecked(
                    DateTime.Now.Ticks + 
                    Guid.NewGuid().GetHashCode() * 31 + 
                    i * 17 +
                    Environment.TickCount * 13
                );
                var name = $"WT_{i + 1:D3}_{waveType}_{Math.Abs(uniqueId):X8}";
                
                // ОГРОМНЫЕ случайные вариации brightness и warmth для РЕАЛЬНО уникальности
                // Каждая таблица получает РАДИКАЛЬНО разные параметры
                double variedBrightness = brightness + (_globalRandom.NextDouble() - 0.5) * 0.3; // ±30%
                double variedWarmth = warmth + (_globalRandom.NextDouble() - 0.5) * 0.3; // ±30%
                variedBrightness = Math.Max(0, Math.Min(1, variedBrightness));
                variedWarmth = Math.Max(0, Math.Min(1, variedWarmth));
                
                // Добавляем небольшую задержку для гарантии разных timestamp'ов
                if (i > 0 && i % 10 == 0)
                {
                    System.Threading.Thread.Sleep(1);
                }
                
                wavetables.Add(GenerateWavetable(
                    name, tableSize, waveType, 440.0, 1.0, harmonics, frameCount,
                    variedBrightness, variedWarmth));
            }
            
            return wavetables;
        }
        
        /// <summary>
        /// Сохраняет волновую таблицу в WAV файл (10 секунд, ~440 Hz)
        /// </summary>
        public void SaveWavetableToWav(Wavetable wavetable, string filePath)
        {
            using (var writer = new NAudio.Wave.WaveFileWriter(filePath, new NAudio.Wave.WaveFormat(44100, 16, 1)))
            {
                int totalSamplesFor10sec = 441000; // 44100 * 10
                int samplesPerFrame = wavetable.TableSize;
                int samplesPerFrameDuration = totalSamplesFor10sec / wavetable.FrameCount;
                int freqMultiplier = 2; // Частота ~55 Hz (-3 октавы)
                
                var buffer = new byte[totalSamplesFor10sec * 2];
                int bufferPos = 0;
                
                int currentFrame = 0;
                int position = 0;
                int samplesInCurrentFrame = 0;
                
                for (int i = 0; i < totalSamplesFor10sec; i++)
                {
                    // Переключаемся на следующий фрейм
                    if (samplesInCurrentFrame >= samplesPerFrameDuration && currentFrame < wavetable.FrameCount - 1)
                    {
                        currentFrame++;
                        samplesInCurrentFrame = 0;
                    }
                    
                    int frameStart = currentFrame * samplesPerFrame;
                    int wrappedPos = position - frameStart;
                    
                    // Цикличное воспроизведение внутри фрейма
                    if (wrappedPos >= samplesPerFrame)
                    {
                        position = frameStart;
                        wrappedPos = 0;
                    }
                    
                    // Ускоряем чтение для повышения частоты
                    int readPos = frameStart + (wrappedPos * freqMultiplier) % samplesPerFrame;
                    float sample = wavetable.Samples[readPos];
                    
                    position++;
                    samplesInCurrentFrame++;
                    
                    // Конвертируем в 16-bit PCM
                    short sample16 = (short)(Math.Max(-1.0f, Math.Min(1.0f, sample)) * short.MaxValue);
                    buffer[bufferPos++] = (byte)(sample16 & 0xFF);
                    buffer[bufferPos++] = (byte)((sample16 >> 8) & 0xFF);
                }
                
                writer.Write(buffer, 0, buffer.Length);
            }
        }
        
        /// <summary>
        /// Генерирует Hard Sync осциллятор с уникальными параметрами
        /// УЛУЧШЕННАЯ ВЕРСИЯ: более мягкий и гармоничный
        /// </summary>
        private void GenerateHardSyncFrame(float[] samples, FrameParameters parameters)
        {
            int tableSize = samples.Length;
            double phaseIncrement = TWO_PI / tableSize;
            
            var random = new Random(unchecked(
                parameters.GetHashCode() * 31 + 
                Environment.TickCount * 17 + 
                Guid.NewGuid().GetHashCode() * 13
            ));
            
            double syncRatio = 1.0 + random.NextDouble() * 3.5;
            double harmonicAmp = 0.1 + random.NextDouble() * 0.25;
            double subharmonicAmp = 0.05 + random.NextDouble() * 0.15;
            
            harmonicAmp *= (0.6 + parameters.Brightness * 0.4);
            subharmonicAmp *= (0.6 + parameters.Warmth * 0.4);
            
            // Phase distortion
            double phaseDistortion = 0.03 + random.NextDouble() * 0.1;
            
            double masterPhase = 0;
            double slavePhase = 0;
            double harmFreq = 2.0 + random.NextDouble() * 1.5;
            
            for (int i = 0; i < tableSize; i++)
            {
                // Phase distortion
                double distortedIndex = i + Math.Sin(i * 0.2) * phaseDistortion;
                double distortedPhaseInc = phaseIncrement * (1.0 + Math.Sin(distortedIndex * 0.1) * 0.05);
                
                masterPhase += distortedPhaseInc;
                if (masterPhase >= TWO_PI)
                {
                    masterPhase -= TWO_PI;
                    slavePhase = 0;
                }
                
                slavePhase += distortedPhaseInc * syncRatio;
                
                float slaveSaw = (float)((slavePhase / TWO_PI) * 2.0 - 1.0);
                float harmonic = (float)FastSin(slavePhase * harmFreq) * (float)harmonicAmp;
                float subharmonic = (float)FastSin(slavePhase * 0.5) * (float)subharmonicAmp;
                
                double value = slaveSaw * 0.5f + harmonic + subharmonic;
                
                // Wave folding
                if (Math.Abs(value) > 1.2)
                {
                    value = Math.Sign(value) * (2.4 - Math.Abs(value));
                }
                
                samples[i] = (float)value;
            }
            
            // HardSync - цифровой пресет, оставляем резкие пики и впадины без сглаживания
            NormalizeInPlace(samples);
        }
        
        /// <summary>
        /// Генерирует PWM (Pulse Width Modulation) волну с уникальными параметрами
        /// УЛУЧШЕННАЯ ВЕРСИЯ: более мягкий и гармоничный
        /// </summary>
        private void GeneratePWMFrame(float[] samples, FrameParameters parameters)
        {
            int tableSize = samples.Length;
            double phaseIncrement = TWO_PI / tableSize;
            
            // Уникальный seed (улучшенный)
            var random = new Random(unchecked(
                parameters.GetHashCode() * 31 + 
                Environment.TickCount * 17 + 
                Guid.NewGuid().GetHashCode() * 13
            ));
            
            double basePulseWidth = 0.1 + random.NextDouble() * 0.8;
            double lfoFreq = 0.05 + random.NextDouble() * 0.8;
            double lfoDepth = 0.1 + random.NextDouble() * 0.4;
            
            // Применяем Brightness и Warmth
            lfoDepth *= (0.5 + parameters.Brightness * 0.5);
            
            // Phase distortion
            double phaseDistortion = 0.02 + random.NextDouble() * 0.08;
            
            for (int i = 0; i < tableSize; i++)
            {
                // Phase distortion
                double phase = (i + Math.Sin(i * 0.15) * phaseDistortion) * phaseIncrement;
                
                double lfo = FastSin(phase * lfoFreq + parameters.Phase) * lfoDepth;
                double currentWidth = Math.Max(0.05, Math.Min(0.95, basePulseWidth + lfo));
                
                double wavePos = (phase / TWO_PI) % 1.0;
                float pulseWave = wavePos < currentWidth ? 1.0f : -1.0f;
                
                // Добавляем гармоники для теплоты
                float harmonic3 = (float)FastSin(phase * 3.0) * 0.2f * (float)(0.8 + parameters.Warmth * 0.4);
                float harmonic5 = (float)FastSin(phase * 5.0) * 0.15f * (float)(0.8 + parameters.Warmth * 0.4);
                
                double value = pulseWave * 0.6f + harmonic3 + harmonic5;
                
                // Wave folding
                if (Math.Abs(value) > 1.3)
                {
                    value = Math.Sign(value) * (2.6 - Math.Abs(value));
                }
                
                samples[i] = (float)value;
            }
            
            // PWM - не сглаживаем, оставляем резкие цифровые переходы
            NormalizeInPlace(samples);
        }
        
        /// <summary>
        /// Генерирует формантный звук с выделенными голосовыми гармониками
        /// УПРОЩЁННАЯ ВЕРСИЯ: только основные форманты без избыточных гармоник
        /// </summary>
        private void GenerateFormantFrame(float[] samples, FrameParameters parameters)
        {
            int tableSize = samples.Length;
            double phaseIncrement = TWO_PI / tableSize;
            
            var random = new Random(unchecked(
                parameters.GetHashCode() * 31 + 
                Environment.TickCount * 17
            ));
            
            // Основной тон
            double fundamental = 0.5 + random.NextDouble() * 0.3;
            
            // Выделяем только голосовые гармоники (до 6-й гармоники)
            int vocalsHarmonics = 3 + random.Next(4); // От 3 до 6 гармоник
            
            // Phase distortion
            double phaseDistortion = 0.02 + random.NextDouble() * 0.05;
            
            for (int i = 0; i < tableSize; i++)
            {
                // Phase distortion
                double phase = (i + Math.Sin(i * 0.1) * phaseDistortion) * phaseIncrement;
                double value = 0.0;
                
                // Основной тон
                value += FastSin(phase) * fundamental;
                
                // Добавляем только голосовые гармоники с упрощённой формантной фильтрацией
                for (int h = 1; h <= vocalsHarmonics && h < 8; h++)
                {
                    double amplitude = Math.Pow(1.0 / (h + 1), 1.2); // Более быстрый спад
                    
                    // Выделяем нечётные гармоники (голосовые)
                    if (h % 2 == 1)
                        amplitude *= 1.5;
                    
                    // Яркость: слегка усиливает
                    amplitude *= (0.8 + parameters.Brightness * 0.4);
                    
                    // Теплота: слегка усиливает нижние гармоники
                    if (h <= 3)
                        amplitude *= (0.9 + parameters.Warmth * 0.2);
                    
                    // Лёгкая формантная окраска
                    amplitude *= Math.Sin(phase * h) * 0.3 + 1.0;
                    
                    value += amplitude * FastSin(phase * (h + 1));
                }
                
                // Wave folding
                if (Math.Abs(value) > 1.6)
                {
                    value = Math.Sign(value) * (3.2 - Math.Abs(value));
                }
                
                samples[i] = (float)value;
            }
            
            NormalizeInPlace(samples);
        }
        
        /// <summary>
        /// Генерирует металлический звук с уникальными параметрами
        /// УЛУЧШЕННАЯ ВЕРСИЯ: более музыкальный и гармоничный
        /// </summary>
        private void GenerateMetallicFrame(float[] samples, FrameParameters parameters)
        {
            int tableSize = samples.Length;
            double phaseIncrement = TWO_PI / tableSize;
            
            var random = new Random(unchecked(
                parameters.GetHashCode() * 31 + 
                Environment.TickCount * 17 + 
                Guid.NewGuid().GetHashCode() * 13
            ));
            
            double[] metallicRatios = new double[7];
            for (int i = 0; i < metallicRatios.Length; i++)
            {
                metallicRatios[i] = 1.0 + random.NextDouble() * 5.0;
            }
            Array.Sort(metallicRatios);
            
            double ringFreq1 = 2.0 + random.NextDouble() * 3.0;
            double ringFreq2 = 2.5 + random.NextDouble() * 3.5;
            double ringFreq3 = 3.0 + random.NextDouble() * 3.0;
            double ringFreq4 = 3.5 + random.NextDouble() * 3.0;
            
            double fmFreq = 1.5 + random.NextDouble() * 2.5;
            double fmDepth = 1.5 + random.NextDouble() * 3.0;
            
            fmDepth *= (0.7 + parameters.Brightness * 0.3);
            
            // Phase distortion
            double phaseDistortion = 0.03 + random.NextDouble() * 0.1;
            
            for (int i = 0; i < tableSize; i++)
            {
                // Phase distortion
                double phase = (i + Math.Sin(i * 0.2) * phaseDistortion) * phaseIncrement;
                double value = 0.0;
                
                for (int m = 0; m < metallicRatios.Length; m++)
                {
                    double amplitude = Math.Exp(-m * 0.35);
                    
                    if (m < 3)
                        amplitude *= (0.8 + parameters.Warmth * 0.4);
                    
                    value += amplitude * FastSin(phase * metallicRatios[m] + parameters.Phase);
                }
                
                double ring1 = FastSin(phase * ringFreq1) * FastSin(phase * ringFreq2);
                double ring2 = FastSin(phase * ringFreq3) * FastSin(phase * ringFreq4);
                value += (ring1 + ring2) * 0.15;
                
                double fmMod = FastSin(phase * fmFreq + parameters.Phase) * fmDepth;
                value += FastSin(phase + fmMod) * 0.2;
                
                value += (_globalRandom.NextDouble() * 2.0 - 1.0) * 0.03;
                
                // Wave folding
                if (Math.Abs(value) > 1.8)
                {
                    value = Math.Sign(value) * (3.6 - Math.Abs(value));
                }
                
                samples[i] = (float)value;
            }
            
            // Metallic - цифровой пресет, убираем сглаживание для острых пиков
            NormalizeInPlace(samples);
        }
        
        /// <summary>
        /// Параметры фрейма с дополнительными характеристиками
        /// </summary>
        public struct FrameParameters  
        {
            public double Frequency;
            public double Phase;
            public int Harmonics;
            public double ModulationDepth;
            public double ModulationFrequency;
            public double Brightness; // 0.0 - 1.0, управляет яркостью звука
            public double Warmth;     // 0.0 - 1.0, управляет теплотой звука
        }
    }
}