using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record GetTechnicianTasksQuery(string TechnicianId) : IRequest<List<ServiceRequestDto>>;

    public class GetTechnicianTasksHandler : IRequestHandler<GetTechnicianTasksQuery, List<ServiceRequestDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTechnicianTasksHandler(IApplicationDbContext context) => _context = context;

        public async Task<List<ServiceRequestDto>> Handle(GetTechnicianTasksQuery request, CancellationToken cancellationToken)
        {
            return await _context.ServiceRequests
                .Include(s => s.Vehicle)
                .Where(s => s.TechnicianId == request.TechnicianId &&
                           (s.Status == "Assigned" || s.Status == "In Progress"))
                .OrderByDescending(s => s.Priority == "Urgent")
                .ThenByDescending(s => s.Priority == "High")
                .ThenByDescending(s => s.RequestedDate)
                .Select(s => new ServiceRequestDto
                {
                    Id = s.Id,
                    Description = s.Description ?? string.Empty,
                    Status = s.Status ?? "Assigned",
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