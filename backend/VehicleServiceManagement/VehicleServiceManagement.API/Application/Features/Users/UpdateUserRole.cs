using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Users
{
    public record UpdateUserRoleCommand(string UserId, string NewRole) : IRequest<(bool Success, string? ErrorMessage)>;

    public class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleCommand, (bool Success, string? ErrorMessage)>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IApplicationDbContext _context;

        public UpdateUserRoleHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage)> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) 
                return (false, "User not found.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault();

            if (currentRole?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true)
            {
                return (false, "Cannot modify Admin role. Admin role is immutable for security reasons.");
            }
            if (request.NewRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var existingAdmins = await _userManager.GetUsersInRoleAsync("Admin");
                if (existingAdmins.Any(a => a.Id != request.UserId))
                {
                    return (false, "Cannot assign Admin role. Only one Admin user is allowed in the system.");
                }
            }

            if (currentRole?.Equals("Technician", StringComparison.OrdinalIgnoreCase) == true && 
                !request.NewRole.Equals("Technician", StringComparison.OrdinalIgnoreCase))
            {
                var activeServiceRequests = await _context.ServiceRequests
                    .Where(s => s.TechnicianId == user.Id &&
                               (s.Status == "Requested" || s.Status == "Assigned" || s.Status == "In Progress"))
                    .CountAsync(cancellationToken);

                if (activeServiceRequests > 0)
                {
                    return (false, "Technician cannot be deactivated as service requests are assigned to him.");
                }
            }

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return (false, $"Failed to remove existing roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
            if (!addResult.Succeeded)
            {
                return (false, $"Failed to assign role: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
            }

            return (true, null);
        }
    }
}