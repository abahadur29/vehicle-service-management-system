using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record GetTechnicianServiceHistoryQuery(string TechnicianId) : IRequest<List<ServiceRequestDto>>;

    public class GetTechnicianServiceHistoryHandler : IRequestHandler<GetTechnicianServiceHistoryQuery, List<ServiceRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTechnicianServiceHistoryHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<ServiceRequestDto>> Handle(GetTechnicianServiceHistoryQuery request, CancellationToken cancellationToken)
        {
            return await _context.ServiceRequests
                .Include(s => s.Vehicle)
                .Where(s => s.TechnicianId == request.TechnicianId &&
                           (s.Status == "Completed" || s.Status == "Closed"))
                .OrderByDescending(s => s.CompletionDate ?? s.RequestedDate)
                .Select(s => new ServiceRequestDto
                {
                    Id = s.Id,
                    Description = s.Description ?? string.Empty,
                    Status = s.Status ?? "Completed",
                    Priority = s.Priority ?? "Normal",
                    RequestedDate = s.RequestedDate,
                    CompletionDate = s.CompletionDate,
                    VehicleId = s.VehicleId,
                    VehicleModel = s.Vehicle != null ? $"{s.Vehicle.Make} {s.Vehicle.Model}" : "Unknown Vehicle"
                })
                .ToListAsync(cancellationToken);
        }
    }
}

