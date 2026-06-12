using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Interfaces;

public interface IPermitRepository
{
    Task<PermitApplication?> GetByIdAsync(int id, bool includeRelated = false);
    Task<List<PermitApplication>> GetAllAsync(int page, int pageSize, PermitStatus? status = null);
    Task<List<PermitApplication>> GetByApplicantIdAsync(string applicantId, int page, int pageSize);
    Task<List<PermitApplication>> GetByFacilityIdAsync(int facilityId, int page, int pageSize);
    Task<int> CountAllAsync(PermitStatus? status = null);
    Task<int> CountByApplicantIdAsync(string applicantId);
    Task<PermitApplication> AddAsync(PermitApplication permit);
    Task UpdateAsync(PermitApplication permit);
    Task AddStatusHistoryAsync(PermitStatusHistory history);
}
