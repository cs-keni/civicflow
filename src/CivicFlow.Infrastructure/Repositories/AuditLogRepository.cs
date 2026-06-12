using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;
using CivicFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Repositories;

public class AuditLogRepository(CivicFlowDbContext db) : IAuditLogRepository
{
    public Task<List<AuditLog>> GetAllAsync(int page, int pageSize, string? entityType = null, string? userId = null)
    {
        var q = db.AuditLogs.AsQueryable();
        if (entityType is not null) q = q.Where(a => a.EntityType == entityType);
        if (userId is not null) q = q.Where(a => a.UserId == userId);
        return q.OrderByDescending(a => a.OccurredAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
    }

    public Task<int> CountAsync(string? entityType = null, string? userId = null)
    {
        var q = db.AuditLogs.AsQueryable();
        if (entityType is not null) q = q.Where(a => a.EntityType == entityType);
        if (userId is not null) q = q.Where(a => a.UserId == userId);
        return q.CountAsync();
    }

    public Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId) =>
        db.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.OccurredAt)
            .ToListAsync();
}
