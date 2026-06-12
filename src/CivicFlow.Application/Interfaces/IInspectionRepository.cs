using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Interfaces;

public interface IInspectionRepository
{
    Task<Inspection?> GetByIdAsync(int id, bool includeRelated = false);
    Task<List<Inspection>> GetAllAsync(int page, int pageSize, InspectionStatus? status = null);
    Task<List<Inspection>> GetByInspectorIdAsync(string inspectorId, int page, int pageSize);
    Task<List<Inspection>> GetByFacilityIdAsync(int facilityId, int page, int pageSize);
    Task<List<Inspection>> GetByPermitIdAsync(int permitId);
    Task<int> CountAllAsync(InspectionStatus? status = null);
    Task<int> CountByInspectorIdAsync(string inspectorId);
    Task<Inspection> AddAsync(Inspection inspection);
    Task UpdateAsync(Inspection inspection);
}
