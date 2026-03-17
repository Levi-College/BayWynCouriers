namespace BayWyn_Couriers.Models
{
    public class JobAssignment : Job
    {
        // To hold the additional details of the assigned job
        public int AssignmentID { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliverySlot { get; set; }
    }
}
