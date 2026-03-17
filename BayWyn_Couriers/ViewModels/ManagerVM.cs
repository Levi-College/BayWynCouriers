using BayWyn_Couriers.Models;
using BayWyn_Couriers.Utilities;
using BayWyn_Couriers.Views.AdminSubViews;
using BayWyn_Couriers.Views.ManagerSubViews;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using static BayWyn_Couriers.ViewModels.AdminVM;

namespace BayWyn_Couriers.ViewModels
{
    public class ManagerVM : ViewModelBase
    {
        public ManagerVM(NavigationVM _nav)
        {
            // When the LogoutCommand is executed (e.g., when a logout button is clicked in the UI), it will call the ExecuteLogout method,
            // which will handle the logout logic such as clearing the user session and navigating back to the login screen.
            _navigationVM = _nav; // Assigning the passed navigation view model to the private field _navigationVM, allowing the AdminVM to use it for navigation purposes (e.g., navigating back to the login screen after logout)
            LogoutCommand = new RelayCommand(ExecuteLogout); // Giving the LogoutCommand a meaning using Relay command 

            // Admin jobs page commands
            // Intializing other commands for the admin dashboard (e.g., JobsCommand for viewing pending jobs)
            ManagerJobsCommand = new RelayCommand(ManagerJobsPage);
            ReportsCommand = new RelayCommand(ReportsPage);
            ManagerContractsCommand = new RelayCommand(ManagerContractsPage);
            ManagerClientsCommand = new RelayCommand(ManagerClientsPage);
            ManagerCouriersCommand = new RelayCommand(ManagerCouriersPage);

            RefreshJobsCommand = new RelayCommand(RefreshJobsPage);
            RefreshClientsCommand = new RelayCommand(RefreshClientsPage);
            RefreshCouriersCommand = new RelayCommand(RefreshCouriersPage);
            RefreshContractsCommand = new RelayCommand(RefreshContractsPage);

            DayJobsReportCommand = new RelayCommand(ShowDayReport);
            MonthlyJobsReportCommand = new RelayCommand(ShowMonthlyJobReport);
            ContractsJobReportCommand = new RelayCommand(ShowContractsJobReport);
            ClientsValueReportCommand = new RelayCommand(ShowClientValueReport);
            GenerateDayJobsReportCommand = new RelayCommand(GenerateDayJobsReport);
            GenerateMonthlyJobsReportCommand = new RelayCommand(GenerateMonthlyJobsReport);
            GenerateAllJobsReportCommand = new RelayCommand(GenerateContractsJobReport);
            GenerateValueReportCommand = new RelayCommand(GenerateClientValueReport);

            // Setting the start page as the jobs page
            ManagerJobsPage(null);
        }


        // Declaring variables
        private NavigationVM _navigationVM; // A private field to hold the reference to the navigation view model, which will be used to navigate to different views based on the user's role after a successful login
        private object _currentSubView; // A private field to hold the reference to the current subview (used to change the pages/user controls)
        private Job _selectedJob; // Private field to hold the reference to the selected job from the observable collection of pending jobs in the admin clients page
        private string _selectedJobStatus = "All"; // Setting the private variable
        private User _selectedCourier; // To hold the selected courier for dropdown display
        private Client _selectedClient; // To hold the client details when adding a new job
        private string _selectedClientStatus = "All"; // Setting the variable to filter clients 
        private decimal _costOfJob = 10m; // To update the price of the job (when creating a new one)
        private Contract _selectedContract; // Hold contract details
        private string _selectedContractStatus = "All"; //Default status for the contracts list

        // Report page
        private string _dayReportVisibility = "Hidden";
        private string _monthlyReportVisibility = "Hidden";
        private string _allJobsReportVisibility = "Hidden";
        private string _monthlyValueReportVisbility = "Hidden";
        private DateTime _dateForDayJobReport = DateTime.Today;
        private string _selectedMonthForReport;
        private string _selectedYearForReport;

        // Lists and observable collections
        public ObservableCollection<Job> AllJobs { get; set; } = new ObservableCollection<Job>(); // To hold the jobs (used for filtered list as well)
        public ObservableCollection<User> CouriersList { get; set; } = new ObservableCollection<User>(); // Hold all the courier names and ID (using the user class)
        public ObservableCollection<Client> ClientList { get; set; } = new ObservableCollection<Client>(); // Holds all the clients (dropdown)        
        public ObservableCollection<Contract> AllContracts { get; set; } = new ObservableCollection<Contract>(); // To hold the list of contracts

        public List<string> JobsFilterList { get; } = new List<string> { "All", "Pending", "Approved", "Assigned", "Accepted", "Cancelled", "Completed" }; // A list of string for the items in the job status combo box (item source)
        public List<string> ContractsFilterList { get; } = new List<string> { "All", "Active", "Expired" }; // A list of string for the items in the job status combo box (item source)
        public List<string> ContractsStatusList { get; } = new List<string> { "Active", "Expired" };  // A list to show the conditions in the edit box
        public List<string> ClientsFilterList { get; } = new List<string> { "All", "Contract", "No Contract/Expired" }; // Filter to show contract vs no contract clients

        // Report
        public Dictionary<string, string> SlotsDictionary { get; set; } //Dictionary to hold the time slot name and the time
        public ObservableCollection<JobAssignment> ReportJobs { get; set; } = new ObservableCollection<JobAssignment>(); // To hold the daily jobs of the courier
        public ObservableCollection<CourierGroupHeader> GroupedMonthlyReport { get; set; } = new ObservableCollection<CourierGroupHeader>(); // To hold the headers for the courier group
        public ObservableCollection<ClientGroupHeader> GroupedMonthlyClientReport { get; set; } = new ObservableCollection<ClientGroupHeader>(); // To hold the headers for the courier group
        public ObservableCollection<ClientValueItem> MonthlyClientValueReportList { get; set; } = new ObservableCollection<ClientValueItem>(); // To hold the headers for the courier group
        public List<string> MonthsList { get; set; } = new List<string>
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        // Used to get the month number using the dropdown value
        private Dictionary<string, int> _monthToNumberMap = new Dictionary<string, int>()
        {
            { "January", 1 },{ "February", 2 },{ "March", 3 },{ "April", 4 },{ "May", 5 },
            { "June", 6 },{ "July", 7 },{ "August", 8 },{ "September", 9 },{ "October", 10 },
            { "November", 11 },{ "December", 12 }
        };
        public List<string> YearsList { get; set; } = new List<string> { "2026" };


        // Creating a property for the selected job to display the details by accessing the Job properites (e.g., JobId, ClientId, CourierId, JobStatus) in the JobDetails property.
        // This allows the admin user to see the details of the selected job in the UI (e.g., in a details panel) when they select a job from the list of pending jobs.
        public Job SelectedJob
        {
            get => _selectedJob;
            set
            {
                // Do conditional checks

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

                CostOfJob = _selectedJob.Cost;

                // Only if the job has a valid courier ID
                if (_selectedJob.CourierId != 0)
                {
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
                }
                else { SelectedCourier = null; }


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

        // To update the cost of the job based on the selected courier
        public decimal CostOfJob
        {
            get => _costOfJob;
            set
            {
                _costOfJob = value;
                OnPropertyChanged();
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
                    CostOfJob -= CostOfJob; //Clearing the value
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
                        if (SelectedJob != null) { SelectedJob.CourierId = value.UserId; }
                    }
                }
            }
        }

        public DateTime DateForDayJobReport
        {
            get => _dateForDayJobReport;
            set
            {
                _dateForDayJobReport = value;
                OnPropertyChanged(nameof(DateForDayJobReport));
            }
        }
        private void SetupSlotMap()
        {
            SlotsDictionary = new Dictionary<string, string>();
            DateTime startTime = DateTime.Today.AddHours(8).AddMinutes(30); // 08:30 AM

            //32 slots
            for (int i = 1; i <= 32; i++)
            {
                string slotCode = $"S{i}";
                // Format as 08:30, 08:45, etc.
                string timeDisplay = startTime.ToString("HH:mm");

                // Adding the slot code (S1,S2,S3) and their display names (Time)
                SlotsDictionary.Add(slotCode, timeDisplay);

                // Increment by 15 minutes for the next slot
                startTime = startTime.AddMinutes(15);
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

                    // Setting the client id
                    if (_selectedClient != null)
                    {
                        // Only if a job is selected as this property is also used in the clients page
                        if (_selectedJob != null)
                        {
                            SelectedJob.ClientId = value.ClientId;
                            CostOfJob = GetCostOfTheJob(value.ClientId);
                        }
                    }
                }
            }
        }

        // To set and get the select job status (for filtering the data grid)
        public string SelectedClientStatus
        {
            get => _selectedClientStatus;
            set
            {
                if (_selectedClientStatus != value)
                {
                    _selectedClientStatus = value;
                    OnPropertyChanged();
                    LoadClientsByStatus(value); // Filtering the jobs list based on the selected job status
                }
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
                if (_selectedContract == null)
                {
                    SelectedClient = null;
                    return;
                }

                //Setting the client in the edit window(using the selected job client Id)
                foreach (Client client in ClientList)
                {
                    if (client.ClientId == _selectedContract.ClientId)
                    {
                        SelectedClient = client;
                        break;
                    }
                }
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
                    LoadContractsByStatus(value); // Filtering the jobs list based on the selected job status
                }
            }
        }

        // Report visibility buttons
        public string DayReportVisibility
        {
            get => _dayReportVisibility;
            set { _dayReportVisibility = value; OnPropertyChanged(); }
        }
        public string MonthlyReportVisibility
        {
            get => _monthlyReportVisibility;
            set { _monthlyReportVisibility = value; OnPropertyChanged(); }
        }
        public string AllJobsReportVisibility
        {
            get => _allJobsReportVisibility;
            set { _allJobsReportVisibility = value; OnPropertyChanged(); }
        }
        public string MonthlyValueReportVisibility
        {
            get => _monthlyValueReportVisbility;
            set { _monthlyValueReportVisbility = value; OnPropertyChanged(); }
        }

        public string SelectedMonthForReport
        {
            get => _selectedMonthForReport;
            set { _selectedMonthForReport = value; OnPropertyChanged(); }

        }

        public string SelectedYearForReport
        {
            get => _selectedYearForReport;
            set { _selectedYearForReport = value; OnPropertyChanged(); }
        }

        // Establishing the commands for the admin dashboard
        public ICommand LogoutCommand { get; }
        public ICommand ManagerJobsCommand { get; }
        public ICommand ManagerContractsCommand { get; }
        public ICommand ManagerClientsCommand { get; }
        public ICommand ManagerCouriersCommand { get; }
        public ICommand ReportsCommand { get; }
        public ICommand RefreshJobsCommand { get; }
        public ICommand RefreshClientsCommand { get; }
        public ICommand RefreshCouriersCommand { get; }
        public ICommand RefreshContractsCommand { get; }

        //Reports page
        public ICommand DayJobsReportCommand { get; }
        public ICommand MonthlyJobsReportCommand { get; }
        public ICommand ContractsJobReportCommand { get; }
        public ICommand ClientsValueReportCommand { get; }

        public ICommand GenerateMonthlyJobsReportCommand { get; }
        public ICommand GenerateDayJobsReportCommand { get; }
        public ICommand GenerateValueReportCommand { get; }
        public ICommand GenerateAllJobsReportCommand { get; }

        // Property to get or set the current subview displayed in the admin dashboard.
        // This allows the admin dashboard to display different content based on user interactions (e.g., viewing pending jobs, managing contracts, etc.)
        // Notify the view that the CurrentSubView property has changed, allowing the UI to update accordingly (e.g., displaying the new subview content)
        public object CurrentSubView
        {
            get { return _currentSubView; }
            set { _currentSubView = value; OnPropertyChanged(nameof(CurrentSubView)); }
        }


        // Command to handle the action of viewing pending jobs. When executed, it will set the CurrentSubView to a new instance of the AdminJobs view, which will display the pending jobs to the admin user.
        private void ManagerJobsPage(object? obj)
        {
            CurrentSubView = new ManagerJobs();
            RefreshPage(); // Refreshing the fields and the page
            GetCouriers(); // Populate the status filter
            GetClients(); // Populate the clients combo box
            GetAllJobs();
        }

        private void ManagerContractsPage(object? obj)
        {
            CurrentSubView = new ManagerContracts();
            RefreshPage(); // Refreshing the fields and the page
            GetClients(); // Populate the clients combo box
            GetAllContracts();
        }

        private void ReportsPage(object? obj)
        {
            CurrentSubView = new AdminReports();
            SetupSlotMap();// Setting up the slot map 

            //Clearing all the item sources
            ReportJobs.Clear();
            GroupedMonthlyClientReport.Clear();
            GroupedMonthlyReport.Clear();
            MonthlyClientValueReportList.Clear();
        }

        private void ManagerClientsPage(object? obj)
        {
            CurrentSubView = new ManagerClients();
            GetClients(); //Updates the clients observable collection
            RefreshPage(); //Refreshes the page           
        }

        private void ManagerCouriersPage(object? obj)
        {
            CurrentSubView = new ManagerCouriers();
            GetCouriers();
        }


        // Methods used

        // Logout
        public void ExecuteLogout(object? obj)
        {
            //Clearing all the item sources
            ReportJobs.Clear();
            GroupedMonthlyClientReport.Clear();
            GroupedMonthlyReport.Clear();
            MonthlyClientValueReportList.Clear();

            // Setting dimensions for the login screen
            _navigationVM.WindowWidth = 400;
            _navigationVM.WindowHeight = 450;
            _navigationVM.CurrentView = new LoginVM(_navigationVM); // Updating the current view to a instance of LoginVM. _sending the view model to be used as well
        }

        // Refresh
        public void RefreshPage()
        {
            SelectedJob = null; // Clearing all the fields   
            SelectedCourier = null; // Clear the dropdown selections
            SelectedClient = null;
            SelectedClientStatus = "All";
            SelectedContract = null;
            if (CostOfJob != null) { CostOfJob = 0; }
            GetCouriers(); // Populate the status filter
            GetClients(); // Populate the clients combo box

            //Clearing all the item sources
            ReportJobs.Clear();
            GroupedMonthlyClientReport.Clear();
            GroupedMonthlyReport.Clear();
            MonthlyClientValueReportList.Clear();
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
                // Update this
                SqlCommand cmd = new SqlCommand("SELECT * FROM Users WHERE UserRole = 'Courier' AND WorkingStatus = 'Active' ", mySqlCon);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    CouriersList.Clear();
                    while (reader.Read())
                    {
                        CouriersList.Add(new User
                        {
                            UserId = Convert.ToInt32(reader["UserID"]),
                            UserName = reader["UserName"].ToString(),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            PhoneNumber = reader["Phone"].ToString(),
                            Address = reader["UserAddress"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
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
                SqlCommand cmd = new SqlCommand("SELECT * FROM Clients WHERE Status = 'Active' ", mySqlCon);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    ClientList.Clear();
                    while (reader.Read())
                    {
                        ClientList.Add(new Client
                        {
                            ClientId = Convert.ToInt32(reader["ClientID"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            ClientAddress = reader["ClientAddress"].ToString(),
                            Phone = reader["Phone"].ToString()
                        });
                    }
                }
                else
                {
                    MessageBox.Show("Reader has no rows");
                }

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }

        private void LoadClientsByStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return;
            ClientList.Clear();
            if (status == "All") { GetClients(); return; }

            // Sql setup
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                if (status == "Contract")
                {
                    // Command setup to check the status of the client for all active contract clients
                    SqlCommand cmdGetClients = new SqlCommand("SELECT c.* FROM Clients c INNER JOIN Contracts co ON c.ClientID = co.ClientID " +
                        "WHERE co.ContractStatus = 'Active' AND c.Status = 'Active' ", mySqlCon);
                    SqlDataReader reader = cmdGetClients.ExecuteReader();

                    if (reader.HasRows)
                    {
                        ClientList.Clear();
                        while (reader.Read())
                        {
                            ClientList.Add(new Client
                            {
                                ClientId = Convert.ToInt32(reader["ClientID"]),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                ClientAddress = reader["ClientAddress"].ToString(),
                                Phone = reader["Phone"].ToString()
                            });
                        }
                    }

                }
                else if (status == "No Contract/Expired")
                {
                    SqlCommand cmdGetClients = new SqlCommand("SELECT c.* FROM Clients c LEFT JOIN Contracts co ON c.ClientID = co.ClientID " +
                        "WHERE (co.ContractStatus IS NULL OR co.ContractStatus = 'Expired') AND c.Status='Active'", mySqlCon);
                    SqlDataReader reader = cmdGetClients.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ClientList.Add(new Client
                            {
                                ClientId = Convert.ToInt32(reader["ClientID"]),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                ClientAddress = reader["ClientAddress"].ToString(),
                                Phone = reader["Phone"].ToString()
                            });
                        }
                    }
                }
                else { MessageBox.Show("No Clients to display"); return; }

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
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
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.*, c.Name AS ClientName FROM Jobs j INNER JOIN Clients c ON j.ClientID = c.ClientID WHERE JobStatus = @Status AND c.Status = 'Active' ", mySqlCon);
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
                                 EndDate = drlistJobs["EndDate"] == DBNull.Value ? Convert.ToDateTime(drlistJobs["StartDate"]) : Convert.ToDateTime(drlistJobs["EndDate"]),
                                 JobStatus = drlistJobs["JobStatus"].ToString(),
                                 DeliveryAddress = drlistJobs["DeliveryAddress"].ToString(),
                                 Description = drlistJobs["Description"].ToString(),
                                 Cost = Convert.ToDecimal(drlistJobs["Cost"])
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
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }

        // This method is used to update or filter the data grid source based on the selected job status
        private void LoadContractsByStatus(string contractStatus)
        {
            if (contractStatus == null) { return; } // Checking for null value

            //If the status is "All", call show all jobs method
            if (contractStatus == "All") { GetAllContracts(); return; }

            RefreshPage(); // Refreshing before updating the form

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Setting up the sql command
                SqlCommand cmGetContracts = new SqlCommand("SELECT cnt.*, c.Name AS ClientName FROM Contracts cnt INNER " +
                    "JOIN Clients c ON cnt.ClientID = c.ClientID WHERE ContractStatus = @contractStatus AND c.Status = 'Active' ", mySqlCon);
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
                                 CompanyName = reader["ClientName"].ToString(), // Now available from the JOIN
                                 StartDate = (reader["StartDate"]) == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["StartDate"]),
                                 EndDate = (reader["EndDate"]) == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["EndDate"]),
                                 Notes = reader["Notes"] == DBNull.Value ? "No Notes" : reader["Notes"].ToString(),
                                 ContractStatus = reader["ContractStatus"].ToString(),
                                 Address = reader["Address"].ToString(),
                                 Email = reader["Email"].ToString(),
                                 PhoneNumber = reader["Phone"].ToString(),
                                 MonthlyCost = reader["MonthlyCost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["MonthlyCost"]),
                                 CostPerJob = reader["CostPerJob"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CostPerJob"])
                             }
                          );
                    }
                    reader.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }

        private decimal GetCostOfTheJob(int clientID)
        {

            decimal jobCost = 0; // Setting the variable

            // Checking if the status of the client in the database is active
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Command setup to check the status of the client
                SqlCommand cmdGetStatus = new SqlCommand("SELECT ContractStatus FROM Contracts WHERE ClientID = @ClientID", mySqlCon);
                cmdGetStatus.Parameters.AddWithValue("@ClientID", clientID);
                SqlDataReader reader = cmdGetStatus.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (reader["ContractStatus"].ToString() == "Active")
                        {
                            return 2.5m;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return 10m;
            }
            return 10m;
        }

        public void RefreshJobsPage(object? obj) { RefreshPage(); }
        public void RefreshClientsPage(object? obj) { RefreshPage(); SelectedClientStatus = "All"; SelectedClient = null; }
        public void RefreshCouriersPage(object? obj) { RefreshPage(); }
        public void RefreshContractsPage(object? obj) { RefreshPage(); }

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
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.*, c.Name AS ClientName FROM Jobs j INNER JOIN Clients c ON j.ClientID = c.ClientID WHERE c.Status = 'Active' ", mySqlCon);
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
                                 JobId = Convert.ToInt32(listJobs["JobId"]),
                                 ClientId = Convert.ToInt32(listJobs["ClientId"]),
                                 ClientName = listJobs["ClientName"].ToString(), // Now available from the JOIN

                                 // Handling potential NULLs for CourierID
                                 CourierId = listJobs["CourierId"] == DBNull.Value ? 0 : Convert.ToInt32(listJobs["CourierId"]),

                                 StartDate = Convert.ToDateTime(listJobs["StartDate"]),
                                 // If end date is null (it will be set as the start date)
                                 EndDate = listJobs["EndDate"] == DBNull.Value ? Convert.ToDateTime(listJobs["StartDate"]) : Convert.ToDateTime(listJobs["EndDate"]),
                                 JobStatus = listJobs["JobStatus"].ToString(),
                                 DeliveryAddress = listJobs["DeliveryAddress"].ToString(),
                                 Description = listJobs["Description"].ToString(),
                                 Cost = Convert.ToDecimal(listJobs["Cost"])
                             }
                          );
                    }
                    listJobs.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }
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
                SqlCommand cmGetJobs = new SqlCommand("SELECT cnt.*, c.Name AS ClientName FROM Contracts cnt INNER JOIN Clients c ON cnt.ClientID = c.ClientID WHERE c.Status = 'Active' ", mySqlCon);
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
                                 CompanyName = reader["ClientName"].ToString(), // Now available from the JOIN
                                 StartDate = (reader["StartDate"]) == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["StartDate"]),
                                 EndDate = (reader["EndDate"]) == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["EndDate"]),
                                 Notes = reader["Notes"] == DBNull.Value ? "No Notes" : reader["Notes"].ToString(),
                                 ContractStatus = reader["ContractStatus"].ToString(),
                                 Address = reader["Address"].ToString(),
                                 Email = reader["Email"].ToString(),
                                 PhoneNumber = reader["Phone"].ToString(),
                                 MonthlyCost = reader["MonthlyCost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["MonthlyCost"]),
                                 CostPerJob = reader["CostPerJob"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["CostPerJob"])
                             }
                          );
                    }
                    reader.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }

        }


        //Reports methods/logic
        private void ShowClientValueReport(object? obj)
        {
            HideAllReports();
            // Set stackpanel to visible
            MonthlyValueReportVisibility = "Visible";
        }
        private void GenerateClientValueReport(object? obj)
        {
            // Conditional check to check if the year and month is selected
            if (SelectedMonthForReport == null || SelectedYearForReport == null)
            {
                MessageBox.Show("Please selected the month and year to generate report");
                return;
            }

            // Load the report using the selected month and year
            GetClientValueReport(_monthToNumberMap[SelectedMonthForReport], int.Parse(SelectedYearForReport));
        }

        private void ShowContractsJobReport(object? obj)
        {
            HideAllReports();
            AllJobsReportVisibility = "Visible";
        }

        private void GenerateContractsJobReport(object? obj)
        {
            // Conditional check to check if the year and month is selected
            if (SelectedMonthForReport == null || SelectedYearForReport == null)
            {
                MessageBox.Show("Please selected the month and year to generate report");
                return;
            }
            // Load the report using the selected month and year
            LoadMonthlyClientReport(_monthToNumberMap[SelectedMonthForReport], int.Parse(SelectedYearForReport));
        }

        private void GenerateMonthlyJobsReport(object? obj)
        {
            LoadMonthlyReport();
        }

        private void GenerateDayJobsReport(object? obj)
        {
            if (SelectedCourier == null || DateForDayJobReport == null)
            {
                MessageBox.Show("Please select a courier and a date to display the report");
                return;
            }
            else
            {
                ReportJobs.Clear();
                GetDailyJobsReport(SelectedCourier.UserId.ToString());
            }
        }

        private void ShowMonthlyJobReport(object? obj)
        {
            HideAllReports();
            // Set stackpanel to visible
            MonthlyReportVisibility = "Visible";
            LoadMonthlyReport();
        }

        private void ShowDayReport(object? obj)
        {
            HideAllReports();
            // Set stackpanel to visible
            DayReportVisibility = "Visible";
            //GetCouriers();
        }

        private void HideAllReports()
        {
            DayReportVisibility = "Hidden";
            MonthlyReportVisibility = "Hidden";
            AllJobsReportVisibility = "Hidden";
            MonthlyValueReportVisibility = "Hidden";
        }

        private void GetDailyJobsReport(string userID)
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();
            try
            {
                // Setting up the sql command to get the jobs for the day
                // Filtered out using todays date
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.JobID, j.CourierID, j.DeliveryAddress, j.Description, j.JobStatus, " +
                    "c.ClientID, c.Name AS ClientName, ja.DeliverySlot, ja.DeliveryDate " +
                    "FROM Jobs j INNER JOIN JobAssignments ja ON j.JobID = ja.JobID " +
                    "INNER JOIN Clients c ON j.ClientID = c.ClientID " +
                    "WHERE ja.CourierID = @CourierID " +
                    "AND j.JobStatus = 'Accepted' " +
                    "AND CAST(ja.DeliveryDate AS DATE)= @Date " +
                    "ORDER BY ja.DeliverySlot DESC", mySqlCon);

                cmGetJobs.Parameters.AddWithValue("@CourierID", userID); // Pass the ID here
                cmGetJobs.Parameters.AddWithValue("@Date", DateForDayJobReport.Date); // Date chosen

                SqlDataReader reader = cmGetJobs.ExecuteReader();

                // Looping through the data reader and adding them to the list
                if (reader.HasRows)
                {
                    ReportJobs.Clear();
                    while (reader.Read())
                    {
                        ReportJobs.Add(
                             new JobAssignment
                             {
                                 //AssignmentID
                                 JobId = Convert.ToInt32(reader["JobId"]),
                                 // Handling potential NULLs for CourierID
                                 CourierId = reader["CourierId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CourierId"]),
                                 ClientId = Convert.ToInt32(reader["ClientId"]),
                                 ClientName = reader["ClientName"].ToString(), // Now available from the JOIN
                                 DeliveryAddress = reader["DeliveryAddress"].ToString(),
                                 Description = reader["Description"].ToString(),
                                 JobStatus = reader["JobStatus"].ToString(),

                                 //DeliverySlot = reader["DeliverySlot"].ToString(),
                                 // Gets the time of delivery (using the slot from the database)
                                 DeliverySlot = SlotsDictionary[reader["DeliverySlot"].ToString()],
                                 DeliveryDate = Convert.ToDateTime(reader["DeliveryDate"])
                             }
                          );
                    }
                    reader.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }

        public void LoadMonthlyReport()
        {
            // Get all the jobs for the month
            List<JobReport> allJobs = GetJobsForTheMonth();

            //Clearing the list
            if (GroupedMonthlyReport.Count > 0) { GroupedMonthlyReport.Clear(); }

            // Loop through each job
            foreach (JobReport currentJob in allJobs)
            {
                // We need to check: Do we already have an Expander/Group for this courier?
                CourierGroupHeader foundGroup = null;

                foreach (var group in GroupedMonthlyReport)
                {
                    if (group.GroupName == currentJob.CourierName)
                    {
                        foundGroup = group;
                        break; // We found the right cubby, stop looking!
                    }
                }

                // 4. Use an IF/ELSE to decide what to do
                if (foundGroup != null)
                {
                    // IF we found the group, just add this job to their list
                    foundGroup.GroupJobs.Add(currentJob);
                }
                else
                {
                    // ELSE (if this is the first time we see this courier's name)
                    // Create a brand new group for them
                    CourierGroupHeader newGroup = new CourierGroupHeader();
                    newGroup.GroupName = currentJob.CourierName;
                    newGroup.GroupJobs = new List<Job>(); // Initialize their job list
                    newGroup.GroupJobs.Add(currentJob);   // Add their first job

                    // Add the whole new group to our main list
                    GroupedMonthlyReport.Add(newGroup);
                }
            }
        }

        public void LoadMonthlyClientReport(int month, int year)
        {

            // Get all the jobs for the month
            List<ClientMonthlyJobReport> allJobs = GetClientJobsForTheMonth(month, year);

            //Clearing the list
            if (GroupedMonthlyClientReport.Count > 0) { GroupedMonthlyClientReport.Clear(); }

            // Loop through each job
            foreach (ClientMonthlyJobReport currentJob in allJobs)
            {
                // We need to check: Do we already have an Expander/Group for this courier?
                ClientGroupHeader foundGroup = null;

                // Going through the card header
                foreach (var group in GroupedMonthlyClientReport)
                {
                    // Comparing the client names
                    if (group.ClientName == currentJob.ClientName)
                    {
                        foundGroup = group;
                        break; // Go out of the foreach loop as the job needs to be added to the found card (client)
                    }
                }

                // If the group is not null, the currentJob is added to the group (set in the foreach loop)
                if (foundGroup != null)
                {
                    // Adding the job to the group (client group)
                    foundGroup.ClientJobs.Add(currentJob);
                }
                else // Since the group is null (only happens if the group does not exist), a new group is added (which will be later used to add jobs)
                {
                    // Create a new group (card) for the client
                    ClientGroupHeader newGroup = new ClientGroupHeader();
                    newGroup.ClientName = currentJob.ClientName;
                    newGroup.ClientEmail = currentJob.ClientEmail;
                    newGroup.ClientJobs = new List<ClientMonthlyJobReport>(); // Setting up the jobs list
                    newGroup.ClientJobs.Add(currentJob);   // Adding the current job

                    // Add the whole new group to the main
                    GroupedMonthlyClientReport.Add(newGroup);
                }
            }
        }

        private List<JobReport> GetJobsForTheMonth()
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                SqlCommand cmGetJobs = new SqlCommand(
                    "SELECT j.JobID, j.CourierID, u.UserName AS CourierName, j.DeliveryAddress, j.Description, j.JobStatus, j.EndDate, " +
                    "c.ClientID, c.Name AS ClientName " +
                    "FROM Jobs j " +
                    "INNER JOIN Clients c ON j.ClientID = c.ClientID " +
                    "INNER JOIN Users u ON j.CourierID = u.UserID " + // JOIN to get Courier Name
                    "WHERE j.JobStatus = 'Completed' " +
                    "AND MONTH(j.EndDate) = @Month " +
                    "AND YEAR(j.EndDate) = @Year " +
                    "ORDER BY u.UserName, j.EndDate DESC", mySqlCon);

                // Filter by the current month and year
                cmGetJobs.Parameters.AddWithValue("@Month", DateTime.Now.Month);
                cmGetJobs.Parameters.AddWithValue("@Year", DateTime.Now.Year);

                SqlDataReader reader = cmGetJobs.ExecuteReader();

                // Temporary list to hold every job found
                List<JobReport> tempAllJobs = new List<JobReport>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tempAllJobs.Add(new JobReport
                        {
                            JobId = Convert.ToInt32(reader["JobId"]),
                            CourierId = reader["CourierId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CourierId"]),
                            CourierName = reader["CourierName"].ToString(),
                            ClientId = Convert.ToInt32(reader["ClientId"]),
                            ClientName = reader["ClientName"].ToString(),
                            DeliveryAddress = reader["DeliveryAddress"].ToString(),
                            Description = reader["Description"].ToString(),
                            JobStatus = reader["JobStatus"].ToString(),
                            EndDate = Convert.ToDateTime(reader["EndDate"]),
                        });
                    }
                }
                else { MessageBox.Show("No data to be viewed. Edit and try again."); }
                reader.Close();
                return tempAllJobs;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
            return null;
        }

        private List<ClientMonthlyJobReport> GetClientJobsForTheMonth(int month, int year)
        {

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {   // Links multiple tables (Left join so that null values are used if no record)
                SqlCommand cmGetJobs = new SqlCommand(
                    "SELECT j.JobID, j.StartDate AS DateCreated, j.DeliveryAddress AS Address, j.Cost, " +
                    "c.Name AS ClientName, c.Email AS ClientEmail, " +
                    "CASE " +
                    "WHEN con.ContractID IS NULL THEN 'No Contract' " +
                    "WHEN con.ContractStatus = 'Expired' THEN 'Expired' " +
                    "ELSE 'Active' " +
                    "END AS CalculatedClientStatus " +
                    "FROM Jobs j INNER JOIN Clients c ON j.ClientID = c.ClientID " +
                    "LEFT JOIN Contracts con ON c.ClientID = con.ClientID " +
                    "WHERE j.JobStatus NOT IN ('Pending') AND c.Status = 'Active'" +
                    "AND MONTH(j.StartDate) = @Month " +
                    "AND YEAR(j.StartDate) = @Year " +
                    "ORDER BY c.Name ASC, j.StartDate DESC ", mySqlCon);

                // Filter by the current month and year
                cmGetJobs.Parameters.AddWithValue("@Month", month);
                cmGetJobs.Parameters.AddWithValue("@Year", year);

                SqlDataReader reader = cmGetJobs.ExecuteReader();

                // Temporary list to hold every job found
                List<ClientMonthlyJobReport> tempAllJobs = new List<ClientMonthlyJobReport>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tempAllJobs.Add(new ClientMonthlyJobReport
                        {
                            JobID = Convert.ToInt32(reader["JobID"]),
                            DateCreated = Convert.ToDateTime(reader["DateCreated"]),
                            DeliveryAddress = reader["Address"].ToString(),
                            Cost = Convert.ToDecimal(reader["Cost"]),
                            ClientName = reader["ClientName"].ToString(),
                            ClientEmail = reader["ClientEmail"].ToString(),
                            Status = reader["CalculatedClientStatus"].ToString()
                        });
                    }
                }
                reader.Close();
                return tempAllJobs;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
            return null;
        }

        public void GetClientValueReport(int month, int year)
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();
            MonthlyClientValueReportList.Clear();
            try
            {
                // 1. Updated SQL: Removed CourierID filter, added JOIN to Users to get CourierName, 
                // and changed Date filter to look at Month/Year.
                SqlCommand cmdGetValue = new SqlCommand("SELECT c.ClientID, c.Name, c.Email, " +
                    "COALESCE(co.ContractStatus, 'None') AS ContractStatus, " +
                    "COALESCE(co.MonthlyCost, 0) AS MonthlyContractFee, " +
                    "SUM(ISNULL(j.Cost, 0)) AS TotalJobsCost, " +
                    "COALESCE(co.MonthlyCost, 0) + SUM(ISNULL(j.Cost, 0)) AS TotalValue " +
                    "FROM Clients c LEFT JOIN Contracts co ON c.ClientID = co.ClientID " +
                    "LEFT JOIN Jobs j ON c.ClientID = j.ClientID AND j.JobStatus != 'Pending' AND MONTH(j.StartDate) = @Month AND YEAR(j.StartDate) = @Year " +
                    "WHERE c.Status = 'Active' GROUP BY c.ClientID, c.Name, c.Email, co.ContractStatus, co.MonthlyCost", mySqlCon);

                // Filter by the current month and year
                cmdGetValue.Parameters.AddWithValue("@Month", month);
                cmdGetValue.Parameters.AddWithValue("@Year", year);

                SqlDataReader reader = cmdGetValue.ExecuteReader();

                // Clearing the observable collection
                //MonthlyClientValueReportList.Clear();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        MonthlyClientValueReportList.Add(new ClientValueItem
                        {
                            ClientId = Convert.ToInt32(reader["ClientID"]),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            ContractStatus = reader["ContractStatus"].ToString(),
                            MonthlyContractFee = Convert.ToDecimal(reader["MonthlyContractFee"]),
                            TotalJobsCost = Convert.ToDecimal(reader["TotalJobsCost"]),
                            MonthlyValue = Convert.ToDecimal(reader["TotalValue"]),
                        });
                    }
                }
                reader.Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }

    }
}

