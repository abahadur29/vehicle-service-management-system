namespace VehicleServiceManagement.API.Application.DTOs.Admin
{
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalServicesCompleted { get; set; }
        public List<InvoiceSummaryDto> RecentInvoices { get; set; } = new();
    }

    public class InvoiceSummaryDto
    {
        public int InvoiceId { get; set; }
        public string CustomerVehicle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}