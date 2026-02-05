using BayWyn_Couriers.Models;
using BayWyn_Couriers.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace BayWyn_Couriers.Pages
{
    /// <summary>
    /// Interaction logic for JobsPage.xaml
    /// </summary>
    public partial class JobsPage : Page
    {

        // Observable collection to hold the pending jobs
        public ObservableCollection<Job> PendingJobs { get; set; }

        // Declaring the admin service
        AdminVM adService = new AdminVM();

        public JobsPage()
        {
            InitializeComponent();


            PendingJobs = adService.GetPendingJobs();
            MessageBox.Show("Number of pending jobs: " + PendingJobs.Count);

            // Giving the data context for the data binding
            this.DataContext = this;

        }
    }
}
