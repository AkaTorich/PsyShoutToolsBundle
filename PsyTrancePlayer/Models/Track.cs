using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PsyTrancePlayer.Models;

public class Track : INotifyPropertyChanged
{
    private string _filePath = string.Empty;
    private string _title = string.Empty;
    private string _artist = string.Empty;
    private TimeSpan _duration;
    private int _bpm;
    private bool _isPlaying;

    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged();
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public string Artist
    {
        get => _artist;
        set
        {
            _artist = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            _duration = value;
            OnPropertyChanged();
        }
    }

    public int BPM
    {
        get => _bpm;
        set
        {
            _bpm = value;
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
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
