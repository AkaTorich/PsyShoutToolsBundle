using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace PsyTrancePlayer.Services;

public class SampleAggregator : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly float[] _writeBuffer;  // Buffer for audio thread
    private readonly float[] _readBuffer;   // Buffer for UI thread
    private readonly object _lockObject = new object();
    private int _writeIndex;
    private int _sampleCount;
    private readonly int _bufferSize;
    private volatile bool _needsSwap = false;

    public WaveFormat WaveFormat => _source.WaveFormat;

    public SampleAggregator(ISampleProvider source)
    {
        _source = source;
        _bufferSize = 8192; // Increased buffer size
        _writeBuffer = new float[_bufferSize];
        _readBuffer = new float[_bufferSize];
        _writeIndex = 0;
        _sampleCount = 0;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = _source.Read(buffer, offset, count);

        // Save samples to write buffer - minimal lock time
        for (int i = 0; i < samplesRead; i++)
        {
            float sample = buffer[offset + i];

            _writeBuffer[_writeIndex] = sample;
            _writeIndex = (_writeIndex + 1) % _bufferSize;

            if (_sampleCount < _bufferSize)
                _sampleCount++;
        }

        _needsSwap = true;

        return samplesRead;
    }

    public float[] GetSamples(int count)
    {
        // Quick swap if needed - very short lock
        if (_needsSwap)
        {
            lock (_lockObject)
            {
                Array.Copy(_writeBuffer, _readBuffer, _bufferSize);
                _needsSwap = false;
            }
        }

        // Read from read buffer - no lock needed
        var result = new float[count];
        var available = Math.Min(count, _sampleCount);

        if (available > 0)
        {
            // Get most recent samples
            var startIndex = (_writeIndex - available + _bufferSize) % _bufferSize;

            for (int i = 0; i < available; i++)
            {
                var index = (startIndex + i) % _bufferSize;
                result[i] = _readBuffer[index];
            }
        }

        // Fill remainder with zeros if needed
        for (int i = available; i < count; i++)
        {
            result[i] = 0f;
        }

        return result;
    }
}

