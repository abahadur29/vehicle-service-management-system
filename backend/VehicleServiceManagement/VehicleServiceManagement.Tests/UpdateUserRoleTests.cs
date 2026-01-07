using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using VehicleServiceManagement.API.Application.Features.Users;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Infrastructure.Data;

namespace VehicleServiceManagement.Tests.Features.Users
{
    public class UpdateUserRoleTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly UpdateUserRoleHandler _handler;

        public UpdateUserRoleTests()
        {
            // Professional Setup: Mocking the UserManager is complex; we use a helper store
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null!, null!, null!, null!);

            _contextMock = new Mock<IApplicationDbContext>();
            var serviceRequestsMock = new Mock<DbSet<ServiceRequest>>();
            _contextMock.Setup(x => x.ServiceRequests).Returns(serviceRequestsMock.Object);

            _handler = new UpdateUserRoleHandler(_userManagerMock.Object, _roleManagerMock.Object, _contextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldSuccessfullyChangeUserRole()
        {
            // --- 1. Arrange ---
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "test@vehicle.com" };
            var currentRoles = new List<string> { "Customer" };
            var newRole = "Technician";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(currentRoles);
            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, currentRoles)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, newRole)).ReturnsAsync(IdentityResult.Success);
            
            var serviceRequestsData = new List<ServiceRequest>().AsQueryable();
            var serviceRequestsMock = new Mock<DbSet<ServiceRequest>>();
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.Provider).Returns(serviceRequestsData.Provider);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.Expression).Returns(serviceRequestsData.Expression);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.ElementType).Returns(serviceRequestsData.ElementType);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.GetEnumerator()).Returns(serviceRequestsData.GetEnumerator());
            _contextMock.Setup(x => x.ServiceRequests).Returns(serviceRequestsMock.Object);

            // --- 2. Act ---
            var result = await _handler.Handle(new UpdateUserRoleCommand(userId, newRole), CancellationToken.None);

            // --- 3. Assert ---
            result.Success.Should().BeTrue(); // Confirms role was updated
            result.ErrorMessage.Should().BeNull();
            _userManagerMock.Verify(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, newRole), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReject_WhenTryingToModifyAdminRole()
        {
            // Arrange
            var userId = "admin-123";
            var user = new ApplicationUser { Id = userId, Email = "admin@vehicle.com" };
            var currentRoles = new List<string> { "Admin" };
            var newRole = "Customer";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(currentRoles);
            
            var serviceRequestsData = new List<ServiceRequest>().AsQueryable();
            var serviceRequestsMock = new Mock<DbSet<ServiceRequest>>();
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.Provider).Returns(serviceRequestsData.Provider);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.Expression).Returns(serviceRequestsData.Expression);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.ElementType).Returns(serviceRequestsData.ElementType);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.GetEnumerator()).Returns(serviceRequestsData.GetEnumerator());
            _contextMock.Setup(x => x.ServiceRequests).Returns(serviceRequestsMock.Object);

            // Act
            var result = await _handler.Handle(new UpdateUserRoleCommand(userId, newRole), CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Cannot modify Admin role");
            _userManagerMock.Verify(x => x.RemoveFromRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()), Times.Never);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReject_WhenTryingToAssignAdminRole_AndAdminExists()
        {
            // Arrange
            var userId = "user-123";
            var existingAdminId = "admin-456";
            var user = new ApplicationUser { Id = userId, Email = "user@vehicle.com" };
            var existingAdmin = new ApplicationUser { Id = existingAdminId, Email = "admin@vehicle.com" };
            var currentRoles = new List<string> { "Customer" };
            var newRole = "Admin";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(currentRoles);
            _userManagerMock.Setup(x => x.GetUsersInRoleAsync("Admin")).ReturnsAsync(new List<ApplicationUser> { existingAdmin });
            
            var serviceRequestsData = new List<ServiceRequest>().AsQueryable();
            var serviceRequestsMock = new Mock<DbSet<ServiceRequest>>();
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.Provider).Returns(serviceRequestsData.Provider);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.Expression).Returns(serviceRequestsData.Expression);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.ElementType).Returns(serviceRequestsData.ElementType);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.GetEnumerator()).Returns(serviceRequestsData.GetEnumerator());
            _contextMock.Setup(x => x.ServiceRequests).Returns(serviceRequestsMock.Object);

            // Act
            var result = await _handler.Handle(new UpdateUserRoleCommand(userId, newRole), CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Only one Admin user is allowed");
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldAllow_WhenTryingToAssignAdminRole_AndNoAdminExists()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@vehicle.com" };
            var currentRoles = new List<string> { "Customer" };
            var newRole = "Admin";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(currentRoles);
            _userManagerMock.Setup(x => x.GetUsersInRoleAsync("Admin")).ReturnsAsync(new List<ApplicationUser>());
            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, currentRoles)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, newRole)).ReturnsAsync(IdentityResult.Success);
            
            var serviceRequestsData = new List<ServiceRequest>().AsQueryable();
            var serviceRequestsMock = new Mock<DbSet<ServiceRequest>>();
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.Provider).Returns(serviceRequestsData.Provider);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.Expression).Returns(serviceRequestsData.Expression);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.ElementType).Returns(serviceRequestsData.ElementType);
            serviceRequestsMock.As<IQueryable<ServiceRequest>>().Setup(m => m.GetEnumerator()).Returns(serviceRequestsData.GetEnumerator());
            _contextMock.Setup(x => x.ServiceRequests).Returns(serviceRequestsMock.Object);

            // Act
            var result = await _handler.Handle(new UpdateUserRoleCommand(userId, newRole), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.ErrorMessage.Should().BeNull();
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
        }
    }
}