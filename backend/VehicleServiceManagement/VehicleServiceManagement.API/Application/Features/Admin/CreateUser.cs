using MediatR;
using Microsoft.AspNetCore.Identity;
using VehicleServiceManagement.API.Application.DTOs.Admin;
using VehicleServiceManagement.API.Core.Entities;

namespace VehicleServiceManagement.API.Application.Features.Admin
{
    public record CreateUserCommand(CreateUserDto Dto) : IRequest<(bool Success, string? ErrorMessage, string? UserId)>;

    public class CreateUserHandler : IRequestHandler<CreateUserCommand, (bool Success, string? ErrorMessage, string? UserId)>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Success, string? ErrorMessage, string? UserId)> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var allowedRoles = new[] { "Manager", "Technician", "Customer" };
            if (!allowedRoles.Contains(dto.Role, StringComparer.OrdinalIgnoreCase))
            {
                if (dto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "Admin role cannot be assigned. Only one Admin is allowed in the system.", null);
                }
                return (false, "Invalid role. Must be Manager, Technician, or Customer.", null);
            }

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (existingUser != null)
            {
                return (false, "Email is already registered.", null);
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email.Trim().ToLowerInvariant(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, $"User creation failed: {errors}", null);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                return (false, $"Role assignment failed: {errors}", null);
            }

            return (true, null, user.Id);
        }
    }
}

