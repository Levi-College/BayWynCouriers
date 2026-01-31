using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using BayWyn_Couriers.Models;
using System.Windows;

namespace BayWyn_Couriers.Service
{
    internal class LoginService
    {
        // To hold the current user information
        public int UserId;
        public string UserName;
        public string Role;

        public bool checkLogin(string userName, string password) {

            // Getting the database connection string
            string myCon = ConfigurationManager.ConnectionStrings["BayWynCouriersDB"].ConnectionString;


            SqlConnection mySqlCon = new SqlConnection(myCon);
            // Opening the SQL connection
            mySqlCon.Open();

            try
            {
                // Creating the SQL command to check for user credential
                string sqlQuery = "SELECT COUNT(1) FROM Users WHERE UserName=@UserName AND LoginPassword=@LoginPassword";
  

                SqlCommand cmLogin = new SqlCommand();
                cmLogin.Connection = mySqlCon;
                cmLogin.CommandType = CommandType.Text;
                cmLogin.CommandText = "SELECT * FROM Users WHERE Username=@UserName AND LoginPassword=@LoginPassword";
                cmLogin.Parameters.AddWithValue("@UserName", userName);
                cmLogin.Parameters.AddWithValue("@LoginPassword", password);

                SqlDataReader loginCheck = cmLogin.ExecuteReader();


                // If a record is found, open the main application window
                if (loginCheck.HasRows)
                {

                    MessageBox.Show("Login successful!");

                    loginCheck.Read();

                    MessageBox.Show(loginCheck[0].ToString());

                    UserId = Convert.ToInt32(loginCheck["UserId"]);
                    //UserName = loginCheck"Username"].ToString();
                    Role = loginCheck["UserRole"].ToString();
                    loginCheck.Close();
               

                    return true;
                }
                else
                {
                    Console.WriteLine("Invalid username or password.");
                }
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
            return false;


        }
            
        }
}
