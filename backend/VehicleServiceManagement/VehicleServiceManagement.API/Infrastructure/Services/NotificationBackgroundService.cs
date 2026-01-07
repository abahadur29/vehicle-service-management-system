using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Core.Entities;
using VehicleServiceManagement.API.Core.Interfaces;

namespace VehicleServiceManagement.API.Infrastructure.Services
{
    public class ServiceNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ServiceNotificationBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var evt in ServiceNotificationQueue.Channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var serviceRequest = await db.ServiceRequests
                    .Include(sr => sr.Technician)
                    .FirstOrDefaultAsync(sr => sr.Id == evt.ServiceRequestId, stoppingToken);

                    if (serviceRequest == null)
                    {
                        Console.WriteLine($"Warning: Service request {evt.ServiceRequestId} not found for notification processing.");
                        continue;
                    }

                    Console.WriteLine($"Processing notification for service {evt.ServiceRequestId}, Action: {evt.Action}, TargetUsers: {string.Join(", ", evt.TargetUserIds)}");

                var targetUserIds = evt.TargetUserIds;
                if (!targetUserIds.Any())
                {
                    targetUserIds = DetermineTargetUsers(serviceRequest, evt.Action, evt.Changes);
                }

                    var managers = await userManager.GetUsersInRoleAsync("Manager");
                    var onlyManagers = managers.Select(u => u.Id).Distinct().ToList();

                if (evt.Action == "Added")
                {
                    var firstChange = evt.Changes.FirstOrDefault();
                    
                    foreach (var targetUserId in targetUserIds)
                    {
                        var existingNotification = await db.ServiceChangeHistories
                            .FirstOrDefaultAsync(
                                n => n.ServiceRequestId == evt.ServiceRequestId &&
                                     n.TargetUserId == targetUserId &&
                                     n.Action == "Added" &&
                                     n.ChangedOn > DateTime.UtcNow.AddMinutes(-1),
                                stoppingToken);

                        if (existingNotification != null) continue;

                        var targetUser = await userManager.FindByIdAsync(targetUserId);
                        var userRoles = targetUser != null ? await userManager.GetRolesAsync(targetUser) : new List<string>();
                        var isManagerOrAdmin = userRoles.Any(r => r == "Manager" || r == "Admin");

                        string friendlyMessage = GenerateFriendlyMessage(
                            evt.ServiceRequestId,
                            evt.Action,
                            firstChange.Key,
                            firstChange.Value.OldValue,
                            firstChange.Value.NewValue,
                            targetUserId == serviceRequest.CustomerId ? "Customer" :
                            targetUserId == serviceRequest.TechnicianId ? "Technician" :
                            isManagerOrAdmin ? "Manager" : "User"
                        );

                        db.ServiceChangeHistories.Add(new ServiceChangeHistory
                        {
                            ServiceRequestId = evt.ServiceRequestId,
                            TargetUserId = targetUserId,
                            Action = evt.Action,
                            Message = friendlyMessage,
                            FieldName = firstChange.Key,
                            OldValue = firstChange.Value.OldValue,
                            NewValue = firstChange.Value.NewValue,
                            ChangedOn = DateTime.UtcNow
                        });
                    }

                    if (firstChange.Key == "Status")
                        {
                            foreach (var managerId in onlyManagers)
                            {
                                if (targetUserIds.Contains(managerId)) continue;

                            var existingOversightNotification = await db.ServiceChangeHistories
                                .FirstOrDefaultAsync(
                                    n => n.ServiceRequestId == evt.ServiceRequestId &&
                                             n.TargetUserId == managerId &&
                                         n.Action == "Added" &&
                                         n.ChangedOn > DateTime.UtcNow.AddMinutes(-1),
                                    stoppingToken);

                            if (existingOversightNotification != null) continue;

                            string oversightMessage = GenerateFriendlyMessage(
                                evt.ServiceRequestId,
                                evt.Action,
                                firstChange.Key,
                                firstChange.Value.OldValue,
                                firstChange.Value.NewValue,
                                "Manager"
                            );

                            db.ServiceChangeHistories.Add(new ServiceChangeHistory
                            {
                                ServiceRequestId = evt.ServiceRequestId,
                                    TargetUserId = managerId,
                                Action = evt.Action,
                                Message = oversightMessage,
                                FieldName = firstChange.Key,
                                OldValue = firstChange.Value.OldValue,
                                NewValue = firstChange.Value.NewValue,
                                ChangedOn = DateTime.UtcNow
                            });
                        }
                    }
                }
                else
                {
                        bool hasTechnicianChange = evt.Changes.ContainsKey("TechnicianId");
                        bool hasStatusChange = evt.Changes.ContainsKey("Status");
                        bool isCombinedAssignment = hasTechnicianChange && hasStatusChange;

                        if (isCombinedAssignment && !string.IsNullOrEmpty(serviceRequest.CustomerId))
                        {
                            var customerId = serviceRequest.CustomerId;
                            
                            var existingCombinedNotification = await db.ServiceChangeHistories
                                .FirstOrDefaultAsync(
                                    n => n.ServiceRequestId == evt.ServiceRequestId &&
                                         n.TargetUserId == customerId &&
                                         n.Action == evt.Action &&
                                         n.FieldName == "TechnicianId" &&
                                         n.ChangedOn > DateTime.UtcNow.AddMinutes(-1),
                                    stoppingToken);

                            if (existingCombinedNotification == null)
                            {
                                var statusChange = evt.Changes["Status"];
                                var techChange = evt.Changes["TechnicianId"];
                                
                                string combinedMessage = GenerateCombinedMessage(
                                    evt.ServiceRequestId,
                                    statusChange.NewValue,
                                    techChange.NewValue
                                );

                                db.ServiceChangeHistories.Add(new ServiceChangeHistory
                                {
                                    ServiceRequestId = evt.ServiceRequestId,
                                    TargetUserId = customerId,
                                    Action = evt.Action,
                                    Message = combinedMessage,
                                    FieldName = "TechnicianId",
                                    OldValue = techChange.OldValue,
                                    NewValue = techChange.NewValue,
                                    ChangedOn = DateTime.UtcNow
                                });
                            }
                        }

                        if (isCombinedAssignment && !string.IsNullOrEmpty(serviceRequest.TechnicianId))
                        {
                            var technicianId = serviceRequest.TechnicianId;
                            
                            var existingCombinedTechnicianNotification = await db.ServiceChangeHistories
                                .FirstOrDefaultAsync(
                                    n => n.ServiceRequestId == evt.ServiceRequestId &&
                                         n.TargetUserId == technicianId &&
                                         n.Action == evt.Action &&
                                         n.FieldName == "TechnicianId" &&
                                         n.ChangedOn > DateTime.UtcNow.AddMinutes(-1),
                                    stoppingToken);

                            if (existingCombinedTechnicianNotification == null)
                            {
                                var statusChange = evt.Changes["Status"];
                                var techChange = evt.Changes["TechnicianId"];
                                
                                string combinedTechnicianMessage = GenerateCombinedTechnicianMessage(
                                    evt.ServiceRequestId,
                                    statusChange.NewValue
                                );

                                db.ServiceChangeHistories.Add(new ServiceChangeHistory
                                {
                                    ServiceRequestId = evt.ServiceRequestId,
                                    TargetUserId = technicianId,
                                    Action = evt.Action,
                                    Message = combinedTechnicianMessage,
                                    FieldName = "TechnicianId",
                                    OldValue = techChange.OldValue,
                                    NewValue = techChange.NewValue,
                                    ChangedOn = DateTime.UtcNow
                                });
                            }
                        }

                        if (isCombinedAssignment)
                        {
                            var statusChange = evt.Changes["Status"];
                            var techChange = evt.Changes["TechnicianId"];
                            
                            foreach (var managerId in onlyManagers)
                            {
                                if (targetUserIds.Contains(managerId)) continue;
                                
                                var existingCombinedManagerNotification = await db.ServiceChangeHistories
                                    .FirstOrDefaultAsync(
                                        n => n.ServiceRequestId == evt.ServiceRequestId &&
                                             n.TargetUserId == managerId &&
                                             n.Action == evt.Action &&
                                             n.FieldName == "TechnicianId" &&
                                             n.ChangedOn > DateTime.UtcNow.AddMinutes(-1),
                                        stoppingToken);

                                if (existingCombinedManagerNotification == null)
                                {
                                    string combinedManagerMessage = GenerateCombinedManagerMessage(
                                        evt.ServiceRequestId,
                                        statusChange.NewValue
                                    );

                                    db.ServiceChangeHistories.Add(new ServiceChangeHistory
                                    {
                                        ServiceRequestId = evt.ServiceRequestId,
                                        TargetUserId = managerId,
                                        Action = evt.Action,
                                        Message = combinedManagerMessage,
                                        FieldName = "TechnicianId",
                                        OldValue = techChange.OldValue,
                                        NewValue = techChange.NewValue,
                                        ChangedOn = DateTime.UtcNow
                                    });
                                }
                            }
                        }

                        foreach (var change in evt.Changes)
                        {
                            foreach (var targetUserId in targetUserIds)
                            {
                                var isCustomer = targetUserId == serviceRequest.CustomerId;
                                var isTechnician = targetUserId == serviceRequest.TechnicianId;
                                
                                if (isCombinedAssignment && (change.Key == "TechnicianId" || change.Key == "Status"))
                                {
                                    if (isCustomer || isTechnician)
                                    {
                                        var existingCombined = await db.ServiceChangeHistories
                                            .FirstOrDefaultAsync(
                                                n => n.ServiceRequestId == evt.ServiceRequestId &&
                                                     n.TargetUserId == targetUserId &&
                                                     n.Action == evt.Action &&
                                                     n.FieldName == "TechnicianId" &&
                                                     n.ChangedOn > DateTime.UtcNow.AddMinutes(-1),
                                                stoppingToken);
                                        if (existingCombined != null)
                                        {
                                            continue;
                                        }
                                    }
                                    continue;
                                }

                                var existingNotification = await db.ServiceChangeHistories
                                    .FirstOrDefaultAsync(
                                        n => n.ServiceRequestId == evt.ServiceRequestId &&
                                             n.TargetUserId == targetUserId &&
                                             n.Action == evt.Action &&
                                             n.FieldName == change.Key &&
                                             n.NewValue == change.Value.NewValue &&
                                             n.ChangedOn > DateTime.UtcNow.AddMinutes(-1),
                                        stoppingToken);

                                if (existingNotification != null)
                                {
                                    continue;
                                }

                            var targetUser = await userManager.FindByIdAsync(targetUserId);
                            var userRoles = targetUser != null ? await userManager.GetRolesAsync(targetUser) : new List<string>();
                            var isManagerOrAdmin = userRoles.Any(r => r == "Manager" || r == "Admin");
                                
                                string userRole;
                                if (targetUserId == serviceRequest.CustomerId)
                                {
                                    userRole = "Customer";
                                }
                                else if (targetUserId == serviceRequest.TechnicianId)
                                {
                                    userRole = "Technician";
                                }
                                else if (isManagerOrAdmin)
                                {
                                    userRole = "Manager";
                                }
                                else
                                {
                                    userRole = "User";
                                }
                                
                            string friendlyMessage = GenerateFriendlyMessage(
                                evt.ServiceRequestId,
                                evt.Action,
                                change.Key,
                                change.Value.OldValue,
                                change.Value.NewValue,
                                    userRole
                            );

                                if (string.IsNullOrEmpty(friendlyMessage))
                                {
                                    continue;
                                }

                                var notification = new ServiceChangeHistory
                            {
                                ServiceRequestId = evt.ServiceRequestId,
                                TargetUserId = targetUserId,
                                Action = evt.Action,
                                Message = friendlyMessage,
                                FieldName = change.Key,
                                OldValue = change.Value.OldValue,
                                NewValue = change.Value.NewValue,
                                ChangedOn = DateTime.UtcNow
                                };
                                
                                db.ServiceChangeHistories.Add(notification);
                                Console.WriteLine($"Created notification for user {targetUserId}: {friendlyMessage}");
                        }

                        if (change.Key == "Status" || change.Key == "TechnicianId")
                            {
                                foreach (var managerId in onlyManagers)
                                {
                                    if (targetUserIds.Contains(managerId)) continue;

                                    if (isCombinedAssignment && (change.Key == "TechnicianId" || change.Key == "Status"))
                                    {
                                        continue;
                                    }

                                var existingOversightNotification = await db.ServiceChangeHistories
                                    .FirstOrDefaultAsync(
                                        n => n.ServiceRequestId == evt.ServiceRequestId &&
                                                 n.TargetUserId == managerId &&
                                             n.Action == evt.Action &&
                                             n.FieldName == change.Key &&
                                             n.ChangedOn > DateTime.UtcNow.AddMinutes(-1),
                                        stoppingToken);

                                if (existingOversightNotification != null) continue;

                                string oversightMessage = GenerateFriendlyMessage(
                                    evt.ServiceRequestId,
                                    evt.Action,
                                    change.Key,
                                    change.Value.OldValue,
                                    change.Value.NewValue,
                                    "Manager"
                                );

                                db.ServiceChangeHistories.Add(new ServiceChangeHistory
                                {
                                    ServiceRequestId = evt.ServiceRequestId,
                                        TargetUserId = managerId,
                                    Action = evt.Action,
                                    Message = oversightMessage,
                                    FieldName = change.Key,
                                    OldValue = change.Value.OldValue,
                                    NewValue = change.Value.NewValue,
                                    ChangedOn = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }

                    var savedCount = await db.SaveChangesAsync(stoppingToken);
                    Console.WriteLine($"Saved {savedCount} notification(s) for service {evt.ServiceRequestId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR processing notification for service {evt.ServiceRequestId}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }
        }

        private List<string> DetermineTargetUsers(ServiceRequest serviceRequest, string action, Dictionary<string, (string OldValue, string NewValue)> changes)
        {
            var targets = new List<string>();

            if (!string.IsNullOrEmpty(serviceRequest.CustomerId))
            {
                targets.Add(serviceRequest.CustomerId);
            }

            if (changes.ContainsKey("TechnicianId") && !string.IsNullOrEmpty(serviceRequest.TechnicianId))
            {
                targets.Add(serviceRequest.TechnicianId);
            }

            if (changes.ContainsKey("Status") && !string.IsNullOrEmpty(serviceRequest.TechnicianId))
            {
                if (!targets.Contains(serviceRequest.TechnicianId))
                {
                    targets.Add(serviceRequest.TechnicianId);
                }
            }

            return targets.Distinct().ToList();
        }

        private string GenerateCombinedMessage(int requestId, string statusValue, string technicianId)
        {
            return $"A technician has been assigned to work on your vehicle for service #{requestId}. Your service is now {statusValue}.";
        }

        private string GenerateCombinedTechnicianMessage(int requestId, string statusValue)
        {
            return $"You have been assigned to service request #{requestId}. Status updated to '{statusValue}'.";
        }

        private string GenerateCombinedManagerMessage(int requestId, string statusValue)
        {
            return $"Technician assigned to service request #{requestId} and status changed to '{statusValue}'.";
        }

        private string GenerateFriendlyMessage(int requestId, string action, string fieldName, string oldValue, string newValue, string userRole)
        {
            if (action == "Added")
            {
                return userRole == "Customer"
                    ? $"Your service request #{requestId} has been successfully booked."
                    : $"New service request #{requestId} has been created.";
            }

            return fieldName switch
            {
                "Status" => (oldValue, newValue, userRole) switch
                {
                    ("Assigned", "In Progress", "Technician") => $"You have started working on Service Request #{requestId}.",
                    ("Assigned", "In Progress", "Customer") => $"The technician has started working on your service #{requestId}.",
                    (_, "Completed", "Customer") => $"Your service #{requestId} has been completed. Please complete the payment to collect your vehicle.",
                    (_, "Closed", "Customer") => $"The status of your service #{requestId} has been updated to 'Closed' and payment is done.",
                    (_, "Closed", "Manager") => $"Service request #{requestId} has been closed and the payment is done.",
                    (_, _, "Customer") => $"The status of your service #{requestId} has been updated to '{newValue}'.",
                    (_, _, "Technician") => $"Service request #{requestId} status has been updated to '{newValue}'.",
                    (_, _, "Manager") => $"Service request #{requestId} status changed to '{newValue}'.",
                    _ => $"Service #{requestId} status updated to '{newValue}'."
                },
                "TechnicianId" => userRole switch
                {
                    "Customer" => $"A technician has been assigned to work on your vehicle for service #{requestId}.",
                    "Technician" => $"You have been assigned to service request #{requestId}.",
                    "Manager" => $"Technician assigned to service request #{requestId}.",
                    _ => $"Technician assigned to service #{requestId}."
                },
                "RequestedDate" => userRole switch
                {
                    "Customer" => $"The scheduled date for your service #{requestId} has been changed to {newValue}.",
                    "Technician" => $"The scheduled date for service request #{requestId} has been changed to {newValue}.",
                    "Manager" => $"Service request #{requestId} scheduled date changed to {newValue}.",
                    _ => $"Service #{requestId} scheduled date updated to {newValue}."
                },
                _ => ""
            };
        }
    }
}