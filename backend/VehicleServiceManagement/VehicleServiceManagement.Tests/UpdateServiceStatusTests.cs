using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Services
{
    public class UpdateServiceStatusTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Handle_ShouldUpdateStatus_WhenValid()
        {
            // Arrange
            var options = CreateNewContextOptions("UpdateStatusDb_Success");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                Status = "Requested",
                Description = "Oil Change"
            };

            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new UpdateServiceStatusHandler(context);
            var command = new UpdateServiceStatusCommand(1, "Assigned", null, null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            var updatedService = await context.ServiceRequests.FindAsync(1);
            updatedService!.Status.Should().Be("Assigned");
        }

        [Fact]
        public async Task Handle_ShouldAssignTechnician_WhenTechnicianIdProvided()
        {
            // Arrange
            var options = CreateNewContextOptions("UpdateStatusAssignTechDb");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 2,
                Status = "Requested",
                TechnicianId = null
            };

            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new UpdateServiceStatusHandler(context);
            var command = new UpdateServiceStatusCommand(2, "Assigned", "tech-123", null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            var updatedService = await context.ServiceRequests.FindAsync(2);
            updatedService!.Status.Should().Be("Assigned");
            updatedService.TechnicianId.Should().Be("tech-123");
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenServiceNotFound()
        {
            // Arrange
            var options = CreateNewContextOptions("UpdateStatusNotFoundDb");
            using var context = new ApplicationDbContext(options);

            var handler = new UpdateServiceStatusHandler(context);
            var command = new UpdateServiceStatusCommand(999, "Assigned", null, null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
}

