using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.Features.Reports;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Reports
{
    public class GetByPriorityTests
    {
        [Fact]
        public async Task Handle_ShouldOnlyReturnHighPriorityTasks_WhenFilteredByHigh()
        {
            // ---------- Arrange ----------
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            context.ServiceRequests.AddRange(
                new ServiceRequest
                {
                    Priority = "High",
                    Description = "Brake Failure",
                    RequestedDate = DateTime.UtcNow,
                    ServiceCategoryId = 1
                },
                new ServiceRequest
                {
                    Priority = "Normal",
                    Description = "Oil Change",
                    RequestedDate = DateTime.UtcNow,
                    ServiceCategoryId = 1
                }
            );

            await context.SaveChangesAsync();

            var handler = new GetByPriorityHandler(context);

            // ---------- Act ----------
            var result = await handler.Handle(
                new GetByPriorityQuery("high"), // lowercase on purpose
                CancellationToken.None
            );

            // ---------- Assert ----------
            result.Should().NotBeNull();
            result.Should().HaveCount(
                1,
                "because only one service request has 'High' priority"
            );

            result.First().Priority.Should().Be("High");
            result.First().Description.Should().Be("Brake Failure");
        }
    }
}
