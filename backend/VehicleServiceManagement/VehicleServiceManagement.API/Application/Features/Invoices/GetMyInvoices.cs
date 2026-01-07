using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Application.DTOs.Services;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Application.Features.Invoices
{
    public record GetMyInvoicesQuery(string UserId) : IRequest<List<InvoiceDto>>;

    public class GetMyInvoicesHandler : IRequestHandler<GetMyInvoicesQuery, List<InvoiceDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetMyInvoicesHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InvoiceDto>> Handle(GetMyInvoicesQuery request, CancellationToken cancellationToken)
        {
            var invoices = await _context.Invoices
                .Include(i => i.ServiceRequest)
                    .ThenInclude(s => s!.ServiceCategory)
                .Include(i => i.ServiceRequest)
                    .ThenInclude(s => s!.UsedParts)
                        .ThenInclude(sp => sp.Part)
                .Where(i => i.ServiceRequest != null 
                    && i.ServiceRequest.CustomerId == request.UserId)
                .OrderByDescending(i => i.IssuedDate)
                .Select(i => new InvoiceDto
                {
                    InvoiceId = i.Id,
                    ServiceRequestId = i.ServiceRequestId,
                    TotalAmount = i.TotalAmount,
                    LabourFee = i.ServiceRequest!.ServiceCategory != null ? i.ServiceRequest.ServiceCategory.BasePrice : 0,
                    IssuedDate = i.IssuedDate,
                    PaymentStatus = i.PaymentStatus ?? "Pending",
                    PartsUsed = i.ServiceRequest!.UsedParts.Select(sp => new PartUsageDetailDto
                    {
                        PartName = sp.Part != null ? sp.Part.Name : "Unknown Part",
                        Quantity = sp.QuantityUsed,
                        PricePerUnit = sp.Part != null ? sp.Part.UnitPrice : 0
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return invoices;
        }
    }
}

