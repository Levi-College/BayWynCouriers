using BayWyn_Couriers.Utilities;
using System.Windows.Input;
using BayWyn_Couriers.Views;

namespace BayWyn_Couriers.ViewModels
{
    public class NavigationVM : ViewModelBase
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
        public object CurrentView { 
            get { return _currentView; }
            // Update the current view and notify any listeners that the property has changed
            // OnPropertyChanged is a method from the base class ViewModelBase that raises the PropertyChanged event
            // to notify the UI of changes to the property
            set { _currentView = value; OnPropertyChanged(); }
        } 

        /// <summary>
        /// Gets or sets the command that navigates to the home page.
        /// </summary>
        public ICommand LoginCommand { get; set; } = null!;   // Defining the command for navigating to the login page (initially set to null, will be initialized in the constructor)
        public ICommand AdminCommand { get; set; } = null!;   // Command for navigating to the admin dashboard page
        public ICommand CourierCommand { get; set; } = null!;   // Command for navigating to the courier dashboard page
        public ICommand LCCommand { get; set; } = null!;   // Command for navigating to the logistics coordinator dashboard page

        public ICommand LogoutCommand { get; set; } = null!;   // Command for logging out and navigating back to the login page

        /// <summary>
        /// Sets the current view to the home page by initializing a new instance of the <see cref="HomeVM"/> class.
        /// </summary>
        /// <param name="obj">An optional parameter that is not used in this method.</param>
        private void Login(object? obj) => CurrentView = new LoginVM(this);

        // Can also be written as:
        // private void Login(object? obj)
        // {
        //     CurrentView = new LoginVM(this);
        // }

        private void AdminDashboard(object? obj) => CurrentView = new AdminVM(this);
        private void CourierDashboard(object? obj) => CurrentView = new CourierVM();
        private void LCDashboard(object? obj) => CurrentView = new LCVM();

        private void Logout(object? obj) => CurrentView = new LoginVM(this);

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
            LogoutCommand = new RelayCommand(Logout);

            // Startup Page. This sets the initial view to the login page when the application starts every time.
            // LoginVM(this) is used to pass the reference of the NavigationVM to the LoginVM, allowing the LoginVM to navigate to other views based on the user's role after a successful login.
            // if (this) is not passed, the LoginVM will not have access to the NavigationVM and will not be able to change the current view after a successful login.
            // It was added to allow the LoginVM to call the methods in the NavigationVM to change the current view based on the user's role after a successful login.
            CurrentView = new LoginVM(this);
        }
    }
}
