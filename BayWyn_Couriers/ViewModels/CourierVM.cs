using BayWyn_Couriers.Models;
using BayWyn_Couriers.Utilities;
using BayWyn_Couriers.Views.CourierSubViews;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BayWyn_Couriers.ViewModels
{
    public class CourierVM : ViewModelBase
    {
        // Constructor
        public CourierVM(NavigationVM _nav, string _userID = "0")
        {
            // Initializing the LogoutCommand with a new RelayCommand that executes the ExecuteLogout method when invoked
            // When the LogoutCommand is executed (e.g., when a logout button is clicked in the UI), it will call the ExecuteLogout method,
            // which will handle the logout logic such as clearing the user session and navigating back to the login screen.
            _navigationVM = _nav; // Assigning the passed navigation view model to the private field _navigationVM, allowing the AdminVM to use it for navigation purposes (e.g., navigating back to the login screen after logout)
            UserID = _userID;
            LogoutCommand = new RelayCommand(ExecuteLogout); // Giving the LogoutCommand a meaning }
            CourierPendingJobsCommand = new RelayCommand(CourierPendingJobsPage);
            CourierAcceptedJobsCommand = new RelayCommand(CourierAcceptedJobsPage);
            CourierShiftCommand = new RelayCommand(CourierShiftPage);

            // Manage jobs page commands
            AcceptJobCommand = new RelayCommand(AcceptSelectedJob);
            RejectJobCommand = new RelayCommand(RejectSelectedJob);

            // Shift page commands
            StartShiftCommand = new RelayCommand(StartShift);
            EndShiftCommand = new RelayCommand(EndShift);
            CompleteDeliveryCommand = new RelayCommand(CompleteDelivery);
            // Not able to deliver

            // Setting up the time slot dictionary (used to get the time from the slot)
            SetupSlotMap();
            StartTimer();
        }


        // Establishing the commands for the admin dashboard
        public ICommand LogoutCommand { get; }
        public ICommand CourierPendingJobsCommand { get; }
        public ICommand CourierAcceptedJobsCommand { get; }

        public ICommand CourierShiftCommand { get; }

        public ICommand AcceptJobCommand { get; }
        public ICommand RejectJobCommand { get; }

        // Start shift
        public ICommand StartShiftCommand { get; }
        public ICommand EndShiftCommand { get; }
        public ICommand CompleteDeliveryCommand { get; }
        // Not able to deliver





        // Declaring variables
        // A private field to hold the reference to the navigation view model, which will be used to navigate to different views based on the user's role after a successful login
        private NavigationVM _navigationVM;
        // A private field to hold the reference to the current subview, which can be used to display different content within the admin dashboard based on user interactions (e.g., viewing pending jobs, managing contracts, etc.)
        private object _currentSubView;
        private string _courierID;
        private JobAssignment _selectedJob; // Private field to hold the reference to the selected job from the observable collection of pending jobs in the admin clients page
        private string _selectedJobStatus = "All"; // Setting the private variable
        private string _userID;

        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;
        private string _shiftTimerDisplay = "00:00:00";
        private string _currentTimeDisplay = "00:00";

        private bool _enableCompleteJobButton = false;
        private bool _startShiftEnabler = false;
        private bool _endShiftEnabler = false;    
        public ObservableCollection<JobAssignment> PendingJobs { get; set; } = new ObservableCollection<JobAssignment>(); // To hold the jobs waiting to be accepted
        public ObservableCollection<JobAssignment> AcceptedJobs { get; set; } = new ObservableCollection<JobAssignment>();
        public ObservableCollection<JobAssignment> DailyJobs { get; set; } = new ObservableCollection<JobAssignment>(); // To hold the daily jobs of the courier
        public Dictionary<string, string> SlotsDictionary { get; set; } //Dictionary to hold the time slot name and the time
 
        // Objects
        // To update the sub view
        // Notify the view that the CurrentSubView property has changed, allowing the UI to update accordingly (e.g., displaying the new subview content)
        public object CurrentSubView
        {
            get { return _currentSubView; }
            set { _currentSubView = value; OnPropertyChanged(nameof(CurrentSubView)); }
        }

        // Getting the selected job to display details
        // If the selected job changed, update all the values using Property changed
        public JobAssignment SelectedJob
        {
            get => _selectedJob;
            set { if (_selectedJob != value) { _selectedJob = value; OnPropertyChanged(); } }
        }

        // Setting the UserID
        public string UserID
        {
            get => _userID;
            set
            {
                if (_userID != value)
                {
                    _userID = value;
                    OnPropertyChanged();
                }
            }
        }

        // To update tht timer displayed in the shift page
        public string ShiftTimerDisplay
        {
            get => _shiftTimerDisplay;
            set{_shiftTimerDisplay = value;OnPropertyChanged();}
        }

        public string CurrentTimeDisplay
        {
            get => _currentTimeDisplay;
            set { _currentTimeDisplay = value; OnPropertyChanged(); }
        }

        public bool EnableCompleteJobButton
        {
            get => _enableCompleteJobButton;
            set { _enableCompleteJobButton = value; OnPropertyChanged(); }
        }

        // To adjust the logic for enabling and disabling the start and end shift button

        public bool StartShiftEnabler
        {
            get => _startShiftEnabler;
            set { _startShiftEnabler = value; OnPropertyChanged(); }
        }

        public bool EndShiftEnabler
        {
            get => _endShiftEnabler;
            set { _endShiftEnabler = value; OnPropertyChanged(); }
        }


        // Methods
        private void CourierPendingJobsPage(object? obj)
        {
            CurrentSubView = new CourierPendingJobs();
            RefreshPage(); // Refreshing the fields and the page
            GetPendingJobs(); // Populates the list of pending acceptance jobs
        }

        private void CourierAcceptedJobsPage(object? obj)
        {
            CurrentSubView = new CourierAcceptedJobs();
            RefreshPage(); // Refreshing the fields and the page
            GetAcceptedJobs(); // Populates the list of pending acceptance jobs
        }

        private void CourierShiftPage(object? obj)
        {
            CurrentSubView = new CourierShift();
            RefreshPage(); // Refreshing the fields and the page
            GetDailyJobs(UserID);
        }


        // Logout
        public void ExecuteLogout(object? obj)
        {
            // Setting dimensions for the login screen
            _navigationVM.WindowWidth = 400;
            _navigationVM.WindowHeight = 450;
            _navigationVM.CurrentView = new LoginVM(_navigationVM); // Updating the current view to a instance of LoginVM. _sending the view model to be used as well
        }

        // To get the time from the dictionary as the slots are stored in the database  
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

        // Refresh
        public void RefreshPage()
        {
            SelectedJob = null; // Clearing all the fields   
            //SelectedCourier = null; // Clear the dropdown selections
            //DatePickerEnabled = false;
        }


        //Accept the selected job (using selectedJob object and properties)
        private void AcceptSelectedJob(object? obj)
        {
            //Returning if the obj is null
            if (obj == null) { return; }

            // Casting the object as a job
            SelectedJob = (JobAssignment)obj;

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                SqlCommand cmdAcceptJob = new SqlCommand("UPDATE Jobs SET JobStatus = 'Accepted' WHERE JobID = @JobID", mySqlCon);
                cmdAcceptJob.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                cmdAcceptJob.ExecuteNonQuery();

                // Removing the jobs from the list 
                PendingJobs.Remove(SelectedJob);

            }
            catch (Exception ex){MessageBox.Show(ex.Message);}
        }

        //Reject the selected job 
        private void RejectSelectedJob(object? obj)
        {
            //Returning if the obj is null
            if (obj == null) { return; }

            // Casting the object as a jobassignment : job
            SelectedJob = (JobAssignment)obj;

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Delete the assignment record from the job assignment table
                SqlCommand cmdDel = new SqlCommand("DELETE FROM JobAssignments WHERE JobID = @JobID", mySqlCon);
                cmdDel.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                cmdDel.ExecuteNonQuery();

                // Set Job status back to 'Approved' so LC can see it again
                SqlCommand cmdUpd = new SqlCommand("UPDATE Jobs SET JobStatus = 'Approved' WHERE JobID = @JobID", mySqlCon);
                cmdUpd.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                cmdUpd.ExecuteNonQuery();

                PendingJobs.Remove(SelectedJob);
                MessageBox.Show("Job Rejected. It has been sent back to the Logistics Coordinator.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }

        public void GetPendingJobs()
        {
            RefreshPage(); // Refreshing before updating the form

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Setting up the sql command
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.JobID, j.CourierID, j.DeliveryAddress, j.Description, j.JobStatus, " +
                    "c.ClientID, c.Name AS ClientName, ja.DeliverySlot, ja.DeliveryDate " +
                    "FROM Jobs j INNER JOIN JobAssignments ja ON j.JobID = ja.JobID " +
                    "INNER JOIN Clients c ON j.ClientID = c.ClientID " +
                    "WHERE ja.CourierID = @CourierID " +
                    "AND j.JobStatus = 'Assigned' ", mySqlCon);
                cmGetJobs.Parameters.AddWithValue("@CourierID", UserID); // Pass the ID here

                SqlDataReader reader = cmGetJobs.ExecuteReader();

                // Looping through the data reader and adding them to the list
                if (reader.HasRows)
                {
                    PendingJobs.Clear();
                    while (reader.Read())
                    {
                        PendingJobs.Add(
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
                                 DeliverySlot = SlotsDictionary[reader["DeliverySlot"].ToString()],
                                 DeliveryDate = Convert.ToDateTime(reader["DeliveryDate"])
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

        public void GetAcceptedJobs()
        {
            RefreshPage(); // Refreshing before updating the form

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Setting up the sql command
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.JobID, j.CourierID, j.DeliveryAddress, j.Description, j.JobStatus, " +
                    "c.ClientID, c.Name AS ClientName, ja.DeliverySlot, ja.DeliveryDate " +
                    "FROM Jobs j INNER JOIN JobAssignments ja ON j.JobID = ja.JobID " +
                    "INNER JOIN Clients c ON j.ClientID = c.ClientID " +
                    "WHERE ja.CourierID = @CourierID " +
                    "AND j.JobStatus = 'Accepted' ", mySqlCon);
                cmGetJobs.Parameters.AddWithValue("@CourierID", UserID); // Pass the ID here

                SqlDataReader reader = cmGetJobs.ExecuteReader();

                // Looping through the data reader and adding them to the list
                if (reader.HasRows)
                {
                    AcceptedJobs.Clear();
                    while (reader.Read())
                    {
                        AcceptedJobs.Add(
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
                                 DeliverySlot = SlotsDictionary[reader["DeliverySlot"].ToString()],
                                 DeliveryDate = Convert.ToDateTime(reader["DeliveryDate"])
                             }
                          );
                    }
                    reader.Close();
                }
            }
            catch (Exception ex){MessageBox.Show(ex.Message);}
            finally{mySqlCon.Close();}
        }

        // Gets the jobs for the current day for the couriers shift
        private void GetDailyJobs(string userID)
        {
            RefreshPage(); // Refreshing before updating the form

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
                    "AND ja.DeliveryDate = CAST(GETDATE() AS DATE)" +
                    "ORDER BY ja.DeliverySlot DESC", mySqlCon);

                cmGetJobs.Parameters.AddWithValue("@CourierID", UserID); // Pass the ID here

                SqlDataReader reader = cmGetJobs.ExecuteReader();

                // Looping through the data reader and adding them to the list
                if (reader.HasRows)
                {
                    DailyJobs.Clear();
                    StartShiftEnabler = true; // Enabling the start shift timer only if there are any jobs for the day
                    while (reader.Read())
                    {
                        DailyJobs.Add(
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
                else{MessageBox.Show("No jobs for today");}
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { mySqlCon.Close(); }
        }

        // Starts when the courier page is loaded
        private void StartTimer()
        {
            // Add the start time to the database (if a shifts table is there)
            _stopwatch = new Stopwatch();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {   
                CurrentTimeDisplay = DateTime.Now.ToString(@"HH:mm"); // Live time
                if (_stopwatch.IsRunning) { ShiftTimerDisplay = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss"); }
            };
            _timer.Start();
        }

        //Start shift methods
        private void StartShift(object? obj)
        {
            // Starting the stop watch
            _stopwatch.Start();

            // Enable all the confirm buttons
            EnableCompleteJobButton = true;
            EndShiftEnabler = true;
            StartShiftEnabler = false; // Cannot start the shift again
        }

        private void EndShift(object? obj)
        {           
            _stopwatch.Stop(); // Stop and reset
            _stopwatch.Reset();

            ShiftTimerDisplay = "00:00:00"; // Clearing the elapsed time
            MessageBox.Show("Thank you for the shift. Have a great time off");
            // Disabling and enabling the buttons
            EnableCompleteJobButton = false;
            StartShiftEnabler = true ;
            EndShiftEnabler= false;
        }

        private void CompleteDelivery(object? obj)
        {
            //Returning if the obj is null
            if (obj == null) { return; }

            // Casting the object as a job. To get the details
            SelectedJob = (JobAssignment)obj;

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                SqlCommand cmdCompleteJob = new SqlCommand("UPDATE Jobs SET JobStatus = 'Completed', EndDate = @EndDate WHERE JobID = @JobID", mySqlCon);
                cmdCompleteJob.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                cmdCompleteJob.Parameters.AddWithValue("@EndDate",DateTime.Now);
                cmdCompleteJob.ExecuteNonQuery();

                SqlCommand cmdRemoveAssignment = new SqlCommand("DELETE FROM JobAssignments WHERE JobID = @JobID",mySqlCon);
                cmdRemoveAssignment.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                cmdRemoveAssignment.ExecuteNonQuery();

                // Removing the jobs from the lists 
                AcceptedJobs.Remove(SelectedJob);
                DailyJobs.Remove(SelectedJob);

                MessageBox.Show("Delivery completed");
            }
            catch (Exception ex){MessageBox.Show(ex.Message);}
        }

    }
}
