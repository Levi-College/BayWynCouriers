using BayWyn_Couriers.Models;
using BayWyn_Couriers.Utilities;
using BayWyn_Couriers.Views.LCSubViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BayWyn_Couriers.ViewModels
{
    public class LCVM : ViewModelBase
    {
        // Constructor
        public LCVM(NavigationVM _nav)
        {
            // Initializing the LogoutCommand with a new RelayCommand that executes the ExecuteLogout method when invoked
            // When the LogoutCommand is executed (e.g., when a logout button is clicked in the UI), it will call the ExecuteLogout method,
            // which will handle the logout logic such as clearing the user session and navigating back to the login screen.
            _navigationVM = _nav; // Assigning the passed navigation view model to the private field _navigationVM, allowing the AdminVM to use it for navigation purposes (e.g., navigating back to the login screen after logout)
            LogoutCommand = new RelayCommand(ExecuteLogout); // Giving the LogoutCommand a meaning }
            LCJobsCommand = new RelayCommand(LCJobsPage);
        }

        // Declarations
        // A private field to hold the reference to the navigation view model, which will be used to navigate to different views based on the user's role after a successful login
        private NavigationVM _navigationVM;
        // A private field to hold the reference to the current subview, which can be used to display different content within the admin dashboard based on user interactions (e.g., viewing pending jobs, managing contracts, etc.)
        private object _currentSubView;

        // Establishing the commands for the admin dashboard
        public ICommand LogoutCommand { get; }
        public ICommand LCJobsCommand { get; }

        // To update the sub view
        public object CurrentSubView
        {
            get { return _currentSubView; }
            set
            {
                _currentSubView = value;
                OnPropertyChanged(nameof(CurrentSubView)); // Notify the view that the CurrentSubView property has changed, allowing the UI to update accordingly (e.g., displaying the new subview content)
            }
        }
        // Logout
        public void ExecuteLogout(object? obj)
        {
            // Setting dimensions for the login screen
            _navigationVM.WindowWidth = 600;
            _navigationVM.WindowHeight = 300;
            _navigationVM.CurrentView = new LoginVM(_navigationVM); // Updating the current view to a instance of LoginVM. _sending the view model to be used as well
        }

        private void LCJobsPage(object? obj)
        {
            CurrentSubView = new LCJobs();
        }
    }
}

