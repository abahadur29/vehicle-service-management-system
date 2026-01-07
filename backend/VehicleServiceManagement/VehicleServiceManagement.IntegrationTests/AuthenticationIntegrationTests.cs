using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VehicleServiceManagement.API;
using VehicleServiceManagement.API.Application.DTOs.Auth;
using Xunit;

namespace VehicleServiceManagement.IntegrationTests
{
    public class AuthenticationIntegrationTests : IntegrationTestBase
    {
        public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
            : base(factory, $"IntegrationTestDb_Auth_{Guid.NewGuid()}")
        {
        }

        [Fact]
        public async Task Register_ShouldCreateUser_AndReturnSuccess()
        {
            // Arrange
            var registerDto = new RegisterRequestDto
            {
                FullName = "Test User",
                Email = $"testuser_{Guid.NewGuid():N}@example.com",
                Password = "Test!123456",
                PhoneNumber = "1234567890"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Registration failed with {response.StatusCode}: {errorContent}");
            }
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("message").GetString().Should().Contain("successful");
        }

        [Fact]
        public async Task Login_ShouldReturnJWTToken_WhenCredentialsAreValid()
        {
            // Arrange - First register
            var email = $"logintest_{Guid.NewGuid():N}@example.com";
            var password = "Test!123456";
            
            var registerDto = new RegisterRequestDto
            {
                FullName = "Login Test User",
                Email = email,
                Password = password,
                PhoneNumber = "1234567890"
            };
            await Client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Act - Login
            var loginDto = new LoginRequestDto
            {
                Email = email,
                Password = password
            };
            var response = await Client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            result.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
            result.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_ShouldReturnUnauthorized_WithoutToken()
        {
            // Act
            var response = await Client.GetAsync("/api/Vehicles/my-vehicles");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProtectedEndpoint_ShouldReturnOk_WithValidToken()
        {
            // Arrange - Register and login
            var email = $"tokentest_{Guid.NewGuid():N}@example.com";
            var password = "Test!123456";
            
            var token = await RegisterAndLoginAsync(email, password, "Token Test User");

            // Act - Use token
            SetAuthToken(token);
            var response = await Client.GetAsync("/api/Vehicles/my-vehicles");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound); // OK if empty, NotFound if no vehicles
        }
    }
}
