using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<ServiceChangeHistory> ServiceChangeHistories { get; set; }
        public DbSet<ServiceRequestPart> ServiceRequestParts { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Part>()
                .Property(p => p.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ServiceCategory>()
                .Property(s => s.BasePrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ServiceRequestPart>()
                .HasOne(srp => srp.ServiceRequest)
                .WithMany(sr => sr.UsedParts)
                .HasForeignKey(srp => srp.ServiceRequestId);

            builder.Entity<ServiceRequest>()
                .HasOne(sr => sr.ServiceCategory)
                .WithMany()
                .HasForeignKey(sr => sr.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceRequest>()
                .HasIndex(s => new { s.VehicleId, s.Status });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}