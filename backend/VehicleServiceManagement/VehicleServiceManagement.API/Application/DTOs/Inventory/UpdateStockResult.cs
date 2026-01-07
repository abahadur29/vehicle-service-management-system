namespace VehicleServiceManagement.API.Application.DTOs.Inventory
{
    public class UpdateStockResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int NewQuantity { get; set; }
        public string PartName { get; set; } = string.Empty;
    }
}
