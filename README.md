# 🏫 SchoolMap – Iskola Digitális Térkép

Modern WPF alkalmazás, amely egy iskola digitális információs térképeként működik, hasonlóan a plázákban található interaktív térképes kijelzőkhöz.

## 📸 Funkciók

- **Interaktív térkép** – Canvas alapú térképes megjelenítés kattintható helyiségekkel
- **Emeletváltás** – Földszint, 1. emelet, 2. emelet között váltás
- **Kategóriaszűrés** – Tantermek, irodák, közösségi terek, mosdók, speciális termek
- **Keresés** – Terem neve vagy leírása alapján szöveges keresés
- **Részletek panel** – Kiválasztott helyiség neve, leírása, emelete, kategóriája
- **Modern kiosk UI** – Érintőkijelzőre optimalizált, nagy gombok, lekerekített sarkok, árnyékok
- **Útvonaltervezés előkészítve** – Későbbi fejlesztésre kész gomb

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
├── Views/
│   └── (későbbi nézetek helye)
├── Converters/
│   └── BoolToVisibilityConverter.cs  # Bool → Visibility konverter
├── Data/
│   └── rooms.json         # Mintaadatok JSON formátumban
├── App.xaml               # Alkalmazás erőforrások és stílusok
├── App.xaml.cs
├── MainWindow.xaml         # Főablak UI (térkép, keresés, szűrők, info panel)
└── MainWindow.xaml.cs      # Térkép rajzolás logikája
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
| `Data/rooms.json` | Mintaadatok 21 helyiséggel, 3 emelettel |
| `App.xaml` | Globális stílusok: színek, gombok, kártyák, szövegek |
| `MainWindow.xaml` | A teljes UI felépítés XAML-ben |
| `MainWindow.xaml.cs` | Canvas térkép rajzolás, kattintás kezelés |

## 🚀 Indítás

```bash
cd SchoolMap
dotnet run
```

Vagy Visual Studio-ban nyisd meg a `SchoolMap.csproj` fájlt és futtasd.

## 📈 Továbbfejlesztési lehetőségek

### Adatbázis bevezetése
1. Hozz létre egy `IDataService` interfészt a `DataService` alapján
2. Készíts egy `DatabaseService` implementációt (Entity Framework / Dapper)
3. Cseréld ki a `MainViewModel`-ben a `DataService`-t az interfészre
4. Használj Dependency Injection-t (pl. `Microsoft.Extensions.DependencyInjection`)

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

### Több emelet egyszerre
- 3D-s emelet nézet
- Emelet közötti navigáció animáció

## 🎨 Technológia

- **C# / .NET 8**
- **WPF** (Windows Presentation Foundation)
- **XAML** alapú UI
- **MVVM** minta (enyhített változat)
- **JSON** alapú adattárolás (adatbázis nélkül)
