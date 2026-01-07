using MediatR;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Services;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record UpdateServiceStatusCommand(
        int RequestId,
        string Status,
        string? TechnicianId = null,
        DateTime? NewDate = null,
        string? Priority = null) : IRequest<bool>;

    public class UpdateServiceStatusHandler : IRequestHandler<UpdateServiceStatusCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UpdateServiceStatusHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateServiceStatusCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.ServiceRequests.FindAsync(new object[] { request.RequestId }, cancellationToken);

            if (service == null) return false;

            var changes = new Dictionary<string, (string OldValue, string NewValue)>();

            if (!string.IsNullOrEmpty(request.Status) && service.Status != request.Status)
            {
                changes["Status"] = (service.Status ?? string.Empty, request.Status);
                service.Status = request.Status;
            }

            if (!string.IsNullOrEmpty(request.TechnicianId) && service.TechnicianId != request.TechnicianId)
            {
                changes["TechnicianId"] = (service.TechnicianId ?? "Unassigned", request.TechnicianId);
                service.TechnicianId = request.TechnicianId;
            }

            if (request.NewDate.HasValue && service.RequestedDate != request.NewDate.Value)
            {
                changes["RequestedDate"] = (service.RequestedDate.ToString("dd/MM/yyyy"), request.NewDate.Value.ToString("dd/MM/yyyy"));
                service.RequestedDate = request.NewDate.Value;
            }

            if (!string.IsNullOrEmpty(request.Priority) && service.Priority != request.Priority)
            {
                changes["Priority"] = (service.Priority ?? string.Empty, request.Priority);
                service.Priority = request.Priority;
            }

            await _context.SaveChangesAsync(cancellationToken);

            if (changes.Any())
            {
                var targetUserIds = new List<string>();
                
                if (!string.IsNullOrEmpty(service.CustomerId))
                {
                    targetUserIds.Add(service.CustomerId);
                }

                if (changes.ContainsKey("TechnicianId") && !string.IsNullOrEmpty(service.TechnicianId))
                {
                    if (!targetUserIds.Contains(service.TechnicianId))
                    {
                        targetUserIds.Add(service.TechnicianId);
                    }
                }
                else if (changes.ContainsKey("Status") && !string.IsNullOrEmpty(service.TechnicianId))
                {
                    if (!targetUserIds.Contains(service.TechnicianId))
                    {
                        targetUserIds.Add(service.TechnicianId);
                    }
                }

                await ServiceNotificationQueue.Channel.Writer.WriteAsync(new ServiceChangeEventDto
                {
                    ServiceRequestId = service.Id,
                    Action = "Updated",
                    Changes = changes,
                    TargetUserIds = targetUserIds
                }, cancellationToken);
            }

            return true;
        }
    }
}