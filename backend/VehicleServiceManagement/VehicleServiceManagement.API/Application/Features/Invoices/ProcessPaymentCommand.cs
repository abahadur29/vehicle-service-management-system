using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Application.DTOs.Services; 

namespace VehicleServiceManagement.API.Application.Features.Invoices
{
    public record ProcessPaymentCommand(int InvoiceId, string? UserId = null, string? UserRole = null) : IRequest<(bool Success, string? ErrorMessage)>;

    public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, (bool Success, string? ErrorMessage)>
    {
        private readonly IApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProcessPaymentHandler(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool Success, string? ErrorMessage)> Handle(ProcessPaymentCommand request, CancellationToken ct)
        {
            var invoice = await _context.Invoices
                .Include(i => i.ServiceRequest)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct);
            
            if (invoice == null) 
                return (false, "Invoice not found.");

            if (invoice.PaymentStatus == "Paid")
                return (false, "Invoice has already been paid.");

            if (!string.IsNullOrEmpty(request.UserId) && !string.IsNullOrEmpty(request.UserRole))
            {
                var isAdmin = request.UserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                
                if (!isAdmin && invoice.ServiceRequest?.CustomerId != request.UserId)
                {
                    return (false, "You do not have permission to pay this invoice.");
                }
            }

            invoice.PaymentStatus = "Paid";

            if (invoice.ServiceRequest != null)
            {
                invoice.ServiceRequest.Status = "Closed"; 
            }

            await _context.SaveChangesAsync(ct);

            if (invoice.ServiceRequest != null)
            {
                var managers = await _userManager.GetUsersInRoleAsync("Manager");
                var managerIds = managers.Select(m => m.Id).ToList();
                var targetUserIds = new List<string>(managerIds);
                
                if (!string.IsNullOrEmpty(invoice.ServiceRequest.CustomerId))
                {
                    targetUserIds.Add(invoice.ServiceRequest.CustomerId);
                }
                
                await ServiceNotificationQueue.Channel.Writer.WriteAsync(new ServiceChangeEventDto
                {
                    ServiceRequestId = invoice.ServiceRequest.Id,
                    Action = "Updated",
                    Changes = new Dictionary<string, (string OldValue, string NewValue)>
                    {
                        { "Status", ("Completed", "Closed") }
                    },
                    TargetUserIds = targetUserIds
                }, ct);
            }

            return (true, null);
        }
    }
}