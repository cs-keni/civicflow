using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;
using CivicFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Repositories;

public class FacilityRepository(CivicFlowDbContext db) : IFacilityRepository
{
    public Task<Facility?> GetByIdAsync(int id) =>
        db.Facilities.FirstOrDefaultAsync(f => f.Id == id);

    public Task<List<Facility>> GetAllAsync(int page, int pageSize, bool activeOnly = false)
    {
        var q = db.Facilities.AsQueryable();
        if (activeOnly) q = q.Where(f => f.IsActive);
        return q.OrderBy(f => f.LegalName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
    }

    public Task<List<Facility>> GetByOwnerIdAsync(string ownerId, int page, int pageSize) =>
        db.Facilities
            .Where(f => f.OwnerId == ownerId)
            .OrderBy(f => f.LegalName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public Task<int> CountAsync(bool activeOnly = false)
    {
        var q = db.Facilities.AsQueryable();
        if (activeOnly) q = q.Where(f => f.IsActive);
        return q.CountAsync();
    }

    public Task<int> CountByOwnerIdAsync(string ownerId) =>
        db.Facilities.CountAsync(f => f.OwnerId == ownerId);

    public async Task<Facility> AddAsync(Facility facility)
    {
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();
        return facility;
    }

    public async Task UpdateAsync(Facility facility)
    {
        db.Facilities.Update(facility);
        await db.SaveChangesAsync();
    }
}
