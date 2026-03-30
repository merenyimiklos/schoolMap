using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SchoolMap.ViewModels
{
    /// <summary>
    /// Egyszerű INotifyPropertyChanged implementáció.
    /// Minden ViewModel ebből származik.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Értesíti a UI-t, hogy egy tulajdonság megváltozott.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Beállít egy mezőt és értesíti a UI-t, ha változott.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
