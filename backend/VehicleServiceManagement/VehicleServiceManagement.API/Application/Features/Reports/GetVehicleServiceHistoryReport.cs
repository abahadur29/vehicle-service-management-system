using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Reports;

namespace VehicleServiceManagement.API.Application.Features.Reports
{
    public record GetVehicleServiceHistoryReportQuery() : IRequest<List<VehicleServiceHistoryReportDto>>;

    public class GetVehicleServiceHistoryReportHandler : IRequestHandler<GetVehicleServiceHistoryReportQuery, List<VehicleServiceHistoryReportDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetVehicleServiceHistoryReportHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<VehicleServiceHistoryReportDto>> Handle(GetVehicleServiceHistoryReportQuery request, CancellationToken ct)
        {
            var report = await _context.ServiceRequests
                .Include(sr => sr.Vehicle)
                .Where(sr => sr.Status == "Completed" || sr.Status == "Closed")
                .GroupBy(sr => new
                {
                    sr.VehicleId,
                    sr.Vehicle!.Make,
                    sr.Vehicle.Model,
                    sr.Vehicle.LicensePlate,
                    sr.Vehicle.Year
                })
                .Select(g => new VehicleServiceHistoryReportDto
                {
                    VehicleId = g.Key.VehicleId,
                    Make = g.Key.Make ?? "Unknown",
                    Model = g.Key.Model,
                    LicensePlate = g.Key.LicensePlate,
                    Year = g.Key.Year,
                    TotalServices = g.Count(),
                    LastServiceDate = g
                        .OrderByDescending(sr => sr.CompletionDate ?? sr.RequestedDate)
                        .Select(sr => sr.CompletionDate ?? sr.RequestedDate)
                        .FirstOrDefault(),
                    LastServiceStatus = g
                        .OrderByDescending(sr => sr.CompletionDate ?? sr.RequestedDate)
                        .Select(sr => sr.Status)
                        .FirstOrDefault() ?? "N/A"
                })
                .OrderByDescending(v => v.LastServiceDate)
                .ToListAsync(ct);

            return report;
        }
    }
}

