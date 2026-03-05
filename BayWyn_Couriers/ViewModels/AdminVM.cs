using BayWyn_Couriers.Models;
using BayWyn_Couriers.Utilities;
using BayWyn_Couriers.Views;
using BayWyn_Couriers.Views.AdminSubViews;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace BayWyn_Couriers.ViewModels
{
    public class AdminVM : ViewModelBase
    {

        // Declaring variables (simple ones)

        // A private field to hold the reference to the navigation view model, which will be used to navigate to different views based on the user's role after a successful login
        private NavigationVM _navigationVM;
        // A private field to hold the reference to the current subview, which can be used to display different content within the admin dashboard based on user interactions (e.g., viewing pending jobs, managing contracts, etc.)
        private object _currentSubView;
        private Job _selectedJob; // Private field to hold the reference to the selected job from the observable collection of pending jobs in the admin clients page
        private string _selectedJobStatus = "All"; // Setting the private variable
        private User _selectedCourier; // To hold the selected courier for dropdown display
        private Client _selectedClient; // To hold the client details when adding a new job
        private bool _enableItemsForNewJob = false; // This is used to enable and disable items for adding new job (new job window)

        private Contract _selectedContract; // Hold contract details
        private string _selectedContractStatus = "All"; //Default status for the contracts list
        private bool _enableItemsForNewContract = false;

        // Lists and observable collections
        public ObservableCollection<Job> AllJobs { get; set; } = new ObservableCollection<Job>(); // To hold the jobs (used for filtered list as well)
        public ObservableCollection<User> CouriersList { get; set; } = new ObservableCollection<User>(); // Hold all the courier names and ID (using the user class)
        public ObservableCollection<Client> ClientList { get; set; } = new ObservableCollection<Client>(); // Holds all the clients (dropdown)        
        public List<String> JobsFilterList { get; } = new List<String> { "All", "Pending", "Approved", "Assigned", "Accepted", "Cancelled", "Completed" }; // A list of string for the items in the job status combo box (item source)
        public List<string> JobsStatusList { get; } = new List<string> { "Pending", "Approved", "Assigned", "Accepted", "Cancelled", "Completed" };  // A list to show the conditions in the edit box


        // Used for the contracts page
        public ObservableCollection<Contract> AllContracts { get; set; } = new ObservableCollection<Contract>(); // To hold the list of contracts
        public List<String> ContractsFilterList { get; } = new List<String> { "All", "Active", "Expired"}; // A list of string for the items in the job status combo box (item source)
        public List<string> ContractsStatusList { get; } = new List<string> { "Active", "Expired"};  // A list to show the conditions in the edit box


        // Creating a property for the selected job to display the details by accessing the Job properites (e.g., JobId, ClientId, CourierId, JobStatus) in the JobDetails property.
        // This allows the admin user to see the details of the selected job in the UI (e.g., in a details panel) when they select a job from the list of pending jobs.
        public Job SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged();

                // Updating the select courier (used to update the dropdown)
                // If no job selected let the selected courier and client be null
                if (_selectedJob == null)
                {
                    SelectedCourier = null;
                    SelectedClient = null;
                    return;
                }

                //Matching the courier using the ID
                foreach (User courier in CouriersList)
                {
                    if (courier.UserId == _selectedJob.CourierId)
                    {
                        // Update the selected courier
                        SelectedCourier = courier;
                        break;
                    }
                }

                // Setting the client in the edit window (using the selected job client Id)
                foreach (Client client in ClientList)
                {
                    if (client.ClientId == _selectedJob.ClientId)
                    {
                        SelectedClient = client;
                        break;
                    }
                }
            }
        }

        // To set and get the select job status (for filtering the data grid)
        public string SelectedJobStatus
        {
            get => _selectedJobStatus;
            set
            {
                if (_selectedJobStatus != value)
                {
                    _selectedJobStatus = value;
                    OnPropertyChanged();

                    // Filtering the jobs list based on the selected job status
                    LoadJobsByStatus(value);
                }
            }
        }

        // To hold and update the selected courier details
        public User SelectedCourier
        {
            get => _selectedCourier;
            set
            {
                if (_selectedCourier != value)
                {
                    _selectedCourier = value;
                    OnPropertyChanged();

                    // Updating the courierId of the Job based on the selected new courier
                    if (_selectedCourier != null)
                    {
                        SelectedJob.CourierId = value.UserId;
                    }
                }
            }
        }

        // To hold and update the selected courier details
        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                // If same value (ignore)
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    OnPropertyChanged();

                    // Updating the courierId of the Job based on the selected new courier
                    if (_selectedCourier != null)
                    {
                        SelectedJob.ClientId = value.ClientId;
                    }
                }
            }
        }

        // To update the boolean in UI when it is updated in code
        public bool EnableItemsForNewJob
        {
            get => _enableItemsForNewJob;
            set
            {
                _enableItemsForNewJob = value;
                OnPropertyChanged();
            }
        }


        // Contracts page 
        public Contract SelectedContract
        {
            get => _selectedContract;
            set
            {
                _selectedContract = value;
                OnPropertyChanged();

                // Updating the select courier (used to update the dropdown)
                // If no job selected let the selected courier and client be null
                //if (_selectedContract == null)
                //{
                //    SelectedCourier = null;
                //    SelectedClient = null;
                //    return;
                //}

                //Matching the courier using the ID
                //foreach (User courier in CouriersList)
                //{
                //    if (courier.UserId == _selectedJob.CourierId)
                //    {
                //        // Update the selected courier
                //        SelectedCourier = courier;
                //        break;
                //    }
                //}

                // Setting the client in the edit window (using the selected job client Id)
                //foreach (Client client in ClientList)
                //{
                //    if (client.ClientId == _selectedJob.ClientId)
                //    {
                //        SelectedClient = client;
                //        break;
                //    }
                //}
            }
        }

        // To set and get the select job status (for filtering the data grid)
        public string SelectedContractStatus
        {
            get => _selectedContractStatus;
            set
            {
                if (_selectedContractStatus != value)
                {
                    _selectedContractStatus = value;
                    OnPropertyChanged();

                    // Filtering the jobs list based on the selected job status
                    LoadContractsByStatus(value);
                }
            }
        }

        // To update the boolean in UI when it is updated in code
        public bool EnableItemsForNewContract
        {
            get => _enableItemsForNewContract;
            set
            {
                _enableItemsForNewContract = value;
                OnPropertyChanged();
            }
        }



        // Establishing the commands for the admin dashboard
        public ICommand LogoutCommand { get; }
        public ICommand JobsCommand { get; }
        public ICommand ContractsCommand { get; }
        public ICommand ClientsCommand { get; }
        public ICommand CouriersCommand { get; }
        public ICommand ReportsCommand { get; }


        // Admin jobs commands
        public ICommand AddJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand UpdateJobCommand { get; }

        public ICommand NewJobCommand { get; }


        // Admin contracts page commands
        public ICommand AddContractCommand { get; }
        public ICommand DeleteContractCommand { get; }
        public ICommand UpdateContractCommand { get; }

        public ICommand NewContractCommand { get; }

        // Admin couriers page commands

        // Admin reports page commands


        // AdminVM logic



        // Property to get or set the current subview displayed in the admin dashboard.
        // This allows the admin dashboard to display different content based on user interactions (e.g., viewing pending jobs, managing contracts, etc.)
        public object CurrentSubView
        {
            get { return _currentSubView; }
            set
            {
                _currentSubView = value;
                OnPropertyChanged(nameof(CurrentSubView)); // Notify the view that the CurrentSubView property has changed, allowing the UI to update accordingly (e.g., displaying the new subview content)
            }
        }


        // Page viewing logic (in the admin window)

        // Command to handle the action of viewing pending jobs. When executed, it will set the CurrentSubView to a new instance of the AdminJobs view, which will display the pending jobs to the admin user.
        private void JobsPage(object? obj)
        {
            CurrentSubView = new AdminJobs();
            RefreshPage(); // Refreshing the fields and the page
            GetCouriers(); // Populate the status filter
            GetClients(); // Populate the clients combo box
            LoadJobsByStatus("All"); // Loads all the jobs initially 
            EnableItemsForNewJob = false; // Used to enable and disable buttons for the edit window
        }

        private void ContractsPage(object? obj)
        {
            CurrentSubView = new AdminContracts();
            RefreshPage(); // Refreshing the fields and the page
            //GetContracts(); // Populate the status filter
            //GetClients(); // Populate the clients combo box
            LoadContractsByStatus("All"); // Loads all the jobs initially 
            EnableItemsForNewContract = false; // Used to enable and disable buttons for the edit window
        }

        private void ReportsPage(object? obj) => CurrentSubView = new AdminReports();
        
        private void ClientsPage(object? obj) => CurrentSubView = new AdminClients();
        private void CouriersPage(object? obj) => CurrentSubView = new AdminCouriers();


        public AdminVM(NavigationVM _nav)
        {
            // Initializing the LogoutCommand with a new RelayCommand that executes the ExecuteLogout method when invoked
            // When the LogoutCommand is executed (e.g., when a logout button is clicked in the UI), it will call the ExecuteLogout method,
            // which will handle the logout logic such as clearing the user session and navigating back to the login screen.
            _navigationVM = _nav; // Assigning the passed navigation view model to the private field _navigationVM, allowing the AdminVM to use it for navigation purposes (e.g., navigating back to the login screen after logout)
            LogoutCommand = new RelayCommand(ExecuteLogout); // Giving the LogoutCommand a meaning 

            // Intializing other commands for the admin dashboard (e.g., JobsCommand for viewing pending jobs)
            JobsCommand = new RelayCommand(JobsPage); // Giving the JobsCommand a meaning (when executed, it will call the JobsPage method to set the CurrentSubView to the AdminJobs view, allowing the admin user to see the pending jobs)
            ReportsCommand = new RelayCommand(ReportsPage); // Giving the ReportsCommand a meaning (when executed, it will call the ReportsPage method to set the CurrentSubView to the AdminReports view, allowing the admin user to see various reports related to the courier service)
            ContractsCommand = new RelayCommand(ContractsPage); // Giving the ContractsCommand a meaning (when executed, it will call the ContractsPage method to set the CurrentSubView to the AdminContracts view, allowing the admin user to manage contracts with clients)
            ClientsCommand = new RelayCommand(ClientsPage); // Giving the ClientsCommand a meaning (when executed, it will call the ClientsPage method to set the CurrentSubView to the AdminClients view, allowing the admin user to manage client information and interactions)
            CouriersCommand = new RelayCommand(CouriersPage); // Giving the CouriersCommand a meaning (when executed, it will call the CouriersPage method to set the CurrentSubView to the AdminCouriers view, allowing the admin user to manage courier information and interactions)

            AddJobCommand = new RelayCommand(AddNewJob); // Establishes the logic of AddJobCommand
            DeleteJobCommand = new RelayCommand(
                execute: obj => DeleteJob(obj),
                canExecute: obj => SelectedJob != null // Logic: Disable if SelectedJob is null
            );
            UpdateJobCommand = new RelayCommand(UpdateJob);
            NewJobCommand = new RelayCommand(NewJob);


            // Setting up contracts commands

        }


        // Methods used

        // Logout
        public void ExecuteLogout(object? obj)
        {
            // Setting dimensions for the login screen
            _navigationVM.WindowWidth = 600;
            _navigationVM.WindowHeight = 300;
            _navigationVM.CurrentView = new LoginVM(_navigationVM); // Updating the current view to a instance of LoginVM. _sending the view model to be used as well
        }

        // Refresh
        public void RefreshPage()
        {
            SelectedJob = null; // Clearing all the fields   
            SelectedCourier = null; // Clear the dropdown selections
            SelectedClient = null;
        }

        //To get all the couriers with their ID and to add it to the list of couriers. Used for combo box dropdown
        private void GetCouriers()
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                SqlCommand cmd = new SqlCommand("SELECT UserID, Username FROM Users WHERE UserRole = 'Courier'", mySqlCon);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        CouriersList.Clear();
                        while (reader.Read())
                        {
                            CouriersList.Add(new User
                            {
                                UserId = Convert.ToInt32(reader["UserID"]),
                                UserName = reader["UserName"].ToString(),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // To get the clients to store in the combo box item source
        private void GetClients()
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                SqlCommand cmd = new SqlCommand("SELECT ClientID, Name FROM Clients", mySqlCon);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        ClientList.Clear();
                        while (reader.Read())
                        {
                            ClientList.Add(new Client
                            {
                                ClientId = Convert.ToInt32(reader["ClientID"]),
                                Name = reader["Name"].ToString(),
                            });
                        }
                    }
                    else
                    {
                        MessageBox.Show("Reader has no rows");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        // Admin Jobs CRUD
        public void NewJob(object? obj) 
        {
            // Refreshing the texboxes in the edit window
            GetAllJobs();
            MessageBox.Show("Please enter details in the Edit/New window. After completion click Add");

            // Setting the boolean to show client list to true
            EnableItemsForNewJob = true;

            // Creating an empty SelectedJob so that the values can be used to add it to the database
            SelectedJob = new Job()
            {
                StartDate = DateTime.Now.Date,
                JobStatus = "Pending",
                Cost = 2.5,
            };
        }

        public void AddNewJob(object? obj)
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            MessageBox.Show("Adding job");
            try
            {
                // Setting up the sql command
                SqlCommand cmAddJob = new SqlCommand("INSERT INTO Jobs (ClientID, DeliveryAddress, Description, Cost,  JobStatus) " +
                    "VALUES(@ClientID, @DeliveryAddress, @Description, @Cost, @JobStatus)", mySqlCon);


                // Use the ID to find the record, then set the new values
                cmAddJob.Parameters.AddWithValue("@ClientID", SelectedClient.ClientId);
                cmAddJob.Parameters.AddWithValue("@DeliveryAddress", SelectedJob.DeliveryAddress);
                cmAddJob.Parameters.AddWithValue("@Description", SelectedJob.Description);
                cmAddJob.Parameters.AddWithValue("@Cost", SelectedJob.Cost);
                cmAddJob.Parameters.AddWithValue("@JobStatus", SelectedJob.JobStatus);

                cmAddJob.ExecuteReader();
                MessageBox.Show("Job Added Successfully");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                mySqlCon.Close();
            }

            finally
            {
                mySqlCon.Close();
            }
            GetAllJobs(); 
        }

        public void UpdateJob(object? obj)
        {
            //Checking if a job is selected
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job to be update");
                return;
            }
            else
            {
                // Confirming deletion
                MessageBoxResult result = MessageBox.Show("Confirm Update", "Update", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Setting up sql connection
                    string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
                    SqlConnection mySqlCon = new(myCon);
                    mySqlCon.Open();

                    try
                    {
                        // Setting up the sql command
                        SqlCommand cmUpdateJob = new SqlCommand("UPDATE Jobs SET CourierID = @CourierID, " +
                            "DeliveryAddress = @DeliveryAddress, " +
                            "Description = @Description, JobStatus = @JobStatus " +
                            "WHERE JobID = @JobID", mySqlCon);

                        // Use the ID to find the record, then set the new values using the parameters
                        cmUpdateJob.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                        cmUpdateJob.Parameters.AddWithValue("@CourierID", SelectedJob.CourierId);
                        cmUpdateJob.Parameters.AddWithValue("@DeliveryAddress", SelectedJob.DeliveryAddress);
                        cmUpdateJob.Parameters.AddWithValue("@Description", SelectedJob.Description);
                        cmUpdateJob.Parameters.AddWithValue("@JobStatus", SelectedJob.JobStatus);

                        cmUpdateJob.ExecuteReader(); // Running the sql command to update the database
                        MessageBox.Show("Job Updated Successfully");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        mySqlCon.Close();
                    }

                    finally
                    {
                        mySqlCon.Close();
                    }
                    GetAllJobs();
                }
            }
        }

        public void DeleteJob(object? obj)
        {
            //Checking if a job is selected
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job to be deleted");
                return;
            }
            else
            {
                // Confirming deletion
                MessageBoxResult result = MessageBox.Show("Are you sure", "Deletion", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Setting up sql connection
                    string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
                    SqlConnection mySqlCon = new(myCon);
                    mySqlCon.Open();

                    try
                    {
                        // Setting up the sql command
                        SqlCommand cmDeleteJob = new SqlCommand("DELETE FROM Jobs WHERE JobID=@ID", mySqlCon);
                        cmDeleteJob.Parameters.AddWithValue("@ID", SelectedJob.JobId);
                        cmDeleteJob.ExecuteReader();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        mySqlCon.Close();
                    }

                    finally
                    {
                        mySqlCon.Close();
                    }
                    GetAllJobs(); // Refresh and load
                }
            }
        }

        public void GetAllJobs()
        {
            RefreshPage(); // Refreshing before updating the form

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Setting up the sql command
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.*, c.Name AS ClientName FROM Jobs j INNER JOIN Clients c ON j.ClientID = c.ClientID", mySqlCon);
                SqlDataReader listJobs = cmGetJobs.ExecuteReader();

                // Looping through the data reader and adding them to the list
                if (listJobs.HasRows)
                {
                    AllJobs.Clear();
                    while (listJobs.Read())
                    {
                        AllJobs.Add(
                             new Job
                             {
                                 //JobId = Convert.ToInt32(listJobs["JobId"]),
                                 //ClientId = Convert.ToInt32(listJobs["ClientId"]),
                                 //CourierId = listJobs["CourierId"] as int? ?? 0,
                                 //ClientName = listJobs["Name"].ToString(),
                                 ////CourierName = listJobs["CourierName"].ToString(),
                                 //StartDate = Convert.ToDateTime(listJobs["StartDate"]),
                                 //JobStatus = listJobs["JobStatus"].ToString(),
                                 //DeliveryAddress = listJobs["DeliveryAddress"].ToString(),
                                 //Description = listJobs["Description"].ToString(),

                                 JobId = Convert.ToInt32(listJobs["JobId"]),
                                 ClientId = Convert.ToInt32(listJobs["ClientId"]),
                                 ClientName = listJobs["ClientName"].ToString(), // Now available from the JOIN

                                 // Handling potential NULLs for CourierID
                                 CourierId = listJobs["CourierId"] == DBNull.Value ? 0 : Convert.ToInt32(listJobs["CourierId"]),

                                 StartDate = Convert.ToDateTime(listJobs["StartDate"]),
                                 JobStatus = listJobs["JobStatus"].ToString(),
                                 DeliveryAddress = listJobs["DeliveryAddress"].ToString(),
                                 Description = listJobs["Description"].ToString(),
                                 Cost = Convert.ToDouble(listJobs["Cost"])
                             }
                          );
                    }
                    listJobs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                mySqlCon.Close();
            }

            finally
            {
                mySqlCon.Close();
            }
        }

        // This method is used to update or filter the data grid source based on the selected job status
        private void LoadJobsByStatus(string jobStatus)
        {
            if (jobStatus == null) // Checking for null value
            {
                return;
            }

            //If the status is "All", call show all jobs method
            if (jobStatus == "All")
            {
                GetAllJobs();
                return;
            }

            // Going through the database to get all jobs status that are pending
            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            // Opening the SQL connection
            mySqlCon.Open();

            try
            {
                // Creating the SQL command to check for user credential
                //SqlCommand cmGetJobs = new SqlCommand("SELECT * FROM Jobs WHERE JobStatus = @Status",mySqlCon);
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.*, c.Name AS ClientName FROM Jobs j INNER JOIN Clients c ON j.ClientID = c.ClientID WHERE JobStatus = @Status", mySqlCon);
                cmGetJobs.Parameters.AddWithValue("@Status", jobStatus);
                SqlDataReader drlistJobs = cmGetJobs.ExecuteReader();

                // If a record is found, open the main application window
                if (drlistJobs.HasRows)
                {
                    AllJobs.Clear(); // Clearing the collection before adding to it to avoid duplicates
                    while (drlistJobs.Read()) // Reading through each record found
                    {
                        // Adding the jobs to the pending jobs collection so that the data grid in the admin clients page can display the pending jobs to the admin user when they navigate to the Jobs page in the admin dashboard
                        AllJobs.Add(
                             new Job  //For each record found, add it to the observable collection
                             {
                                 JobId = Convert.ToInt32(drlistJobs["JobId"]),
                                 ClientId = Convert.ToInt32(drlistJobs["ClientId"]),
                                 ClientName = drlistJobs["ClientName"].ToString(), // Now available from the JOIN
                                 // Handling potential NULLs for CourierID (Ternary operation - if variable == (Null value) ? (execute this) : (if not execute this)
                                 CourierId = drlistJobs["CourierId"] == DBNull.Value ? 0 : Convert.ToInt32(drlistJobs["CourierId"]),
                                 StartDate = Convert.ToDateTime(drlistJobs["StartDate"]),
                                 JobStatus = drlistJobs["JobStatus"].ToString(),
                                 DeliveryAddress = drlistJobs["DeliveryAddress"].ToString(),
                                 Description = drlistJobs["Description"].ToString(),
                                 Cost = Convert.ToDouble(drlistJobs["Cost"])
                             }
                          );
                    }
                    // Closing the data reader
                    drlistJobs.Close();
                }
                else
                {
                    MessageBox.Show("No jobs available for the filter");
                    AllJobs.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                mySqlCon.Close();
            }
            finally
            {
                mySqlCon.Close();
            }
        }



        // Admin contracts CRUD


        // This method is used to update or filter the data grid source based on the selected job status
        private void LoadContractsByStatus(string contractStatus)
        {
            if (contractStatus == null) // Checking for null value
            {
                return;
            }

            //If the status is "All", call show all jobs method
            if (contractStatus == "All")
            {
                GetAllContracts();
                return;
            }

            RefreshPage(); // Refreshing before updating the form

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Setting up the sql command
                SqlCommand cmGetContracts = new SqlCommand("SELECT cnt.*, c.Name AS ClientName FROM Contracts cnt INNER JOIN Clients c ON cnt.ClientID = c.ClientID WHERE ContractStatus = @contractStatus", mySqlCon);
                cmGetContracts.Parameters.AddWithValue("@contractStatus", contractStatus);
                SqlDataReader reader = cmGetContracts.ExecuteReader();

                // Looping through the data reader and adding them to the list
                if (reader.HasRows)
                {
                    AllContracts.Clear();
                    while (reader.Read())
                    {
                        AllContracts.Add(
                             new Contract
                             {
                                 ContractId = Convert.ToInt32(reader["ContractId"]),
                                 ClientId = Convert.ToInt32(reader["ClientId"]),
                                 CompanyName = reader["CompanyName"].ToString(), // Now available from the JOIN
                                 StartDate = (reader["StartDate"]) == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["StartDate"]),
                                 EndDate = (reader["EndDate"]) == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["EndDate"]),
                                 Notes = reader["Notes"] == DBNull.Value ? "No Notes" : reader["Notes"].ToString(),
                                 ContractStatus = reader["ContractStatus"].ToString(),
                                 Address = reader["Address"].ToString(),
                                 Email = reader["Email"].ToString(),
                                 PhoneNumber = reader["Phone"].ToString(),
                                 MonthlyCost = reader["MonthlyCost"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MonthlyCost"]),
                                 CostPerJob = reader["CostPerJob"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CostPerJob"])
                             }
                          );
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                mySqlCon.Close();
            }

            finally
            {
                mySqlCon.Close();
            }
        }


        // Get all contracts
        public void GetAllContracts()
        {
            RefreshPage(); // Refreshing before updating the form

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Setting up the sql command
                SqlCommand cmGetJobs = new SqlCommand("SELECT cnt.*, c.Name AS ClientName FROM Contracts cnt INNER JOIN Clients c ON cnt.ClientID = c.ClientID", mySqlCon);
                SqlDataReader reader = cmGetJobs.ExecuteReader();

                // Looping through the data reader and adding them to the list
                if (reader.HasRows)
                {
                    AllContracts.Clear();
                    while (reader.Read())
                    {
                        AllContracts.Add(
                             new Contract
                             {
                                 ContractId = Convert.ToInt32(reader["ContractId"]),
                                 ClientId = Convert.ToInt32(reader["ClientId"]),
                                 CompanyName = reader["CompanyName"].ToString(), // Now available from the JOIN
                                 StartDate = (reader["StartDate"]) == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["StartDate"]),
                                 EndDate = (reader["EndDate"]) == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["EndDate"]),
                                 Notes = reader["Notes"] == DBNull.Value ? "No Notes" : reader["Notes"].ToString(),
                                 ContractStatus = reader["ContractStatus"].ToString(),
                                 Address = reader["Address"].ToString(),
                                 Email = reader["Email"].ToString(),
                                 PhoneNumber = reader["Phone"].ToString(),
                                 MonthlyCost = reader["MonthlyCost"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MonthlyCost"]),
                                 CostPerJob = reader["CostPerJob"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CostPerJob"])
                             }
                          );
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                mySqlCon.Close();
            }

            finally
            {
                mySqlCon.Close();
            }
        }

        public void NewContract(object? obj)
        {
            // Refreshing the texboxes in the edit window
            GetAllContracts();
            MessageBox.Show("Please enter details in the Edit/New window. After completion click Add");

            // Setting the boolean to show client list to true
            EnableItemsForNewJob = true;

            // Creating an empty SelectedJob so that the values can be used to add it to the database
            //SelectedJob = new Job()
            //{
            //    StartDate = DateTime.Now.Date,
            //    JobStatus = "Pending",
            //    Cost = 2.5,
            //};
        }

        public void AddNewContract(object? obj)
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            MessageBox.Show("Adding job");
            try
            {
                // Start date of the contract is as provided. But the end date should be 1 month after the date.
                // The price should also be based on the contract status

                // Setting up the sql command
                SqlCommand cmAddJob = new SqlCommand("INSERT INTO Jobs (ClientID, DeliveryAddress, Description, Cost,  JobStatus) " +
                    "VALUES(@ClientID, @DeliveryAddress, @Description, @Cost, @JobStatus)", mySqlCon);


                // Use the ID to find the record, then set the new values
                cmAddJob.Parameters.AddWithValue("@ClientID", SelectedClient.ClientId);
                cmAddJob.Parameters.AddWithValue("@DeliveryAddress", SelectedJob.DeliveryAddress);
                cmAddJob.Parameters.AddWithValue("@Description", SelectedJob.Description);
                cmAddJob.Parameters.AddWithValue("@Cost", SelectedJob.Cost);
                cmAddJob.Parameters.AddWithValue("@JobStatus", SelectedJob.JobStatus);

                cmAddJob.ExecuteReader();
                MessageBox.Show("Job Added Successfully");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                mySqlCon.Close();
            }

            finally
            {
                mySqlCon.Close();
            }
            GetAllContracts();
        }

        public void UpdateContract(object? obj)
        {
            //Checking if a job is selected
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job to be update");
                return;
            }
            else
            {
                // Confirming deletion
                MessageBoxResult result = MessageBox.Show("Confirm Update", "Update", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Setting up sql connection
                    string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
                    SqlConnection mySqlCon = new(myCon);
                    mySqlCon.Open();

                    try
                    {
                        // Setting up the sql command
                        SqlCommand cmUpdateJob = new SqlCommand("UPDATE Jobs SET CourierID = @CourierID, " +
                            "DeliveryAddress = @DeliveryAddress, " +
                            "Description = @Description, JobStatus = @JobStatus " +
                            "WHERE JobID = @JobID", mySqlCon);

                        // Use the ID to find the record, then set the new values using the parameters
                        cmUpdateJob.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                        cmUpdateJob.Parameters.AddWithValue("@CourierID", SelectedJob.CourierId);
                        cmUpdateJob.Parameters.AddWithValue("@DeliveryAddress", SelectedJob.DeliveryAddress);
                        cmUpdateJob.Parameters.AddWithValue("@Description", SelectedJob.Description);
                        cmUpdateJob.Parameters.AddWithValue("@JobStatus", SelectedJob.JobStatus);

                        cmUpdateJob.ExecuteReader(); // Running the sql command to update the database
                        MessageBox.Show("Job Updated Successfully");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        mySqlCon.Close();
                    }

                    finally
                    {
                        mySqlCon.Close();
                    }
                    GetAllContracts();
                }
            }
        }

        public void DeleteContract(object? obj)
        {
            //Checking if a job is selected
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job to be deleted");
                return;
            }
            else
            {
                // Confirming deletion
                MessageBoxResult result = MessageBox.Show("Are you sure", "Deletion", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Setting up sql connection
                    string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
                    SqlConnection mySqlCon = new(myCon);
                    mySqlCon.Open();

                    try
                    {
                        // Setting up the sql command
                        SqlCommand cmDeleteJob = new SqlCommand("DELETE FROM Jobs WHERE JobID=@ID", mySqlCon);
                        cmDeleteJob.Parameters.AddWithValue("@ID", SelectedJob.JobId);
                        cmDeleteJob.ExecuteReader();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        mySqlCon.Close();
                    }

                    finally
                    {
                        mySqlCon.Close();
                    }
                    GetAllContracts(); // Refresh and load
                }
            }
        }




        // Get contracts based on status (filter)

        // Add new contract

        // Delete a contract

        // Update a contract
    }
}

