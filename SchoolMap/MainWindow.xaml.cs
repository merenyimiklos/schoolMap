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
/// Felelős a térkép Canvas rajzolásáért és a ViewModel összekapcsolásáért.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        // ViewModel létrehozása és beállítása
        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        // Feliratkozás a térkép frissítés eseményre
        _viewModel.MapRefreshRequested += DrawMap;

        // Első rajzolás, ha az ablak betöltődött
        Loaded += (_, _) => DrawMap();

        // Újrarajzolás méretváltozáskor
        MapCanvas.SizeChanged += (_, _) => DrawMap();
    }

    /// <summary>
    /// Kategória szín – minden kategóriának egyedi színe van a térképen.
    /// </summary>
    private static SolidColorBrush GetCategoryColor(Category category)
    {
        return category switch
        {
            Category.Tanterem => new SolidColorBrush(Color.FromRgb(0x00, 0xB4, 0xD8)),       // Kék
            Category.Iroda => new SolidColorBrush(Color.FromRgb(0x6C, 0x5C, 0xE7)),          // Lila
            Category.KozossegiTer => new SolidColorBrush(Color.FromRgb(0x00, 0xB8, 0x94)),    // Zöld
            Category.Mosdo => new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xB0)),          // Szürke
            Category.SpecialisTer => new SolidColorBrush(Color.FromRgb(0xFD, 0x79, 0x44)),    // Narancs
            _ => new SolidColorBrush(Color.FromRgb(0xBD, 0xBD, 0xBD)),                       // Alapértelmezett
        };
    }

    /// <summary>
    /// Kiemelt (kiválasztott) kategória szín.
    /// </summary>
    private static SolidColorBrush GetHighlightColor()
    {
        return new SolidColorBrush(Color.FromRgb(0xFF, 0xB7, 0x03)); // Arany kiemelés
    }

    /// <summary>
    /// A térkép rajzolása a Canvas-ra.
    /// Minden szűrés/emeletváltás/kiválasztás után újrarajzolódik.
    /// </summary>
    private void DrawMap()
    {
        MapCanvas.Children.Clear();

        if (_viewModel.Rooms == null || _viewModel.Rooms.Count == 0)
            return;

        var canvasWidth = MapCanvas.ActualWidth;
        var canvasHeight = MapCanvas.ActualHeight;

        if (canvasWidth <= 0 || canvasHeight <= 0)
            return;

        // Skálázás kiszámítása – a helyiségek koordinátái rögzített
        // rendszerben vannak, ezt a Canvas méretéhez igazítjuk
        double maxX = 0, maxY = 0;
        foreach (var room in _viewModel.Rooms)
        {
            var rx = room.X + room.Width;
            var ry = room.Y + room.Height;
            if (rx > maxX) maxX = rx;
            if (ry > maxY) maxY = ry;
        }

        // Margó és skálázás
        double margin = 30;
        double scaleX = (canvasWidth - 2 * margin) / (maxX + 20);
        double scaleY = (canvasHeight - 2 * margin) / (maxY + 20);
        double scale = Math.Min(scaleX, scaleY);
        scale = Math.Min(scale, 1.5); // Ne legyen túl nagy

        // Folyosó háttér rajzolása
        DrawCorridor(canvasWidth, canvasHeight, margin);

        // Helyiségek rajzolása
        foreach (var room in _viewModel.Rooms)
        {
            DrawRoom(room, scale, margin);
        }
    }

    /// <summary>
    /// Folyosó háttér vizuális elem a térkép hangulathoz.
    /// </summary>
    private void DrawCorridor(double canvasWidth, double canvasHeight, double margin)
    {
        // Vízszintes folyosó
        var corridor = new Rectangle
        {
            Width = canvasWidth - margin,
            Height = 40,
            Fill = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0)),
            RadiusX = 6,
            RadiusY = 6
        };
        Canvas.SetLeft(corridor, margin / 2);
        Canvas.SetTop(corridor, canvasHeight / 2 - 20);
        MapCanvas.Children.Add(corridor);

        // Függőleges folyosó
        var vertCorridor = new Rectangle
        {
            Width = 40,
            Height = canvasHeight - margin,
            Fill = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0)),
            RadiusX = 6,
            RadiusY = 6
        };
        Canvas.SetLeft(vertCorridor, canvasWidth / 2 - 20);
        Canvas.SetTop(vertCorridor, margin / 2);
        MapCanvas.Children.Add(vertCorridor);
    }

    /// <summary>
    /// Egy helyiség vizuális megjelenítése a térképen.
    /// </summary>
    private void DrawRoom(Room room, double scale, double margin)
    {
        double x = room.X * scale + margin;
        double y = room.Y * scale + margin;
        double w = room.Width * scale;
        double h = room.Height * scale;

        bool isSelected = room == _viewModel.SelectedRoom;

        // Háttér szín meghatározása
        var bgColor = isSelected ? GetHighlightColor() : GetCategoryColor(room.Category);
        var borderColor = isSelected
            ? new SolidColorBrush(Color.FromRgb(0xE0, 0x9F, 0x00))
            : new SolidColorBrush(Color.FromArgb(60, 0, 0, 0));

        // Kártya-szerű keret a helyiségnek
        var border = new Border
        {
            Width = w,
            Height = h,
            Background = bgColor,
            BorderBrush = borderColor,
            BorderThickness = new Thickness(isSelected ? 3 : 1),
            CornerRadius = new CornerRadius(10),
            Cursor = Cursors.Hand,
            Tag = room,
            Effect = new DropShadowEffect
            {
                BlurRadius = isSelected ? 16 : 8,
                ShadowDepth = isSelected ? 4 : 2,
                Opacity = isSelected ? 0.35 : 0.15,
                Color = Colors.Black
            }
        };

        // Tartalom: ikon + név
        var content = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(4)
        };

        // Ikon
        if (!string.IsNullOrEmpty(room.Icon))
        {
            content.Children.Add(new TextBlock
            {
                Text = room.Icon,
                FontSize = Math.Max(14, Math.Min(w, h) * 0.25),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 2)
            });
        }

        // Név
        var nameBlock = new TextBlock
        {
            Text = room.Name,
            FontSize = Math.Max(10, Math.Min(w, h) * 0.11),
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = w - 10,
            MaxHeight = h * 0.4,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        content.Children.Add(nameBlock);

        border.Child = content;

        // Kattintás kezelése
        border.MouseLeftButtonDown += (sender, _) =>
        {
            if (sender is Border b && b.Tag is Room clickedRoom)
            {
                _viewModel.SelectRoomCommand.Execute(clickedRoom);
            }
        };

        // Hover effektus
        border.MouseEnter += (sender, _) =>
        {
            if (sender is Border b)
            {
                b.Opacity = 0.85;
            }
        };
        border.MouseLeave += (sender, _) =>
        {
            if (sender is Border b)
            {
                b.Opacity = 1.0;
            }
        };

        Canvas.SetLeft(border, x);
        Canvas.SetTop(border, y);
        MapCanvas.Children.Add(border);
    }
}
