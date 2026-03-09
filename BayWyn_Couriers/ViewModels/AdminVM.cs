using BayWyn_Couriers.Models;
using BayWyn_Couriers.Utilities;
using BayWyn_Couriers.Views;
using BayWyn_Couriers.Views.AdminSubViews;
using System.Collections.ObjectModel;
using System.Configuration;
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
        private string _selectedJobStatus = "Pending"; // Setting the private variable
        private User _selectedCourier; // To hold the selected courier for dropdown display
        private Client _selectedClient; // To hold the client details when adding a new job
        private decimal _costOfJob = 10m; // To update the price of the job (when creating a new one)
        private bool _enableItemsForNewJob = false; // This is used to enable and disable items for adding new job (new job window)

        private Contract _selectedContract; // Hold contract details
        private string _selectedContractStatus = "All"; //Default status for the contracts list
        private bool _enableItemsForNewContract = false;

        private string _selectedClientStatus = "All"; // Setting the variable to filter clients 

        // Lists and observable collections
        public ObservableCollection<Job> AllJobs { get; set; } = new ObservableCollection<Job>(); // To hold the jobs (used for filtered list as well)
        public ObservableCollection<User> CouriersList { get; set; } = new ObservableCollection<User>(); // Hold all the courier names and ID (using the user class)
        public ObservableCollection<Client> ClientList { get; set; } = new ObservableCollection<Client>(); // Holds all the clients (dropdown)        
        public List<String> JobsFilterList { get; } = new List<String> { "All", "Pending", "Approved", "Assigned", "Accepted", "Cancelled", "Completed" }; // A list of string for the items in the job status combo box (item source)
        public List<string> JobsStatusList { get; } = new List<string> { "Pending", "Approved", "Assigned", "Accepted", "Cancelled", "Completed" };  // A list to show the conditions in the edit box


        // Used for the contracts page
        public ObservableCollection<Contract> AllContracts { get; set; } = new ObservableCollection<Contract>(); // To hold the list of contracts
        public List<String> ContractsFilterList { get; } = new List<String> { "All", "Active", "Expired" }; // A list of string for the items in the job status combo box (item source)
        public List<string> ContractsStatusList { get; } = new List<string> { "Active", "Expired" };  // A list to show the conditions in the edit box
        public List<string> ClientsFilterList { get; } = new List<string> { "All","Contract", "No Contract/Expired" }; // Filter to show contract vs no contract clients

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
                    // Updating the cost based on if the client is in the contract table or the contracts is expired
                    //CostOfJob = GetCostOfTheJob(SelectedJob.ClientId);
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

                    // Filtering the jobs list based on the selected job status
                    LoadClientsByStatus(value);
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
                if (_selectedContract == null)
                {
                    SelectedClient = null;
                    return;
                }

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
        public ICommand RefreshJobsCommand {  get; }


        // Admin contracts page commands
        public ICommand AddContractCommand { get; }
        public ICommand DeleteContractCommand { get; }
        public ICommand UpdateContractCommand { get; }
        public ICommand RenewContractCommand { get; }
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
            LoadJobsByStatus("Pending"); // Loads all the pending approval jobs
            EnableItemsForNewJob = false; // Used to enable and disable buttons for the edit window
        }

        private void ContractsPage(object? obj)
        {
            CurrentSubView = new AdminContracts();
            RefreshPage(); // Refreshing the fields and the page
            //GetContracts(); // Populate the status filter
            GetClients(); // Populate the clients combo box
            LoadContractsByStatus("All"); // Loads all the jobs initially 
            EnableItemsForNewContract = false; // Used to enable and disable buttons for the edit window
        }

        private void ReportsPage(object? obj) => CurrentSubView = new AdminReports();

        private void ClientsPage(object? obj)
        {
            CurrentSubView = new AdminClients();
            GetClients(); //Updates the clients observable collection
        } 

        private void CouriersPage(object? obj) => CurrentSubView = new AdminCouriers();


        public AdminVM(NavigationVM _nav)
        {
            // When the LogoutCommand is executed (e.g., when a logout button is clicked in the UI), it will call the ExecuteLogout method,
            // which will handle the logout logic such as clearing the user session and navigating back to the login screen.
            _navigationVM = _nav; // Assigning the passed navigation view model to the private field _navigationVM, allowing the AdminVM to use it for navigation purposes (e.g., navigating back to the login screen after logout)
            LogoutCommand = new RelayCommand(ExecuteLogout); // Giving the LogoutCommand a meaning using Relay command 

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
            RefreshJobsCommand = new RelayCommand(RefreshJobsPage);


            // Setting up contracts commands
            // Admin contracts page commands
            AddContractCommand = new RelayCommand(AddNewContract);
            DeleteContractCommand = new RelayCommand(
                execute: obj => DeleteContract(obj),
                canExecute: obj => SelectedContract != null // Logic: Disable if SelectedJob is null
            );
            RenewContractCommand = new RelayCommand(RenewContract);
            UpdateContractCommand = new RelayCommand(UpdateContract);
            NewContractCommand = new RelayCommand(NewContract);

            // Update the database
            // If contract has expired change the price and status text. Check the date. If the end date is before today then make the change

            // Setting the start page as the jobs page
            JobsPage(null);
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

            //RefreshPage(); // Refreshing the fields and the page
            GetCouriers(); // Populate the status filter
            GetClients(); // Populate the clients combo box
            //LoadJobsByStatus("Pending"); // Loads all the jobs initially 
            EnableItemsForNewJob = false; // Used to enable and disable buttons for the edit window
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
                SqlCommand cmd = new SqlCommand("SELECT * FROM Clients", mySqlCon);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadClientsByStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return;

            ClientList.Clear();
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                if (status == "Contract")
                {
                    // Command setup to check the status of the client for all active contract clients
                    SqlCommand cmdGetClients = new SqlCommand("SELECT c.* FROM Clients c INNER JOIN Contracts co ON c.ClientID = co.ClientID WHERE co.ContractStatus = 'Active'", mySqlCon);
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
                else if(status == "No Contract/Expired")
                {
                    SqlCommand cmdGetClients = new SqlCommand("SELECT c.* FROM Clients c LEFT JOIN Contracts co ON c.ClientID = co.ClientID WHERE co.ContractStatus IS NULL OR co.ContractStatus = 'Expired'", mySqlCon);
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
            catch (Exception ex){ MessageBox.Show(ex.Message); }
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


        public void RefreshJobsPage(object? obj)
        {
            RefreshPage();
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
                Cost = 0
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

                // Logic to find out the price based on the client. If client contract status == active, cost of job is 2.5, else it is 10
                cmAddJob.Parameters.AddWithValue("@Cost", GetCostOfTheJob(SelectedClient.ClientId));


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
                        SqlCommand cmUpdateJob = new SqlCommand("UPDATE Jobs SET " +
                            "DeliveryAddress = @DeliveryAddress, " +
                            "Description = @Description, JobStatus = @JobStatus " +
                            "WHERE JobID = @JobID", mySqlCon);

                        // Use the ID to find the record, then set the new values using the parameters
                        cmUpdateJob.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                        cmUpdateJob.Parameters.AddWithValue("@DeliveryAddress", SelectedJob.DeliveryAddress);
                        cmUpdateJob.Parameters.AddWithValue("@Description", SelectedJob.Description);

                        // Setting the status.
                        // If status is pending, approve it, else keep the status
                        if (SelectedJob.JobStatus == "Pending")
                        {
                            cmUpdateJob.Parameters.AddWithValue("@JobStatus", "Approved");
                        }
                        else
                        {
                            cmUpdateJob.Parameters.AddWithValue("@JobStatus", SelectedJob.JobStatus);
                        }
                        

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

            // Check the status (expiry date) // If contract past the date upadted the database  (status and price)

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
                                 CompanyName = reader["ClientName"].ToString(), // Now available from the JOIN
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
            EnableItemsForNewContract = true;

            // Creating an empty SelectedJob so that the values can be used to add it to the database
            SelectedContract = new Contract()
            {
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date.AddMonths(1), // Contracts lasts for 1 month
                ContractStatus = "Active",
                MonthlyCost = 50m,
                CostPerJob = 2.5m,
            };
        }

        public void AddNewContract(object? obj)
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Check if the client already has a contract
                //MessageBox.Show("Client has a contract. Please update the existing contract, Set to active, etc")


                // If no contracts
                // Setting up the sql command
                SqlCommand cmdAddContract = new SqlCommand("INSERT INTO Contracts (ClientID, Address, Phone, Email, Notes, ContractStatus, StartDate, EndDate, MonthlyCost, CostPerJob) " +
                    "VALUES (@ClientID, @Address, @Phone, @Email, @Notes, @ContractStatus, @StartDate, @EndDate, @MonthlyCost, @CostPerJob)", mySqlCon);


                cmdAddContract.Parameters.AddWithValue("@ClientID", SelectedClient.ClientId);
                cmdAddContract.Parameters.AddWithValue("@Address", SelectedContract.Address);
                cmdAddContract.Parameters.AddWithValue("@Phone", SelectedContract.PhoneNumber);
                cmdAddContract.Parameters.AddWithValue("@Email", SelectedContract.Email);
                cmdAddContract.Parameters.AddWithValue("@Notes", (object)SelectedContract.Notes ?? DBNull.Value);
                cmdAddContract.Parameters.AddWithValue("@ContractStatus", SelectedContract.ContractStatus);
                cmdAddContract.Parameters.AddWithValue("@StartDate", SelectedContract.StartDate);
                cmdAddContract.Parameters.AddWithValue("@EndDate", SelectedContract.EndDate);
                cmdAddContract.Parameters.AddWithValue("@MonthlyCost", SelectedContract.MonthlyCost);
                cmdAddContract.Parameters.AddWithValue("@CostPerJob", SelectedContract.CostPerJob);

                cmdAddContract.ExecuteNonQuery();
                MessageBox.Show("Contract Added Successfully");
                GetAllContracts(); // Refresh the list
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding contract: " + ex.Message);
            }

        }

        public void UpdateContract(object? obj)
        {
            //Checking if a job is selected
            if (SelectedContract == null)
            {
                MessageBox.Show("Please select a contract to be update");
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
                        SqlCommand cmd = new SqlCommand("UPDATE Contracts SET ContractStatus = @ContractStatus, StartDate = @StartDate , EndDate = @EndDate , " +
                            "Address = @Address, Phone = @Phone, MonthlyCost = @MonthlyCost , CostPerJob = @CostPerJob, " +
                            "Email = @Email, Notes = @Notes WHERE ContractID = @ContractID", mySqlCon);

                        // Use the ID to find the record, then set the new values using the parameters
                        cmd.Parameters.AddWithValue("@StartDate", SelectedContract.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", SelectedContract.EndDate);
                        cmd.Parameters.AddWithValue("@ContractID", SelectedContract.ContractId);
                        cmd.Parameters.AddWithValue("@Address", SelectedContract.Address);
                        cmd.Parameters.AddWithValue("@Phone", SelectedContract.PhoneNumber);
                        cmd.Parameters.AddWithValue("@Email", SelectedContract.Email);
                        cmd.Parameters.AddWithValue("@ContractStatus", SelectedContract.ContractStatus);
                        cmd.Parameters.AddWithValue("@Notes", (object)SelectedContract.Notes ?? DBNull.Value);

                        // Adjusting the price based on the status
                        if (SelectedContract.ContractStatus == "Active")
                        {
                            cmd.Parameters.AddWithValue("@MonthlyCost", 50m);
                            cmd.Parameters.AddWithValue("@CostPerJob", 2.5m);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@MonthlyCost", 0m);
                            cmd.Parameters.AddWithValue("@CostPerJob", 2.5m);
                        }

                        cmd.ExecuteReader(); // Running the sql command to update the database
                        MessageBox.Show("Contract Updated Successfully");
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

        //Renews the contracts (extends by 1 month. No details can be changed. Only the dates change)
        public void RenewContract(object? obj)
        {
            // Checking if a courier is selected
            if (SelectedContract == null)
            {
                MessageBox.Show("Please Select a contract to renew");
                return;
            }

            // If Contract status is active, don't renew
            if (SelectedContract.ContractStatus == "Active")
            {
                MessageBox.Show("This contract is already Active and does not need renewal yet.");
                return;
            }

            // 2. If contract status is expired, renew for 1 month
            if (SelectedContract.ContractStatus == "Expired")
            {
                // Setting the new start and end date (today and 1 month)
                SelectedContract.StartDate = DateTime.Now.Date;
                SelectedContract.EndDate = DateTime.Now.Date.AddMonths(1);

                // Change status back to Active
                SelectedContract.ContractStatus = "Active";

                // 3. Notify the UI that these properties have changed
                // This ensures the TextBoxes on your form update immediately
                OnPropertyChanged(nameof(SelectedContract));

                MessageBox.Show("Contract dates have been reset for 1 month. Click 'Update Contract' to save these changes to the database.");

            }
        }

        public void DeleteContract(object? obj)
        {
            if (SelectedContract == null)
            {
                MessageBox.Show("Please select a contract to delete.");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this contract?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
                SqlConnection mySqlCon = new(myCon);
                mySqlCon.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM Contracts WHERE ContractID = @ID", mySqlCon);
                    cmd.Parameters.AddWithValue("@ID", SelectedContract.ContractId);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Contract Deleted.");
                    GetAllContracts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting contract: " + ex.Message);
                }
                finally
                {
                    mySqlCon.Close();
                }
            }

        }
    }
}

