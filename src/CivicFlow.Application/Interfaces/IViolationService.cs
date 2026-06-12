using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;

namespace CivicFlow.Application.Interfaces;

public interface IViolationService
{
    Task<PaginatedResult<ViolationDto>> GetViolationsAsync(int page, int pageSize);
    Task<ViolationDto?> GetViolationAsync(int id);
    Task<ViolationDto> CreateViolationAsync(CreateViolationRequest request);
    Task<ViolationDto?> UpdateStatusAsync(int id, UpdateViolationStatusRequest request);
    Task<PaginatedResult<ViolationDto>> GetByFacilityAsync(int facilityId, int page, int pageSize);
}
