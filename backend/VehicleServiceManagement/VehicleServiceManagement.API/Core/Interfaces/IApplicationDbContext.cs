using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;

namespace VehicleServiceManagement.API.Core.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Vehicle> Vehicles { get; set; }
        DbSet<ServiceRequest> ServiceRequests { get; set; }
        DbSet<Part> Parts { get; set; }
        DbSet<Invoice> Invoices { get; set; }
        DbSet<ServiceRequestPart> ServiceRequestParts { get; set; }
        DbSet<ServiceChangeHistory> ServiceChangeHistories { get; set; }
        DbSet<ServiceCategory> ServiceCategories { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}