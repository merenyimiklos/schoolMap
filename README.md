# 🏫 SchoolMap – Iskola Digitális Térkép

Modern WPF alkalmazás, amely egy iskola digitális információs térképeként működik, hasonlóan a plázákban található interaktív térképes kijelzőkhöz. Az "A" épület 3 emeletét (földszint, 1. emelet, 2. emelet) jeleníti meg L-alakú elrendezésben.

## 📸 Funkciók

- **Interaktív 2D térkép** – Canvas alapú térképes megjelenítés kattintható, lekerekített téglalapokkal
- **Emeletváltás** – Földszint, 1. emelet, 2. emelet között váltás
- **Kategóriaszűrés** – Tantermek, irodák, közösségi terek, mosdók, speciális termek
- **Keresés** – Terem neve vagy leírása alapján szöveges keresés
- **Részletek panel** – Kiválasztott helyiség neve, leírása, emelete, kategóriája
- **Modern kiosk UI** – Érintőkijelzőre optimalizált, nagy gombok, lekerekített sarkok, árnyékok
- **Hover és kijelölés effekt** – Szoba kiemelése egérmozgatásra és kattintásra

## 🏗️ Projektstruktúra

```
SchoolMap/
├── Models/
│   ├── Room.cs            # Helyiség modell (név, pozíció, kategória, stb.)
│   ├── Floor.cs           # Emelet modell
│   └── Category.cs        # Kategória enum (Tanterem, Iroda, stb.)
├── Services/
│   └── DataService.cs     # Adatkezelő szolgáltatás (JSON fájlból olvas)
├── ViewModels/
│   ├── BaseViewModel.cs   # INotifyPropertyChanged alap osztály
│   ├── RelayCommand.cs    # ICommand implementáció
│   └── MainViewModel.cs   # Főablak ViewModel (keresés, szűrés, kijelölés)
├── Converters/
│   └── BoolToVisibilityConverter.cs  # Bool → Visibility konverter
├── Data/
│   └── rooms.json         # Helyiségek adatai JSON formátumban (50 terem)
├── App.xaml               # Alkalmazás erőforrások és stílusok
├── App.xaml.cs
├── MainWindow.xaml         # Főablak UI (térkép, keresés, szűrők, info panel)
└── MainWindow.xaml.cs      # 2D Canvas térkép rajzolás logikája
```

## 🔧 Melyik fájl mire való?

| Fájl | Funkció |
|------|---------|
| `Models/Room.cs` | Egy helyiség összes adata: név, leírás, pozíció, méret, kategória |
| `Models/Floor.cs` | Emelet adatai (szám, megjelenítési név) |
| `Models/Category.cs` | Helyiség típusok felsorolása (enum) |
| `Services/DataService.cs` | Adatok betöltése JSON-ból, keresés, szűrés |
| `ViewModels/MainViewModel.cs` | A főablak logikája: emeletváltás, keresés, szűrés, kijelölés |
| `ViewModels/BaseViewModel.cs` | Property change értesítés alaposztály |
| `ViewModels/RelayCommand.cs` | Parancs binding a XAML-ből |
| `Converters/BoolToVisibilityConverter.cs` | Bool-Visibility konverzió XAML bindinghez |
| `Data/rooms.json` | Helyiségek adatai 50 teremmel, 3 emelettel |
| `App.xaml` | Globális stílusok: színek, gombok, kártyák, szövegek |
| `MainWindow.xaml` | A teljes UI felépítés XAML-ben |
| `MainWindow.xaml.cs` | 2D Canvas térkép rajzolás, kattintás kezelés |

## 🚀 Indítás

```bash
cd SchoolMap
dotnet run
```

Vagy Visual Studio-ban nyisd meg a `SchoolMap.csproj` fájlt és futtasd.

## 📐 Hogyan működik a koordinátás rajzolás?

A térkép megjelenítés a `MainWindow.xaml.cs` fájlban történik, és a következő lépésekből áll:

### 1. Virtuális koordináta-rendszer
Minden szoba a `rooms.json` fájlban egy **virtuális koordináta-rendszerben** van elhelyezve:
- **X, Y**: a szoba bal felső sarkának pozíciója (pixelben)
- **Width, Height**: a szoba mérete (pixelben)
- Az alapértelmezett terület kb. **900×520 pixel** (a szobák befoglaló mérete)

### 2. Automatikus méretezés (scaling)
A `DrawMap()` metódus kiszámítja, hogyan fér be a térkép a Canvas tényleges méretébe:
```csharp
double scaleX = (canvasWidth - 2 * margin) / maxX;
double scaleY = (canvasHeight - 2 * margin) / maxY;
double scale = Math.Min(scaleX, scaleY);
```
Így a térkép mindig arányosan jelenik meg, bármilyen ablakméretben.

### 3. Középre igazítás (offset)
```csharp
double offsetX = (canvasWidth - maxX * scale) / 2;
double offsetY = (canvasHeight - maxY * scale) / 2;
```

### 4. Szobák rajzolása
Minden szoba egy `Rectangle` (lekerekített sarkok, árnyék) + `TextBlock` (szobaszám a közepén):
```csharp
double x = room.X * scale + offsetX;
double y = room.Y * scale + offsetY;
double w = room.Width * scale;
double h = room.Height * scale;
```

## ➕ Hogyan lehet új termet hozzáadni?

### 1. Szerkeszd a `Data/rooms.json` fájlt
Adj hozzá egy új bejegyzést a `rooms` tömbhöz:
```json
{
  "id": 51,
  "name": "038 Könyvtár",
  "description": "Az iskola könyvtára.",
  "floor": 0,
  "building": "A",
  "category": "KozossegiTer",
  "x": 630,
  "y": 400,
  "width": 200,
  "height": 100,
  "icon": "📖"
}
```

### 2. Paraméterek magyarázata
| Mező | Jelentés |
|------|----------|
| `id` | Egyedi azonosító (növekvő szám) |
| `name` | Terem neve (szám + megnevezés) |
| `description` | Rövid leírás |
| `floor` | Emelet (0 = földszint, 1 = 1. emelet, 2 = 2. emelet) |
| `building` | Épület azonosító |
| `category` | Kategória: `Tanterem`, `Iroda`, `KozossegiTer`, `Mosdo`, `SpecialisTer`, `Egyeb` |
| `x`, `y` | Bal felső sarok pozíciója a virtuális térképen |
| `width`, `height` | Méret pixelben |
| `icon` | Emoji ikon a listához |

### 3. Koordináta tippek
- Nézd meg a meglévő szobák koordinátáit az adott emeleten
- A szobák ne fedjék egymást (X+Width < következő X)
- Általános terem méret: kb. 100-120 széles, 60-70 magas

## 🗄️ Hogyan lehet később adatbázisra átállni?

### 1. lépés: Interfész létrehozása
Készíts egy `IDataService` interfészt a jelenlegi `DataService` metódusai alapján:
```csharp
public interface IDataService
{
    void LoadData();
    List<Room> GetAllRooms();
    List<Floor> GetAllFloors();
    List<Room> GetRoomsByCategory(Category category);
    List<Room> GetRoomsByFloor(int floorLevel);
    List<Room> SearchRooms(string query);
    Room? GetRoomById(int id);
}
```

### 2. lépés: DatabaseService implementáció
Készíts egy új osztályt pl. Entity Framework Core-ral:
```csharp
public class DatabaseService : IDataService
{
    private readonly SchoolDbContext _context;

    public DatabaseService(string connectionString)
    {
        _context = new SchoolDbContext(connectionString);
    }

    public List<Room> GetAllRooms() => _context.Rooms.ToList();
    public List<Floor> GetAllFloors() => _context.Floors.ToList();
    // ... többi metódus
}
```

### 3. lépés: ViewModel módosítás
A `MainViewModel`-ben cseréld ki a konkrét osztályt az interfészre:
```csharp
private readonly IDataService _dataService;

public MainViewModel(IDataService dataService)
{
    _dataService = dataService;
    // ...
}
```

### 4. lépés: Dependency Injection (opcionális)
Az `App.xaml.cs`-ben konfiguráld:
```csharp
var services = new ServiceCollection();
services.AddSingleton<IDataService, DatabaseService>();
services.AddTransient<MainViewModel>();
```

## 📈 Továbbfejlesztési lehetőségek

### Admin felület
- Terem hozzáadás / szerkesztés / törlés
- Drag & drop terem pozícionálás a térképen
- Emelet kezelés

### Útvonaltervezés
- Gráf alapú útvonal számítás két terem között
- Vizuális útvonal kijelzés a térképen
- Lépcsőházi csomópontok kezelése emeletek között

### Órarend integráció
- Teremfoglalási adatok megjelenítése
- Aktuális óra kijelzése
- Tanár kereső

### Több épület támogatás
- "B" épület hozzáadása
- Épületek közötti navigáció

## 🎨 Technológia

- **C# / .NET 8**
- **WPF** (Windows Presentation Foundation)
- **XAML** alapú UI
- **MVVM** minta (enyhített változat)
- **JSON** alapú adattárolás (adatbázis nélkül)
