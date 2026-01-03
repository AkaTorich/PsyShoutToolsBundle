using System.Collections.ObjectModel;

namespace PsyTrancePlayer.Models;

public class Playlist
{
    public ObservableCollection<Track> Tracks { get; set; } = new();
    public string Name { get; set; } = "My Playlist";
}
