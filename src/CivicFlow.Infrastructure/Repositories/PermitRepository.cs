using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using CivicFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Repositories;

public class PermitRepository(CivicFlowDbContext db) : IPermitRepository
{
    public Task<PermitApplication?> GetByIdAsync(int id, bool includeRelated = false)
    {
        var q = db.PermitApplications.AsQueryable();
        if (includeRelated)
            q = q.Include(p => p.StatusHistory)
                 .Include(p => p.ReviewComments);
        return q.FirstOrDefaultAsync(p => p.Id == id);
    }

    public Task<List<PermitApplication>> GetAllAsync(int page, int pageSize, PermitStatus? status = null)
    {
        var q = db.PermitApplications.AsQueryable();
        if (status.HasValue) q = q.Where(p => p.Status == status);
        return q.OrderByDescending(p => p.SubmittedAt ?? DateTime.MinValue)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();
    }

    public Task<List<PermitApplication>> GetByApplicantIdAsync(string applicantId, int page, int pageSize) =>
        db.PermitApplications
            .Where(p => p.ApplicantId == applicantId)
            .OrderByDescending(p => p.SubmittedAt ?? DateTime.MinValue)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public Task<List<PermitApplication>> GetByFacilityIdAsync(int facilityId, int page, int pageSize) =>
        db.PermitApplications
            .Where(p => p.FacilityId == facilityId)
            .OrderByDescending(p => p.SubmittedAt ?? DateTime.MinValue)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

    public Task<int> CountAllAsync(PermitStatus? status = null)
    {
        var q = db.PermitApplications.AsQueryable();
        if (status.HasValue) q = q.Where(p => p.Status == status);
        return q.CountAsync();
    }

    public Task<int> CountByApplicantIdAsync(string applicantId) =>
        db.PermitApplications.CountAsync(p => p.ApplicantId == applicantId);

    public async Task<PermitApplication> AddAsync(PermitApplication permit)
    {
        db.PermitApplications.Add(permit);
        await db.SaveChangesAsync();
        return permit;
    }

    public async Task UpdateAsync(PermitApplication permit)
    {
        db.PermitApplications.Update(permit);
        await db.SaveChangesAsync();
    }

    public async Task AddStatusHistoryAsync(PermitStatusHistory history)
    {
        db.PermitStatusHistories.Add(history);
        await db.SaveChangesAsync();
    }
}
