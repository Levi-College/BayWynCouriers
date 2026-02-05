using BayWyn_Couriers.Utilities;
using System.Windows.Input;

namespace BayWyn_Couriers.ViewModels
{
    class NavigationVM : ViewModelBase
    {
        /// <summary>
        /// Represents the current view object being managed.  This field is intended for internal use only.
        /// </summary>
        private object _currentView = null!;

        /// <summary>
        /// Gets or sets the current view displayed in the application.
        /// </summary>
        /// <remarks>Changing this property raises the <see
        /// cref="INotifyPropertyChanged.PropertyChanged"/> event.</remarks>
        public object CurrentView { get { return _currentView; } set { _currentView = value; OnPropertyChanged(); } }

        /// <summary>
        /// Gets or sets the command that navigates to the home page.
        /// </summary>
        public ICommand LoginCommand { get; set; } = null!;   // Home Page
        public ICommand AdminCommand { get; set; } = null!;   // Find Password Page
        public ICommand CourierCommand { get; set; } = null!;   // New Password Page
        public ICommand LCCommand { get; set; } = null!;   // View All Passwords Page
        //public ICommand EditPasswordCommand { get; set; } = null!;   // Edit Password Page

        /// <summary>
        /// Sets the current view to the home page by initializing a new instance of the <see cref="HomeVM"/> class.
        /// </summary>
        /// <param name="obj">An optional parameter that is not used in this method.</param>
        private void Login(object? obj) => CurrentView = new LoginWindowVM();
        private void AdminDashboard(object? obj) => CurrentView = new AdminVM();
        private void CourierDashboard(object? obj) => CurrentView = new CourierVM();
        private void LCDashboard(object? obj) => CurrentView = new LCVM();

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationVM"/> class, setting up commands and the default
        /// view.
        /// </summary>
        /// <remarks>This constructor initializes the commands used for navigation and sets the default
        /// view to the home page.</remarks>
        public NavigationVM()
        {
            // Initializing the commands with their respective methods
            // These commands will be bound to the UI elements (e.g., buttons) in the XAML, allowing users to navigate between different views when they interact with those elements.
            LoginCommand = new RelayCommand(Login);
            AdminCommand = new RelayCommand(AdminDashboard);
            CourierCommand = new RelayCommand(CourierDashboard);
            LCCommand = new RelayCommand(LCDashboard);

            // Startup Page
            CurrentView = new LoginWindowVM();
        }
    }
}
