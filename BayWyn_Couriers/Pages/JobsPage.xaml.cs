using BayWyn_Couriers.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BayWyn_Couriers.Pages
{
    /// <summary>
    /// Interaction logic for JobsPage.xaml
    /// </summary>
    public partial class JobsPage : Page
    {
        public JobsPage()
        {
            InitializeComponent();

            List<User> users = new List<User>();
            users.Add(new User() { UserId = 1, UserName = "John Doe", Role = "Client" });
            users.Add(new User() { UserId = 2, UserName = "John Doe", Role = "Client" });
            users.Add(new User() { UserId = 3, UserName = "John Doe", Role = "Client" });

            jobsList.ItemsSource = users;

        }
    }
}
