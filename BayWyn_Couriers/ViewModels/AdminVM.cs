using BayWyn_Couriers.Models;
using BayWyn_Couriers.Utilities;
using BayWyn_Couriers.Views;
using BayWyn_Couriers.Views.AdminSubViews;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BayWyn_Couriers.ViewModels
{
    public class AdminVM : ViewModelBase
    {
        // A private field to hold the reference to the navigation view model, which will be used to navigate to different views based on the user's role after a successful login
        private NavigationVM _navigationVM;
        // A private field to hold the reference to the current subview, which can be used to display different content within the admin dashboard based on user interactions (e.g., viewing pending jobs, managing contracts, etc.)
        private object _currentSubView;
        private Job _selectedJob; // Private field to hold the reference to the selected job from the observable collection of pending jobs in the admin clients page

        public ObservableCollection<Job> PendingJobs { get; set; } = new ObservableCollection<Job>();// Observable collection to hold the pending jobs
        public ObservableCollection<Job> AllJobs { get; set; } = new ObservableCollection<Job>(); // To hold the jobs (irrespective of status)
        public ObservableCollection<Contract> ContractsList { get; set; } // Observable collection to hold the list of contracts

        // To hold the selected job from the observable collection of customers in the admin clients page
        //public Job SelectedJob { get; set; }

        // Creating a property for the selected job to display the details by accessing the Job properites (e.g., JobId, ClientId, CourierId, JobStatus) in the JobDetails property.
        // This allows the admin user to see the details of the selected job in the UI (e.g., in a details panel) when they select a job from the list of pending jobs.
        public Job SelectedJob {
            get => _selectedJob;
            set
            {
                _selectedJob= value;
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
        public ICommand AddJobCommand {  get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand UpdateJobCommand { get; }

        public ICommand NewJobCommand { get; }



        public void ExecuteLogout(object? obj)
        {
            _navigationVM.WindowWidth = 600;
            _navigationVM.WindowHeight = 300;
            // Code to handle logout logic, such as clearing user session and navigating to the login screen
            // Navigating back to the login screen by setting the CurrentView of the navigation view model to a new instance of the LoginVM
            _navigationVM.CurrentView = new LoginVM(_navigationVM);
        }


        // Property to get or set the current subview displayed in the admin dashboard. This allows the admin dashboard to display different content based on user interactions (e.g., viewing pending jobs, managing contracts, etc.)
        public object CurrentSubView
        {
            get { return _currentSubView; }
            set
            {
                _currentSubView = value;
                OnPropertyChanged(nameof(CurrentSubView)); // Notify the view that the CurrentSubView property has changed, allowing the UI to update accordingly (e.g., displaying the new subview content)
            }
        }

        // Command to handle the action of viewing pending jobs. When executed, it will set the CurrentSubView to a new instance of the AdminJobs view, which will display the pending jobs to the admin user.
        private void JobsPage(object? obj)
        {
            CurrentSubView = new AdminJobs();
            GetAllJobs(); // To display all jobs
        }

        private void ReportsPage(object? obj) => CurrentSubView = new AdminReports();
        private void ContractsPage(object? obj) => CurrentSubView = new AdminContracts();
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
            
            AddJobCommand = new RelayCommand(AddNewJob);
            DeleteJobCommand = new RelayCommand(
                execute: obj => DeleteJob(obj),
                canExecute: obj => SelectedJob != null // Logic: Disable if SelectedJob is null
            );
            UpdateJobCommand = new RelayCommand(UpdateJob);
            NewJobCommand = new RelayCommand(NewJob);
        }

        //
        public void NewJob(object? obj)
        {
            // Refreshing the texboxes in the edit window
            GetAllJobs();
            MessageBox.Show("Please enter details in the Edit/New window. After completion click Add");

            // Creating an empty SelectedJob so that the values can be used to add it to the database
            SelectedJob = new Job()
            {
                StartDate = DateTime.Now.Date,
                JobStatus = "Pending",
                Cost = 2.5,
            };
        }

        // Function to add job to the database
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
                cmAddJob.Parameters.AddWithValue("@ClientID", SelectedJob.CourierId);
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
                //Console.WriteLine("An error occurred while closing the connection: " + ex.Message);
                mySqlCon.Close();
            }

            finally
            {
                mySqlCon.Close();
            }

            // Refreshing the jobs data grid after deletion by calling get all jobs
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


                        // Use the ID to find the record, then set the new values
                        cmUpdateJob.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                        cmUpdateJob.Parameters.AddWithValue("@CourierID", SelectedJob.CourierId);
                        cmUpdateJob.Parameters.AddWithValue("@DeliveryAddress", SelectedJob.DeliveryAddress);
                        cmUpdateJob.Parameters.AddWithValue("@Description", SelectedJob.Description);
                        cmUpdateJob.Parameters.AddWithValue("@JobStatus", SelectedJob.JobStatus);

                        cmUpdateJob.ExecuteReader();
                        MessageBox.Show("Job Updated Successfully");
                        // Refreshing the jobs data grid after deletion by calling get all jobs
                        GetAllJobs();
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
                MessageBoxResult result = MessageBox.Show("Are you sure","Deletion",MessageBoxButton.YesNo);

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

                        // Refreshing the jobs data grid after deletion by calling get all jobs
                        GetAllJobs();
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
                }
            }
        }

        public void GetAllJobs()
        {
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Setting up the sql command
                SqlCommand cmGetJobs = new SqlCommand("SELECT * FROM Jobs",mySqlCon);
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
                                CourierId = listJobs["CourierId"] as int? ?? 0,
                                StartDate = Convert.ToDateTime(listJobs["StartDate"]),
                                JobStatus = listJobs["JobStatus"].ToString(),
                                DeliveryAddress = listJobs["DeliveryAddress"].ToString(),
                                Description = listJobs["Description"].ToString(),
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

        public void GetPendingJobs()
        {
            // Going through the database to get all jobs status that are pending
            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            // Opening the SQL connection
            mySqlCon.Open();

            try
            {
                // Creating the SQL command to check for user credential
                SqlCommand cmGetJobs = new SqlCommand("SELECT * FROM Jobs WHERE JobStatus = @Status",mySqlCon);
                cmGetJobs.Parameters.AddWithValue("@Status", "Pending");
                SqlDataReader listJobs = cmGetJobs.ExecuteReader();

                // If a record is found, open the main application window
                if (listJobs.HasRows)
                {
                    PendingJobs.Clear(); // Clearing the collection before adding to it to avoid duplicates
                    while (listJobs.Read()) // Reading through each record found
                    {
                        // Adding the jobs to the pending jobs collection so that the data grid in the admin clients page can display the pending jobs to the admin user when they navigate to the Jobs page in the admin dashboard
                        PendingJobs.Add(
                            new Job  //For each record found, add it to the observable collection
                            {
                                JobId = Convert.ToInt32(listJobs["JobId"]),
                                ClientId = Convert.ToInt32(listJobs["ClientId"]),
                                CourierId = Convert.ToInt32(listJobs["CourierId"]),
                                StartDate = Convert.ToDateTime(listJobs["StartDate"]),
                                JobStatus = listJobs["JobStatus"].ToString(),
                                DeliveryAddress = listJobs["DeliveryAddress"].ToString(),
                                Description = listJobs["Description"].ToString(),
                            }
                         );
                    }
                    // Closing the data reader
                    listJobs.Close();  
                }
                else
                {
                    MessageBox.Show("Error in getting the list");
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine("An error occurred while closing the connection: " + ex.Message);
                mySqlCon.Close();
            }

            finally
            {
                mySqlCon.Close();
            }
        }

        // Method to get all contracts from the database
        public ObservableCollection<Contract> GetAllContracts()
        {
            // Going through the database to get all jobs status that are pending
            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;

            SqlConnection mySqlCon = new(myCon);
            // Opening the SQL connection
            mySqlCon.Open();

            try
            {
                // Creating the SQL command to check for user credential

                SqlCommand cmGetContracts = new SqlCommand();
                cmGetContracts.Connection = mySqlCon;
                cmGetContracts.CommandType = CommandType.Text;
                cmGetContracts.CommandText = "SELECT * FROM Jobs WHERE JobStatus=@Status";

                cmGetContracts.Parameters.AddWithValue("@Status", "Pending");

                SqlDataReader lstContracts = cmGetContracts.ExecuteReader();

                // If a record is found, open the main application window
                if (lstContracts.HasRows)
                {

                    // Initializing the observable collection
                    ContractsList = new ObservableCollection<Contract>();

                    // Reading through each record found
                    while (lstContracts.Read())
                    {
                        // Initializing the observable collection
                        ContractsList.Add(

                            //For each record found, add it to the observable collection
                            new Contract
                            {
                                ClientId = Convert.ToInt32(lstContracts["ClientId"]),
                                CompanyName = lstContracts["CompanyName"].ToString(),
                                Address = lstContracts["Address"].ToString(),
                                PhoneNumber = lstContracts["PhoneNumber"].ToString(),
                                ContractStatus = lstContracts["ContractStatus"].ToString(),
                            }
                         );
                    }

                    // Closing the data reader
                    lstContracts.Close();

                    // Returning the observable collection of jobs with pending status
                    return ContractsList;
                }
                else
                {

                    return ContractsList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while closing the connection: " + ex.Message);
                mySqlCon.Close();

                return ContractsList;

            }

            finally
            {
                mySqlCon.Close();
            }
        }
    }
}

