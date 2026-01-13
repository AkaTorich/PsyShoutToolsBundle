using System.Collections.Generic;

namespace SpirePresetsGenerator.Models
{
    public sealed class Preset
    {
        public string icon { get; set; }
        public string vendor { get; set; }
        public string favorite { get; set; }
        public string bank { get; set; }
        public List<string> tags { get; set; }
        public string author { get; set; }
        public string notes { get; set; }
        public Dictionary<string, double> parameters { get; set; }

        public Preset()
        {
            icon = string.Empty;
            vendor = string.Empty;
            favorite = "no";
            bank = string.Empty;
            tags = new List<string>();
            author = string.Empty;
            notes = string.Empty;
            parameters = new Dictionary<string, double>();
        }
    }

    public sealed class ArpConfig
    {
        public bool Enabled { get; set; }
        public double Mode { get; set; }
        public double Octave { get; set; }
        public double Speed { get; set; }
        public string Pattern { get; set; }
        public string ModeName { get; set; }
        public string SpeedName { get; set; }
        public string ScaleCategory { get; set; }
        public string ScaleName { get; set; }
    }
} 