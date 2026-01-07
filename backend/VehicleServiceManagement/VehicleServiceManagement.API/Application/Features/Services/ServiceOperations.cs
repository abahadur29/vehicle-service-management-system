using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Services;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record GetServiceNotificationsQuery(string UserId) : IRequest<List<ServiceChangeHistory>>;

    public record CancelBookingCommand(int RequestId) : IRequest<bool>;

    public class ServiceOperationsHandler :
        IRequestHandler<GetServiceNotificationsQuery, List<ServiceChangeHistory>>,
        IRequestHandler<CancelBookingCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public ServiceOperationsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceChangeHistory>> Handle(GetServiceNotificationsQuery request, CancellationToken ct)
        {
            return await _context.ServiceChangeHistories
                .Where(sch => sch.TargetUserId == request.UserId)
                .OrderByDescending(x => x.ChangedOn)
                .Take(20)
                .ToListAsync(ct);
        }

        public async Task<bool> Handle(CancelBookingCommand request, CancellationToken ct)
        {
            var booking = await _context.ServiceRequests.FindAsync(new object[] { request.RequestId }, ct);

            if (booking == null)
                return false;

            if (booking.Status != "Requested" || !string.IsNullOrEmpty(booking.TechnicianId))
                return false;

            booking.Status = "Cancelled";
            await ServiceNotificationQueue.Channel.Writer.WriteAsync(new ServiceChangeEventDto
            {
                ServiceRequestId = booking.Id,
                Action = "Updated",
                Changes = new Dictionary<string, (string OldValue, string NewValue)>
                {
                    { "Status", ("Requested", "Cancelled") }
                },
                TargetUserIds = new List<string> { booking.CustomerId }
            }, ct);

            return await _context.SaveChangesAsync(ct) > 0;
        }
    }
}