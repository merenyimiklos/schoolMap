using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SchoolMap.Models;

namespace SchoolMap.Services
{
    /// <summary>
    /// JSON struktúra az adatfájl deszerializálásához.
    /// </summary>
    internal class RoomData
    {
        [JsonPropertyName("floors")]
        public List<Floor> Floors { get; set; } = new();

        [JsonPropertyName("rooms")]
        public List<RoomDto> Rooms { get; set; } = new();
    }

    /// <summary>
    /// JSON-ból olvasott terem adatok (a Category stringként érkezik).
    /// </summary>
    internal class RoomDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("floor")]
        public int Floor { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// Adatszolgáltató osztály – jelenleg JSON fájlból olvas.
    /// Később ez az osztály cserélhető adatbázisos megvalósításra
    /// anélkül, hogy a ViewModel-eket módosítani kellene.
    /// 
    /// Bővítési lehetőség:
    ///   - IDataService interfész kinyerése
    ///   - Entity Framework vagy Dapper alapú implementáció
    ///   - Dependency Injection a ViewModelekbe
    /// </summary>
    public class DataService
    {
        private List<Room> _rooms = new();
        private List<Floor> _floors = new();

        /// <summary>
        /// Betölti az adatokat a JSON fájlból.
        /// </summary>
        public void LoadData()
        {
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "rooms.json");

            if (!File.Exists(jsonPath))
            {
                // Ha nincs JSON fájl, használjunk üres listákat
                _rooms = new List<Room>();
                _floors = new List<Floor>();
                return;
            }

            var json = File.ReadAllText(jsonPath);
            var data = JsonSerializer.Deserialize<RoomData>(json);

            if (data != null)
            {
                _floors = data.Floors;
                _rooms = data.Rooms.Select(dto => new Room
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    Floor = dto.Floor,
                    Category = Enum.TryParse<Category>(dto.Category, out var cat) ? cat : Category.Egyeb,
                    X = dto.X,
                    Y = dto.Y,
                    Width = dto.Width,
                    Height = dto.Height,
                    Icon = dto.Icon,
                    IsHighlighted = false
                }).ToList();
            }
        }

        /// <summary>Visszaadja az összes helyiséget.</summary>
        public List<Room> GetAllRooms() => _rooms;

        /// <summary>Visszaadja az összes emeletet.</summary>
        public List<Floor> GetAllFloors() => _floors;

        /// <summary>Szűrés kategória alapján.</summary>
        public List<Room> GetRoomsByCategory(Category category)
            => _rooms.Where(r => r.Category == category).ToList();

        /// <summary>Szűrés emelet alapján.</summary>
        public List<Room> GetRoomsByFloor(int floorLevel)
            => _rooms.Where(r => r.Floor == floorLevel).ToList();

        /// <summary>Keresés név vagy leírás alapján (kis-nagybetű érzéketlen).</summary>
        public List<Room> SearchRooms(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _rooms;

            var lower = query.ToLowerInvariant();
            return _rooms.Where(r =>
                r.Name.ToLowerInvariant().Contains(lower) ||
                r.Description.ToLowerInvariant().Contains(lower)
            ).ToList();
        }

        /// <summary>Egy konkrét helyiség lekérése ID alapján.</summary>
        public Room? GetRoomById(int id)
            => _rooms.FirstOrDefault(r => r.Id == id);
    }
}
