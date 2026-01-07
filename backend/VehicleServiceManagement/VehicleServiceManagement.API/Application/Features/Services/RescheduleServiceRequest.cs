using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Services;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record RescheduleServiceRequestCommand(int RequestId, DateTime NewRequestedDate, string CustomerId) : IRequest<bool>;

    public class RescheduleServiceRequestHandler : IRequestHandler<RescheduleServiceRequestCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public RescheduleServiceRequestHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(RescheduleServiceRequestCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.ServiceRequests
                .FirstOrDefaultAsync(s => s.Id == request.RequestId, cancellationToken);

            if (service == null) return false;

            if (service.CustomerId != request.CustomerId || service.Status != "Requested")
                return false;

            var oldDate = service.RequestedDate;
            service.RequestedDate = request.NewRequestedDate;

            await _context.SaveChangesAsync(cancellationToken);

            var targetUserIds = new List<string> { service.CustomerId };
            if (!string.IsNullOrEmpty(service.TechnicianId) && !targetUserIds.Contains(service.TechnicianId))
            {
                targetUserIds.Add(service.TechnicianId);
            }

            await ServiceNotificationQueue.Channel.Writer.WriteAsync(new ServiceChangeEventDto
            {
                ServiceRequestId = service.Id,
                Action = "Updated",
                Changes = new Dictionary<string, (string OldValue, string NewValue)>
                {
                    { "RequestedDate", (oldDate.ToString("dd/MM/yyyy"), request.NewRequestedDate.ToString("dd/MM/yyyy")) }
                },
                TargetUserIds = targetUserIds
            }, cancellationToken);

            return true;
        }
    }
}

