using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Invoices;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Invoices
{
    public class GetMyInvoicesTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Handle_ShouldReturnOnlyUserInvoices()
        {
            // Arrange
            var options = CreateNewContextOptions("GetMyInvoicesDb");
            using var context = new ApplicationDbContext(options);

            var category = new ServiceCategory { Id = 1, Name = "Test Category", BasePrice = 500 };
            var user1Vehicle = new Vehicle { Id = 1, LicensePlate = "ABC-123", UserId = "user-1" };
            var user2Vehicle = new Vehicle { Id = 2, LicensePlate = "XYZ-789", UserId = "user-2" };

            var service1 = new ServiceRequest { Id = 1, VehicleId = 1, Status = "Completed", CustomerId = "user-1", ServiceCategoryId = 1 };
            var service2 = new ServiceRequest { Id = 2, VehicleId = 2, Status = "Completed", CustomerId = "user-2", ServiceCategoryId = 1 };

            var invoice1 = new Invoice { Id = 1, ServiceRequestId = 1, TotalAmount = 1000, PaymentStatus = "Pending" };
            var invoice2 = new Invoice { Id = 2, ServiceRequestId = 2, TotalAmount = 2000, PaymentStatus = "Paid" };

            context.ServiceCategories.Add(category);
            context.Vehicles.AddRange(user1Vehicle, user2Vehicle);
            context.ServiceRequests.AddRange(service1, service2);
            context.Invoices.AddRange(invoice1, invoice2);
            await context.SaveChangesAsync();

            var handler = new GetMyInvoicesHandler(context);

            // Act
            var result = await handler.Handle(new GetMyInvoicesQuery("user-1"), CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().InvoiceId.Should().Be(1);
            result.First().TotalAmount.Should().Be(1000);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoInvoices()
        {
            // Arrange
            var options = CreateNewContextOptions("GetMyInvoicesEmptyDb");
            using var context = new ApplicationDbContext(options);

            var handler = new GetMyInvoicesHandler(context);

            // Act
            var result = await handler.Handle(new GetMyInvoicesQuery("user-with-no-invoices"), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldOrderByDateDescending()
        {
            // Arrange
            var options = CreateNewContextOptions("GetMyInvoicesOrderedDb");
            using var context = new ApplicationDbContext(options);

            var category = new ServiceCategory { Id = 1, Name = "Test Category", BasePrice = 500 };
            var vehicle = new Vehicle { Id = 1, LicensePlate = "ABC-123", UserId = "user-1" };
            var service1 = new ServiceRequest { Id = 1, VehicleId = 1, Status = "Completed", CustomerId = "user-1", ServiceCategoryId = 1 };
            var service2 = new ServiceRequest { Id = 2, VehicleId = 1, Status = "Completed", CustomerId = "user-1", ServiceCategoryId = 1 };

            var invoice1 = new Invoice 
            { 
                Id = 1, 
                ServiceRequestId = 1, 
                TotalAmount = 1000, 
                IssuedDate = DateTime.UtcNow.AddDays(-5) // Older
            };
            var invoice2 = new Invoice 
            { 
                Id = 2, 
                ServiceRequestId = 2, 
                TotalAmount = 2000, 
                IssuedDate = DateTime.UtcNow // Newer
            };

            context.ServiceCategories.Add(category);
            context.Vehicles.Add(vehicle);
            context.ServiceRequests.AddRange(service1, service2);
            context.Invoices.AddRange(invoice1, invoice2);
            await context.SaveChangesAsync();

            var handler = new GetMyInvoicesHandler(context);

            // Act
            var result = await handler.Handle(new GetMyInvoicesQuery("user-1"), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.First().InvoiceId.Should().Be(2); // Newest first
            result.Last().InvoiceId.Should().Be(1); // Oldest last
        }
    }
}

