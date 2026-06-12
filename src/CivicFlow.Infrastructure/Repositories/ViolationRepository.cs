using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using CivicFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Repositories;

public class ViolationRepository(CivicFlowDbContext db) : IViolationRepository
{
    public Task<Violation?> GetByIdAsync(int id) =>
        db.Violations.FirstOrDefaultAsync(v => v.Id == id);

    public Task<List<Violation>> GetAllAsync(int page, int pageSize, ViolationStatus? status = null)
    {
        var q = db.Violations.AsQueryable();
        if (status.HasValue) q = q.Where(v => v.Status == status);
        return q.OrderByDescending(v => v.Id)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
    }

    public Task<List<Violation>> GetByFacilityIdAsync(int facilityId, int page, int pageSize) =>
        db.Violations
            .Where(v => v.FacilityId == facilityId)
            .OrderByDescending(v => v.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public Task<List<Violation>> GetByInspectionIdAsync(int inspectionId) =>
        db.Violations.Where(v => v.InspectionId == inspectionId).ToListAsync();

    public Task<int> CountAllAsync(ViolationStatus? status = null)
    {
        var q = db.Violations.AsQueryable();
        if (status.HasValue) q = q.Where(v => v.Status == status);
        return q.CountAsync();
    }

    public async Task<Violation> AddAsync(Violation violation)
    {
        db.Violations.Add(violation);
        await db.SaveChangesAsync();
        return violation;
    }

    public async Task UpdateAsync(Violation violation)
    {
        db.Violations.Update(violation);
        await db.SaveChangesAsync();
    }
}
