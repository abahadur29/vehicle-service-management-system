using MediatR;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

public record ApproveServiceCommand(int RequestId) : IRequest<bool>;

public class ApproveServiceHandler : IRequestHandler<ApproveServiceCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public ApproveServiceHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(ApproveServiceCommand request, CancellationToken ct)
    {
        var service = await _context.ServiceRequests
            .Include(s => s.ServiceCategory)
            .Include(s => s.UsedParts).ThenInclude(up => up.Part)
            .FirstOrDefaultAsync(s => s.Id == request.RequestId, ct);

        if (service == null) return false;

        if (service.Status == "Completed")
        {
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.ServiceRequestId == service.Id, ct);
            
            if (existingInvoice != null)
            {
                return true;
            }
            
            // Calculate total: parts cost + service fee
            decimal partsCost = service.UsedParts.Sum(up => (up.Part?.UnitPrice ?? 0) * up.QuantityUsed);
            decimal laborFee = service.ServiceCategory?.BasePrice ?? 0;
            
            var invoice = new Invoice
            {
                ServiceRequestId = service.Id,
                TotalAmount = partsCost + laborFee,
                IssuedDate = DateTime.UtcNow,
                PaymentStatus = "Pending"
            };
            
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        if (service.Status == "Pending Review")
        {
            // Calculate total: parts cost + service fee
            decimal partsCost = service.UsedParts.Sum(up => (up.Part?.UnitPrice ?? 0) * up.QuantityUsed);
            decimal laborFee = service.ServiceCategory?.BasePrice ?? 0;

            var invoice = new Invoice
            {
                ServiceRequestId = service.Id,
                TotalAmount = partsCost + laborFee,
                IssuedDate = DateTime.UtcNow,
                PaymentStatus = "Pending"
            };

            service.Status = "Completed";
            service.CompletionDate = DateTime.UtcNow;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        return false;
    }
}