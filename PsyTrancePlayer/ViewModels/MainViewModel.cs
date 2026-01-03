using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using PsyTrancePlayer.Models;
using PsyTrancePlayer.Services;

namespace PsyTrancePlayer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly AudioPlayerService _audioPlayer;
    private readonly BpmDetectorService _bpmDetector;
    private readonly VisualizationService _visualizer;
    private readonly DispatcherTimer _timer;

    private Track? _currentTrack;
    private TimeSpan _currentPosition;
    private bool _isPlaying;
    private float _volume = 0.5f;
    private float[] _spectrumData = new float[64];
    private bool _isUserSeeking = false;

    public ObservableCollection<Track> Playlist { get; } = new();

    public Track? CurrentTrack
    {
        get => _currentTrack;
        set
        {
            if (_currentTrack != null)
            {
                _currentTrack.PropertyChanged -= Track_PropertyChanged;
            }
            
            _currentTrack = value;
            
            if (_currentTrack != null)
            {
                _currentTrack.PropertyChanged += Track_PropertyChanged;
                TotalDuration = _currentTrack.Duration;
            }
            else
            {
                TotalDuration = TimeSpan.Zero;
            }
            
            OnPropertyChanged();
            // Обновляем команды при изменении трека
            CommandManager.InvalidateRequerySuggested();
        }
    }
    
    private void Track_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Track.Duration) && sender is Track track)
        {
            TotalDuration = track.Duration;
        }
    }

    public TimeSpan CurrentPosition
    {
        get => _currentPosition;
        set
        {
            _currentPosition = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentPositionSeconds));
        }
    }

    public double CurrentPositionSeconds
    {
        get => _currentPosition.TotalSeconds;
        set
        {
            if (Math.Abs(value - _currentPosition.TotalSeconds) > 0.5)
            {
                _isUserSeeking = true;
                var newPosition = TimeSpan.FromSeconds(value);
                _audioPlayer.Seek(newPosition);
                _currentPosition = newPosition;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPosition));
                _isUserSeeking = false;
            }
        }
    }

    private TimeSpan _totalDuration;
    public TimeSpan TotalDuration
    {
        get => _totalDuration;
        private set
        {
            _totalDuration = value;
            OnPropertyChanged();
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            _isPlaying = value;
            OnPropertyChanged();
            // Обновляем команды при изменении состояния воспроизведения
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            _audioPlayer.Volume = value;
            OnPropertyChanged();
        }
    }

    public float[] SpectrumData
    {
        get => _spectrumData;
        set
        {
            _spectrumData = value;
            OnPropertyChanged();
        }
    }

    public ICommand PlayCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand AddFilesCommand { get; }
    public ICommand RemoveTrackCommand { get; }
    public ICommand PlayTrackCommand { get; }

    public MainViewModel()
    {
        _audioPlayer = new AudioPlayerService();
        _bpmDetector = new BpmDetectorService();
        _visualizer = new VisualizationService();

        PlayCommand = new RelayCommand(_ => Play(), _ => CurrentTrack != null);
        PauseCommand = new RelayCommand(_ => Pause(), _ => IsPlaying);
        StopCommand = new RelayCommand(_ => Stop(), _ => CurrentTrack != null);
        NextCommand = new RelayCommand(_ => PlayNext(), _ => Playlist.Count > 0);
        PreviousCommand = new RelayCommand(_ => PlayPrevious(), _ => Playlist.Count > 0);
        AddFilesCommand = new RelayCommand(_ => AddFiles());
        RemoveTrackCommand = new RelayCommand(track => RemoveTrack(track as Track));
        PlayTrackCommand = new RelayCommand(async track => await PlayTrackAsync(track as Track));

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33)  // 30 FPS instead of 60 - reduces CPU usage
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        _audioPlayer.PlaybackStopped += (s, e) => PlayNext();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (_audioPlayer.IsPlaying && CurrentTrack != null)
            {
                var newPosition = _audioPlayer.CurrentTime;
                var totalTime = _audioPlayer.TotalTime;

                if (!_isUserSeeking && newPosition != CurrentPosition)
                {
                    CurrentPosition = newPosition;
                }
                
                // Проверяем, достиг ли трек конца (с запасом в 0.5 секунды)
                if (totalTime > TimeSpan.Zero && (totalTime - newPosition).TotalSeconds <= 0.5)
                {
                    // Трек закончился, переключаем на следующий
                    PlayNext();
                    return;
                }
                
                // Получаем реальные аудиоданные для визуализации
                var samples = _audioPlayer.GetCurrentSamples(2048);
                if (samples != null && samples.Length > 0)
                {
                    var spectrum = _visualizer.GetSpectrumData(samples, 64);
                    SpectrumData = _visualizer.SmoothSpectrum(spectrum, SpectrumData, 0.5f);
                }
                else
                {
                    // Если данных нет, используем простую визуализацию
                    GenerateSimpleVisualization();
                }
            }
            else if (CurrentTrack != null && !_audioPlayer.IsPlaying)
            {
                // Обновляем позицию даже когда не играет
                var newPosition = _audioPlayer.CurrentTime;
                if (!_isUserSeeking && newPosition != CurrentPosition)
                {
                    CurrentPosition = newPosition;
                }
                // Только затухаем, не генерируем новую анимацию
                FadeSpectrum();
            }
            else
            {
                // Полностью затухаем когда нет трека
                FadeSpectrum();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Timer_Tick error: {ex.Message}");
        }
    }

    private void GenerateSimpleVisualization()
    {
        // Используем позицию воспроизведения для синхронизации визуализации
        var position = CurrentPosition.TotalSeconds;
        var volume = _volume;
        var newSpectrum = new float[64];

        for (int i = 0; i < 64; i++)
        {
            // Генерируем волновую визуализацию на основе позиции воспроизведения
            var frequency = (i + 1) * 0.15;
            var wave = Math.Sin(position * frequency * 2 * Math.PI + i * 0.1);
            var amplitude = volume * 0.6 + 0.2;
            newSpectrum[i] = (float)(Math.Abs(wave) * amplitude);
        }

        SpectrumData = _visualizer.SmoothSpectrum(newSpectrum, SpectrumData, 0.5f);
    }

    private void FadeSpectrum()
    {
        var faded = new float[SpectrumData.Length];
        for (int i = 0; i < SpectrumData.Length; i++)
        {
            faded[i] = SpectrumData[i] * 0.95f; // Плавное затухание
        }
        SpectrumData = faded;
    }

    private async void AddFiles()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Audio Files|*.mp3;*.wav;*.flac;*.ogg|All Files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            foreach (var file in dialog.FileNames)
            {
                await AddTrackAsync(file);
            }
        }
    }

    private async Task AddTrackAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageBox.Show(
                $"File not found:\n{filePath}",
                "File Not Found",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var track = new Track
        {
            FilePath = filePath,
            Title = Path.GetFileNameWithoutExtension(filePath),
            Artist = "Unknown Artist"
        };

        try
        {
            await Task.Run(() =>
            {
                track.BPM = _bpmDetector.DetectBpm(filePath);
                track.Duration = AudioPlayerService.GetDuration(filePath);
            });

            Playlist.Add(track);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to add track:\n{filePath}\n\n{ex.Message}",
                "Error Adding Track",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void RemoveTrack(Track? track)
    {
        if (track != null)
        {
            if (track == CurrentTrack)
            {
                Stop();
            }
            Playlist.Remove(track);
        }
    }

    private async Task PlayTrackAsync(Track? track)
    {
        if (track == null) return;

        try
        {
            CurrentTrack = track;
            await _audioPlayer.LoadTrackAsync(track);
            // TotalDuration обновится автоматически через Track_PropertyChanged
            Play();
        }
        catch (FileNotFoundException)
        {
            MessageBox.Show(
                $"Audio file not found:\n{track.FilePath}\n\nThe file may have been moved or deleted.",
                "File Not Found",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            CurrentTrack = null;
        }
        catch (NotSupportedException)
        {
            MessageBox.Show(
                $"Unsupported audio format:\n{Path.GetExtension(track.FilePath)}\n\nPlease use MP3, WAV, FLAC, OGG, or other supported formats.",
                "Unsupported Format",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            CurrentTrack = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to load audio file:\n{ex.Message}\n\nFile: {track.FilePath}",
                "Playback Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            CurrentTrack = null;
        }
    }

    private async void Play()
    {
        if (CurrentTrack != null)
        {
            try
            {
                // Если трек не загружен, загружаем его
                if (_audioPlayer.CurrentTrack != CurrentTrack)
                {
                    await _audioPlayer.LoadTrackAsync(CurrentTrack);
                }
                
                _audioPlayer.Play();
                IsPlaying = _audioPlayer.IsPlaying;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to play track:\n{ex.Message}\n\nFile: {CurrentTrack.FilePath}",
                    "Playback Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void Pause()
    {
        _audioPlayer.Pause();
        IsPlaying = _audioPlayer.IsPlaying;
    }

    private void Stop()
    {
        _audioPlayer.Stop();
        IsPlaying = false;
        CurrentPosition = TimeSpan.Zero;
    }

    private void PlayNext()
    {
        if (Playlist.Count == 0) return;

        var currentIndex = CurrentTrack != null ? Playlist.IndexOf(CurrentTrack) : -1;
        var nextIndex = (currentIndex + 1) % Playlist.Count;

        _ = PlayTrackAsync(Playlist[nextIndex]);
    }

    private void PlayPrevious()
    {
        if (Playlist.Count == 0) return;

        var currentIndex = CurrentTrack != null ? Playlist.IndexOf(CurrentTrack) : -1;
        var prevIndex = currentIndex <= 0 ? Playlist.Count - 1 : currentIndex - 1;

        _ = PlayTrackAsync(Playlist[prevIndex]);
    }

    public void PauseVisualization()
    {
        _timer.Stop();
    }

    public void ResumeVisualization()
    {
        _timer.Start();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
