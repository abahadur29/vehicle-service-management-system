using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs;

namespace VehicleServiceManagement.API.Application.Features.Reports
{
    public record GetByPriorityQuery(string Priority) : IRequest<List<ServiceRequestDto>>;

    public class GetByPriorityHandler
        : IRequestHandler<GetByPriorityQuery, List<ServiceRequestDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetByPriorityHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<ServiceRequestDto>> Handle(
            GetByPriorityQuery request,
            CancellationToken ct)
        {
            var priority = request.Priority.Trim();

            var result = _context.ServiceRequests
                .AsNoTracking()
                .AsEnumerable()
                .Where(s =>
                    !string.IsNullOrWhiteSpace(s.Priority) &&
                    s.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.RequestedDate)
                .Select(s => new ServiceRequestDto
                {
                    Id = s.Id,
                    VehicleModel = s.Vehicle != null ? s.Vehicle.Model : "Unknown",
                    Description = s.Description ?? string.Empty,
                    Status = s.Status ?? "Requested",
                    Priority = s.Priority ?? "Normal",
                    RequestedDate = s.RequestedDate
                })
                .ToList();

            return Task.FromResult(result);
        }
    }
}
