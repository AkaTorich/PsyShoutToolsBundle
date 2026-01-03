using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Vorbis;
using PsyTrancePlayer.Models;

namespace PsyTrancePlayer.Services;

public class AudioPlayerService : IDisposable
{
    private WaveOutEvent? _waveOut;
    private WaveStream? _audioStream;
    private VolumeSampleProvider? _volumeProvider;
    private SampleAggregator? _sampleAggregator;
    private Track? _currentTrack;
    private bool _isPlaying;
    private float _volume = 0.5f;
    private bool _wasManuallyStopped = false;

    public event EventHandler<float[]>? AudioDataAvailable;
    public event EventHandler? PlaybackStopped;

    public Track? CurrentTrack => _currentTrack;
    public bool IsPlaying => _isPlaying || (_waveOut?.PlaybackState == PlaybackState.Playing);
    public TimeSpan CurrentTime => _audioStream?.CurrentTime ?? TimeSpan.Zero;
    public TimeSpan TotalTime => _audioStream?.TotalTime ?? TimeSpan.Zero;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_volumeProvider != null)
                _volumeProvider.Volume = _volume;
        }
    }

    public AudioPlayerService()
    {
    }

    public Task LoadTrackAsync(Track track)
    {
        if (!File.Exists(track.FilePath))
        {
            throw new FileNotFoundException($"Audio file not found: {track.FilePath}");
        }

        var extension = Path.GetExtension(track.FilePath).ToLowerInvariant();
        var supportedFormats = new[] { ".mp3", ".wav", ".flac", ".ogg", ".aac", ".wma", ".m4a" };
        if (!Array.Exists(supportedFormats, ext => ext == extension))
        {
            throw new NotSupportedException($"Unsupported audio format: {extension}");
        }

        Stop();

        try
        {
            _currentTrack = track;
            
            // Выбираем правильный ридер в зависимости от формата
            WaveStream? reader = null;
            
            switch (extension)
            {
                case ".flac":
                    // Используем MediaFoundationReader для FLAC (работает на Windows 10+)
                    try
                    {
                        reader = new MediaFoundationReader(track.FilePath);
                    }
                    catch
                    {
                        // Если MediaFoundation не поддерживает, пробуем через AudioFileReader
                        reader = new AudioFileReader(track.FilePath);
                    }
                    break;
                case ".ogg":
                    reader = new VorbisWaveReader(track.FilePath);
                    break;
                default:
                    // MP3, WAV и другие форматы через AudioFileReader
                    reader = new AudioFileReader(track.FilePath);
                    break;
            }
            
            _audioStream = reader;
            
            // Создаем провайдер для управления громкостью и агрегатор для визуализации
            var sampleProvider = reader.ToSampleProvider();
            _sampleAggregator = new SampleAggregator(sampleProvider);
            _volumeProvider = new VolumeSampleProvider(_sampleAggregator)
            {
                Volume = _volume
            };

            if (track.Duration == TimeSpan.Zero)
            {
                track.Duration = _audioStream.TotalTime;
            }

            // Проверяем наличие аудио устройств
            if (WaveOut.DeviceCount == 0)
            {
                throw new Exception("No audio output devices found");
            }

            _waveOut = new WaveOutEvent
            {
                DesiredLatency = 200,  // 200ms buffer to prevent audio clicks
                NumberOfBuffers = 3     // 3 buffers for extra protection
            };
            _waveOut.Init(_volumeProvider);
            _wasManuallyStopped = false;
            _waveOut.PlaybackStopped += (s, e) =>
            {
                _isPlaying = false;
                if (_currentTrack != null)
                    _currentTrack.IsPlaying = false;
                
                // Вызываем событие только если трек действительно закончился (не был остановлен вручную)
                if (!_wasManuallyStopped && _audioStream != null)
                {
                    // Проверяем, достиг ли трек конца (с запасом в 1 секунду для надежности)
                    var timeDiff = (_audioStream.TotalTime - _audioStream.CurrentTime).TotalSeconds;
                    if (timeDiff <= 1.0)
                    {
                        PlaybackStopped?.Invoke(this, EventArgs.Empty);
                    }
                }
                _wasManuallyStopped = false; // Сбрасываем флаг
            };
        }
        catch (DllNotFoundException dllEx)
        {
            Stop();
            throw new Exception($"Missing codec library for format {extension}. Please install required codecs or use MP3/WAV format. Error: {dllEx.Message}", dllEx);
        }
        catch (Exception ex)
        {
            Stop();
            var errorMsg = $"Failed to load audio file: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMsg += $"\nInner exception: {ex.InnerException.Message}";
            }
            throw new Exception(errorMsg, ex);
        }

        return Task.CompletedTask;
    }

    public void Play()
    {
        if (_waveOut == null || _audioStream == null)
        {
            System.Diagnostics.Debug.WriteLine("Play: WaveOut or AudioStream is null");
            return;
        }

        try
        {
            var state = _waveOut.PlaybackState;
            System.Diagnostics.Debug.WriteLine($"Play: Current state = {state}");
            
            if (state == PlaybackState.Stopped)
            {
                _waveOut.Play();
            }
            else if (state == PlaybackState.Paused)
            {
                _waveOut.Play();
            }
            
            _isPlaying = true;
            if (_currentTrack != null)
                _currentTrack.IsPlaying = true;
                
            System.Diagnostics.Debug.WriteLine($"Play: After play, state = {_waveOut.PlaybackState}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Play error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Play error stack: {ex.StackTrace}");
            _isPlaying = false;
        }
    }

    public void Pause()
    {
        _wasManuallyStopped = true; // Пауза тоже считается ручной остановкой
        if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
        {
            try
            {
                _waveOut.Pause();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Pause error: {ex.Message}");
            }
        }
        _isPlaying = false;
        if (_currentTrack != null)
            _currentTrack.IsPlaying = false;
    }

    public void Stop()
    {
        _wasManuallyStopped = true; // Помечаем, что остановка была вручную
        try
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Stop error: {ex.Message}");
        }

        try
        {
            if (_audioStream != null)
            {
                _audioStream.Dispose();
                _audioStream = null;
            }
            _volumeProvider = null;
            _sampleAggregator = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AudioStream dispose error: {ex.Message}");
        }

        _isPlaying = false;
        if (_currentTrack != null)
            _currentTrack.IsPlaying = false;
    }

    public void Seek(TimeSpan position)
    {
        if (_audioStream != null)
        {
            _audioStream.CurrentTime = position;
        }
    }

    public float[] GetCurrentSamples(int sampleCount)
    {
        if (_sampleAggregator != null)
        {
            return _sampleAggregator.GetSamples(sampleCount);
        }
        return new float[sampleCount];
    }

    public static TimeSpan GetDuration(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Audio file not found: {filePath}");
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        WaveStream? reader = null;

        try
        {
            switch (extension)
            {
                case ".flac":
                    try
                    {
                        reader = new MediaFoundationReader(filePath);
                    }
                    catch
                    {
                        reader = new AudioFileReader(filePath);
                    }
                    break;
                case ".ogg":
                    reader = new VorbisWaveReader(filePath);
                    break;
                default:
                    reader = new AudioFileReader(filePath);
                    break;
            }

            return reader.TotalTime;
        }
        finally
        {
            reader?.Dispose();
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
