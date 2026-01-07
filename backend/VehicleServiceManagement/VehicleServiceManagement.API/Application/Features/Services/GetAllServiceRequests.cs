using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record GetAllServiceRequestsQuery() : IRequest<List<ServiceRequestDto>>;

    public class GetAllServiceRequestsHandler : IRequestHandler<GetAllServiceRequestsQuery, List<ServiceRequestDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllServiceRequestsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceRequestDto>> Handle(GetAllServiceRequestsQuery request, CancellationToken cancellationToken)
        {
            return await _context.ServiceRequests
                .Include(s => s.Vehicle)
                .Include(s => s.Technician)
                .OrderByDescending(s => s.RequestedDate)
                .ThenByDescending(s => s.Id)
                .Select(s => new ServiceRequestDto
                {
                    Id = s.Id,
                    Description = s.Description,
                    Status = s.Status,
                    Priority = s.Priority,
                    RequestedDate = s.RequestedDate,
                    VehicleId = s.VehicleId,
                    VehicleModel = s.Vehicle != null ? $"{s.Vehicle.Make} {s.Vehicle.Model}" : "Unknown",
                    TechnicianName = s.Technician != null ? s.Technician.FullName : "Not Assigned"
                })
                .ToListAsync(cancellationToken);
        }
    }
}