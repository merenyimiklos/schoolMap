using System.Collections.ObjectModel;
using System.Windows.Input;
using SchoolMap.Models;
using SchoolMap.Services;

namespace SchoolMap.ViewModels
{
    /// <summary>
    /// A főablak ViewModel-je.
    /// Kezeli a keresést, szűrést, emeletváltást és helyiségkijelölést.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly DataService _dataService;

        // === Szűrt helyiségek listája (oldalsó listában) ===
        private ObservableCollection<Room> _rooms = new();
        public ObservableCollection<Room> Rooms
        {
            get => _rooms;
            set => SetProperty(ref _rooms, value);
        }

        // === Összes helyiség (3D térképhez – minden emelet) ===
        private List<Room> _allRooms = new();
        public List<Room> AllRooms
        {
            get => _allRooms;
            set => SetProperty(ref _allRooms, value);
        }

        // === Emeletek listája (emeletváltó gombokhoz) ===
        private ObservableCollection<Floor> _floors = new();
        public ObservableCollection<Floor> Floors
        {
            get => _floors;
            set => SetProperty(ref _floors, value);
        }

        // === Kiválasztott emelet ===
        private Floor? _selectedFloor;
        public Floor? SelectedFloor
        {
            get => _selectedFloor;
            set
            {
                if (SetProperty(ref _selectedFloor, value))
                    ApplyFilters();
            }
        }

        // === Kiválasztott helyiség (részletek megjelenítéséhez) ===
        private Room? _selectedRoom;
        public Room? SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                if (_selectedRoom != null)
                    _selectedRoom.IsHighlighted = false;

                if (SetProperty(ref _selectedRoom, value))
                {
                    if (_selectedRoom != null)
                        _selectedRoom.IsHighlighted = true;

                    OnPropertyChanged(nameof(IsRoomSelected));
                    OnPropertyChanged(nameof(SelectedRoomFloorName));
                    OnPropertyChanged(nameof(SelectedRoomCategoryName));

                    RefreshMap();
                }
            }
        }

        /// <summary>Van-e kiválasztott helyiség (info panel láthatóságához)</summary>
        public bool IsRoomSelected => _selectedRoom != null;

        /// <summary>A kiválasztott helyiség emeletének neve</summary>
        public string SelectedRoomFloorName
        {
            get
            {
                if (_selectedRoom == null) return "";
                var floor = _floors.FirstOrDefault(f => f.Level == _selectedRoom.Floor);
                return floor?.Name ?? $"{_selectedRoom.Floor}. emelet";
            }
        }

        /// <summary>A kiválasztott helyiség kategóriájának olvasható neve</summary>
        public string SelectedRoomCategoryName
        {
            get
            {
                if (_selectedRoom == null) return "";
                return GetCategoryDisplayName(_selectedRoom.Category);
            }
        }

        // === Keresőmező tartalma ===
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilters();
            }
        }

        // === Kategóriaszűrő ===
        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                    ApplyFilters();
            }
        }

        // === Kategória gombok ===
        public ObservableCollection<CategoryItem> Categories { get; } = new();

        // === Parancsok ===
        public ICommand SelectRoomCommand { get; }
        public ICommand SelectFloorCommand { get; }
        public ICommand SelectCategoryCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand ClearSelectionCommand { get; }

        /// <summary>
        /// Esemény, amit a View figyelhet a térkép újrarajzolásához.
        /// </summary>
        public event Action? MapRefreshRequested;

        public MainViewModel()
        {
            _dataService = new DataService();
            _dataService.LoadData();

            SelectRoomCommand = new RelayCommand(OnSelectRoom);
            SelectFloorCommand = new RelayCommand(OnSelectFloor);
            SelectCategoryCommand = new RelayCommand(OnSelectCategory);
            ClearFilterCommand = new RelayCommand(_ => ClearFilters());
            ClearSelectionCommand = new RelayCommand(_ => SelectedRoom = null);

            Categories.Add(new CategoryItem { Category = null, DisplayName = "🏠  Mind", IsSelected = true });
            Categories.Add(new CategoryItem { Category = Category.Tanterem, DisplayName = "📚  Tantermek" });
            Categories.Add(new CategoryItem { Category = Category.Iroda, DisplayName = "💼  Tanári" });
            Categories.Add(new CategoryItem { Category = Category.KozossegiTer, DisplayName = "☕  Közösségi" });
            Categories.Add(new CategoryItem { Category = Category.Mosdo, DisplayName = "🚻  Mosdók" });
            Categories.Add(new CategoryItem { Category = Category.SpecialisTer, DisplayName = "🔬  Speciális" });

            LoadData();
        }

        /// <summary>Adatok betöltése a service-ből.</summary>
        private void LoadData()
        {
            Floors = new ObservableCollection<Floor>(_dataService.GetAllFloors());
            AllRooms = _dataService.GetAllRooms();

            SelectedFloor = Floors.FirstOrDefault(f => f.Level == 0) ?? Floors.FirstOrDefault();

            ApplyFilters();
        }

        /// <summary>
        /// Szűrők alkalmazása: emelet + keresés + kategória.
        /// Ez frissíti a szűrt listát, a 3D térkép minden emeletet mutat.
        /// </summary>
        private void ApplyFilters()
        {
            var results = _dataService.GetAllRooms().AsEnumerable();

            // Emelet szűrés
            if (_selectedFloor != null)
                results = results.Where(r => r.Floor == _selectedFloor.Level);

            // Kategória szűrés
            if (_selectedCategory.HasValue)
                results = results.Where(r => r.Category == _selectedCategory.Value);

            // Szöveges keresés
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var lower = _searchText.ToLowerInvariant();
                results = results.Where(r =>
                    r.Name.ToLowerInvariant().Contains(lower) ||
                    r.Description.ToLowerInvariant().Contains(lower));
            }

            Rooms = new ObservableCollection<Room>(results);
            RefreshMap();
        }

        /// <summary>Helyiség kiválasztása.</summary>
        private void OnSelectRoom(object? parameter)
        {
            if (parameter is Room room)
            {
                // Auto-switch to the room's floor
                var roomFloor = Floors.FirstOrDefault(f => f.Level == room.Floor);
                if (roomFloor != null && _selectedFloor != roomFloor)
                {
                    _selectedFloor = roomFloor;
                    OnPropertyChanged(nameof(SelectedFloor));
                    ApplyFilters();
                }
                SelectedRoom = room;
            }
            else if (parameter is int id)
            {
                SelectedRoom = _dataService.GetRoomById(id);
            }
        }

        /// <summary>Emeletváltás.</summary>
        private void OnSelectFloor(object? parameter)
        {
            if (parameter is Floor floor)
            {
                SelectedFloor = floor;
            }
        }

        /// <summary>Kategória szűrő választás.</summary>
        private void OnSelectCategory(object? parameter)
        {
            if (parameter is CategoryItem item)
            {
                foreach (var cat in Categories)
                    cat.IsSelected = false;
                item.IsSelected = true;

                SelectedCategory = item.Category;
            }
        }

        /// <summary>Minden szűrő törlése.</summary>
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedCategory = null;
            SelectedRoom = null;

            foreach (var cat in Categories)
                cat.IsSelected = cat.Category == null;

            ApplyFilters();
        }

        /// <summary>Térkép újrarajzolás kérése.</summary>
        private void RefreshMap()
        {
            MapRefreshRequested?.Invoke();
        }

        /// <summary>Kategória megjelenítési neve.</summary>
        public static string GetCategoryDisplayName(Category category)
        {
            return category switch
            {
                Category.Tanterem => "Tanterem",
                Category.Iroda => "Tanári szoba",
                Category.KozossegiTer => "Közösségi tér",
                Category.Mosdo => "Mosdó",
                Category.SpecialisTer => "Speciális terem",
                Category.Egyeb => "Egyéb",
                _ => category.ToString()
            };
        }
    }

    /// <summary>
    /// Kategória szűrő elem a UI-hoz.
    /// </summary>
    public class CategoryItem : BaseViewModel
    {
        public Category? Category { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
