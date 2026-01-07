namespace VehicleServiceManagement.API.Application.DTOs
{
    public class ServiceRequestDto
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public DateTime RequestedDate { get; set; }

        public DateTime? CompletionDate { get; set; }
  
        public int VehicleId { get; set; }

        public string VehicleModel { get; set; } = string.Empty;

        public string? TechnicianName { get; set; }
    }
}