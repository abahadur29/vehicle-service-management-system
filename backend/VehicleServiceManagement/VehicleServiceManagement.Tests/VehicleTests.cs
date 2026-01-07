using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehicleServiceManagement.API.Application.Features.Vehicles;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Infrastructure.Data;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Vehicles
{
    public class VehicleTests
    {
        private DbContextOptions<ApplicationDbContext> CreateNewContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task AddVehicleHandler_ShouldCreateVehicle_AndReturnVehicleDto()
        {
            // Arrange
            var options = CreateNewContextOptions("AddVehicleDb");
            using var context = new ApplicationDbContext(options);

            var handler = new AddVehicleHandler(context);
            var command = new AddVehicleCommand(
                "ABC-1234",
                "Camry",
                "Toyota",
                2020,
                "user-123"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.LicensePlate.Should().Be("ABC-1234");
            result.Model.Should().Be("Camry");
            result.Make.Should().Be("Toyota");
            result.Year.Should().Be(2020);

            var savedVehicle = await context.Vehicles.FindAsync(result.Id);
            savedVehicle.Should().NotBeNull();
            savedVehicle!.UserId.Should().Be("user-123");
        }

        [Fact]
        public async Task GetMyVehiclesQuery_ShouldReturnOnlyUserVehicles()
        {
            // Arrange
            var options = CreateNewContextOptions("GetMyVehiclesDb");
            using var context = new ApplicationDbContext(options);

            var user1Id = "user-1";
            var user2Id = "user-2";

            context.Vehicles.AddRange(
                new Vehicle { LicensePlate = "ABC-123", Model = "Camry", Make = "Toyota", Year = 2020, UserId = user1Id },
                new Vehicle { LicensePlate = "XYZ-789", Model = "Civic", Make = "Honda", Year = 2021, UserId = user1Id },
                new Vehicle { LicensePlate = "DEF-456", Model = "Accord", Make = "Honda", Year = 2022, UserId = user2Id }
            );
            await context.SaveChangesAsync();

            // Mock AutoMapper
            var mapperConfig = new MapperConfiguration(cfg => {
                cfg.CreateMap<Vehicle, VehicleServiceManagement.API.Application.DTOs.VehicleDto>()
                    .ForMember(dest => dest.OwnerFullName, opt => opt.MapFrom(src => ""));
            });
            var mapper = mapperConfig.CreateMapper();

            var handler = new GetMyVehiclesHandler(context, mapper);

            // Act
            var result = await handler.Handle(new GetMyVehiclesQuery(user1Id), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.All(v => v.LicensePlate == "ABC-123" || v.LicensePlate == "XYZ-789").Should().BeTrue();
            result.Any(v => v.LicensePlate == "DEF-456").Should().BeFalse();
        }
    }
}

