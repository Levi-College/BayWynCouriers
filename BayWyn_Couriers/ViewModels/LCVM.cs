using BayWyn_Couriers.Models;
using BayWyn_Couriers.Utilities;
using BayWyn_Couriers.Views.LCSubViews;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
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
            LCApprovedJobsCommand = new RelayCommand(LCApprovedJobs);
            LCCompletedJobsCommand = new RelayCommand(LCCompletedJobsPage);
            LCAssignedJobsCommand = new RelayCommand(LCAssignedJobsPage);
            AssignJobCommand = new RelayCommand(AssignJob);
            UnAssignJobCommand = new RelayCommand(UnAssignJob);

            SetupSlotMap();
            InitializeTimeSlots();

            LCApprovedJobs(null);// Showing the default page

           
        }


        // Establishing the commands for the admin dashboard
        public ICommand LogoutCommand { get; }
        public ICommand LCApprovedJobsCommand { get; }
        public ICommand LCAssignedJobsCommand { get; }
        public ICommand LCCompletedJobsCommand { get; }
        public ICommand AssignJobCommand { get; }
        public ICommand UnAssignJobCommand { get; }



        // Declaring variables
        // A private field to hold the reference to the navigation view model, which will be used to navigate to different views based on the user's role after a successful login
        private NavigationVM _navigationVM;
        // A private field to hold the reference to the current subview, which can be used to display different content within the admin dashboard based on user interactions (e.g., viewing pending jobs, managing contracts, etc.)
        private object _currentSubView;
        private DateTime _selectedDeliveryDate = GetDay(DateTime.Today.AddDays(1));
        private Job _selectedJob; // Private field to hold the reference to the selected job from the observable collection of pending jobs in the admin clients page
        private JobAssignment _selectedJobAssignment;
        private string _selectedJobStatus = "All"; // Setting the private variable
        private User _selectedCourier; // To hold the selected courier for dropdown display
        private bool _datePickerEnabled = false; // To disable the date picker unless a courier is selected
        private bool _timePickerEnabled = false; // To disable timer pickers unless a date is picked
        private bool _enableCourierSelection = false; // Disables the courier dropdown by default

        public List<String> LCJobsFilterList { get; } = ["All", "Approved", "Assigned", "Accepted", "Cancelled", "Completed" ]; // A list of string for the items in the job status combo box (item source)
        public List<string> JobsStatusList { get; } = ["Approved", "Assigned", "Accepted", "Cancelled", "Completed"];  // A list to show the conditions in the edit box        
        public ObservableCollection<Job> AllJobs { get; set; } = new (); // To hold the jobs (used for filtered list as well)
        public ObservableCollection<JobAssignment> AllJobAssignments { get; set; } = new ();
        public ObservableCollection<User> CouriersList { get; set; } = new (); // Hold all the courier names and ID (using the user class)
        public Dictionary<string, string> SlotsDictionary { get; set; } //Dictionary to hold the time slot name and the time
        //public ObservableCollection<TimeSlot> TimeSlots { get; set; } // Used to set up the time slots (radio buttons)

        private readonly List<String> lstBreaksType1 = ["S15", "S16", "S17", "S18"]; // Break slots
        private readonly List<String> lstBreaksType2 = ["S19", "S20", "S21", "S22"]; // Break slots

        private ObservableCollection<TimeSlot> _timeSlots;
        public ObservableCollection<TimeSlot> TimeSlots
        {
            get => _timeSlots;
            set
            {
                _timeSlots = value;
                // This is the "Magic" that tells the UI to redraw the radio buttons
                OnPropertyChanged(nameof(TimeSlots));
            }
        }

        // Setting up classes
        // Class used for setting up the timeslot for the couriers
        // Inheriting view model base to use on property changed
        public class TimeSlot : ViewModelBase
        {
            public string SlotName { get; set; }    // e.g., "S1"

            private string _displayName;
            // Using propert changed so that the display name can be set as booked or break
            public string DisplayName // e.g., "09:00 - 09:20"
            {
                get => _displayName;
                set
                {
                    _displayName = value;
                    OnPropertyChanged();
                }
            }

            private bool _isEnabled;
            public bool IsEnabled
            {
                get => _isEnabled;
                set
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        // Objects
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

        // Property for the selected delivery date
        public DateTime SelectedDeliveryDate
        {
            get => _selectedDeliveryDate;
            set
            {
                // Only allow future dates
                if (value.Date <= DateTime.Today)
                {
                    MessageBox.Show("Please select a future date");
                    // Reset to the next available weekday or just don't update
                    return;
                }

                // Ignore weekends (only Mon-Fri allowed)
                if (value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday)
                {
                    MessageBox.Show("BayWyn Couriers operates Monday to Friday only. Please select a weekday.");
                    return;
                }

                // Change value only if conditions are passed
                _selectedDeliveryDate = value;
                OnPropertyChanged(nameof(SelectedDeliveryDate));

                // 4. Refresh slots (added a null check for SelectedCourier to prevent crashes)
                if (SelectedCourier != null)
                {
                    // As soon as the date changes, refresh the slot availability using the selectedcourierID
                    RefreshAvailableSlots(SelectedDeliveryDate, SelectedCourier.UserId);
                    TimePickerEnabled = true; //After refreshing the slots, the items control is enabled
                }
            }
        }


        // Getting the selected job to display details
        public Job SelectedJob
        {
            get => _selectedJob;
            set
            {
                // Do conditional checks
                if (_selectedJob != value) { _selectedJob = value; OnPropertyChanged(); }


                // Updating the select courier (used to update the dropdown)
                // If no job selected let the selected courier and client be null
                // Also the courier selection is set to false
                if (_selectedJob == null) { SelectedCourier = null; EnableCourierSelection = false; return; }

                // Refreshing the page (delivery slot and time booking)
                RefreshBookingPage();
                //Enabling the courier selection in the edit window
                EnableCourierSelection = true;
                DatePickerEnabled = false;
                TimePickerEnabled = false;
                //Matching the courier using the ID
                foreach (User courier in CouriersList)
                {
                    if (courier.UserId == _selectedJob.CourierId)
                    {
                        // Update the selected courier
                        SelectedCourier = courier;
                        break;
                    }
                    else { SelectedCourier = null; }
                }
            }
        }

        public JobAssignment SelectedJobAssignment
        {
            get => _selectedJobAssignment;
            set
            {
                // Do conditional checks
                if (_selectedJobAssignment != value) { _selectedJobAssignment = value; OnPropertyChanged(); }


                // Updating the select courier (used to update the dropdown)
                // If no job selected let the selected courier and client be null
                // Also the courier selection is set to false
                if (_selectedJobAssignment == null) { SelectedCourier = null; return; }

                //Matching the courier using the ID
                foreach (User courier in CouriersList)
                {
                    if (courier.UserId == _selectedJobAssignment.CourierId)
                    {
                        // Update the selected courier
                        SelectedCourier = courier;
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
                    LoadJobsByStatus(value);// Filtering the jobs list based on the selected job status
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
                    if (_selectedCourier != null && SelectedJob != null)
                    {
                        SelectedJob.CourierId = value.UserId;
                    }
                    SelectedDeliveryDate = GetDay(DateTime.Today.AddDays(1)); // Setting the date to tomorrow
                    if (SelectedCourier != null)
                    {
                        RefreshAvailableSlots(SelectedDeliveryDate, SelectedCourier.UserId);
                    }                    
                    // Enabling the date picker but disabling the time picker
                    DatePickerEnabled = true;
                    TimePickerEnabled = true; // Only enabled when a date is selected
                }
            }
        }

        // To update the boolean in UI when it is updated in code
        public bool DatePickerEnabled
        {
            get => _datePickerEnabled;
            set { _datePickerEnabled = value; OnPropertyChanged(); }
        }

        // To update the binding for the items control used to display the slots
        public bool TimePickerEnabled
        {
            get => _timePickerEnabled;
            set { _timePickerEnabled = value; OnPropertyChanged(); }
        }

        public bool EnableCourierSelection
        {
            get => _enableCourierSelection;
            set
            {
                if (SelectedJob != null)
                {
                    _enableCourierSelection = value;
                    OnPropertyChanged();
                    return;
                }
                _enableCourierSelection = false;
                OnPropertyChanged();

            }
        }
        // Methods
        private void LCApprovedJobs(object? obj)
        {
            CurrentSubView = new LCApprovedJobs();
            RefreshPage(); // Refreshing the fields and the page
            GetCouriers(); // Populate the status filter
            LoadJobsByStatus("Approved"); // Loads all the jobs for the page
            //SetupSlotMap();
            //InitializeTimeSlots();
        }

        private void LCAssignedJobsPage(object? obj)
        {
            CurrentSubView = new LCAssignedJobs();
            RefreshPage(); // Refreshing the fields and the page
            SelectedJobAssignment = null;
            GetCouriers(); // Populate the status filter
            GetAllAssignedJobs();
            //SetupSlotMap();
            //InitializeTimeSlots();
        }

        private void LCCompletedJobsPage(object? obj)
        {
            CurrentSubView = new LCCompletedJobs();
            RefreshPage(); // Refreshing the fields and the page
            GetCouriers(); // Populate the status filter
            LoadJobsByStatus("Completed"); // Loads all the jobs initially 
        }

        // Logout
        public void ExecuteLogout(object? obj)
        {
            // Setting dimensions for the login screen
            _navigationVM.WindowWidth = 400;
            _navigationVM.WindowHeight = 450;
            _navigationVM.CurrentView = new LoginVM(_navigationVM); // Updating the current view to a instance of LoginVM. _sending the view model to be used as well
        }


        // To get a valid date for the variable
        private static DateTime GetDay(DateTime date)
        {
            // Keep adding days until we hit a day that isn't Sat or Sun
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            return date;
        }

        // Refresh
        public void RefreshPage()
        {
            SelectedJob = null; // Clearing all the fields   
            SelectedCourier = null; // Clear the dropdown selections
            DatePickerEnabled = false;
            TimePickerEnabled = false;
            EnableCourierSelection = false;
            //SetupSlotMap();
            //InitializeTimeSlots(); // Resetting the time slots
        }

        public void RefreshBookingPage()
        {
            SelectedCourier = null; // Clear the dropdown selections
            DatePickerEnabled = false;
            TimePickerEnabled = false;
        }

        // Sets up the dictionary 
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

        // Adding the TimeSlot objects which will be used by the radio button in the data template
        private void InitializeTimeSlots()
        {
            var slots = new ObservableCollection<TimeSlot>();
            foreach (var entry in SlotsDictionary)
            {
                slots.Add(new TimeSlot
                {
                    SlotName = entry.Key,        // "S1"
                    DisplayName = entry.Value,   // "08:30"
                    IsEnabled = true
                });
            }
            TimeSlots = slots;
        }

        public void RefreshAvailableSlots(DateTime selectedDate, int courierId)
        {
            if (courierId <= 0) return; //Only if a valid courierID is sent, null ignored
            // 1. Reset all slots to enabled first
            //foreach (var slot in TimeSlots) slot.IsEnabled = true;
            InitializeTimeSlots();
        

            // Disabling break slots (12-1pm)
            foreach (var slot in TimeSlots)
            {
                // Alternating the breaks to the coueirs. Even number couriers get type 1 breaks while odd numbers get type 2
                if (courierId%2 == 0)
                {
                    if (lstBreaksType1.Contains(slot.SlotName))
                    {
                        slot.IsEnabled = false;
                        slot.DisplayName = "Break/Disabled";
                    }
                }
                else
                {
                    if (lstBreaksType2.Contains(slot.SlotName))
                    {
                        slot.IsEnabled = false;
                        slot.DisplayName = "Break/Disabled";
                    }
                }               
            }

            //Finding out slots that are already booked.Looping throught the slots first then looping through each item in the list
            var takenSlots = LoadTakenSlotsFromDatabase(selectedDate, courierId);

            foreach (var slot in TimeSlots)
            {
                // If slot name in the list set the isEnabled to false and change the display name
                if (takenSlots.Contains(slot.SlotName))

                {
                    slot.IsEnabled = false;
                    slot.DisplayName = "Booked";
                }

            }
        }


        // Gets the slots already booked for the courier
        private List<string> LoadTakenSlotsFromDatabase(DateTime date, int courierID)
        {
            List<string> usedTimeSlots = new List<string>();
            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                SqlCommand cmd = new SqlCommand("SELECT DeliverySlot FROM JobAssignments WHERE CourierID = @CourierID AND DeliveryDate = @Date", mySqlCon);
                // Use parameters to prevent SQL Injection
                cmd.Parameters.AddWithValue("@CourierID", courierID);
                cmd.Parameters.AddWithValue("@Date", date.Date); // .Date ensures we only compare the day

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        // Add each booked slot code to our list
                        usedTimeSlots.Add(reader["DeliverySlot"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                mySqlCon.Close();
            }

            return usedTimeSlots;

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
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.*, c.Name AS ClientName FROM Jobs j INNER JOIN Clients c ON j.ClientID = c.ClientID WHERE JobStatus NOT IN ('Pending', 'Completed')", mySqlCon);
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

        public void GetAllAssignedJobs()
        {
            RefreshPage(); // Refreshing before updating the form

            // Setting up sql connection
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
            SqlConnection mySqlCon = new(myCon);
            mySqlCon.Open();

            try
            {
                // Setting up the sql command
                SqlCommand cmGetJobs = new SqlCommand("SELECT j.*, ja.* FROM Jobs j INNER JOIN JobAssignments ja ON j.JobID = ja.JobID WHERE JobStatus IN ('Assigned','Accepted')", mySqlCon);
                SqlDataReader reader = cmGetJobs.ExecuteReader();

                // Looping through the data reader and adding them to the list
                if (reader.HasRows)
                {
                    AllJobAssignments.Clear();
                    while (reader.Read())
                    {
                        AllJobAssignments.Add(
                             new JobAssignment
                             {
                                 JobId = Convert.ToInt32(reader["JobId"]),
                                 ClientId = Convert.ToInt32(reader["ClientId"]),

                                 // Handling potential NULLs for CourierID
                                 CourierId = reader["CourierId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CourierId"]),

                                 StartDate = Convert.ToDateTime(reader["StartDate"]),
                                 // If end date is null (it will be set as the start date)
                                 EndDate = reader["EndDate"] == DBNull.Value ? Convert.ToDateTime(reader["StartDate"]) : Convert.ToDateTime(reader["EndDate"]),
                                 JobStatus = reader["JobStatus"].ToString(),
                                 DeliveryAddress = reader["DeliveryAddress"].ToString(),
                                 Description = reader["Description"].ToString(),
                                 Cost = Convert.ToDecimal(reader["Cost"]),
                                 DeliveryDate = Convert.ToDateTime(reader["DeliveryDate"]),

                                 //Getting the time from the slot
                                 DeliverySlot = SlotsDictionary[reader["DeliverySlot"].ToString()]
                             }
                          );
                    }
                    reader.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); mySqlCon.Close(); }
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

        // To assign the job. Updates the JobAssignment table, Jobs Table, Refreshed the jobs list (dont have pending)
        public void AssignJob(object? obj)
        {
            //Checking if a job is selected
            if (SelectedJob == null)
            {
                MessageBox.Show("Please select a job to assign");
                return;
            }
            else
            {
                // Confirming deletion
                MessageBoxResult result = MessageBox.Show("Confirm Update", "Update", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Testing the time slot selected


                    // Setting up sql connection
                    string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
                    SqlConnection mySqlCon = new(myCon);
                    mySqlCon.Open();

                    try
                    {
                        SqlCommand cmdAssign = new SqlCommand("INSERT INTO JobAssignments (JobID, CourierID, DeliveryDate, DeliverySlot) " +
                            "VALUES(@JobID, @CourierID, @Date, @Slot)", mySqlCon);

                        cmdAssign.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                        cmdAssign.Parameters.AddWithValue("@CourierID", SelectedCourier.UserId);
                        cmdAssign.Parameters.AddWithValue("@Date", SelectedDeliveryDate);
                        // Get the SlotName (e.g., "S1") from the radio button the user clicked
                        // It gets the first matching value (s.IsSelected.SlotName)
                        cmdAssign.Parameters.AddWithValue("@Slot", TimeSlots.FirstOrDefault(s => s.IsSelected).SlotName);

                        cmdAssign.ExecuteNonQuery();

                        // 2. Update the main Jobs table status
                        string updateQuery = "UPDATE Jobs SET JobStatus = 'Assigned' WHERE JobID = @JobID";

                        SqlCommand cmdUpdate = new SqlCommand("UPDATE Jobs SET JobStatus = 'Assigned', CourierID = @CourierID WHERE JobID = @JobID", mySqlCon);
                        cmdUpdate.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                        cmdUpdate.Parameters.AddWithValue("@CourierID", SelectedCourier.UserId);

                        cmdUpdate.ExecuteNonQuery();
                        MessageBox.Show("Job Assigned Successfully!");
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                    finally { mySqlCon.Close(); }

                    RefreshPage();
                    // Refresh the LC's list to remove the now-assigned job and only show the approved (pending to be assigned)
                    LoadJobsByStatus("Approved");
                }
            }
        }


        // Unassign the job selected from the courier
        private void UnAssignJob(object? obj)
        {
            // 1. Validation check
            if (SelectedJobAssignment == null) { MessageBox.Show("Please select a job to unassign"); return; }

            // 2. Confirmation Check
            MessageBoxResult result = MessageBox.Show("Confirm Unassignment. To assign to another courier, go to pending assignment page", "Unassign", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // Setting up sql connection
                string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;
                SqlConnection mySqlCon = new SqlConnection(myCon);
                mySqlCon.Open();

                try
                {
                    SqlCommand cmdDelete = new SqlCommand("DELETE FROM JobAssignments WHERE JobID = @JobID", mySqlCon);
                    cmdDelete.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                    cmdDelete.ExecuteNonQuery();

                    SqlCommand cmdUpdate = new SqlCommand("UPDATE Jobs SET JobStatus = 'Approved', CourierID = NULL WHERE JobID = @JobID", mySqlCon);
                    cmdUpdate.Parameters.AddWithValue("@JobID", SelectedJob.JobId);
                    cmdUpdate.ExecuteNonQuery();

                    MessageBox.Show("Job successfully unassigned. It is now back in the pending assignment list.");

                    RefreshPage(); // Calling your method to refresh the UI and lists
                    LoadJobsByStatus("Assigned");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                finally { mySqlCon.Close(); }

            }
        }
    }
}

