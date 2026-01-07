using MediatR;
using Microsoft.AspNetCore.Identity;
using VehicleServiceManagement.API.Core.Entities;

namespace VehicleServiceManagement.API.Application.Features.Users
{
    public record GetTechniciansQuery() : IRequest<List<StaffUserDto>>;

    public record StaffUserDto(string Id, string FullName);

    public class GetTechniciansHandler : IRequestHandler<GetTechniciansQuery, List<StaffUserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetTechniciansHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<StaffUserDto>> Handle(GetTechniciansQuery request, CancellationToken cancellationToken)
        {
            var technicians = await _userManager.GetUsersInRoleAsync("Technician");

            return technicians.Select(u => new StaffUserDto(u.Id, u.FullName)).ToList();
        }
    }
}