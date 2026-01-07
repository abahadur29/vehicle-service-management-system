using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace VehicleServiceManagement.API.Core.Entities
{
    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Requested";

        [Required]
        public string Priority { get; set; } = "Normal";

        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletionDate { get; set; }

        [Required]
        public int ServiceCategoryId { get; set; }
        public virtual ServiceCategory? ServiceCategory { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        public int VehicleId { get; set; }
        public virtual Vehicle? Vehicle { get; set; }

        public string? TechnicianId { get; set; }
        public virtual ApplicationUser? Technician { get; set; }

        public ICollection<ServiceRequestPart> UsedParts { get; set; } = new List<ServiceRequestPart>();
    }
}