using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BayWyn_Couriers.Utilities
{
    // This is the base class for all view models in the MVVM pattern. It implements the INotifyPropertyChanged interface, which allows it to notify the view when a property value has changed. This is essential for data binding in WPF applications, as it ensures that the UI updates automatically when the underlying data changes.
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
