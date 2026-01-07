using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Reports;

namespace VehicleServiceManagement.API.Application.Features.Reports
{
    public record GetTechnicianWorkloadQuery() : IRequest<List<TechnicianWorkloadDto>>;

    public class GetTechnicianWorkloadHandler : IRequestHandler<GetTechnicianWorkloadQuery, List<TechnicianWorkloadDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTechnicianWorkloadHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TechnicianWorkloadDto>> Handle(GetTechnicianWorkloadQuery request, CancellationToken ct)
        {
            var serviceRequests = await _context.ServiceRequests
                .Where(s => s.Status == "Assigned" || s.Status == "In Progress")
                .Include(s => s.Vehicle)
                .Include(s => s.Technician)
                .ToListAsync(ct);

            if (!serviceRequests.Any())
            {
                return new List<TechnicianWorkloadDto>();
            }

            var workload = serviceRequests
                .GroupBy(s => new { 
                    TechnicianId = s.TechnicianId ?? "Unassigned",
                    TechnicianName = s.Technician != null ? s.Technician.FullName : "Unassigned"
                })
                .Select(g => new TechnicianWorkloadDto
                {
                    TechnicianId = g.Key.TechnicianId,
                    TechnicianName = g.Key.TechnicianName,
                    ActiveTasksCount = g.Count(),
                    CurrentVehicleModels = g
                        .Where(sr => sr.Vehicle != null)
                        .Select(sr => sr.Vehicle!.Model)
                        .Distinct()
                        .DefaultIfEmpty("Unknown")
                        .ToList()
                })
                .OrderByDescending(res => res.ActiveTasksCount)
                .ToList();

            return workload;
        }
    }
}