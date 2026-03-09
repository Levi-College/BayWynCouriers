using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BayWyn_Couriers.Models.User;

namespace BayWyn_Couriers.Models
{
    public class Client
    {
        // Additional properties specific to Client can be added here in the future
        public int ClientId {  get; set; }
        public string Name {  get; set; }
        //public string CompanyName { get; set; }
        public string ClientAddress { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
       
    }
}
