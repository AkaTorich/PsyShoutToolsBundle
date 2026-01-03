using System;
using System.Numerics;

namespace PsyTrancePlayer.Services;

public class VisualizationService
{
    private const int FftSize = 2048;

    public float[] GetSpectrumData(float[] audioSamples, int barCount = 64)
    {
        if (audioSamples == null || audioSamples.Length < FftSize)
        {
            return new float[barCount];
        }

        var fftBuffer = new Complex[FftSize];
        
        // Применяем оконную функцию Ханна для уменьшения утечки спектра
        for (int i = 0; i < FftSize; i++)
        {
            var window = 0.5f * (1.0f - (float)Math.Cos(2.0 * Math.PI * i / (FftSize - 1))); // Hann window
            fftBuffer[i] = new Complex(audioSamples[i] * window, 0);
        }

        PerformFFT(fftBuffer);

        var spectrum = new float[barCount];
        int samplesPerBar = (FftSize / 2) / barCount;

        // Логарифмическое масштабирование для лучшей визуализации
        for (int i = 0; i < barCount; i++)
        {
            float sum = 0;
            int count = 0;
            
            for (int j = 0; j < samplesPerBar; j++)
            {
                int index = i * samplesPerBar + j;
                if (index < FftSize / 2 && index > 0)
                {
                    var magnitude = (float)fftBuffer[index].Magnitude;
                    // Логарифмическое масштабирование
                    sum += (float)Math.Log10(1 + magnitude * 100);
                    count++;
                }
            }
            
            spectrum[i] = count > 0 ? sum / count : 0;
        }

        NormalizeSpectrum(spectrum);
        return spectrum;
    }

    private void PerformFFT(Complex[] buffer)
    {
        int n = buffer.Length;
        if (n <= 1) return;

        var even = new Complex[n / 2];
        var odd = new Complex[n / 2];

        for (int i = 0; i < n / 2; i++)
        {
            even[i] = buffer[i * 2];
            odd[i] = buffer[i * 2 + 1];
        }

        PerformFFT(even);
        PerformFFT(odd);

        for (int k = 0; k < n / 2; k++)
        {
            var t = Complex.FromPolarCoordinates(1.0, -2.0 * Math.PI * k / n) * odd[k];
            buffer[k] = even[k] + t;
            buffer[k + n / 2] = even[k] - t;
        }
    }

    private void NormalizeSpectrum(float[] spectrum)
    {
        float max = 0;
        foreach (var value in spectrum)
        {
            if (value > max) max = value;
        }

        if (max > 0.001f) // Минимальный порог для избежания деления на очень маленькие числа
        {
            for (int i = 0; i < spectrum.Length; i++)
            {
                var normalized = spectrum[i] / max;
                // Усиливаем низкие значения для лучшей визуализации
                spectrum[i] = (float)Math.Pow(Math.Min(normalized, 1.0f), 0.7f);
            }
        }
        else
        {
            // Если все значения очень маленькие, обнуляем
            for (int i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = 0f;
            }
        }
    }

    public float[] SmoothSpectrum(float[] current, float[] previous, float smoothing = 0.7f)
    {
        if (previous == null || previous.Length != current.Length)
            return current;

        var smoothed = new float[current.Length];
        for (int i = 0; i < current.Length; i++)
        {
            smoothed[i] = current[i] * (1 - smoothing) + previous[i] * smoothing;
        }

        return smoothed;
    }
}

