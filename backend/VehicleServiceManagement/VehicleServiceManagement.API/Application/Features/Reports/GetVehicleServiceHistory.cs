using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Vehicles;

namespace VehicleServiceManagement.API.Application.Features.Reports
{
    public record GetVehicleServiceHistoryQuery(int VehicleId, string? UserId = null, string? UserRole = null) : IRequest<List<VehicleHistoryDto>>;

    public class GetVehicleServiceHistoryHandler : IRequestHandler<GetVehicleServiceHistoryQuery, List<VehicleHistoryDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetVehicleServiceHistoryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleHistoryDto>> Handle(GetVehicleServiceHistoryQuery request, CancellationToken ct)
        {
            var baseQuery = _context.ServiceRequests
                .Where(s => s.VehicleId == request.VehicleId &&
                           (s.Status == "Completed" || s.Status == "Closed"));

            if (!string.IsNullOrEmpty(request.UserRole) && !string.IsNullOrEmpty(request.UserId))
            {
                var role = request.UserRole.ToLowerInvariant();
                
                if (role == "customer")
                {
                    baseQuery = baseQuery
                        .Where(s => _context.Vehicles
                            .Any(v => v.Id == s.VehicleId && v.UserId == request.UserId));
                }
                else if (role == "technician")
                {
                    baseQuery = baseQuery.Where(s => s.TechnicianId == request.UserId);
                }
            }

            var query = baseQuery
                .Include(s => s.ServiceCategory)
                .Include(s => s.Vehicle)
                .Include(s => s.UsedParts)
                    .ThenInclude(up => up.Part);

            var history = await query
                .OrderByDescending(s => s.CompletionDate ?? s.RequestedDate)
                .Select(s => new VehicleHistoryDto
                {
                    ServiceId = s.Id,
                    ServiceName = s.ServiceCategory != null ? s.ServiceCategory.Name : "General Service",
                    CompletionDate = s.CompletionDate ?? s.RequestedDate,
                    Description = s.Description ?? "No description provided",
                    PartsReplaced = s.UsedParts.Select(up => up.Part != null ? up.Part.Name : "Unknown Part").ToList(),
                    TotalCost = _context.Invoices
                        .Where(i => i.ServiceRequestId == s.Id)
                        .Select(i => i.TotalAmount)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            return history;
        }
    }
}