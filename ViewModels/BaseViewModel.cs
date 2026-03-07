using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PersonalFinanceTracker.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Force refresh all bound properties. 
        /// Empty string notifies the UI that every property may have changed.
        /// </summary>
        public virtual void RefreshAllProperties()
        {
            OnPropertyChanged(string.Empty);
        }
    }
}