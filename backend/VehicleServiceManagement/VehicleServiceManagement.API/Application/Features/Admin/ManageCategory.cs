using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Admin;

namespace VehicleServiceManagement.API.Application.Features.Admin
{
    public record ManageCategoryCommand(ManageCategoryDto Dto) : IRequest<bool>;

    public class ManageCategoryHandler : IRequestHandler<ManageCategoryCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public ManageCategoryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(ManageCategoryCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            if (dto.Id == 0)
            {
                var newCategory = new ServiceCategory
                {
                    Name = dto.Name.Trim(),
                    BasePrice = dto.BasePrice,
                    Description = dto.Description?.Trim() ?? string.Empty,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ServiceCategories.Add(newCategory);
            }
            else
            {
                var existing = await _context.ServiceCategories
                    .FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

                if (existing == null) return false;

                existing.Name = dto.Name.Trim();
                existing.BasePrice = dto.BasePrice;
                existing.Description = dto.Description?.Trim() ?? string.Empty;
                existing.IsActive = dto.IsActive;

                _context.ServiceCategories.Update(existing);
            }

            var result = await _context.SaveChangesAsync(cancellationToken);
            return result > 0;
        }
    }
}