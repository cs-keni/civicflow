using CivicFlow.Application.Interfaces;

namespace CivicFlow.Infrastructure.Services;

/// <summary>
/// No-op notifier registered by AddInfrastructure so Application services compile
/// without SignalR deps. Program.cs overrides this with SignalRNotifier after AddSignalR().
/// </summary>
public sealed class NullRealtimeNotifier : IRealtimeNotifier
{
    public void NotifyPermitSubmitted(int permitId, string applicationNumber, string facilityName, string applicantId) { }
    public void NotifyPermitStatusChanged(int permitId, string applicationNumber, string newStatus, string applicantId) { }
    public void NotifyInspectionScheduled(int inspectionId, string inspectionNumber, string facilityName, string inspectorId, DateTime scheduledDate) { }
    public void NotifyAdminActivity(string entityType, string entityId, string action, string description) { }
}
