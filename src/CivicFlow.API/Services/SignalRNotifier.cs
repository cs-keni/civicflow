using CivicFlow.API.Hubs;
using CivicFlow.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CivicFlow.API.Services;

/// <summary>
/// Real-time notifier backed by SignalR. All sends are fire-and-forget (D12):
/// failures are logged but never propagated to the calling service.
/// </summary>
public sealed class SignalRNotifier(
    IHubContext<PermitStatusHub> permitHub,
    IHubContext<ReviewQueueHub> reviewQueueHub,
    IHubContext<InspectionHub> inspectionHub,
    IHubContext<AdminActivityHub> adminHub,
    ILogger<SignalRNotifier> logger) : IRealtimeNotifier
{
    public void NotifyPermitSubmitted(int permitId, string applicationNumber, string facilityName, string applicantId)
    {
        var payload = new { permitId, applicationNumber, facilityName, applicantId, timestamp = DateTime.UtcNow };

        Fire(reviewQueueHub.Clients.Group("staff-reviewers").SendAsync("PermitSubmitted", payload));
        Fire(adminHub.Clients.Group("admin-feed").SendAsync("AdminActivity", new
        {
            entityType = "PermitApplication", entityId = permitId.ToString(),
            action = "Submit", description = $"{applicationNumber} submitted", timestamp = DateTime.UtcNow
        }));
    }

    public void NotifyPermitStatusChanged(int permitId, string applicationNumber, string newStatus, string applicantId)
    {
        var payload = new { permitId, applicationNumber, newStatus, timestamp = DateTime.UtcNow };

        Fire(permitHub.Clients.Group($"applicant-{applicantId}").SendAsync("PermitStatusChanged", payload));
        Fire(adminHub.Clients.Group("admin-feed").SendAsync("AdminActivity", new
        {
            entityType = "PermitApplication", entityId = permitId.ToString(),
            action = newStatus, description = $"{applicationNumber} → {newStatus}", timestamp = DateTime.UtcNow
        }));
    }

    public void NotifyInspectionScheduled(int inspectionId, string inspectionNumber, string facilityName, string inspectorId, DateTime scheduledDate)
    {
        var payload = new { inspectionId, inspectionNumber, facilityName, inspectorId, scheduledDate, timestamp = DateTime.UtcNow };

        Fire(inspectionHub.Clients.Group($"inspector-{inspectorId}").SendAsync("InspectionScheduled", payload));
        Fire(inspectionHub.Clients.Group("staff-reviewers").SendAsync("InspectionScheduled", payload));
        Fire(adminHub.Clients.Group("admin-feed").SendAsync("AdminActivity", new
        {
            entityType = "Inspection", entityId = inspectionId.ToString(),
            action = "Schedule", description = $"{inspectionNumber} scheduled for {scheduledDate:MMM d}", timestamp = DateTime.UtcNow
        }));
    }

    public void NotifyAdminActivity(string entityType, string entityId, string action, string description)
    {
        var payload = new { entityType, entityId, action, description, timestamp = DateTime.UtcNow };
        Fire(adminHub.Clients.Group("admin-feed").SendAsync("AdminActivity", payload));
    }

    private void Fire(Task task) =>
        _ = task.ContinueWith(
            t => logger.LogError(t.Exception, "SignalR send failed"),
            TaskContinuationOptions.OnlyOnFaulted);
}
