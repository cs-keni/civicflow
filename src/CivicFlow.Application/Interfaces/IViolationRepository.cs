using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Interfaces;

public interface IViolationRepository
{
    Task<Violation?> GetByIdAsync(int id);
    Task<List<Violation>> GetAllAsync(int page, int pageSize, ViolationStatus? status = null);
    Task<List<Violation>> GetByFacilityIdAsync(int facilityId, int page, int pageSize);
    Task<List<Violation>> GetByInspectionIdAsync(int inspectionId);
    Task<int> CountAllAsync(ViolationStatus? status = null);
    Task<Violation> AddAsync(Violation violation);
    Task UpdateAsync(Violation violation);
}
