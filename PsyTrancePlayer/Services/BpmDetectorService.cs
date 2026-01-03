using System;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace PsyTrancePlayer.Services;

public class BpmDetectorService
{
    public int DetectBpm(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return 0;

            using var reader = new AudioFileReader(filePath);
            const int sampleSize = 1024 * 100;
            var buffer = new float[sampleSize];
            var samplesRead = reader.Read(buffer, 0, sampleSize);

            if (samplesRead == 0)
                return 0;

            var energy = CalculateEnergy(buffer, samplesRead);
            var peaks = DetectPeaks(energy);
            var bpm = CalculateBpmFromPeaks(peaks, reader.WaveFormat.SampleRate);

            return (int)Math.Round(bpm);
        }
        catch
        {
            return 0;
        }
    }

    private float[] CalculateEnergy(float[] samples, int count)
    {
        const int windowSize = 1024;
        var energyValues = new float[count / windowSize];

        for (int i = 0; i < energyValues.Length; i++)
        {
            float sum = 0;
            for (int j = 0; j < windowSize && (i * windowSize + j) < count; j++)
            {
                var sample = samples[i * windowSize + j];
                sum += sample * sample;
            }
            energyValues[i] = sum / windowSize;
        }

        return energyValues;
    }

    private int[] DetectPeaks(float[] energy)
    {
        var threshold = energy.Average() * 1.5f;
        var peaks = new System.Collections.Generic.List<int>();

        for (int i = 1; i < energy.Length - 1; i++)
        {
            if (energy[i] > threshold &&
                energy[i] > energy[i - 1] &&
                energy[i] > energy[i + 1])
            {
                peaks.Add(i);
            }
        }

        return peaks.ToArray();
    }

    private double CalculateBpmFromPeaks(int[] peaks, int sampleRate)
    {
        if (peaks.Length < 2)
            return 128.0;

        var intervals = new System.Collections.Generic.List<int>();
        for (int i = 1; i < peaks.Length; i++)
        {
            intervals.Add(peaks[i] - peaks[i - 1]);
        }

        if (intervals.Count == 0)
            return 128.0;

        var avgInterval = intervals.Average();
        const int windowSize = 1024;
        var bpm = (60.0 * sampleRate) / (avgInterval * windowSize);

        while (bpm < 100)
            bpm *= 2;
        while (bpm > 200)
            bpm /= 2;

        return bpm;
    }
}
