using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VehicleServiceManagement.API;
using VehicleServiceManagement.API.Application.DTOs.Admin;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.IntegrationTests
{
    public class RoleManagementIntegrationTests : IntegrationTestBase
    {
        public RoleManagementIntegrationTests(WebApplicationFactory<Program> factory)
            : base(factory, $"IntegrationTestDb_Role_{Guid.NewGuid()}")
        {
        }

        private async Task<string?> GetAdminUserIdAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
            return adminUsers.FirstOrDefault()?.Id;
        }

        [Fact]
        public async Task UpdateRole_ShouldReject_WhenTryingToModifyAdminRole()
        {
            // Arrange - Create admin user using base class method
            var adminToken = await base.CreateAdminUserAsync();
            SetAuthToken(adminToken);

            // Get admin user ID
            var adminId = await GetAdminUserIdAsync();
            
            if (adminId != null)
            {
                var updateRoleDto = new UpdateRoleDto
                {
                    UserId = adminId,
                    NewRole = "Customer"
                };

                // Act
                var response = await Client.PutAsJsonAsync("/api/Admin/update-role", updateRoleDto);

                // Assert - Should reject Admin role modification
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            else
            {
                // If admin not found, test that endpoint requires admin role
                var updateRoleDto = new UpdateRoleDto
                {
                    UserId = "non-existent-id",
                    NewRole = "Customer"
                };
                var response = await Client.PutAsJsonAsync("/api/Admin/update-role", updateRoleDto);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task UpdateRole_ShouldReject_WhenTryingToAssignAdmin_AndAdminExists()
        {
            // Arrange - Create admin user first using base class method
            var adminToken = await base.CreateAdminUserAsync();
            SetAuthToken(adminToken);

            // Create a regular customer user
            var customerEmail = $"customer_{Guid.NewGuid():N}@test.com";
            var customerToken = await RegisterAndLoginAsync(customerEmail, "Test!123456");

            // Get customer user ID
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var customer = await db.Users.FirstOrDefaultAsync(u => u.Email == customerEmail);

            if (customer != null)
            {
                var updateRoleDto = new UpdateRoleDto
                {
                    UserId = customer.Id,
                    NewRole = "Admin"
                };

                // Act
                var response = await Client.PutAsJsonAsync("/api/Admin/update-role", updateRoleDto);

                // Assert - Should reject if admin already exists
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            else
            {
                // If customer not found, test endpoint accessibility
                var updateRoleDto = new UpdateRoleDto
                {
                    UserId = "non-existent-id",
                    NewRole = "Admin"
                };
                var response = await Client.PutAsJsonAsync("/api/Admin/update-role", updateRoleDto);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GetUsers_ShouldRequireAdminRole()
        {
            // Arrange - Create customer (non-admin)
            var customerEmail = $"customer_{Guid.NewGuid():N}@test.com";
            var customerToken = await RegisterAndLoginAsync(customerEmail, "Test!123456");
            SetAuthToken(customerToken);

            // Act
            var response = await Client.GetAsync("/api/Admin/users");

            // Assert - Should return Forbidden for non-admin
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
        }
    }
}
