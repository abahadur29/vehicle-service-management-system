using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Reports;

namespace VehicleServiceManagement.API.Application.Features.Reports
{
    public record GetTechPerformanceQuery() : IRequest<List<TechPerformanceDto>>;

    public class GetTechPerformanceHandler : IRequestHandler<GetTechPerformanceQuery, List<TechPerformanceDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTechPerformanceHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<TechPerformanceDto>> Handle(GetTechPerformanceQuery request, CancellationToken ct)
        {
            // Get paid invoices for completed services, group by technician
            var performance = await _context.Invoices
                .Include(i => i.ServiceRequest)
                    .ThenInclude(sr => sr!.Technician)
                .Where(i => i.PaymentStatus == "Paid" 
                    && i.ServiceRequest != null 
                    && i.ServiceRequest.TechnicianId != null
                    && i.ServiceRequest.Technician != null
                    && (i.ServiceRequest.Status == "Completed" || i.ServiceRequest.Status == "Closed"))
                .GroupBy(i => new { 
                    i.ServiceRequest!.TechnicianId, 
                    i.ServiceRequest.Technician!.FullName 
                })
                .Select(g => new TechPerformanceDto
                {
                    TechnicianName = g.Key.FullName,
                    TotalRevenueGenerated = g.Sum(x => x.TotalAmount),
                    JobsCompleted = g.Count()
                })
                .OrderByDescending(res => res.TotalRevenueGenerated)
                .ToListAsync(ct);

            return performance;
        }
    }
}