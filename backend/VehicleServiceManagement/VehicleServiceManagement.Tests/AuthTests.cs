using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using VehicleServiceManagement.API.Application.DTOs.Auth;
using VehicleServiceManagement.API.Application.Features.Auth;
using VehicleServiceManagement.API.Core.Entities;
using Xunit;

namespace VehicleServiceManagement.Tests.Features.Auth
{
    public class AuthTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;

        public AuthTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            
            _configurationMock = new Mock<IConfiguration>();
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["Key"]).Returns("THIS_IS_A_VERY_LONG_SUPER_SECRET_KEY_FOR_VEHICLE_SYSTEM_12345");
            jwtSection.Setup(x => x["Issuer"]).Returns("VehicleApi");
            jwtSection.Setup(x => x["Audience"]).Returns("VehicleUsers");
            _configurationMock.Setup(x => x.GetSection("Jwt")).Returns(jwtSection.Object);
        }

        [Fact]
        public async Task RegisterUserHandler_ShouldReturnTrue_WhenUserCreatedSuccessfully()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "Test@1234",
                PhoneNumber = "1234567890"
            };

            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new RegisterUserHandler(_userManagerMock.Object);

            // Act
            var result = await handler.Handle(new RegisterUserCommand(dto), CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Customer"), Times.Once);
        }

        [Fact]
        public async Task RegisterUserHandler_ShouldReturnFalse_WhenUserCreationFails()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                FullName = "Test User",
                Email = "test@example.com",
                Password = "weak",
                PhoneNumber = "1234567890"
            };

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            var handler = new RegisterUserHandler(_userManagerMock.Object);

            // Act
            var result = await handler.Handle(new RegisterUserCommand(dto), CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginUserHandler_ShouldReturnSuccess_WhenCredentialsAreValid()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Test@1234"
            };

            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = dto.Email,
                FullName = "Test User"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Customer" });

            var handler = new LoginUserHandler(_userManagerMock.Object, _configurationMock.Object);

            // Act
            var result = await handler.Handle(new LoginUserCommand(dto), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Token.Should().NotBeEmpty();
            result.Email.Should().Be(dto.Email);
            result.Role.Should().Be("Customer");
        }

        [Fact]
        public async Task LoginUserHandler_ShouldReturnFailure_WhenCredentialsAreInvalid()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);

            var handler = new LoginUserHandler(_userManagerMock.Object, _configurationMock.Object);

            // Act
            var result = await handler.Handle(new LoginUserCommand(dto), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Token.Should().BeEmpty();
            result.Message.Should().Contain("Invalid");
        }

        [Fact]
        public async Task LoginUserHandler_ShouldAutoAssignCustomerRole_WhenUserHasNoRole()
        {
            // Arrange
            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Test@1234"
            };

            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = dto.Email,
                FullName = "Test User"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, dto.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, "Customer"))
                .ReturnsAsync(IdentityResult.Success);

            var handler = new LoginUserHandler(_userManagerMock.Object, _configurationMock.Object);

            // Act
            var result = await handler.Handle(new LoginUserCommand(dto), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Role.Should().Be("Customer");
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Customer"), Times.Once);
        }
    }
}

