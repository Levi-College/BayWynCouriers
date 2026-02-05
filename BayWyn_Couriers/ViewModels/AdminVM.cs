using BayWyn_Couriers.Models;
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

namespace BayWyn_Couriers.ViewModels
{
    public class AdminVM
    {

        public ObservableCollection<Job> PendingJobs { get; set; } // Observable collection to hold the pending jobs
        public ObservableCollection<Contract> ContractsList { get; set; } // Observable collection to hold the list of contracts

        // Constructor
        public AdminVM() { }


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

