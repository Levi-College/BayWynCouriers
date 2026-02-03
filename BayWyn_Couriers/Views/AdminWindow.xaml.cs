using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BayWyn_Couriers.Pages;

namespace BayWyn_Couriers.Views
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        // Creatings objects for the pages
        JobsPage jobsPage { get; set; } // Adding the jobs page to the admin window
        ContractsPage contractsPage { get; set; } // Adding the contracts page

        public AdminWindow(int userID)
        {
            InitializeComponent();
            // Initializing the pages
            jobsPage = new JobsPage();
            // Setting the default page to JobsPage
            MainFrame.Content = jobsPage;

       
        }

        private void btnContracts_Click(object sender, RoutedEventArgs e)
        {
            // Initializing the contracts page
            contractsPage = new ContractsPage();
            // Setting the page to ContractsPage when the button is clicked
            MainFrame.Content = contractsPage;
        }
    }
}
