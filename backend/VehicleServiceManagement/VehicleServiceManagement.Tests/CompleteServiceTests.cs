using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using VehicleServiceManagement.API.Application.DTOs.Services;

namespace VehicleServiceManagement.Tests.Features.Services
{
    public class CompleteServiceTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Handle_ShouldDeductStock_AndChangeStatusToCompleted_AndGenerateInvoice()
        {
            // --- 1. Arrange ---
            var options = CreateNewContextOptions("StockDeductionDb_Success");
            using var context = new ApplicationDbContext(options);

            var category = new ServiceCategory { Id = 1, Name = "Oil Change", BasePrice = 500 };
            var part = new Part { Id = 101, Name = "Synthetic Engine Oil", StockQuantity = 50, UnitPrice = 1500 };
            var serviceRequest = new ServiceRequest 
            { 
                Id = 1, 
                Status = "In Progress", 
                Description = "Monthly Maintenance",
                ServiceCategoryId = 1,
                ServiceCategory = category,
                CustomerId = "test-customer-id-1" // Required for notification
            };

            context.ServiceCategories.Add(category);
            context.Parts.Add(part);
            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new CompleteServiceHandler(context);
            var partsUsed = new List<PartUsageDto> { new PartUsageDto { PartId = 101, Quantity = 2 } };
            var command = new CompleteServiceCommand(1, partsUsed, "Oil changed and filter replaced.");

            // --- 2. Act ---
            var result = await handler.Handle(command, CancellationToken.None);

            // --- 3. Assert ---
            result.Should().BeTrue();
            var updatedPart = await context.Parts.FindAsync(101);
            updatedPart!.StockQuantity.Should().Be(48); // 50 - 2

            var updatedRequest = await context.ServiceRequests.FindAsync(1);
            updatedRequest!.Status.Should().Be("Completed"); 
            updatedRequest.Description.Should().Contain("Oil changed"); 
            updatedRequest.CompletionDate.Should().NotBeNull();
            
            // Invoice should be generated immediately
            var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.ServiceRequestId == 1);
            invoice.Should().NotBeNull();
            invoice!.TotalAmount.Should().Be(3500); // Parts cost (2 * 1500) + Labor fee (500)
        }

        [Fact]
        public async Task Handle_ShouldNotDeductStock_WhenInventoryIsInsufficient()
        {
            // --- 1. Arrange ---
            var options = CreateNewContextOptions("StockDeductionDb_Failure");
            using var context = new ApplicationDbContext(options);

            var category = new ServiceCategory { Id = 1, Name = "Battery Replacement", BasePrice = 200 };
            // Only 1 battery in stock
            var part = new Part { Id = 200, Name = "Battery", StockQuantity = 1, UnitPrice = 8500 };
            var serviceRequest = new ServiceRequest 
            { 
                Id = 5, 
                Status = "In Progress",
                ServiceCategoryId = 1,
                ServiceCategory = category,
                CustomerId = "test-customer-id-5" // Required for notification
            };

            context.ServiceCategories.Add(category);
            context.Parts.Add(part);
            context.ServiceRequests.Add(serviceRequest);
            await context.SaveChangesAsync();

            var handler = new CompleteServiceHandler(context);

            // Attempt to use 2 batteries (more than available)
            var partsUsed = new List<PartUsageDto> { new PartUsageDto { PartId = 200, Quantity = 2 } };
            var command = new CompleteServiceCommand(5, partsUsed, "Replacing old battery.");

            // --- 2. Act ---
            await handler.Handle(command, CancellationToken.None);

            // --- 3. Assert ---
            var checkPart = await context.Parts.FindAsync(200);
            checkPart!.StockQuantity.Should().Be(1); // Stock must remain unchanged due to validation

            var checkRequest = await context.ServiceRequests.FindAsync(5);
            checkRequest!.Status.Should().BeOneOf("Completed", "In Progress");
        }
    }
}