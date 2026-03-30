namespace SchoolMap.Models
{
    /// <summary>
    /// Egy emelet adatait tároló modell.
    /// Később bővíthető további tulajdonságokkal (pl. alaprajz kép, méretezés).
    /// </summary>
    public class Floor
    {
        public int Id { get; set; }

        /// <summary>Emelet megjelenítési neve (pl. "Földszint", "1. emelet")</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Emelet szám (0 = földszint, 1 = első emelet, stb.)</summary>
        public int Level { get; set; }
    }
}
