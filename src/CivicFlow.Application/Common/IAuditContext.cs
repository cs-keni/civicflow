namespace CivicFlow.Application.Common;

/// <summary>
/// Scoped per-request context populated by AuditLogMiddleware.
/// Injected into CivicFlowDbContext to stamp AuditLog entries automatically.
/// </summary>
public interface IAuditContext
{
    string? UserId { get; set; }
    string? IpAddress { get; set; }
    string? UserAgent { get; set; }
}
