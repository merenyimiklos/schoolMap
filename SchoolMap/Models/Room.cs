namespace SchoolMap.Models
{
    /// <summary>
    /// Egy helyiség (terem, iroda, mosdó, stb.) adatait tároló modell.
    /// Az X, Y, Width, Height mezők a térképes megjelenítéshez kellenek.
    /// </summary>
    public class Room
    {
        public int Id { get; set; }

        /// <summary>Helyiség neve (pl. "101-es tanterem")</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Rövid leírás a helyiségről</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Emelet száma (0 = földszint)</summary>
        public int Floor { get; set; }

        /// <summary>Helyiség kategóriája</summary>
        public Category Category { get; set; }

        /// <summary>Bal felső sarok X pozíció a térképen (pixel)</summary>
        public double X { get; set; }

        /// <summary>Bal felső sarok Y pozíció a térképen (pixel)</summary>
        public double Y { get; set; }

        /// <summary>Helyiség szélessége a térképen (pixel)</summary>
        public double Width { get; set; }

        /// <summary>Helyiség magassága a térképen (pixel)</summary>
        public double Height { get; set; }

        /// <summary>Ki van-e emelve a térképen</summary>
        public bool IsHighlighted { get; set; }

        /// <summary>Ikon neve a kategóriához (pl. "📚", "🏃", "🔬")</summary>
        public string Icon { get; set; } = string.Empty;
    }
}
