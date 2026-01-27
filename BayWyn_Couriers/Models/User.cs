using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayWyn_Couriers.Models
{
    internal class User
    {
        // Setting properties as to ensure they must be provided during object initialization
        
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        private string LoginPassword { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Role { get; set; } // e.g., "Client" or "Courier"

    }
}

    
