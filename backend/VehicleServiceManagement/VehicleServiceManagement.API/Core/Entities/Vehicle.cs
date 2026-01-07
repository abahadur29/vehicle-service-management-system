using System.ComponentModel.DataAnnotations;

namespace VehicleServiceManagement.API.Core.Entities
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        public string Make { get; set; } = string.Empty;

        public int Year { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }
    }
}