using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;

namespace CivicFlow.Application.Interfaces;

public interface IFacilityService
{
    Task<PaginatedResult<FacilityDto>> GetFacilitiesAsync(int page, int pageSize);
    Task<FacilityDto?> GetFacilityAsync(int id);
    Task<FacilityDto> CreateFacilityAsync(CreateFacilityRequest request);
    Task<FacilityDto?> UpdateFacilityAsync(int id, UpdateFacilityRequest request);
    Task<bool> DeactivateAsync(int id);
    Task<bool> CanAccessFacilityAsync(int facilityId);
}
