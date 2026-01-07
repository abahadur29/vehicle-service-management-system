using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Services
{
    public class CancelServiceTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task CancelBookingHandler_ShouldReturnTrue_WhenServiceIsInRequestedStatus()
        {
            // Arrange
            var options = CreateNewContextOptions("CancelServiceDb_Success");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                Status = "Requested",
                Description = "Oil Change",
                RequestedDate = DateTime.UtcNow.AddDays(1)
            };

            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new ServiceOperationsHandler(context);

            // Act
            var result = await handler.Handle(new CancelBookingCommand(1), CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            var cancelledService = await context.ServiceRequests.FindAsync(1);
            cancelledService.Should().NotBeNull(); // Service should still exist (preserve history)
            cancelledService!.Status.Should().Be("Cancelled"); // Status should be updated to Cancelled
        }

        [Fact]
        public async Task CancelBookingHandler_ShouldReturnFalse_WhenServiceIsInProgress()
        {
            // Arrange
            var options = CreateNewContextOptions("CancelServiceDb_Failure");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 2,
                Status = "In Progress",
                Description = "Brake Repair",
                RequestedDate = DateTime.UtcNow
            };

            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new ServiceOperationsHandler(context);

            // Act
            var result = await handler.Handle(new CancelBookingCommand(2), CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            var service = await context.ServiceRequests.FindAsync(2);
            service.Should().NotBeNull(); // Service should still exist
            service!.Status.Should().Be("In Progress"); // Status unchanged
        }

        [Fact]
        public async Task CancelBookingHandler_ShouldReturnFalse_WhenServiceNotFound()
        {
            // Arrange
            var options = CreateNewContextOptions("CancelServiceDb_NotFound");
            using var context = new ApplicationDbContext(options);

            var handler = new ServiceOperationsHandler(context);

            // Act
            var result = await handler.Handle(new CancelBookingCommand(999), CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
}

