using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Admin
{
    public record GetCategoriesQuery(bool ActiveOnly = false) : IRequest<List<ServiceCategory>>;

    public class AdminHandlers : IRequestHandler<GetCategoriesQuery, List<ServiceCategory>>
    {
        private readonly IApplicationDbContext _context;
        public AdminHandlers(IApplicationDbContext context) => _context = context;

        public async Task<List<ServiceCategory>> Handle(GetCategoriesQuery request, CancellationToken ct)
        {
            var query = _context.ServiceCategories.AsQueryable();
            
            if (request.ActiveOnly)
            {
                query = query.Where(c => c.IsActive);
            }
            
            if (request.ActiveOnly)
            {
                return await query.OrderBy(c => c.Name).ToListAsync(ct);
            }
            else
            {
                return await query.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
            }
        }
    }
}