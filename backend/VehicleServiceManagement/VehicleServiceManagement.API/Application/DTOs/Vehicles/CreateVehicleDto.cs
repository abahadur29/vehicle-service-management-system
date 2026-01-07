namespace VehicleServiceManagement.API.Application.DTOs.Vehicles
{
    public class CreateVehicleDto
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}