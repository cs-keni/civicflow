using CivicFlow.Domain.Entities;

namespace CivicFlow.Application.Interfaces;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetAllAsync(int page, int pageSize, string? entityType = null, string? userId = null);
    Task<int> CountAsync(string? entityType = null, string? userId = null);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId);
}
