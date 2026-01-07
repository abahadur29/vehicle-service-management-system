using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Admin
{
    public record ToggleUserActiveCommand(string UserId) : IRequest<(bool Success, string? ErrorMessage)>;

    public class ToggleUserActiveHandler : IRequestHandler<ToggleUserActiveCommand, (bool Success, string? ErrorMessage)>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;

        public ToggleUserActiveHandler(UserManager<ApplicationUser> userManager, IApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<(bool Success, string? ErrorMessage)> Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            {
                return (false, "Cannot deactivate Admin users.");
            }

            bool isCurrentlyActive = user.IsActive;

            if (isCurrentlyActive)
            {
                bool isTechnician = roles.Contains("Technician", StringComparer.OrdinalIgnoreCase);
                bool isManager = roles.Contains("Manager", StringComparer.OrdinalIgnoreCase);

                if (isTechnician || isManager)
                {
                    var activeServiceRequests = await _context.ServiceRequests
                        .Where(s => s.TechnicianId == user.Id &&
                                   (s.Status == "Requested" || s.Status == "Assigned" || s.Status == "In Progress"))
                        .CountAsync(cancellationToken);

                    if (activeServiceRequests > 0)
                    {
                        if (isTechnician)
                        {
                            return (false, "Technician cannot be deactivated as service requests are assigned to him.");
                        }
                        return (false, "User cannot be deactivated as service requests are assigned to him.");
                    }
                }
            }

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Failed to update user status: {errors}");
            }

            return (true, null);
        }
    }
}

