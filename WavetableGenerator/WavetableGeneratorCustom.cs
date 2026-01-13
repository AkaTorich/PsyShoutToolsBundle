using System;

namespace WavetableGenerator
{
    /// <summary>
    /// Генератор волновых таблиц с морфингом между двумя пользовательскими формами
    /// </summary>
    public class WavetableGeneratorCustom
    {
        private const double TWO_PI = Math.PI * 2.0;
        
        /// <summary>
        /// Генерирует волновую таблицу морфингом между двумя пользовательскими волнами
        /// </summary>
        public static Wavetable GenerateMorphingWavetable(float[] startWave, float[] endWave, 
            int frameCount = 256, int tableSize = 2048, double brightness = 0.5, double warmth = 0.5)
        {
            int totalSamples = tableSize * frameCount;
            var wavetable = new Wavetable("Custom", totalSamples, WaveType.Custom, 440.0, 1.0, frameCount);
            wavetable.Brightness = brightness;
            wavetable.Warmth = warmth;
            
            // Генерируем все фреймы прямым морфингом (без FFT для простоты)
            for (int frame = 0; frame < frameCount; frame++)
            {
                double progress = (double)frame / (frameCount - 1);
                int offset = frame * tableSize;
                
                // Прямая интерполяция между волнами
                for (int i = 0; i < tableSize; i++)
                {
                    // ЛИНЕЙНАЯ интерполяция между формами
                    float value = startWave[i] * (float)(1 - progress) + endWave[i] * (float)progress;
                    
                    // Усиление яркости: добавляем высокочастотную модуляцию
                    if (brightness > 0.5)
                    {
                        double phase = (double)i / tableSize * TWO_PI;
                        value += (float)(Math.Sin(phase * 8) * (brightness - 0.5) * 0.2 * progress);
                    }
                    
                    // Усиление теплоты: добавляем низкочастотную модуляцию
                    if (warmth > 0.5)
                    {
                        double phase = (double)i / tableSize * TWO_PI;
                        value += (float)(Math.Sin(phase * 2) * (warmth - 0.5) * 0.3 * (1 - progress));
                    }
                    
                    // Контрастность через нелинейное изменение амплитуды
                    double contrastCurve = Math.Sin(progress * Math.PI);
                    value *= (float)(1 + contrastCurve * 0.2);
                    
                    wavetable.Samples[offset + i] = value;
                }
                
                // Нормализуем фрейм
                float max = 0;
                for (int i = 0; i < tableSize; i++)
                {
                    float abs = Math.Abs(wavetable.Samples[offset + i]);
                    if (abs > max) max = abs;
                }
                if (max > 0)
                {
                    for (int i = 0; i < tableSize; i++)
                        wavetable.Samples[offset + i] /= max * 0.95f;
                }
            }
            
            return wavetable;
        }
        
        /// <summary>
        /// Преобразует массив точек canvas в волну 2048 сэмплов
        /// </summary>
        public static float[] CanvasToWave(float[] canvasPoints, int targetSize = 2048)
        {
            var wave = new float[targetSize];
            int sourceSize = canvasPoints.Length;
            
            // Ресэмплинг с кубической интерполяцией
            for (int i = 0; i < targetSize; i++)
            {
                double sourcePos = (double)i * sourceSize / targetSize;
                int index = (int)sourcePos;
                double frac = sourcePos - index;
                
                // Кубическая интерполяция
                int i0 = Math.Max(0, index - 1);
                int i1 = index;
                int i2 = Math.Min(sourceSize - 1, index + 1);
                int i3 = Math.Min(sourceSize - 1, index + 2);
                
                float p0 = canvasPoints[i0];
                float p1 = canvasPoints[i1];
                float p2 = canvasPoints[i2];
                float p3 = canvasPoints[i3];
                
                wave[i] = CubicInterpolate(p0, p1, p2, p3, frac);
            }
            
            // Нормализуем
            float max = 0;
            foreach (var v in wave)
            {
                float abs = Math.Abs(v);
                if (abs > max) max = abs;
            }
            if (max > 0)
            {
                for (int i = 0; i < wave.Length; i++)
                    wave[i] /= max;
            }
            
            return wave;
        }
        
        private static float CubicInterpolate(float y0, float y1, float y2, float y3, double mu)
        {
            double mu2 = mu * mu;
            double a0 = y3 - y2 - y0 + y1;
            double a1 = y0 - y1 - a0;
            double a2 = y2 - y0;
            double a3 = y1;
            
            return (float)(a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3);
        }
        
        /// <summary>
        /// Сохраняет волновую таблицу в WAV файл (10 секунд, ~440 Hz)
        /// </summary>
        public static void SaveToWav(Wavetable wavetable, string filename)
        {
            using (var writer = new NAudio.Wave.WaveFileWriter(filename, new NAudio.Wave.WaveFormat(44100, 16, 1)))
            {
                int totalSamplesFor10sec = 441000; // 44100 * 10
                int samplesPerFrame = wavetable.TableSize;
                int samplesPerFrameDuration = totalSamplesFor10sec / wavetable.FrameCount;
                int freqMultiplier = 20; // Частота ~440 Hz
                
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
    }
}

