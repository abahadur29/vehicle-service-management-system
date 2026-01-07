using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Services
{
    public class GetMyBookingsTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Handle_ShouldReturnOnlyUserBookings()
        {
            // Arrange
            var options = CreateNewContextOptions("GetMyBookingsDb");
            using var context = new ApplicationDbContext(options);

            var user1Vehicle = new Vehicle { Id = 1, LicensePlate = "ABC-123", UserId = "user-1" };
            var user2Vehicle = new Vehicle { Id = 2, LicensePlate = "XYZ-789", UserId = "user-2" };

            var service1 = new ServiceRequest 
            { 
                Id = 1, 
                VehicleId = 1, 
                Status = "Requested", 
                Description = "Oil Change",
                RequestedDate = DateTime.UtcNow
            };
            var service2 = new ServiceRequest 
            { 
                Id = 2, 
                VehicleId = 2, 
                Status = "In Progress", 
                Description = "Brake Repair",
                RequestedDate = DateTime.UtcNow
            };

            context.Vehicles.AddRange(user1Vehicle, user2Vehicle);
            context.ServiceRequests.AddRange(service1, service2);
            await context.SaveChangesAsync();

            var handler = new GetMyBookingsHandler(context);

            // Act
            var result = await handler.Handle(new GetMyBookingsQuery("user-1"), CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(1);
            result.First().Description.Should().Be("Oil Change");
        }

        [Fact]
        public async Task Handle_ShouldOrderByDateDescending()
        {
            // Arrange
            var options = CreateNewContextOptions("GetMyBookingsOrderedDb");
            using var context = new ApplicationDbContext(options);

            var vehicle = new Vehicle { Id = 1, LicensePlate = "ABC-123", UserId = "user-1" };
            var service1 = new ServiceRequest 
            { 
                Id = 1, 
                VehicleId = 1, 
                Status = "Requested", 
                RequestedDate = DateTime.UtcNow.AddDays(-5) // Older
            };
            var service2 = new ServiceRequest 
            { 
                Id = 2, 
                VehicleId = 1, 
                Status = "Requested", 
                RequestedDate = DateTime.UtcNow // Newer
            };

            context.Vehicles.Add(vehicle);
            context.ServiceRequests.AddRange(service1, service2);
            await context.SaveChangesAsync();

            var handler = new GetMyBookingsHandler(context);

            // Act
            var result = await handler.Handle(new GetMyBookingsQuery("user-1"), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.First().Id.Should().Be(2); // Newest first
            result.Last().Id.Should().Be(1); // Oldest last
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoBookings()
        {
            // Arrange
            var options = CreateNewContextOptions("GetMyBookingsEmptyDb");
            using var context = new ApplicationDbContext(options);

            var handler = new GetMyBookingsHandler(context);

            // Act
            var result = await handler.Handle(new GetMyBookingsQuery("user-with-no-bookings"), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }
    }
}

