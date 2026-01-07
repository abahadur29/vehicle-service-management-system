using System.Text.Json.Serialization;

namespace VehicleServiceManagement.API.Application.DTOs.Reports
{
    public class ServiceTypeRevenueDto
    {
        [JsonPropertyName("serviceType")]
        public string ServiceTypeName { get; set; } = string.Empty;
        
        [JsonPropertyName("revenue")]
        public decimal TotalRevenue { get; set; }
        
        [JsonPropertyName("serviceCount")]
        public int ServiceCount { get; set; }
    }
}