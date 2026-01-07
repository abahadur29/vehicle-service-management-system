namespace VehicleServiceManagement.API.Application.DTOs.Services
{
    public class PartUsageDetailDto
    {
        public string PartName { get; set; } = string.Empty; 
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
    }
}
