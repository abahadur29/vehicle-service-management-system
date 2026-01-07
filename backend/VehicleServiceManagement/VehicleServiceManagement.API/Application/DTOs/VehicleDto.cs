namespace VehicleServiceManagement.API.Application.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public int Year { get; set; }
        public string OwnerFullName { get; set; } = string.Empty; 
    }
}