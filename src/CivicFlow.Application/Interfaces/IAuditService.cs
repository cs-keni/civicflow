using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;

namespace CivicFlow.Application.Interfaces;

public interface IAuditService
{
    Task<PaginatedResult<AuditLogDto>> GetAuditLogAsync(
        int page, int pageSize, string? entityType = null, string? userId = null);
    Task<List<AuditLogDto>> GetByEntityAsync(string entityType, string entityId);
}
