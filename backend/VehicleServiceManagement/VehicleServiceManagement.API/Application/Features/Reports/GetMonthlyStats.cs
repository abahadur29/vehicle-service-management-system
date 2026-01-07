using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Reports;

namespace VehicleServiceManagement.API.Application.Features.Reports
{
    public record GetMonthlyStatsQuery() : IRequest<List<MonthlyRevenueDto>>;

    public class GetMonthlyStatsHandler : IRequestHandler<GetMonthlyStatsQuery, List<MonthlyRevenueDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetMonthlyStatsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MonthlyRevenueDto>> Handle(GetMonthlyStatsQuery request, CancellationToken ct)
        {
            // Group paid invoices by month and year, then sum revenue
            var stats = await _context.Invoices
                .Where(i => i.PaymentStatus == "Paid")
                .GroupBy(i => new { i.IssuedDate.Year, i.IssuedDate.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(i => i.TotalAmount),
                    ServiceCount = g.Count()
                })
                .OrderByDescending(res => res.Year)
                .ThenByDescending(res => res.Month)
                .ToListAsync(ct);

            return stats;
        }
    }
}