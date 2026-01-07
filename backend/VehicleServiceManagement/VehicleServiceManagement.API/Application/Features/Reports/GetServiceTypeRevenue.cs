using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Reports;

namespace VehicleServiceManagement.API.Application.Features.Reports
{
    public record GetServiceTypeRevenueQuery() : IRequest<List<ServiceTypeRevenueDto>>;

    public class GetServiceTypeRevenueHandler : IRequestHandler<GetServiceTypeRevenueQuery, List<ServiceTypeRevenueDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetServiceTypeRevenueHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceTypeRevenueDto>> Handle(GetServiceTypeRevenueQuery request, CancellationToken ct)
        {
            // Group paid invoices by service type and sum revenue
            var report = await _context.Invoices
                .Include(i => i.ServiceRequest)
                    .ThenInclude(sr => sr!.ServiceCategory)
                .Where(i => i.PaymentStatus == "Paid" &&
                            i.ServiceRequest != null &&
                            i.ServiceRequest.ServiceCategory != null)
                .GroupBy(i => i.ServiceRequest!.ServiceCategory!.Name)
                .Select(g => new ServiceTypeRevenueDto
                {
                    ServiceTypeName = g.Key,
                    TotalRevenue = g.Sum(i => i.TotalAmount),
                    ServiceCount = g.Count()
                })
                .OrderByDescending(res => res.TotalRevenue)
                .ToListAsync(ct);

            return report;
        }
    }
}   