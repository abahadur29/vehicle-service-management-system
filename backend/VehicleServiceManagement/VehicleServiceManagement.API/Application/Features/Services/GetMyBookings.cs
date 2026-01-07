using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record GetMyBookingsQuery(string UserId) : IRequest<List<ServiceRequestDto>>;

    public class GetMyBookingsHandler : IRequestHandler<GetMyBookingsQuery, List<ServiceRequestDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetMyBookingsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceRequestDto>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
        {
            return await _context.ServiceRequests
                .Include(s => s.Vehicle)
                .Include(s => s.Technician)
                .Where(s => s.Vehicle != null && s.Vehicle.UserId == request.UserId)
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
                    TechnicianName = s.Technician != null ? s.Technician.FullName : null
                })
                .ToListAsync(cancellationToken);
        }
    }
}