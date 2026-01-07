using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Reports;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using FluentAssertions;

public class GetMonthlyStatsTests
{
    [Fact]
    public async Task Handle_ShouldSumRevenue_ByMonth()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "ReportTest")
            .Options;

        using var context = new ApplicationDbContext(options);
        context.Invoices.AddRange(new List<Invoice> {
            new Invoice { TotalAmount = 500, IssuedDate = new DateTime(2025, 1, 1), PaymentStatus = "Paid" },
            new Invoice { TotalAmount = 500, IssuedDate = new DateTime(2025, 1, 15), PaymentStatus = "Paid" },
            new Invoice { TotalAmount = 200, IssuedDate = new DateTime(2025, 2, 1), PaymentStatus = "Paid" }
        });
        await context.SaveChangesAsync();

        var handler = new GetMonthlyStatsHandler(context);

        // Act
        var result = await handler.Handle(new GetMonthlyStatsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First(r => r.Month == 1).Revenue.Should().Be(1000); // 500 + 500
        result.First(r => r.Month == 2).Revenue.Should().Be(200);
    }
}