using CivicFlow.Application.Common;

namespace CivicFlow.Infrastructure.Services;

public class AuditContext : IAuditContext
{
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
