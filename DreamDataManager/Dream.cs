// Dream.cs
using System;

namespace DreamDiary
{
    [Serializable]
    public class Dream
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
