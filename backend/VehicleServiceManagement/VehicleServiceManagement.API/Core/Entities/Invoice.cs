using System;

namespace VehicleServiceManagement.API.Core.Entities
{
    public class Invoice
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        public ServiceRequest? ServiceRequest { get; set; }
    }
}