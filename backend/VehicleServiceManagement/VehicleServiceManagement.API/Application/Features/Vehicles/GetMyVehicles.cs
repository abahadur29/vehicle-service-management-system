using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Vehicles
{
    public record GetMyVehiclesQuery(string UserId) : IRequest<List<VehicleDto>>;

    public class GetMyVehiclesHandler : IRequestHandler<GetMyVehiclesQuery, List<VehicleDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetMyVehiclesHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<VehicleDto>> Handle(GetMyVehiclesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Vehicles
                .AsNoTracking() 
                .Where(v => v.UserId == request.UserId)
                .ProjectTo<VehicleDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}