namespace VehicleServiceManagement.API.Application.DTOs.Services
{
    public class CreateServiceRequestDto
    {
        public int VehicleId { get; set; }

        public int ServiceCategoryId { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium";

        public DateTime RequestedDate { get; set; }
    }
}