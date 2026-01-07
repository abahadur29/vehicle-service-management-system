using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Inventory;

namespace VehicleServiceManagement.API.Application.Features.Inventory
{
    public record UpdateStockCommand(int PartId, int Quantity) : IRequest<UpdateStockResult>;

    public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, UpdateStockResult>
    {
        private readonly IApplicationDbContext _context;

        public UpdateStockHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UpdateStockResult> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == request.PartId, cancellationToken);

            if (part == null)
            {
                return new UpdateStockResult { Success = false, Message = "Part not found in inventory." };
            }

            if (part.StockQuantity + request.Quantity < 0)
            {
                return new UpdateStockResult { Success = false, Message = "Insufficient stock to perform this deduction." };
            }

            part.StockQuantity += request.Quantity;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateStockResult 
            { 
                Success = true, 
                Message = "Stock updated successfully.",
                NewQuantity = part.StockQuantity,
                PartName = part.Name
            };
        }
    }
}