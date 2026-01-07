using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Services
{
    public class RescheduleServiceTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Handle_ShouldRescheduleService_WhenStatusIsRequested()
        {
            // Arrange
            var options = CreateNewContextOptions("RescheduleServiceDb_Success");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                Status = "Requested",
                Description = "Oil Change",
                RequestedDate = DateTime.UtcNow.AddDays(1),
                CustomerId = "user-123"
            };

            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new RescheduleServiceRequestHandler(context);
            var newDate = DateTime.UtcNow.AddDays(3);
            var command = new RescheduleServiceRequestCommand(1, newDate, "user-123");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            var updatedRequest = await context.ServiceRequests.FindAsync(1);
            updatedRequest.Should().NotBeNull();
            updatedRequest!.RequestedDate.Should().BeCloseTo(newDate, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenStatusIsNotRequested()
        {
            // Arrange
            var options = CreateNewContextOptions("RescheduleServiceDb_Failure");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 2,
                Status = "In Progress",
                Description = "Brake Repair",
                RequestedDate = DateTime.UtcNow,
                CustomerId = "user-123"
            };

            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new RescheduleServiceRequestHandler(context);
            var command = new RescheduleServiceRequestCommand(2, DateTime.UtcNow.AddDays(5), "user-123");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            var unchangedRequest = await context.ServiceRequests.FindAsync(2);
            unchangedRequest!.RequestedDate.Should().NotBeCloseTo(DateTime.UtcNow.AddDays(5), TimeSpan.FromDays(1));
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenUserDoesNotOwnService()
        {
            // Arrange
            var options = CreateNewContextOptions("RescheduleServiceDb_Unauthorized");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 3,
                Status = "Requested",
                Description = "Service",
                RequestedDate = DateTime.UtcNow,
                CustomerId = "user-123" // Different user
            };

            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new RescheduleServiceRequestHandler(context);
            var command = new RescheduleServiceRequestCommand(3, DateTime.UtcNow.AddDays(2), "user-456"); // Different user

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }
    }
}

