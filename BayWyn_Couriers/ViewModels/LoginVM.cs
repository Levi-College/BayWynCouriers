using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using BayWyn_Couriers.Models;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using BayWyn_Couriers.Utilities;


namespace BayWyn_Couriers.ViewModels
{
    public class LoginVM : ViewModelBase
    {
        // To hold the login credentials entered by the user
        private string _userName;
        private string _password;

        // A private field to hold the reference to the navigation view model, which will be used to navigate to different views based on the user's role after a successful login
        private NavigationVM _navigationVM; 

        // To hold the current user information
        public int UserId;
        public string UserFName;
        public string UserLName;
        public string Role;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }


        // Command to execute the login process when the user clicks the login button
        public ICommand LoginCommand { get; }


        public LoginVM(NavigationVM nav)
        {
            _navigationVM = nav;

            LoginCommand = new RelayCommand(ExecuteLogin);
        }


        private void ExecuteLogin(object? obj)
        {
            MessageBox.Show("Attempting to login with username: " + UserName + " and password: " + Password);

            if (checkLogin(UserName, Password))
            {
                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // If the user is an admin, changing the view to the admindashboard, if the user is a courier, changing the view to the courier dashboard, if the user is an LC, changing the view to the LC dashboard
                if (Role == "Admin")
                {
                    MessageBox.Show("Opening Admin Window with the Admin:" + UserId);
                    // Navigating to the admin dashboard view (passing the navigation view model to the admin view model constructor to allow for navigation from the admin dashboard)
                    _navigationVM.CurrentView = new AdminVM(_navigationVM);
                }
               
            }
            else
            {
                // If invalid, show an error message
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool checkLogin(string userName, string password) {

            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;


            SqlConnection mySqlCon = new SqlConnection(myCon);
            // Opening the SQL connection
            mySqlCon.Open();

            try
            {
                // Creating the SQL command to check for user credential
                //string sqlQuery = "SELECT COUNT(1) FROM Users WHERE UserName=@UserName AND LoginPassword=@LoginPassword";
  

                SqlCommand cmLogin = new SqlCommand();
                cmLogin.Connection = mySqlCon;
                cmLogin.CommandType = CommandType.Text;
                cmLogin.CommandText = "SELECT * FROM Users WHERE Username=@UserName AND LoginPassword=@LoginPassword";
                cmLogin.Parameters.AddWithValue("@UserName", userName);
                cmLogin.Parameters.AddWithValue("@LoginPassword", password);

                MessageBox.Show("Line 103");

                SqlDataReader loginCheck = cmLogin.ExecuteReader();
                MessageBox.Show("Line 104");

                // If a record is found, open the main application window
                if (loginCheck.HasRows)
                {
                    loginCheck.Read();

                    UserId = Convert.ToInt32(loginCheck["UserId"]);
                    //UserName = loginCheck"Username"].ToString();
                    Role = loginCheck["UserRole"].ToString();
                    loginCheck.Close();
               

                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while closing the connection: " + ex.Message);
                mySqlCon.Close();

            }
            finally
            {
                mySqlCon.Close();
            }
            return false;


        }
            
        }
}
