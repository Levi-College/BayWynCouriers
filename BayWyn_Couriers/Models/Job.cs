using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BayWyn_Couriers.Models
{
    internal class Job
    {
        // Setting properties as to ensure they must be provided during object initialization
        public int JobId { get; set; }
        public int ClientId { get; set; }
        public int CourierId { get; set; }
        public string DropOffAddress { get; set; }

        // To hold when the job was created and completed (delivery)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Description { get; set; }
        public float Price { get; set; }
    }
}
