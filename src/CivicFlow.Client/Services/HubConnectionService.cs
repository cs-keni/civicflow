using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace CivicFlow.Client.Services;

/// <summary>
/// Manages SignalR hub connections for the Blazor WASM client.
/// Browser sends auth cookies automatically (same-origin, BFF pattern).
/// Lifetime is owned by the DI container (scoped = singleton in WASM).
/// Pages must only subscribe/unsubscribe events — never call DisposeAsync.
/// </summary>
public sealed class HubConnectionService(NavigationManager nav) : IAsyncDisposable
{
    private HubConnection? _permitHub;
    private HubConnection? _reviewQueueHub;
    private HubConnection? _inspectionHub;
    private HubConnection? _adminHub;

    // ── Permit Status ─────────────────────────────────────────────────────────

    public event Action<PermitStatusChangedEvent>? OnPermitStatusChanged;

    public async Task ConnectPermitHubAsync(CancellationToken ct = default)
    {
        if (_permitHub?.State is HubConnectionState.Connected or HubConnectionState.Reconnecting) return;
        _permitHub = BuildConnection("/hubs/permit-status");
        _permitHub.On<PermitStatusChangedEvent>("PermitStatusChanged", e => OnPermitStatusChanged?.Invoke(e));
        await SafeStartAsync(_permitHub, ct);
    }

    // ── Review Queue ──────────────────────────────────────────────────────────

    public event Action<PermitSubmittedEvent>? OnQueueUpdated;

    public async Task ConnectReviewQueueHubAsync(CancellationToken ct = default)
    {
        if (_reviewQueueHub?.State is HubConnectionState.Connected or HubConnectionState.Reconnecting) return;
        _reviewQueueHub = BuildConnection("/hubs/review-queue");
        _reviewQueueHub.On<PermitSubmittedEvent>("PermitSubmitted", e => OnQueueUpdated?.Invoke(e));
        await SafeStartAsync(_reviewQueueHub, ct);
    }

    // ── Inspections ───────────────────────────────────────────────────────────

    public event Action<InspectionScheduledEvent>? OnInspectionScheduled;

    public async Task ConnectInspectionHubAsync(CancellationToken ct = default)
    {
        if (_inspectionHub?.State is HubConnectionState.Connected or HubConnectionState.Reconnecting) return;
        _inspectionHub = BuildConnection("/hubs/inspection");
        _inspectionHub.On<InspectionScheduledEvent>("InspectionScheduled", e => OnInspectionScheduled?.Invoke(e));
        await SafeStartAsync(_inspectionHub, ct);
    }

    // ── Admin Activity ────────────────────────────────────────────────────────

    public event Action<AdminActivityEvent>? OnAdminActivity;

    public async Task ConnectAdminHubAsync(CancellationToken ct = default)
    {
        if (_adminHub?.State is HubConnectionState.Connected or HubConnectionState.Reconnecting) return;
        _adminHub = BuildConnection("/hubs/admin-activity");
        _adminHub.On<AdminActivityEvent>("AdminActivity", e => OnAdminActivity?.Invoke(e));
        await SafeStartAsync(_adminHub, ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private HubConnection BuildConnection(string path) =>
        new HubConnectionBuilder()
            .WithUrl(nav.ToAbsoluteUri(path))
            .WithAutomaticReconnect()
            .Build();

    private static async Task SafeStartAsync(HubConnection conn, CancellationToken ct)
    {
        try { await conn.StartAsync(ct); }
        catch { /* graceful degradation — real-time is advisory, not required */ }
    }

    // Called only by the DI container when the circuit ends (browser tab close).
    public async ValueTask DisposeAsync()
    {
        if (_permitHub is not null) await _permitHub.DisposeAsync();
        if (_reviewQueueHub is not null) await _reviewQueueHub.DisposeAsync();
        if (_inspectionHub is not null) await _inspectionHub.DisposeAsync();
        if (_adminHub is not null) await _adminHub.DisposeAsync();
    }
}

// ── Hub event records ─────────────────────────────────────────────────────────

public record PermitStatusChangedEvent(int PermitId, string ApplicationNumber, string NewStatus, DateTime Timestamp);
public record PermitSubmittedEvent(int PermitId, string ApplicationNumber, string FacilityName, string ApplicantId, DateTime Timestamp);
public record InspectionScheduledEvent(int InspectionId, string InspectionNumber, string FacilityName, string InspectorId, DateTime ScheduledDate, DateTime Timestamp);
public record AdminActivityEvent(string EntityType, string EntityId, string Action, string Description, DateTime Timestamp);
