namespace VehicleServiceManagement.API.Application.DTOs.Reports
{
    public class VehicleServiceHistoryReportDto
    {
        public int VehicleId { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public int Year { get; set; }
        public int TotalServices { get; set; }
        public DateTime? LastServiceDate { get; set; }
        public string LastServiceStatus { get; set; } = string.Empty;
    }
}

