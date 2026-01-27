using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BayWyn_Couriers.Models.User;

namespace BayWyn_Couriers.Models
{
    internal class Client : User
    {
        // Additional properties specific to Client can be added here in the future
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }

       
    }
}
