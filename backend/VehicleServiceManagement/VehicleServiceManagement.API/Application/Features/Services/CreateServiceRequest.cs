using MediatR;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Application.DTOs;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;
using VehicleServiceManagement.API.Application.DTOs.Services;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public record CreateServiceRequestCommand(
        int VehicleId,
        int ServiceCategoryId,
        string Description,
        string Priority,
        DateTime RequestedDate,
        string UserId) : IRequest<ServiceRequestDto>;

    public class CreateServiceRequestHandler : IRequestHandler<CreateServiceRequestCommand, ServiceRequestDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateServiceRequestHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceRequestDto> Handle(CreateServiceRequestCommand request, CancellationToken cancellationToken)
        {
            DateTime requestedDate = request.RequestedDate;
            if (request.RequestedDate.Kind != DateTimeKind.Utc)
            {
                requestedDate = request.RequestedDate.ToUniversalTime();
            }
            
            if (requestedDate.TimeOfDay == TimeSpan.Zero)
            {
                var now = DateTime.UtcNow;
                requestedDate = new DateTime(
                    requestedDate.Year,
                    requestedDate.Month,
                    requestedDate.Day,
                    now.Hour,
                    now.Minute,
                    now.Second,
                    DateTimeKind.Utc
                );
            }
            
            var serviceRequest = new ServiceRequest
            {
                VehicleId = request.VehicleId,
                ServiceCategoryId = request.ServiceCategoryId,
                Description = request.Description ?? string.Empty,
                Priority = !string.IsNullOrEmpty(request.Priority) ? request.Priority : "Normal",
                RequestedDate = requestedDate,
                CustomerId = request.UserId,
                Status = "Requested"
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync(cancellationToken);

            await ServiceNotificationQueue.Channel.Writer.WriteAsync(new ServiceChangeEventDto
            {
                ServiceRequestId = serviceRequest.Id,
                Action = "Added",
                Changes = new Dictionary<string, (string OldValue, string NewValue)>
                {
                    { "Status", (string.Empty, "Requested") }
                },
                TargetUserIds = new List<string> { serviceRequest.CustomerId }
            }, cancellationToken);

            return new ServiceRequestDto
            {
                Id = serviceRequest.Id,
                Description = serviceRequest.Description ?? string.Empty,
                Status = serviceRequest.Status ?? string.Empty,
                Priority = serviceRequest.Priority ?? string.Empty,
                RequestedDate = serviceRequest.RequestedDate,
                VehicleModel = "Booking Confirmed"
            };
        }
    }
}