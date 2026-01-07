using VehicleServiceManagement.API.Application.DTOs.Services;
using VehicleServiceManagement.API.Application.Features.Invoices;

namespace VehicleServiceManagement.API.Application.DTOs
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int ServiceRequestId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal LabourFee { get; set; }
        public DateTime IssuedDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public List<PartUsageDetailDto> PartsUsed { get; set; } = new();
    }
}
