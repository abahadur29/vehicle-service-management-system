using MediatR;
using Microsoft.AspNetCore.Identity;
using VehicleServiceManagement.API.Core.Entities;

namespace VehicleServiceManagement.API.Application.Features.Admin
{
    public record DeleteUserCommand(string UserId) : IRequest<(bool Success, string? ErrorMessage)>;

    public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, (bool Success, string? ErrorMessage)>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Success, string? ErrorMessage)> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            {
                return (false, "Cannot delete Admin users.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"Failed to delete user: {errors}");
            }

            return (true, null);
        }
    }
}

