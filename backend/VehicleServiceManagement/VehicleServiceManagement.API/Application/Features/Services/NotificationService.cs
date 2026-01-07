namespace VehicleServiceManagement.API.Infrastructure.Services
{
    public interface INotificationService { void Send(string userEmail, string message); }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        public NotificationService(ILogger<NotificationService> logger) => _logger = logger;

        public void Send(string userEmail, string message)
        {
            _logger.LogInformation($"NOTIFICATION SENT TO {userEmail}: {message}");
        }
    }
}