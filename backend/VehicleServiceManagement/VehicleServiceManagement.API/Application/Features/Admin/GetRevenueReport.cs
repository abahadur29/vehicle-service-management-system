using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Admin;

namespace VehicleServiceManagement.API.Application.Features.Admin
{
    public record GetRevenueReportQuery() : IRequest<RevenueReportDto>;

    public class GetRevenueReportHandler : IRequestHandler<GetRevenueReportQuery, RevenueReportDto>
    {
        private readonly IApplicationDbContext _context;
        public GetRevenueReportHandler(IApplicationDbContext context) => _context = context;

        public async Task<RevenueReportDto> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
        {
            // Sum all paid invoices
            var totalRevenue = await _context.Invoices
                .Where(i => i.PaymentStatus == "Paid")
                .SumAsync(i => i.TotalAmount, cancellationToken);
            // Count completed services
            var totalCompleted = await _context.Invoices
                .CountAsync(i => i.ServiceRequest != null &&
                               (i.ServiceRequest.Status == "Completed" || i.ServiceRequest.Status == "Closed"),
                               cancellationToken);
            var recentInvoices = await _context.Invoices
                .Include(i => i.ServiceRequest)
                .ThenInclude(s => s!.Vehicle)
                .OrderByDescending(i => i.IssuedDate)
                .Take(10)
                .Select(i => new InvoiceSummaryDto
                {
                    InvoiceId = i.Id,
                    CustomerVehicle = i.ServiceRequest != null && i.ServiceRequest.Vehicle != null
                        ? i.ServiceRequest.Vehicle.Make + " " + i.ServiceRequest.Vehicle.Model
                        : "Unknown Vehicle",
                    Amount = i.TotalAmount,
                    Date = i.IssuedDate,
                    Status = i.PaymentStatus
                })
                .ToListAsync(cancellationToken);

            return new RevenueReportDto
            {
                TotalRevenue = totalRevenue,
                TotalServicesCompleted = totalCompleted,
                RecentInvoices = recentInvoices
            };
        }
    }
}