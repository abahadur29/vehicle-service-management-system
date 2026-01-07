using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Services;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record CompleteServiceCommand(
        int RequestId,
        List<PartUsageDto> PartsUsed,
        string Remarks) : IRequest<bool>;

    public class CompleteServiceHandler : IRequestHandler<CompleteServiceCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        public CompleteServiceHandler(IApplicationDbContext context) => _context = context;

        public async Task<bool> Handle(CompleteServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.ServiceRequests
                .Include(s => s.ServiceCategory)
                .Include(s => s.UsedParts)
                    .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(s => s.Id == request.RequestId, cancellationToken);

            if (service == null) return false;

            foreach (var item in request.PartsUsed)
            {
                var part = await _context.Parts.FindAsync(new object[] { item.PartId }, cancellationToken);

                if (part != null && part.StockQuantity >= item.Quantity)
                {
                    part.StockQuantity -= item.Quantity;

                    var usedPart = new ServiceRequestPart
                    {
                        ServiceRequestId = service.Id,
                        PartId = item.PartId,
                        QuantityUsed = item.Quantity
                    };
                    _context.ServiceRequestParts.Add(usedPart);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            service = await _context.ServiceRequests
                .Include(s => s.ServiceCategory)
                .Include(s => s.UsedParts)
                    .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(s => s.Id == request.RequestId, cancellationToken);

            if (service == null) return false;

            // Add up all parts costs
            decimal calculatedPartsCost = service.UsedParts.Sum(up => (up.Part?.UnitPrice ?? 0) * up.QuantityUsed);
            // Get the service base price
            decimal laborFee = service.ServiceCategory?.BasePrice ?? 0;
            // Total = parts + service fee
            decimal totalAmount = calculatedPartsCost + laborFee;

            var invoice = new Invoice
            {
                ServiceRequestId = service.Id,
                TotalAmount = totalAmount,
                IssuedDate = DateTime.UtcNow,
                PaymentStatus = "Pending"
            };
            _context.Invoices.Add(invoice);

            service.Status = "Completed";
            service.CompletionDate = DateTime.UtcNow;
            service.Description += $"\n--- Technician Remarks ---\n{request.Remarks}";

            await _context.SaveChangesAsync(cancellationToken);

            var targetUserIds = new List<string>();
            if (!string.IsNullOrEmpty(service.CustomerId))
            {
                targetUserIds.Add(service.CustomerId);
            }
            if (!string.IsNullOrEmpty(service.TechnicianId) && !targetUserIds.Contains(service.TechnicianId))
            {
                targetUserIds.Add(service.TechnicianId);
            }

            if (!targetUserIds.Any())
            {
                throw new InvalidOperationException($"Cannot complete service {service.Id}: No target users found for notification.");
            }

            var notificationEvent = new ServiceChangeEventDto
            {
                ServiceRequestId = service.Id,
                Action = "Updated",
                Changes = new Dictionary<string, (string OldValue, string NewValue)>
                {
                    { "Status", ("In Progress", "Completed") }
                },
                TargetUserIds = targetUserIds
            };

            await ServiceNotificationQueue.Channel.Writer.WriteAsync(notificationEvent, cancellationToken);
            Console.WriteLine($"Completion notification queued for service {service.Id}, TargetUsers: {string.Join(", ", targetUserIds)}");

            return true;
        }
    }
}