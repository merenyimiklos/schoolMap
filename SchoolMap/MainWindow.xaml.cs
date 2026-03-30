using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using SchoolMap.Models;
using SchoolMap.ViewModels;

namespace SchoolMap;

/// <summary>
/// A főablak code-behind-ja.
/// Felelős a 2D térkép rajzolásáért Canvas-on.
/// Minden helyiség egy Rectangle + TextBlock (szobaszám) a közepén.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    // Térkép rajzolási konstansok
    private const double MapMargin = 30;
    private const double RoomCornerRadius = 8;
    private const double MinFontSize = 10;
    private const double FontSizeRatio = 0.25;

    public MainWindow()
    {
        InitializeComponent();

        // ViewModel létrehozása és adatkötés
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Térkép újrarajzolás az eseményekre
        _viewModel.MapRefreshRequested += DrawMap;
        Loaded += (_, _) => DrawMap();
        MapCanvas.SizeChanged += (_, _) => DrawMap();
    }

    /// <summary>
    /// Kategória alapján szín meghatározása.
    /// Minden kategória egyedi színt kap a könnyű vizuális megkülönböztetéshez.
    /// </summary>
    private static Color GetCategoryColor(Category category)
    {
        return category switch
        {
            Category.Tanterem => Color.FromRgb(0x00, 0xB4, 0xD8),      // Kék – tantermek
            Category.Iroda => Color.FromRgb(0x6C, 0x5C, 0xE7),          // Lila – irodák
            Category.KozossegiTer => Color.FromRgb(0x00, 0xB8, 0x94),   // Zöld – közösségi terek
            Category.Mosdo => Color.FromRgb(0xA0, 0xA0, 0xB0),          // Szürke – mosdók
            Category.SpecialisTer => Color.FromRgb(0xFD, 0x79, 0x44),   // Narancs – speciális termek
            _ => Color.FromRgb(0xBD, 0xBD, 0xBD),                       // Alapértelmezett szürke
        };
    }

    /// <summary>
    /// A 2D térkép rajzolása a Canvas-ra.
    /// Csak a kiválasztott emelet helyiségeit jeleníti meg.
    ///
    /// Koordináta-rendszer:
    /// - A rooms.json-ban megadott X, Y, Width, Height értékek egy virtuális
    ///   koordináta-rendszerben vannak (kb. 1000x600 méret).
    /// - Ezeket arányosan méretezzük (scale) a Canvas tényleges méretéhez.
    /// - A térképet középre igazítjuk (offset).
    /// </summary>
    private void DrawMap()
    {
        MapCanvas.Children.Clear();

        var allRooms = _viewModel.AllRooms;
        var selectedFloor = _viewModel.SelectedFloor;

        if (allRooms == null || allRooms.Count == 0 || selectedFloor == null)
            return;

        double canvasWidth = MapCanvas.ActualWidth;
        double canvasHeight = MapCanvas.ActualHeight;

        if (canvasWidth <= 0 || canvasHeight <= 0)
            return;

        // Csak a kiválasztott emelet szobái
        var floorRooms = allRooms.Where(r => r.Floor == selectedFloor.Level).ToList();
        if (floorRooms.Count == 0)
            return;

        // A szobák összesített befoglaló mérete
        double maxX = floorRooms.Max(r => r.X + r.Width);
        double maxY = floorRooms.Max(r => r.Y + r.Height);

        // Méretezés kiszámítása – belefér a Canvas-ba margóval
        double scaleX = (canvasWidth - 2 * MapMargin) / maxX;
        double scaleY = (canvasHeight - 2 * MapMargin) / maxY;
        double scale = Math.Min(scaleX, scaleY);

        // Középre igazítás
        double offsetX = (canvasWidth - maxX * scale) / 2;
        double offsetY = (canvasHeight - maxY * scale) / 2;

        // Emelet felirat rajzolása
        DrawFloorLabel(selectedFloor, offsetX, offsetY);

        // Minden szoba kirajzolása
        foreach (var room in floorRooms)
        {
            DrawRoom(room, scale, offsetX, offsetY);
        }
    }

    /// <summary>
    /// Emelet felirat rajzolása a térkép bal felső sarkába.
    /// </summary>
    private void DrawFloorLabel(Floor floor, double offsetX, double offsetY)
    {
        var label = new TextBlock
        {
            Text = $"📍 {floor.Name}",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xB4, 0xD8)),
            IsHitTestVisible = false
        };

        Canvas.SetLeft(label, offsetX);
        Canvas.SetTop(label, offsetY - 25);
        MapCanvas.Children.Add(label);
    }

    /// <summary>
    /// Egy helyiség rajzolása a Canvas-ra.
    /// Lekerekített téglalapot (Rectangle) rajzol, és rátesz egy
    /// szobaszám feliratot (TextBlock) a közepére.
    ///
    /// Interakciók:
    /// - Kattintás: kiválasztja a szobát (SelectRoomCommand)
    /// - Hover: kiemelés (vastagabb keret, világosabb szín)
    /// - Kiválasztott szoba: sárga szín és vastag keret
    /// </summary>
    private void DrawRoom(Room room, double scale, double offsetX, double offsetY)
    {
        // Szoba pozíció és méret a Canvas-on
        double x = room.X * scale + offsetX;
        double y = room.Y * scale + offsetY;
        double w = room.Width * scale;
        double h = room.Height * scale;

        // Állapot meghatározása
        bool isSelected = room == _viewModel.SelectedRoom;
        bool isFiltered = _viewModel.Rooms.Contains(room);
        double roomOpacity = isFiltered ? 1.0 : 0.3;

        // Szín: kiválasztott → sárga, egyébként kategória szín
        var baseColor = isSelected
            ? Color.FromRgb(0xFF, 0xB7, 0x03)
            : GetCategoryColor(room.Category);

        var defaultStroke = isSelected
            ? new SolidColorBrush(Color.FromRgb(0xE0, 0x9F, 0x00))
            : new SolidColorBrush(Color.FromArgb(60, 0, 0, 0));
        double defaultStrokeThickness = isSelected ? 3 : 1;

        // Lekerekített téglalap (Rectangle) a szoba megjelenítéséhez
        var rect = new Rectangle
        {
            Width = w,
            Height = h,
            RadiusX = RoomCornerRadius,
            RadiusY = RoomCornerRadius,
            Fill = new SolidColorBrush(baseColor),
            Stroke = defaultStroke,
            StrokeThickness = defaultStrokeThickness,
            Opacity = roomOpacity,
            Cursor = Cursors.Hand,
            Tag = room,
            // Modern árnyék effekt
            Effect = new DropShadowEffect
            {
                BlurRadius = 6,
                ShadowDepth = 2,
                Opacity = 0.2,
                Color = Colors.Black
            }
        };

        // Kattintás kezelése – szoba kiválasztás
        rect.MouseLeftButtonDown += (sender, _) =>
        {
            if (sender is Rectangle r && r.Tag is Room clickedRoom)
            {
                _viewModel.SelectRoomCommand.Execute(clickedRoom);
            }
        };

        // Hover effekt – belépéskor kiemeli a szobát
        rect.MouseEnter += (sender, _) =>
        {
            if (sender is Rectangle r)
            {
                r.Opacity = Math.Min(1.0, roomOpacity + 0.15);
                r.StrokeThickness = isSelected ? 3 : 2;
                r.Stroke = new SolidColorBrush(Color.FromRgb(0x00, 0xB4, 0xD8));
            }
        };

        // Hover effekt – kilépéskor visszaáll az eredeti állapot
        rect.MouseLeave += (sender, _) =>
        {
            if (sender is Rectangle r)
            {
                r.Opacity = roomOpacity;
                r.StrokeThickness = defaultStrokeThickness;
                r.Stroke = defaultStroke;
            }
        };

        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        MapCanvas.Children.Add(rect);

        // Szobaszám felirat a téglalap közepén
        var roomNumber = room.Name.Split(' ')[0];
        var label = new TextBlock
        {
            Text = roomNumber,
            FontSize = Math.Max(MinFontSize, Math.Min(w, h) * FontSizeRatio),
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            TextAlignment = TextAlignment.Center,
            IsHitTestVisible = false,
            // Árnyék a szöveg olvashatóságáért
            Effect = new DropShadowEffect
            {
                BlurRadius = 3,
                ShadowDepth = 1,
                Opacity = 0.5,
                Color = Colors.Black
            }
        };

        // Felirat középre igazítása a téglalapban
        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(label, x + (w - label.DesiredSize.Width) / 2);
        Canvas.SetTop(label, y + (h - label.DesiredSize.Height) / 2);
        label.Opacity = roomOpacity;
        MapCanvas.Children.Add(label);
    }
}
