namespace VehicleServiceManagement.API.Core.Entities
{
    public class ServiceCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; } 
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true; // Hide category without deleting
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Used for sorting
    }
}