namespace VehicleServiceManagement.API.Application.DTOs.Vehicles
{
    public class VehicleHistoryDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime CompletionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> PartsReplaced { get; set; } = new();
        public decimal TotalCost { get; set; }
    }
}
