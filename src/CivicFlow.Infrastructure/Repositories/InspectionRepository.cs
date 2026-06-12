using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using CivicFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Repositories;

public class InspectionRepository(CivicFlowDbContext db) : IInspectionRepository
{
    public Task<Inspection?> GetByIdAsync(int id, bool includeRelated = false)
    {
        var q = db.Inspections.AsQueryable();
        if (includeRelated) q = q.Include(i => i.Violations);
        return q.FirstOrDefaultAsync(i => i.Id == id);
    }

    public Task<List<Inspection>> GetAllAsync(int page, int pageSize, InspectionStatus? status = null)
    {
        var q = db.Inspections.AsQueryable();
        if (status.HasValue) q = q.Where(i => i.Status == status);
        return q.OrderByDescending(i => i.ScheduledDate)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
    }

    public Task<List<Inspection>> GetByInspectorIdAsync(string inspectorId, int page, int pageSize) =>
        db.Inspections
            .Where(i => i.InspectorId == inspectorId)
            .OrderByDescending(i => i.ScheduledDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public Task<List<Inspection>> GetByFacilityIdAsync(int facilityId, int page, int pageSize) =>
        db.Inspections
            .Where(i => i.FacilityId == facilityId)
            .OrderByDescending(i => i.ScheduledDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public Task<List<Inspection>> GetByPermitIdAsync(int permitId) =>
        db.Inspections
            .Where(i => i.PermitApplicationId == permitId)
            .OrderByDescending(i => i.ScheduledDate)
            .ToListAsync();

    public Task<int> CountAllAsync(InspectionStatus? status = null)
    {
        var q = db.Inspections.AsQueryable();
        if (status.HasValue) q = q.Where(i => i.Status == status);
        return q.CountAsync();
    }

    public Task<int> CountByInspectorIdAsync(string inspectorId) =>
        db.Inspections.CountAsync(i => i.InspectorId == inspectorId);

    public async Task<Inspection> AddAsync(Inspection inspection)
    {
        db.Inspections.Add(inspection);
        await db.SaveChangesAsync();
        return inspection;
    }

    public async Task UpdateAsync(Inspection inspection)
    {
        db.Inspections.Update(inspection);
        await db.SaveChangesAsync();
    }
}
