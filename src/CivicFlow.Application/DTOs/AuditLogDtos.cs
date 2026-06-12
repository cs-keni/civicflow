namespace CivicFlow.Application.DTOs;

public record AuditLogDto(
    long Id,
    string EntityType,
    string EntityId,
    string Action,
    string? UserId,
    DateTime OccurredAt,
    string? OldValues,
    string? NewValues,
    string? IpAddress);
