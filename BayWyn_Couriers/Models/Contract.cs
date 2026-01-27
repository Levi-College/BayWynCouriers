using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayWyn_Couriers.Models
{
    internal class Contract
    {
        // Setting contract properties to ensure they must be provided during object initialization
        public int ContractId { get; set; }
        public int ClientId { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal CostPerJob { get; set; }

    }
}
