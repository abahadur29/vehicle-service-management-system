using System.Text.Json.Serialization;

namespace VehicleServiceManagement.API.Application.DTOs
{
    public class PartDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("price")]
        public decimal? UnitPrice { get; set; }
        
        public int StockQuantity { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
