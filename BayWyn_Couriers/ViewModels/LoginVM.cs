using BayWyn_Couriers.Utilities;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


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
        public string UserId;
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
            // Initializing the login command and linking it to the ExecuteLogin method, which will handle the login logic when the command is executed
            // Relay command is a common implementation of the ICommand interface that allows for parameterized commands in WPF applications, enabling the binding of UI actions to methods in the view model
            LoginCommand = new RelayCommand(ExecuteLogin);

            // Refresh database
            RefreshJobAssignments();
            RefreshContracts();
        }

        private void ExecuteLogin(object? obj)
        {
            // Getting the object that is passed through to get the password
            var passwordBox = obj as PasswordBox;

            if (passwordBox != null)
            {
                Password = passwordBox.Password;
            }

            if (checkLogin(UserName, Password))
            {
                // Setting dimensions (for the dashboard)
                _navigationVM.WindowWidth = 1000;
                _navigationVM.WindowHeight = 800;

                // If the user is an admin, changing the view to the admindashboard, if the user is a courier, changing the view to the courier dashboard, if the user is an LC, changing the view to the LC dashboard
                if (Role == "Admin")
                {
                    // Navigating to the admin dashboard view (passing the navigation view model to the admin view model constructor to allow for navigation from the admin dashboard)
                    _navigationVM.CurrentView = new AdminVM(_navigationVM);
                }
                else if (Role == "LC") { _navigationVM.CurrentView = new LCVM(_navigationVM); }
                else if (Role == "Courier") { _navigationVM.CurrentView = new CourierVM(_navigationVM, UserId); }// Get the user ID and send it to the VM (to display details appropriate for the courier)
                else if (Role == "Owner" || Role == "Manager") { _navigationVM.CurrentView = new ManagerVM(_navigationVM); }
            }
            else
            {
                UserName = "";
                passwordBox.Password = "";
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool checkLogin(string userName, string password)
        {
            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new SqlConnection(myCon);
            mySqlCon.Open();

            try
            {
                // Creating the SQL command to check for user credential
                SqlCommand cmLogin = new SqlCommand("SELECT * FROM Users WHERE WorkingStatus = 'Active' AND Username = @UserName AND LoginPassword = @LoginPassword COLLATE Latin1_General_CS_AS ", mySqlCon);
                cmLogin.Parameters.AddWithValue("@UserName", userName);
                cmLogin.Parameters.AddWithValue("@LoginPassword", password);
                SqlDataReader loginCheck = cmLogin.ExecuteReader();

                // If a record is found, open the main application window
                if (loginCheck.HasRows)
                {
                    loginCheck.Read();
                    UserId = loginCheck["UserId"].ToString();
                    Role = loginCheck["UserRole"].ToString();
                    loginCheck.Close();
                    return true;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
            return false;
        }

        private void RefreshJobAssignments()
        {
            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new SqlConnection(myCon);
            mySqlCon.Open();

            // Using sql transaction so that the jobs are only deleted if both the sql commands work
            SqlTransaction transaction = mySqlCon.BeginTransaction();
            // Going through the jobs assignment and checking the date is in the past
            // Removes the courier and updates the status if the job is failed or delivery date is in the past and has not completed
            // Then removes them from the job assignments (if date in the past) 
            try
            {
                SqlCommand cmdResetJobs = new SqlCommand("Update J SET J.CourierID = NULL, J.JobStatus = 'Approved' FROM " +
                    "Jobs J INNER JOIN JobAssignments JA ON J.JobID = JA.JobID " +
                    "WHERE JA.DeliveryDate < CAST(GETDATE() AS DATE) ", mySqlCon);
                cmdResetJobs.Transaction = transaction;
                cmdResetJobs.ExecuteNonQuery();

                SqlCommand cmdDeleteJobsFromJA = new SqlCommand("DELETE FROM JobAssignments WHERE DeliveryDate<CAST(GETDATE() AS DATE)", mySqlCon);
                cmdDeleteJobsFromJA.Transaction = transaction;
                cmdDeleteJobsFromJA.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (Exception ex) { transaction.Rollback(); MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }

        private void RefreshContracts()
        {
            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new SqlConnection(myCon);
            mySqlCon.Open();

            try
            {
                SqlCommand cmdResetJobs = new SqlCommand("Update Contracts SET ContractStatus = 'Expired' WHERE EndDate < CAST(GETDATE() AS DATE) AND ContractStatus = 'Active' ", mySqlCon);

                cmdResetJobs.ExecuteNonQuery();

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }


    }
}
