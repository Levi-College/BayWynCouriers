using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BayWyn_Couriers.Models
{
    internal class Courier : User
    {
        // Additional properties specific to Courier can be added here in the future
        public string DriverLicenseNumber { get; set; }
        public string ShiftStatus { get; set; } // e.g., "Available", "On Delivery", "Off Duty"
    }
}
