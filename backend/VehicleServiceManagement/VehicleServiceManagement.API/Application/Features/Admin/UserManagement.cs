using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Admin;

namespace VehicleServiceManagement.API.Application.Features.Admin
{
    public record GetAllUsersQuery() : IRequest<List<UserManagementDto>>;
    public record GetAvailableTechniciansQuery() : IRequest<List<UserManagementDto>>;
    public record GetUserRoleDistributionQuery() : IRequest<List<UserRoleDistributionDto>>;

    public class UserRoleDistributionDto
    {
        public string RoleName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class UserManagementHandler :
        IRequestHandler<GetAllUsersQuery, List<UserManagementDto>>,
        IRequestHandler<GetAvailableTechniciansQuery, List<UserManagementDto>>,
        IRequestHandler<GetUserRoleDistributionQuery, List<UserRoleDistributionDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;

        public UserManagementHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<UserManagementDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync(cancellationToken);
            var userList = new List<UserManagementDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserManagementDto
                {
                    Id = user.Id,
                    FullName = user.FullName ?? "N/A",
                    Email = user.Email ?? "N/A",
                    Role = roles.FirstOrDefault() ?? "Customer",
                    IsActive = user.IsActive
                });
            }
            return userList;
        }

        public async Task<List<UserManagementDto>> Handle(GetAvailableTechniciansQuery request, CancellationToken cancellationToken)
        {
            var technicians = await _userManager.GetUsersInRoleAsync("Technician");
            var availableList = new List<UserManagementDto>();

            foreach (var tech in technicians)
            {
                if (!tech.IsActive)
                {
                    continue;
                }

                var activeTasks = await _context.ServiceRequests
                    .CountAsync(s => s.TechnicianId == tech.Id &&
                                (s.Status == "Assigned" || s.Status == "In Progress"),
                                cancellationToken);

                if (activeTasks < 5)
                {
                    availableList.Add(new UserManagementDto
                    {
                        Id = tech.Id,
                        FullName = tech.FullName ?? "N/A",
                        Email = tech.Email ?? "N/A",
                        Role = "Technician",
                        CurrentWorkload = activeTasks,
                        IsActive = tech.IsActive
                    });
                }
            }
            return availableList;
        }

        public async Task<List<UserRoleDistributionDto>> Handle(GetUserRoleDistributionQuery request, CancellationToken cancellationToken)
        {
            var roles = new[] { "Customer", "Technician", "Manager" };
            var distribution = new List<UserRoleDistributionDto>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                distribution.Add(new UserRoleDistributionDto
                {
                    RoleName = role,
                    UserCount = usersInRole.Count
                });
            }

            return distribution;
        }
    }
}