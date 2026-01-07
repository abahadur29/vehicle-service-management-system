using MediatR;
using Microsoft.AspNetCore.Identity;
using VehicleServiceManagement.API.Application.DTOs.Auth;
using VehicleServiceManagement.API.Core.Entities;

namespace VehicleServiceManagement.API.Application.Features.Auth
{
    public record RegisterUserCommand(RegisterRequestDto Dto) : IRequest<bool>;

    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = request.Dto.Email,
                Email = request.Dto.Email,
                FullName = request.Dto.FullName,
                PhoneNumber = request.Dto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, request.Dto.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                return roleResult.Succeeded;
            }
            return false;
        }
    }
}