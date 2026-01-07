using MediatR;
using VehicleServiceManagement.API.Application.DTOs; 
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Vehicles
{
    public record AddVehicleCommand(
        string LicensePlate,
        string Model,
        string Make,
        int Year,
        string UserId) : IRequest<VehicleDto>;

    public class AddVehicleHandler : IRequestHandler<AddVehicleCommand, VehicleDto>
    {
        private readonly IApplicationDbContext _context;
        public AddVehicleHandler(IApplicationDbContext context) => _context = context;

        public async Task<VehicleDto> Handle(AddVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = new Vehicle
            {
                LicensePlate = request.LicensePlate,
                Model = request.Model,
                Make = request.Make,
                Year = request.Year,
                UserId = request.UserId 
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync(cancellationToken);

            return new VehicleDto
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Model = vehicle.Model,
                Make = vehicle.Make,
                Year = vehicle.Year
            };
        }
    }
}