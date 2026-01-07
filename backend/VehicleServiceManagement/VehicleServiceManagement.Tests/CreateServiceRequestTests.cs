using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Services
{
    public class CreateServiceRequestTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Handle_ShouldCreateServiceRequest_WithRequestedStatus()
        {
            // Arrange
            var options = CreateNewContextOptions("CreateServiceRequestDb");
            using var context = new ApplicationDbContext(options);

            var vehicle = new Vehicle
            {
                Id = 1,
                LicensePlate = "ABC-123",
                Model = "Camry",
                Make = "Toyota",
                Year = 2020,
                UserId = "user-123"
            };

            var category = new ServiceCategory
            {
                Id = 1,
                Name = "Oil Change",
                BasePrice = 500
            };

            context.Vehicles.Add(vehicle);
            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();

            var handler = new CreateServiceRequestHandler(context);
            var command = new CreateServiceRequestCommand(
                1, // VehicleId
                1, // ServiceCategoryId
                "Regular oil change service", // Description
                "Normal", // Priority
                DateTime.UtcNow.AddDays(1), // RequestedDate
                "user-123" // UserId
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be("Requested");
            result.Description.Should().Be("Regular oil change service");
            result.Priority.Should().Be("Normal");

            var savedRequest = await context.ServiceRequests.FindAsync(result.Id);
            savedRequest.Should().NotBeNull();
            savedRequest!.Status.Should().Be("Requested");
            savedRequest.VehicleId.Should().Be(1);
            savedRequest.ServiceCategoryId.Should().Be(1);
            savedRequest.CustomerId.Should().Be("user-123");
        }

        [Fact]
        public async Task Handle_ShouldSetDefaultPriority_WhenPriorityIsEmpty()
        {
            // Arrange
            var options = CreateNewContextOptions("CreateServiceRequestDefaultPriorityDb");
            using var context = new ApplicationDbContext(options);

            var vehicle = new Vehicle { Id = 1, LicensePlate = "XYZ-789", UserId = "user-123" };
            var category = new ServiceCategory { Id = 1, Name = "Maintenance", BasePrice = 300 };

            context.Vehicles.Add(vehicle);
            context.ServiceCategories.Add(category);
            await context.SaveChangesAsync();

            var handler = new CreateServiceRequestHandler(context);
            var command = new CreateServiceRequestCommand(
                1, // VehicleId
                1, // ServiceCategoryId
                "Maintenance check", // Description
                string.Empty, // Priority (empty)
                DateTime.UtcNow, // RequestedDate
                "user-123" // UserId
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Priority.Should().Be("Normal"); // Handler defaults empty priority to "Normal"
            var savedRequest = await context.ServiceRequests.FindAsync(result.Id);
            savedRequest!.Priority.Should().Be("Normal");
        }
    }
}

