using System.ComponentModel.DataAnnotations;

namespace VehicleServiceManagement.API.Core.Entities
{
    public class ServiceChangeHistory
    {
        [Key]
        public int Id { get; set; }

        public int ServiceRequestId { get; set; }

        // User who should be notified
        [Required]
        public string TargetUserId { get; set; } = string.Empty;

        // Action type like "Added", "Updated", "Status Changed"
        public string Action { get; set; } = string.Empty;

        // Message shown in notifications
        public string Message { get; set; } = string.Empty;

        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;

        public DateTime ChangedOn { get; set; } = DateTime.Now;
    }
}