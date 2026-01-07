namespace VehicleServiceManagement.API.Application.DTOs.Inventory
{
    public class CreatePartDto
    {
        public string Name { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
