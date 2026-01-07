using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehicleServiceManagement.API.Application.Features.Invoices;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Invoices
{
    public class ProcessPaymentTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Handle_ShouldUpdatePaymentStatus_ToPaid()
        {
            // Arrange
            var options = CreateNewContextOptions("ProcessPaymentDb_Success");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 10,
                CustomerId = "customer-1",
                Status = "Completed",
                Description = "Test Service"
            };
            var invoice = new Invoice
            {
                Id = 1,
                ServiceRequestId = 10,
                TotalAmount = 1500,
                PaymentStatus = "Pending",
                IssuedDate = DateTime.UtcNow,
                ServiceRequest = serviceRequest
            };

            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.GetUsersInRoleAsync("Manager")).ReturnsAsync(new List<ApplicationUser>());

            var handler = new ProcessPaymentHandler(context, userManagerMock.Object);
            var command = new ProcessPaymentCommand(1, "customer-1", "Customer");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            var updatedInvoice = await context.Invoices.Include(i => i.ServiceRequest).FirstOrDefaultAsync(i => i.Id == 1);
            updatedInvoice.Should().NotBeNull();
            updatedInvoice!.PaymentStatus.Should().Be("Paid");
            updatedInvoice.ServiceRequest!.Status.Should().Be("Closed");
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenInvoiceNotFound()
        {
            // Arrange
            var options = CreateNewContextOptions("ProcessPaymentDb_NotFound");
            using var context = new ApplicationDbContext(options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.GetUsersInRoleAsync("Manager")).ReturnsAsync(new List<ApplicationUser>());

            var handler = new ProcessPaymentHandler(context, userManagerMock.Object);
            var command = new ProcessPaymentCommand(999, null, null); // Non-existent invoice

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenInvoiceAlreadyPaid()
        {
            // Arrange
            var options = CreateNewContextOptions("ProcessPaymentDb_AlreadyPaid");
            using var context = new ApplicationDbContext(options);

            var serviceRequest = new ServiceRequest
            {
                Id = 20,
                CustomerId = "customer-1",
                Status = "Closed",
                Description = "Test Service"
            };
            var invoice = new Invoice
            {
                Id = 2,
                ServiceRequestId = 20,
                TotalAmount = 2000,
                PaymentStatus = "Paid", // Already paid
                IssuedDate = DateTime.UtcNow,
                ServiceRequest = serviceRequest
            };

            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.GetUsersInRoleAsync("Manager")).ReturnsAsync(new List<ApplicationUser>());

            var handler = new ProcessPaymentHandler(context, userManagerMock.Object);
            var command = new ProcessPaymentCommand(2, "customer-1", "Customer");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse(); // Should fail if already paid
            result.ErrorMessage.Should().Contain("already been paid");
            var invoiceAfter = await context.Invoices.FindAsync(2);
            invoiceAfter!.PaymentStatus.Should().Be("Paid");
        }
    }
}

