namespace VehicleServiceManagement.API.Application.DTOs.Reports
{
    public class TechPerformanceDto
    {
        public string TechnicianName { get; set; } = string.Empty;
        public decimal TotalRevenueGenerated { get; set; }
        public int JobsCompleted { get; set; }
    }
}