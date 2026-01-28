using BayWyn_Couriers.Service;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BayWyn_Couriers.Models;
using BayWyn_Couriers.Views;


namespace BayWyn_Couriers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Creating an instance of the login service
        LoginService loginService = new LoginService();


        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string Username = txtUsername.Text;
            
            string Password = txtPassword.Text;
            MessageBox.Show(Username+Password);
            // Checking if the credentials are correct

            bool isValidUser = loginService.checkLogin(Username, Password);

            // Checking if the user is valid
            if (isValidUser)
            {
                //Opening windows based on role
                // If valid, open the main application window
                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);


                // Opening admin window if the user is an admin
                if (loginService.Role == "Admin")
                {
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Close();
                }
                



            }
            else
            {
                // If invalid, show an error message
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}