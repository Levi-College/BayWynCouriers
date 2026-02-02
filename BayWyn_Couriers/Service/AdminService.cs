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

namespace BayWyn_Couriers.Service
{
    public class AdminService
    {    

        // Method to return the jobs in an observable collection
        public ObservableCollection<Job> PendingJobs { get; set; }

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
                       
                    
                    listJobs.Close();


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

    }
}

