namespace VehicleServiceManagement.API.Application.DTOs.Reports
{
    public class PendingVsCompletedDto
    {
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public List<StatusCountDto> StatusBreakdown { get; set; } = new();
    }

    public class StatusCountDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}

