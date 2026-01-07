using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using FluentAssertions;

public class ApproveServiceTests
{
    [Fact]
    public async Task Handle_ShouldCreateInvoice_WhenManagerApproves_PendingReview()
    {
        // Test legacy workflow: Service in "Pending Review" status
        // 1. Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "BillingTest_PendingReview")
            .Options;

        using var context = new ApplicationDbContext(options);
        var service = new ServiceRequest
        {
            Id = 10,
            Status = "Pending Review",
            ServiceCategory = new ServiceCategory { BasePrice = 1000 }
        };
        context.ServiceRequests.Add(service);
        await context.SaveChangesAsync();

        var handler = new ApproveServiceHandler(context);

        // 2. Act
        var result = await handler.Handle(new ApproveServiceCommand(10), CancellationToken.None);

        // 3. Assert
        result.Should().BeTrue();
        context.Invoices.Count().Should().Be(1);
        var invoice = await context.Invoices.FirstAsync();
        invoice.TotalAmount.Should().Be(1000);
        
        var updatedService = await context.ServiceRequests.FindAsync(10);
        updatedService!.Status.Should().Be("Completed");
        updatedService.CompletionDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCreateInvoice_WhenServiceAlreadyCompleted()
    {
        // Test new workflow: Service already "Completed" but invoice missing
        // 1. Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "BillingTest_AlreadyCompleted")
            .Options;

        using var context = new ApplicationDbContext(options);
        var service = new ServiceRequest
        {
            Id = 20,
            Status = "Completed",
            CompletionDate = DateTime.UtcNow,
            ServiceCategory = new ServiceCategory { BasePrice = 1500 }
        };
        context.ServiceRequests.Add(service);
        await context.SaveChangesAsync();

        var handler = new ApproveServiceHandler(context);

        // 2. Act
        var result = await handler.Handle(new ApproveServiceCommand(20), CancellationToken.None);

        // 3. Assert
        result.Should().BeTrue();
        context.Invoices.Count().Should().Be(1);
        var invoice = await context.Invoices.FirstAsync();
        invoice.TotalAmount.Should().Be(1500); // Labor fee only (no parts)
        invoice.ServiceRequestId.Should().Be(20);
    }
}