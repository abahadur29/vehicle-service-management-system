using System.Threading.Channels;
using VehicleServiceManagement.API.Application.DTOs.Services;

namespace VehicleServiceManagement.API.Application.Features.Services
{
    public static class ServiceNotificationQueue
    {
        public static Channel<ServiceChangeEventDto> Channel =
            System.Threading.Channels.Channel.CreateUnbounded<ServiceChangeEventDto>();
    }
}