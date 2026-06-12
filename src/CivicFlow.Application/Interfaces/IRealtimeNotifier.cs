namespace CivicFlow.Application.Interfaces;

/// <summary>
/// Fire-and-forget real-time notifications. Implementations must never throw —
/// hub failures must not propagate to the calling service (D12).
/// </summary>
public interface IRealtimeNotifier
{
    void NotifyPermitSubmitted(int permitId, string applicationNumber, string facilityName, string applicantId);
    void NotifyPermitStatusChanged(int permitId, string applicationNumber, string newStatus, string applicantId);
    void NotifyInspectionScheduled(int inspectionId, string inspectionNumber, string facilityName, string inspectorId, DateTime scheduledDate);
    void NotifyAdminActivity(string entityType, string entityId, string action, string description);
}
