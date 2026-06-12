using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;

namespace CivicFlow.Application.Interfaces;

public interface IInspectionService
{
    Task<PaginatedResult<InspectionSummaryDto>> GetInspectionsAsync(int page, int pageSize);
    Task<InspectionDto?> GetInspectionAsync(int id);
    Task<InspectionDto> CreateInspectionAsync(CreateInspectionRequest request);
    Task<InspectionDto?> CompleteInspectionAsync(int id, CompleteInspectionRequest request);
    Task<InspectionDto?> UpdatePublicSummaryAsync(int id, string publicSummary);
    Task<InspectionDto?> CancelAsync(int id);
    Task<PaginatedResult<InspectionSummaryDto>> GetByFacilityAsync(int facilityId, int page, int pageSize);
}
