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
/// Felelős a 3D izometrikus térkép rajzolásáért.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    // Isometric projection parameters
    private const double IsoAngle = 0.46; // ~26 degrees in radians
    private const double VerticalScale = 0.55;
    private const double FloorSpacing = 130;
    private const double FloorSlabHeight = 12;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _viewModel.MapRefreshRequested += DrawMap;

        Loaded += (_, _) => DrawMap();
        MapCanvas.SizeChanged += (_, _) => DrawMap();
    }

    /// <summary>
    /// Kategória szín.
    /// </summary>
    private static Color GetCategoryBaseColor(Category category)
    {
        return category switch
        {
            Category.Tanterem => Color.FromRgb(0x00, 0xB4, 0xD8),
            Category.Iroda => Color.FromRgb(0x6C, 0x5C, 0xE7),
            Category.KozossegiTer => Color.FromRgb(0x00, 0xB8, 0x94),
            Category.Mosdo => Color.FromRgb(0xA0, 0xA0, 0xB0),
            Category.SpecialisTer => Color.FromRgb(0xFD, 0x79, 0x44),
            _ => Color.FromRgb(0xBD, 0xBD, 0xBD),
        };
    }

    /// <summary>
    /// Sötétebb szín a 3D oldalfalakhoz.
    /// </summary>
    private static Color DarkenColor(Color c, double factor = 0.7)
    {
        return Color.FromRgb(
            (byte)(c.R * factor),
            (byte)(c.G * factor),
            (byte)(c.B * factor));
    }

    /// <summary>
    /// Izometrikus projekció: 2D → screen koordináta.
    /// </summary>
    private static Point IsoProject(double x, double y, double floorLevel, double scale, double offsetX, double offsetY)
    {
        double cosA = Math.Cos(IsoAngle);
        double sinA = Math.Sin(IsoAngle);

        double sx = (x * cosA + y * sinA) * scale + offsetX;
        double sy = ((-x * sinA + y * cosA) * VerticalScale - floorLevel * FloorSpacing) * scale + offsetY;

        return new Point(sx, sy);
    }

    /// <summary>
    /// A 3D izometrikus térkép rajzolása.
    /// </summary>
    private void DrawMap()
    {
        MapCanvas.Children.Clear();

        var allRooms = _viewModel.AllRooms;
        var floors = _viewModel.Floors;

        if (allRooms == null || allRooms.Count == 0 || floors == null || floors.Count == 0)
            return;

        var canvasWidth = MapCanvas.ActualWidth;
        var canvasHeight = MapCanvas.ActualHeight;

        if (canvasWidth <= 0 || canvasHeight <= 0)
            return;

        // Calculate scale to fit all floors
        double maxX = 0, maxY = 0;
        foreach (var room in allRooms)
        {
            var rx = room.X + room.Width;
            var ry = room.Y + room.Height;
            if (rx > maxX) maxX = rx;
            if (ry > maxY) maxY = ry;
        }

        int maxLevel = floors.Max(f => f.Level);

        // Calculate bounding box of the isometric projection
        var corners = new[]
        {
            IsoProject(0, 0, 0, 1, 0, 0),
            IsoProject(maxX, 0, 0, 1, 0, 0),
            IsoProject(0, maxY, 0, 1, 0, 0),
            IsoProject(maxX, maxY, 0, 1, 0, 0),
            IsoProject(0, 0, maxLevel, 1, 0, 0),
            IsoProject(maxX, 0, maxLevel, 1, 0, 0),
            IsoProject(0, maxY, maxLevel, 1, 0, 0),
            IsoProject(maxX, maxY, maxLevel, 1, 0, 0),
        };

        double minSx = corners.Min(p => p.X);
        double maxSx = corners.Max(p => p.X);
        double minSy = corners.Min(p => p.Y);
        double maxSy = corners.Max(p => p.Y);

        double projWidth = maxSx - minSx;
        double projHeight = maxSy - minSy;

        double margin = 40;
        double scaleX = (canvasWidth - 2 * margin) / projWidth;
        double scaleY = (canvasHeight - 2 * margin) / projHeight;
        double scale = Math.Min(scaleX, scaleY);
        scale = Math.Min(scale, 1.5);

        double offsetX = (canvasWidth - projWidth * scale) / 2 - minSx * scale;
        double offsetY = (canvasHeight - projHeight * scale) / 2 - minSy * scale;

        // Draw floors from bottom to top
        var sortedFloors = floors.OrderBy(f => f.Level).ToList();
        foreach (var floor in sortedFloors)
        {
            bool isSelectedFloor = _viewModel.SelectedFloor != null && _viewModel.SelectedFloor.Level == floor.Level;
            double floorOpacity = isSelectedFloor ? 1.0 : 0.55;

            // Draw floor slab
            DrawFloorSlab(maxX + 20, maxY + 20, floor.Level, scale, offsetX, offsetY, isSelectedFloor, floorOpacity);

            // Draw floor label
            DrawFloorLabel(floor, maxX + 20, maxY + 20, scale, offsetX, offsetY, isSelectedFloor);

            // Draw rooms on this floor
            var floorRooms = allRooms.Where(r => r.Floor == floor.Level).OrderBy(r => r.Y).ThenBy(r => r.X);
            foreach (var room in floorRooms)
            {
                DrawRoom3D(room, floor.Level, scale, offsetX, offsetY, floorOpacity);
            }
        }
    }

    /// <summary>
    /// Az emelet "padló" lapjának rajzolása.
    /// </summary>
    private void DrawFloorSlab(double width, double height, int level, double scale, double offsetX, double offsetY, bool isSelected, double opacity)
    {
        // Top surface of the slab
        var topTL = IsoProject(0, 0, level, scale, offsetX, offsetY);
        var topTR = IsoProject(width, 0, level, scale, offsetX, offsetY);
        var topBR = IsoProject(width, height, level, scale, offsetX, offsetY);
        var topBL = IsoProject(0, height, level, scale, offsetX, offsetY);

        var topSurface = new Polygon
        {
            Points = new PointCollection { topTL, topTR, topBR, topBL },
            Fill = isSelected
                ? new SolidColorBrush(Color.FromArgb(40, 0x00, 0xB4, 0xD8))
                : new SolidColorBrush(Color.FromArgb(25, 0x1B, 0x2A, 0x4A)),
            Stroke = isSelected
                ? new SolidColorBrush(Color.FromArgb(180, 0x00, 0xB4, 0xD8))
                : new SolidColorBrush(Color.FromArgb(80, 0x1B, 0x2A, 0x4A)),
            StrokeThickness = isSelected ? 2 : 1,
            Opacity = opacity
        };
        MapCanvas.Children.Add(topSurface);

        // Front edge (slab thickness) - left side
        var botBL = IsoProject(0, height, level - FloorSlabHeight / FloorSpacing, scale, offsetX, offsetY);
        var botBR = IsoProject(width, height, level - FloorSlabHeight / FloorSpacing, scale, offsetX, offsetY);

        var frontEdge = new Polygon
        {
            Points = new PointCollection { topBL, topBR, botBR, botBL },
            Fill = new SolidColorBrush(Color.FromArgb(50, 0x1B, 0x2A, 0x4A)),
            Stroke = new SolidColorBrush(Color.FromArgb(60, 0x1B, 0x2A, 0x4A)),
            StrokeThickness = 0.5,
            Opacity = opacity
        };
        MapCanvas.Children.Add(frontEdge);

        // Right edge (slab thickness)
        var botTR = IsoProject(width, 0, level - FloorSlabHeight / FloorSpacing, scale, offsetX, offsetY);

        var rightEdge = new Polygon
        {
            Points = new PointCollection { topTR, topBR, botBR, botTR },
            Fill = new SolidColorBrush(Color.FromArgb(35, 0x1B, 0x2A, 0x4A)),
            Stroke = new SolidColorBrush(Color.FromArgb(60, 0x1B, 0x2A, 0x4A)),
            StrokeThickness = 0.5,
            Opacity = opacity
        };
        MapCanvas.Children.Add(rightEdge);
    }

    /// <summary>
    /// Emelet felirat rajzolása.
    /// </summary>
    private void DrawFloorLabel(Floor floor, double buildingWidth, double buildingHeight, double scale, double offsetX, double offsetY, bool isSelected)
    {
        var labelPos = IsoProject(-30, buildingHeight / 2, floor.Level, scale, offsetX, offsetY);

        var label = new TextBlock
        {
            Text = floor.Name,
            FontSize = isSelected ? 16 : 13,
            FontWeight = isSelected ? FontWeights.Bold : FontWeights.SemiBold,
            Foreground = isSelected
                ? new SolidColorBrush(Color.FromRgb(0x00, 0xB4, 0xD8))
                : new SolidColorBrush(Color.FromRgb(0x5A, 0x6B, 0x8A)),
        };

        Canvas.SetLeft(label, labelPos.X - 80);
        Canvas.SetTop(label, labelPos.Y - 10);
        MapCanvas.Children.Add(label);
    }

    /// <summary>
    /// Egy helyiség 3D-s (izometrikus) megjelenítése.
    /// </summary>
    private void DrawRoom3D(Room room, int level, double scale, double offsetX, double offsetY, double floorOpacity)
    {
        double x = room.X;
        double y = room.Y;
        double w = room.Width;
        double h = room.Height;
        double wallH = 0.06; // Wall height as fraction of FloorSpacing

        bool isSelected = room == _viewModel.SelectedRoom;
        bool isFiltered = _viewModel.Rooms.Contains(room);
        double roomOpacity = isFiltered ? floorOpacity : floorOpacity * 0.3;

        var baseColor = isSelected
            ? Color.FromRgb(0xFF, 0xB7, 0x03)
            : GetCategoryBaseColor(room.Category);
        var darkColor = DarkenColor(baseColor, 0.65);
        var sideColor = DarkenColor(baseColor, 0.8);

        // Top surface of the room
        var tTL = IsoProject(x, y, level + wallH, scale, offsetX, offsetY);
        var tTR = IsoProject(x + w, y, level + wallH, scale, offsetX, offsetY);
        var tBR = IsoProject(x + w, y + h, level + wallH, scale, offsetX, offsetY);
        var tBL = IsoProject(x, y + h, level + wallH, scale, offsetX, offsetY);

        // Bottom of the room (floor level)
        var bTL = IsoProject(x, y, level, scale, offsetX, offsetY);
        var bTR = IsoProject(x + w, y, level, scale, offsetX, offsetY);
        var bBR = IsoProject(x + w, y + h, level, scale, offsetX, offsetY);
        var bBL = IsoProject(x, y + h, level, scale, offsetX, offsetY);

        // Front face (bottom edge)
        var frontFace = new Polygon
        {
            Points = new PointCollection { tBL, tBR, bBR, bBL },
            Fill = new SolidColorBrush(darkColor),
            Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0)),
            StrokeThickness = 0.5,
            Opacity = roomOpacity
        };
        MapCanvas.Children.Add(frontFace);

        // Right face
        var rightFace = new Polygon
        {
            Points = new PointCollection { tTR, tBR, bBR, bTR },
            Fill = new SolidColorBrush(sideColor),
            Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0)),
            StrokeThickness = 0.5,
            Opacity = roomOpacity
        };
        MapCanvas.Children.Add(rightFace);

        // Top face
        var topFace = new Polygon
        {
            Points = new PointCollection { tTL, tTR, tBR, tBL },
            Fill = new SolidColorBrush(baseColor),
            Stroke = isSelected
                ? new SolidColorBrush(Color.FromRgb(0xE0, 0x9F, 0x00))
                : new SolidColorBrush(Color.FromArgb(100, 0xFF, 0xFF, 0xFF)),
            StrokeThickness = isSelected ? 2.5 : 0.8,
            Opacity = roomOpacity,
            Cursor = Cursors.Hand,
            Tag = room
        };

        // Click handler
        topFace.MouseLeftButtonDown += (sender, _) =>
        {
            if (sender is Polygon p && p.Tag is Room clickedRoom)
            {
                _viewModel.SelectRoomCommand.Execute(clickedRoom);
            }
        };

        // Hover effect
        topFace.MouseEnter += (sender, _) =>
        {
            if (sender is Polygon p)
            {
                p.Opacity = Math.Min(1.0, roomOpacity + 0.2);
                p.StrokeThickness = 2;
            }
        };
        topFace.MouseLeave += (sender, _) =>
        {
            if (sender is Polygon p)
            {
                p.Opacity = roomOpacity;
                p.StrokeThickness = isSelected ? 2.5 : 0.8;
            }
        };

        MapCanvas.Children.Add(topFace);

        // Room number label on top face
        if (isFiltered || floorOpacity >= 0.8)
        {
            var center = IsoProject(x + w / 2, y + h / 2, level + wallH, scale, offsetX, offsetY);
            double fontSize = Math.Max(8, Math.Min(w, h) * scale * 0.12);

            // Extract just the room number from the name
            var roomNumber = room.Name.Split(' ')[0];

            var label = new TextBlock
            {
                Text = roomNumber,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                IsHitTestVisible = false,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 3,
                    ShadowDepth = 1,
                    Opacity = 0.6,
                    Color = Colors.Black
                }
            };

            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(label, center.X - label.DesiredSize.Width / 2);
            Canvas.SetTop(label, center.Y - label.DesiredSize.Height / 2);
            label.Opacity = roomOpacity;
            MapCanvas.Children.Add(label);
        }
    }
}
