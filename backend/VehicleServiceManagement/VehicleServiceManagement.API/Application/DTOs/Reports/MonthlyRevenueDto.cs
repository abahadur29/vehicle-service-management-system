namespace VehicleServiceManagement.API.Application.DTOs.Reports
{
    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int ServiceCount { get; set; }
    }
}