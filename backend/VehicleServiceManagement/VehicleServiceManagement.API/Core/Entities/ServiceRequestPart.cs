namespace VehicleServiceManagement.API.Core.Entities
{
    public class ServiceRequestPart
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public ServiceRequest? ServiceRequest { get; set; }
        public int PartId { get; set; }
        public Part? Part { get; set; }

        public int QuantityUsed { get; set; }
    }
}