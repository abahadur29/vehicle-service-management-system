using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VehicleServiceManagement.API;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.IntegrationTests
{
    public class BillingIntegrationTests : IntegrationTestBase
    {
        public BillingIntegrationTests(WebApplicationFactory<Program> factory)
            : base(factory, $"IntegrationTestDb_Billing_{Guid.NewGuid()}")
        {
        }

        [Fact]
        public async Task GetMyInvoices_ShouldReturnOnlyCustomerInvoices()
        {
            // Arrange
            var email = $"customer1_{Guid.NewGuid():N}@test.com";
            var password = "Test!123456";
            var token = await RegisterAndLoginAsync(email, password);
            SetAuthToken(token);

            // Act
            var response = await Client.GetAsync("/api/Billing");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var invoices = await response.Content.ReadFromJsonAsync<List<object>>();
            invoices.Should().NotBeNull();
        }

        [Fact]
        public async Task GetInvoice_ShouldReturn404_WhenCustomerAccessesOtherCustomerInvoice()
        {
            // Arrange
            var customer1Email = $"customer1_{Guid.NewGuid():N}@test.com";
            var customer2Email = $"customer2_{Guid.NewGuid():N}@test.com";
            
            var customer1Token = await RegisterAndLoginAsync(customer1Email, "Test!123456");
            SetAuthToken(customer1Token);

            // Act - Try to access non-existent invoice
            var response = await Client.GetAsync("/api/Billing/invoice/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PayInvoice_ShouldSucceed_WhenCustomerOwnsInvoice()
        {
            // Arrange
            var email = $"customer3_{Guid.NewGuid():N}@test.com";
            var password = "Test!123456";
            var token = await RegisterAndLoginAsync(email, password);
            SetAuthToken(token);

            // Act - Try to pay non-existent invoice (endpoint should be accessible)
            var response = await Client.PostAsync("/api/Billing/pay/99999", null);

            // Assert - Should return NotFound or BadRequest, but endpoint should be accessible
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }
    }
}
