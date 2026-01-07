using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehicleServiceManagement.API.Application.Features.Invoices;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests
{
    public class BillingAuthorizationTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task GetInvoiceByService_ShouldReturnInvoice_WhenCustomerOwnsService()
        {
            // Arrange
            var options = CreateNewContextOptions("GetInvoice_CustomerOwns");
            using var context = new ApplicationDbContext(options);
            var category = new ServiceCategory { Id = 1, Name = "Test Category", BasePrice = 500 };
            var customerId = "customer-1";
            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                CustomerId = customerId,
                Status = "Completed",
                Description = "Test Service",
                ServiceCategoryId = 1
            };
            var invoice = new Invoice
            {
                Id = 1,
                ServiceRequestId = 1,
                TotalAmount = 100,
                PaymentStatus = "Pending"
            };
            context.ServiceCategories.Add(category);
            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var handler = new GetInvoiceByServiceHandler(context);

            // Act
            var result = await handler.Handle(new GetInvoiceByServiceQuery(1, customerId, "Customer"), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.InvoiceId.Should().Be(1);
            result.ServiceRequestId.Should().Be(1);
        }

        [Fact]
        public async Task GetInvoiceByService_ShouldReturnNull_WhenCustomerDoesNotOwnService()
        {
            // Arrange
            var options = CreateNewContextOptions("GetInvoice_CustomerNotOwns");
            using var context = new ApplicationDbContext(options);
            var category = new ServiceCategory { Id = 1, Name = "Test Category", BasePrice = 500 };
            var customerId = "customer-1";
            var otherCustomerId = "customer-2";
            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                CustomerId = otherCustomerId,
                Status = "Completed",
                Description = "Test Service",
                ServiceCategoryId = 1
            };
            var invoice = new Invoice
            {
                Id = 1,
                ServiceRequestId = 1,
                TotalAmount = 100,
                PaymentStatus = "Pending"
            };
            context.ServiceCategories.Add(category);
            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var handler = new GetInvoiceByServiceHandler(context);

            // Act
            var result = await handler.Handle(new GetInvoiceByServiceQuery(1, customerId, "Customer"), CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetInvoiceByService_ShouldReturnInvoice_WhenAdminRequests()
        {
            // Arrange
            var options = CreateNewContextOptions("GetInvoice_Admin");
            using var context = new ApplicationDbContext(options);
            var category = new ServiceCategory { Id = 1, Name = "Test Category", BasePrice = 500 };
            var adminId = "admin-1";
            var customerId = "customer-1";
            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                CustomerId = customerId,
                Status = "Completed",
                Description = "Test Service",
                ServiceCategoryId = 1
            };
            var invoice = new Invoice
            {
                Id = 1,
                ServiceRequestId = 1,
                TotalAmount = 100,
                PaymentStatus = "Pending"
            };
            context.ServiceCategories.Add(category);
            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var handler = new GetInvoiceByServiceHandler(context);

            // Act
            var result = await handler.Handle(new GetInvoiceByServiceQuery(1, adminId, "Admin"), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.InvoiceId.Should().Be(1);
        }

        [Fact]
        public async Task ProcessPayment_ShouldSucceed_WhenCustomerOwnsInvoice()
        {
            // Arrange
            var options = CreateNewContextOptions("ProcessPayment_CustomerOwns");
            using var context = new ApplicationDbContext(options);
            var customerId = "customer-1";
            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                CustomerId = customerId,
                Status = "Completed",
                Description = "Test Service"
            };
            var invoice = new Invoice
            {
                Id = 1,
                ServiceRequestId = 1,
                TotalAmount = 100,
                PaymentStatus = "Pending"
            };
            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.GetUsersInRoleAsync("Manager")).ReturnsAsync(new List<ApplicationUser>());

            var handler = new ProcessPaymentHandler(context, userManagerMock.Object);

            // Act
            var result = await handler.Handle(new ProcessPaymentCommand(1, customerId, "Customer"), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
            
            await context.Entry(invoice).ReloadAsync();
            await context.Entry(serviceRequest).ReloadAsync();
            invoice.PaymentStatus.Should().Be("Paid");
            serviceRequest.Status.Should().Be("Closed");
        }

        [Fact]
        public async Task ProcessPayment_ShouldFail_WhenCustomerDoesNotOwnInvoice()
        {
            // Arrange
            var options = CreateNewContextOptions("ProcessPayment_CustomerNotOwns");
            using var context = new ApplicationDbContext(options);
            var customerId = "customer-1";
            var otherCustomerId = "customer-2";
            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                CustomerId = otherCustomerId,
                Status = "Completed",
                Description = "Test Service"
            };
            var invoice = new Invoice
            {
                Id = 1,
                ServiceRequestId = 1,
                TotalAmount = 100,
                PaymentStatus = "Pending"
            };
            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.GetUsersInRoleAsync("Manager")).ReturnsAsync(new List<ApplicationUser>());

            var handler = new ProcessPaymentHandler(context, userManagerMock.Object);

            // Act
            var result = await handler.Handle(new ProcessPaymentCommand(1, customerId, "Customer"), CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("permission");
        }

        [Fact]
        public async Task ProcessPayment_ShouldSucceed_WhenAdminRequests()
        {
            // Arrange
            var options = CreateNewContextOptions("ProcessPayment_Admin");
            using var context = new ApplicationDbContext(options);
            var adminId = "admin-1";
            var customerId = "customer-1";
            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                CustomerId = customerId,
                Status = "Completed",
                Description = "Test Service"
            };
            var invoice = new Invoice
            {
                Id = 1,
                ServiceRequestId = 1,
                TotalAmount = 100,
                PaymentStatus = "Pending"
            };
            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.GetUsersInRoleAsync("Manager")).ReturnsAsync(new List<ApplicationUser>());

            var handler = new ProcessPaymentHandler(context, userManagerMock.Object);

            // Act
            var result = await handler.Handle(new ProcessPaymentCommand(1, adminId, "Admin"), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            await context.Entry(invoice).ReloadAsync();
            invoice.PaymentStatus.Should().Be("Paid");
        }

        [Fact]
        public async Task ProcessPayment_ShouldFail_WhenInvoiceAlreadyPaid()
        {
            // Arrange
            var options = CreateNewContextOptions("ProcessPayment_AlreadyPaid");
            using var context = new ApplicationDbContext(options);
            var customerId = "customer-1";
            var serviceRequest = new ServiceRequest
            {
                Id = 1,
                CustomerId = customerId,
                Status = "Completed",
                Description = "Test Service"
            };
            var invoice = new Invoice
            {
                Id = 1,
                ServiceRequestId = 1,
                TotalAmount = 100,
                PaymentStatus = "Paid"
            };
            context.ServiceRequests.Add(serviceRequest);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.GetUsersInRoleAsync("Manager")).ReturnsAsync(new List<ApplicationUser>());

            var handler = new ProcessPaymentHandler(context, userManagerMock.Object);

            // Act
            var result = await handler.Handle(new ProcessPaymentCommand(1, customerId, "Customer"), CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("already been paid");
        }
    }
}

