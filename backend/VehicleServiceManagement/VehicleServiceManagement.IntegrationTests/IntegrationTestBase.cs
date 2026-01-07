using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using VehicleServiceManagement.API;
using VehicleServiceManagement.API.Application.DTOs.Auth;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.IntegrationTests
{
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;
        protected readonly string TestDbName;

        public IntegrationTestBase(WebApplicationFactory<Program> factory, string testDbName)
        {
            TestDbName = testDbName;
            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Ensure JWT configuration is available
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "Jwt:Key", "THIS_IS_A_VERY_LONG_SUPER_SECRET_KEY_FOR_VEHICLE_SYSTEM_12345" },
                        { "Jwt:Issuer", "VehicleApi" },
                        { "Jwt:Audience", "VehicleUsers" }
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(d => 
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(testDbName);
                    });

                    // Build service provider and seed database
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    // Ensure database is created
                    db.Database.EnsureCreated();

                    // Seed roles and service categories
                    SeedTestDataAsync(db, roleManager, userManager).GetAwaiter().GetResult();
                });
            });

            Client = Factory.CreateClient();
        }

        protected async Task SeedTestDataAsync(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            // Seed roles
            string[] roles = { "Admin", "Manager", "Technician", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed service categories (required for service requests)
            if (!context.ServiceCategories.Any())
            {
                var categories = new List<ServiceCategory>
                {
                    new ServiceCategory { Name = "Standard Oil Change", BasePrice = 1200m, Description = "Oil change service" },
                    new ServiceCategory { Name = "Brake Service", BasePrice = 3500m, Description = "Brake inspection and repair" },
                    new ServiceCategory { Name = "Engine Diagnostic", BasePrice = 1000m, Description = "Engine diagnostic scan" }
                };
                context.ServiceCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Seed parts (optional, for inventory tests)
            if (!context.Parts.Any())
            {
                var parts = new List<Part>
                {
                    new Part { Name = "Test Oil Filter", UnitPrice = 550.00m, StockQuantity = 30 },
                    new Part { Name = "Test Brake Pads", UnitPrice = 2800.00m, StockQuantity = 12 }
                };
                context.Parts.AddRange(parts);
                await context.SaveChangesAsync();
            }
        }

        protected async Task<string> RegisterAndLoginAsync(
            string email,
            string password = "Test!123456",  // Default password that meets validation requirements
            string fullName = "Test User",
            string phoneNumber = "1234567890")
        {
            // Register user
            var registerDto = new RegisterRequestDto
            {
                FullName = fullName,
                Email = email,
                Password = password,
                PhoneNumber = phoneNumber
            };

            var registerResponse = await Client.PostAsJsonAsync("/api/Auth/register", registerDto);
            
            // If registration fails, check if it's because user already exists
            // If so, we can still try to login. Otherwise, throw error.
            if (!registerResponse.IsSuccessStatusCode && registerResponse.StatusCode != System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await registerResponse.Content.ReadAsStringAsync();
                throw new Exception($"Registration failed: {registerResponse.StatusCode} - {errorContent}");
            }

            // Small delay to ensure user is fully created
            await Task.Delay(100);

            // Login
            var loginDto = new LoginRequestDto
            {
                Email = email,
                Password = password
            };

            var loginResponse = await Client.PostAsJsonAsync("/api/Auth/login", loginDto);
            
            if (!loginResponse.IsSuccessStatusCode)
            {
                var errorContent = await loginResponse.Content.ReadAsStringAsync();
                throw new Exception($"Login failed: {loginResponse.StatusCode} - {errorContent}");
            }

            var loginResult = await loginResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            return loginResult.GetProperty("token").GetString()!;
        }

        protected async Task<string> CreateAdminUserAsync()
        {
            using var scope = Factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var adminEmail = $"admin_{Guid.NewGuid():N}@test.com";
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Test Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin!123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Login to get token
            return await RegisterAndLoginAsync(adminEmail, "Admin!123", "Test Admin");
        }

        protected void SetAuthToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        protected void ClearAuthToken()
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }
}

