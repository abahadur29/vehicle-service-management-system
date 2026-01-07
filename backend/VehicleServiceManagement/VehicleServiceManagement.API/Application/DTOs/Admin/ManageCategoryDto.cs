namespace VehicleServiceManagement.API.Application.DTOs.Admin
{
    public class ManageCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
