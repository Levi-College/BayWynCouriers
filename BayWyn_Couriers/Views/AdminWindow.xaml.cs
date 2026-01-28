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
        JobsPage jobsPage { get; set; }

        public AdminWindow(int userID)
        {
            InitializeComponent();
            // Initializing the pages
            jobsPage = new JobsPage();
            // Setting the default page to JobsPage
            MainFrame.Content = jobsPage;
        }
    }
}
