using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Application.DTOs.Services;

namespace VehicleServiceManagement.API.Application.Features.Invoices
{
    public record GetInvoiceByServiceQuery(int ServiceRequestId, string? UserId = null, string? UserRole = null) : IRequest<InvoiceDto?>;

    public class GetInvoiceByServiceHandler : IRequestHandler<GetInvoiceByServiceQuery, InvoiceDto?>
    {
        private readonly IApplicationDbContext _context;
        public GetInvoiceByServiceHandler(IApplicationDbContext context) => _context = context;

        public async Task<InvoiceDto?> Handle(GetInvoiceByServiceQuery request, CancellationToken cancellationToken)
        {
            var serviceRequest = await _context.ServiceRequests
                .FirstOrDefaultAsync(s => s.Id == request.ServiceRequestId, cancellationToken);

            if (serviceRequest == null)
            {
                return null; 
            }

            if (!string.IsNullOrEmpty(request.UserId) && !string.IsNullOrEmpty(request.UserRole))
            {
                var isAdmin = request.UserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                var isManager = request.UserRole.Equals("Manager", StringComparison.OrdinalIgnoreCase);
                
                if (!isAdmin && !isManager)
                {
                    if (serviceRequest.CustomerId != request.UserId)
                    {
                        return null;
                    }
                }
            }

            var invoice = await _context.Invoices
                .Include(i => i.ServiceRequest)
                    .ThenInclude(s => s!.ServiceCategory)
                .Include(i => i.ServiceRequest)
                    .ThenInclude(s => s!.UsedParts)
                        .ThenInclude(sp => sp.Part)
                .FirstOrDefaultAsync(i => i.ServiceRequestId == request.ServiceRequestId, cancellationToken);

            if (invoice == null)
            {
                return null;
            }

            if (invoice.ServiceRequest == null)
            {
                invoice.ServiceRequest = await _context.ServiceRequests
                    .Include(s => s.ServiceCategory)
                    .Include(s => s.UsedParts)
                        .ThenInclude(sp => sp.Part)
                    .FirstOrDefaultAsync(s => s.Id == invoice.ServiceRequestId, cancellationToken);
            }

            if (invoice.ServiceRequest == null)
            {
                return null;
            }

            return new InvoiceDto
            {
                InvoiceId = invoice.Id,
                ServiceRequestId = invoice.ServiceRequestId,
                TotalAmount = invoice.TotalAmount,
                LabourFee = invoice.ServiceRequest.ServiceCategory != null ? invoice.ServiceRequest.ServiceCategory.BasePrice : 0,
                IssuedDate = invoice.IssuedDate,
                PaymentStatus = invoice.PaymentStatus ?? "Pending",
                PartsUsed = invoice.ServiceRequest.UsedParts.Select(sp => new PartUsageDetailDto
                {
                    PartName = sp.Part?.Name ?? "Unknown Part",
                    Quantity = sp.QuantityUsed,
                    PricePerUnit = sp.Part?.UnitPrice ?? 0
                }).ToList()
            };
        }
    }
}