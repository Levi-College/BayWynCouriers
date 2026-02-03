using BayWyn_Couriers.Models;
using BayWyn_Couriers.Service;
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
    /// Interaction logic for ContractsPage.xaml
    /// </summary>
    public partial class ContractsPage : Page
    {

        // Observable collection to hold the list of contracts
        // Using the observable collection for data binding
        public ObservableCollection<Contract> ContractsList { get; set; }

        // Declaring the admin service
        AdminService adService = new AdminService();

        public ContractsPage()
        {
            InitializeComponent();


            ContractsList = adService.GetAllContracts();

            // Giving the data context for the data binding
            this.DataContext = this;

        }
    }
}
