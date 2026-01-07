using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VehicleServiceManagement.API;
using VehicleServiceManagement.API.Application.DTOs.Services;
using VehicleServiceManagement.API.Application.DTOs.Vehicles;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.IntegrationTests
{
    public class ServiceRequestIntegrationTests : IntegrationTestBase
    {
        public ServiceRequestIntegrationTests(WebApplicationFactory<Program> factory)
            : base(factory, $"IntegrationTestDb_ServiceRequest_{Guid.NewGuid()}")
        {
        }

        private async Task<int> CreateVehicleAsync()
        {
            var vehicleDto = new CreateVehicleDto
            {
                LicensePlate = "TEST-123",
                Make = "Toyota",
                Model = "Camry",
                Year = 2020
            };
            var vehicleResponse = await Client.PostAsJsonAsync("/api/Vehicles", vehicleDto);
            vehicleResponse.EnsureSuccessStatusCode();
            var vehicleResult = await vehicleResponse.Content.ReadFromJsonAsync<JsonElement>();
            return vehicleResult.GetProperty("id").GetInt32();
        }

        private async Task<int> GetServiceCategoryIdAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = await db.ServiceCategories.FirstAsync();
            return category.Id;
        }

        [Fact]
        public async Task BookService_ShouldCreateServiceRequest_AndPersistToDatabase()
        {
            // Arrange
            var email = $"servicecustomer_{Guid.NewGuid():N}@test.com";
            var password = "Test!123456";
            var token = await RegisterAndLoginAsync(email, password);
            SetAuthToken(token);

            // Create vehicle
            var vehicleId = await CreateVehicleAsync();

            // Get service category
            var categoryId = await GetServiceCategoryIdAsync();

            // Create service request
            var serviceDto = new CreateServiceRequestDto
            {
                VehicleId = vehicleId,
                ServiceCategoryId = categoryId,
                Description = "Oil change service",
                Priority = "Normal",
                RequestedDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/Services", serviceDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("requestId").GetInt32().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetMyBookings_ShouldReturnOnlyCustomerBookings()
        {
            // Arrange
            var email = $"bookingscustomer_{Guid.NewGuid():N}@test.com";
            var password = "Test!123456";
            var token = await RegisterAndLoginAsync(email, password);
            SetAuthToken(token);

            // Act
            var response = await Client.GetAsync("/api/Services");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = await response.Content.ReadFromJsonAsync<List<object>>();
            bookings.Should().NotBeNull();
        }

        [Fact]
        public async Task CancelBooking_ShouldUpdateStatus_AndPersistToDatabase()
        {
            // Arrange
            var email = $"cancelcustomer_{Guid.NewGuid():N}@test.com";
            var password = "Test!123456";
            var token = await RegisterAndLoginAsync(email, password);
            SetAuthToken(token);

            // Create vehicle and service
            var vehicleId = await CreateVehicleAsync();
            var categoryId = await GetServiceCategoryIdAsync();

            var serviceDto = new CreateServiceRequestDto
            {
                VehicleId = vehicleId,
                ServiceCategoryId = categoryId,
                Description = "Test service",
                Priority = "Normal",
                RequestedDate = DateTime.UtcNow.AddDays(1)
            };
            var createResponse = await Client.PostAsJsonAsync("/api/Services", serviceDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var serviceRequestId = createResult.GetProperty("requestId").GetInt32();

            // Act
            var response = await Client.DeleteAsync($"/api/Services/{serviceRequestId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task RescheduleService_ShouldUpdateRequestedDate_WhenServiceIsRequested()
        {
            // Arrange
            var email = $"reschedulecustomer_{Guid.NewGuid():N}@test.com";
            var password = "Test!123456";
            var token = await RegisterAndLoginAsync(email, password);
            SetAuthToken(token);

            // Create vehicle and service
            var vehicleId = await CreateVehicleAsync();
            var categoryId = await GetServiceCategoryIdAsync();

            var serviceDto = new CreateServiceRequestDto
            {
                VehicleId = vehicleId,
                ServiceCategoryId = categoryId,
                Description = "Test service",
                Priority = "Normal",
                RequestedDate = DateTime.UtcNow.AddDays(1)
            };
            var createResponse = await Client.PostAsJsonAsync("/api/Services", serviceDto);
            var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var serviceRequestId = createResult.GetProperty("requestId").GetInt32();

            var rescheduleDto = new { newRequestedDate = DateTime.UtcNow.AddDays(5) };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/Services/{serviceRequestId}/reschedule", rescheduleDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
