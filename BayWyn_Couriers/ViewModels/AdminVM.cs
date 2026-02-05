using BayWyn_Couriers.Models;
using BayWyn_Couriers.Utilities;
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
    public class AdminVM : ViewModelBase
    {
        // A private field to hold the reference to the navigation view model, which will be used to navigate to different views based on the user's role after a successful login
        private NavigationVM _navigationVM;

        public ObservableCollection<Job> PendingJobs { get; set; } // Observable collection to hold the pending jobs
        public ObservableCollection<Contract> ContractsList { get; set; } // Observable collection to hold the list of contracts

        
        public ICommand LogoutCommand { get; }


        //public AdminVM(NavigationVM nav)
        //{
        //    _navigationVM = nav;

        //    LogoutCommand = new RelayCommand(ExecuteLogout);
        //}

        public AdminVM(NavigationVM nav)
        {
            _navigationVM = nav;
            // Initializing the LogoutCommand with a new RelayCommand that executes the ExecuteLogout method when invoked
            // When the LogoutCommand is executed (e.g., when a logout button is clicked in the UI), it will call the ExecuteLogout method,
            // which will handle the logout logic such as clearing the user session and navigating back to the login screen.
            LogoutCommand = new RelayCommand(ExecuteLogout); // Giving the LogoutCommand a meaning 
        }


        public void ExecuteLogout(object? obj)
        {
            // Code to handle logout logic, such as clearing user session and navigating to the login screen
            // Navigating back to the login screen by setting the CurrentView of the navigation view model to a new instance of the LoginVM
            _navigationVM.CurrentView = new LoginVM(_navigationVM);
        }

        public ObservableCollection<Job> GetPendingJobs()
        {
            // Going through the database to get all jobs status that are pending
            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;

            MessageBox.Show("Test 1");

            SqlConnection mySqlCon = new(myCon);
            // Opening the SQL connection
            mySqlCon.Open();

            try
            {
                // Creating the SQL command to check for user credential
                MessageBox.Show("Test 2");

                SqlCommand cmGetJobs = new SqlCommand();
                cmGetJobs.Connection = mySqlCon;
                cmGetJobs.CommandType = CommandType.Text;
                cmGetJobs.CommandText = "SELECT * FROM Jobs WHERE JobStatus=@Status";

                cmGetJobs.Parameters.AddWithValue("@Status", "Pending");

                SqlDataReader listJobs = cmGetJobs.ExecuteReader();

                MessageBox.Show("Test 3");


                // If a record is found, open the main application window
                if (listJobs.HasRows)
                {
                    MessageBox.Show("Test 4");
                    // Initializing the observable collection
                    PendingJobs = new ObservableCollection<Job>();

                    // Reading through each record found
                    while (listJobs.Read())
                    {
                        MessageBox.Show("Test 5");
                        // Initializing the observable collection
                        PendingJobs.Add(

                            //For each record found, add it to the observable collection
                            new Job
                            {
                                JobId = Convert.ToInt32(listJobs["JobId"]),
                                ClientId = Convert.ToInt32(listJobs["ClientId"]),
                                CourierId = Convert.ToInt32(listJobs["CourierId"]),
                                JobStatus = listJobs["JobStatus"].ToString(),
                            }
                         );
                    }

                    // Closing the data reader
                    listJobs.Close();

                    // Returning the observable collection of jobs with pending status
                    return PendingJobs;
                }
                else
                {

                    Console.WriteLine("Error (1)");

                    return PendingJobs;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while closing the connection: " + ex.Message);
                mySqlCon.Close();

                return PendingJobs;

            }

            finally
            {
                mySqlCon.Close();
            }


            return PendingJobs;


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

                    Console.WriteLine("Error (1)");

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

