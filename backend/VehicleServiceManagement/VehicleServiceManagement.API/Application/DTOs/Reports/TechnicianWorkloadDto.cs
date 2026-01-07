using System.Text.Json.Serialization;

namespace VehicleServiceManagement.API.Application.DTOs.Reports
{
    public class TechnicianWorkloadDto
    {
        public string TechnicianId { get; set; } = string.Empty;
        public string TechnicianName { get; set; } = string.Empty;
        
        [JsonPropertyName("activeTasks")]
        public int ActiveTasksCount { get; set; }
        
        public List<string> CurrentVehicleModels { get; set; } = new();
    }
}