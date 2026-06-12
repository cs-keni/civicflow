using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;

namespace CivicFlow.Application.Services;

public class AuditService(IAuditLogRepository auditRepo) : IAuditService
{
    public async Task<PaginatedResult<AuditLogDto>> GetAuditLogAsync(
        int page, int pageSize, string? entityType = null, string? userId = null)
    {
        var items = await auditRepo.GetAllAsync(page, pageSize, entityType, userId);
        var total = await auditRepo.CountAsync(entityType, userId);
        return PaginatedResult<AuditLogDto>.Create(items.Select(ToDto).ToList(), total, page, pageSize);
    }

    public async Task<List<AuditLogDto>> GetByEntityAsync(string entityType, string entityId)
    {
        var items = await auditRepo.GetByEntityAsync(entityType, entityId);
        return items.Select(ToDto).ToList();
    }

    private static AuditLogDto ToDto(Domain.Entities.AuditLog a) => new(
        a.Id, a.EntityType, a.EntityId, a.Action.ToString(),
        a.UserId, a.OccurredAt, a.OldValues, a.NewValues, a.IpAddress);
}
