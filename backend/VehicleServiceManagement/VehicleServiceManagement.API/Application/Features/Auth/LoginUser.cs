using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VehicleServiceManagement.API.Application.DTOs.Auth;
using VehicleServiceManagement.API.Core.Entities;

namespace VehicleServiceManagement.API.Application.Features.Auth
{
    public record LoginUserCommand(LoginRequestDto Dto) : IRequest<AuthResponseDto>;

    public class LoginUserHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public LoginUserHandler(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Dto.Password))
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid credentials" };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Your account has been deactivated. Please contact the administrator." };
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (!roles.Any())
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                roles = new List<string> { "Customer" };
            }

            var token = GenerateJwtToken(user, roles);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = token,
                FullName = user.FullName,
                Email = user.Email!,
                UserId = user.Id,
                Role = roles.FirstOrDefault() ?? "Customer",
                Message = "Login Successful"
            };
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}