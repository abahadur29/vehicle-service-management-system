using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Reports;

namespace VehicleServiceManagement.API.Application.Features.Reports
{
    public record GetPendingVsCompletedQuery() : IRequest<PendingVsCompletedDto>;

    public class GetPendingVsCompletedHandler : IRequestHandler<GetPendingVsCompletedQuery, PendingVsCompletedDto>
    {
        private readonly IApplicationDbContext _context;

        public GetPendingVsCompletedHandler(IApplicationDbContext context) => _context = context;

        public async Task<PendingVsCompletedDto> Handle(GetPendingVsCompletedQuery request, CancellationToken ct)
        {
            var pendingCount = await _context.ServiceRequests
                .Where(s => s.Status == "Requested" || s.Status == "Assigned" || s.Status == "In Progress")
                .CountAsync(ct);

            var completedCount = await _context.ServiceRequests
                .Where(s => s.Status == "Completed" || s.Status == "Closed")
                .CountAsync(ct);
            var statusBreakdown = await _context.ServiceRequests
                .GroupBy(s => s.Status)
                .Select(g => new StatusCountDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(ct);

            return new PendingVsCompletedDto
            {
                PendingCount = pendingCount,
                CompletedCount = completedCount,
                StatusBreakdown = statusBreakdown
            };
        }
    }
}

