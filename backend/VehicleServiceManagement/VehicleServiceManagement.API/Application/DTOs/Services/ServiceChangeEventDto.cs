namespace VehicleServiceManagement.API.Application.DTOs.Services
{
    public class ServiceChangeEventDto
    {
        public int ServiceRequestId { get; set; }
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, (string OldValue, string NewValue)> Changes { get; set; } = new();
        
        public List<string> TargetUserIds { get; set; } = new();
    }
}