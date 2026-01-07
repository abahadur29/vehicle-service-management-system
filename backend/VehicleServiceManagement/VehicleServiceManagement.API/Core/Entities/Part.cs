namespace VehicleServiceManagement.API.Core.Entities
{
    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}