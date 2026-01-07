using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Inventory;
using VehicleServiceManagement.API.Application.DTOs;

namespace VehicleServiceManagement.API.Application.Features.Inventory
{
    public record GetAllPartsQuery(string? UserRole = null) : IRequest<List<PartDto>>;
    public record GetLowStockQuery() : IRequest<List<PartDto>>;
    public record AddPartCommand(CreatePartDto Dto) : IRequest<Part>;
    public record UpdatePriceCommand(int PartId, decimal NewPrice) : IRequest<bool>;
    public record ToggleActiveCommand(int PartId) : IRequest<bool>;

    public class InventoryHandlers :
        IRequestHandler<GetAllPartsQuery, List<PartDto>>,
        IRequestHandler<GetLowStockQuery, List<PartDto>>,
        IRequestHandler<AddPartCommand, Part>,
        IRequestHandler<UpdatePriceCommand, bool>,
        IRequestHandler<ToggleActiveCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        public InventoryHandlers(IApplicationDbContext context) => _context = context;

        public async Task<List<PartDto>> Handle(GetAllPartsQuery request, CancellationToken ct)
        {
            var parts = await _context.Parts.ToListAsync(ct);
            var isTechnician = request.UserRole?.Equals("Technician", StringComparison.OrdinalIgnoreCase) == true;
            
            return parts.Select(p => new PartDto
            {
                Id = p.Id,
                Name = p.Name,
                UnitPrice = isTechnician ? null : p.UnitPrice,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive
            }).ToList();
        }

        public async Task<List<PartDto>> Handle(GetLowStockQuery request, CancellationToken ct)
        {
            var parts = await _context.Parts.Where(p => p.StockQuantity < 5 && p.IsActive).ToListAsync(ct);
            return parts.Select(p => new PartDto
            {
                Id = p.Id,
                Name = p.Name,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive
            }).ToList();
        }   

        public async Task<Part> Handle(AddPartCommand request, CancellationToken ct)
        {
            var part = new Part
            {
                Name = request.Dto.Name,
                StockQuantity = request.Dto.StockQuantity,
                UnitPrice = request.Dto.UnitPrice,
                IsActive = true
            };
            _context.Parts.Add(part);
            await _context.SaveChangesAsync(ct);
            return part;
        }

        public async Task<bool> Handle(UpdatePriceCommand request, CancellationToken ct)
        {
            var part = await _context.Parts.FindAsync(new object[] { request.PartId }, ct);
            if (part == null) return false;
            
            part.UnitPrice = request.NewPrice;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> Handle(ToggleActiveCommand request, CancellationToken ct)
        {
            var part = await _context.Parts.FindAsync(new object[] { request.PartId }, ct);
            if (part == null) return false;
            
            part.IsActive = !part.IsActive;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}