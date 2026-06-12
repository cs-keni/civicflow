using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Services;

public class InspectionService(
    IInspectionRepository inspectionRepo,
    IFacilityRepository facilityRepo,
    IPermitRepository permitRepo,
    ICurrentUserService currentUser) : IInspectionService
{
    public async Task<PaginatedResult<InspectionSummaryDto>> GetInspectionsAsync(int page, int pageSize)
    {
        List<Inspection> items;
        int total;

        if (currentUser.IsAdminOrStaff)
        {
            items = await inspectionRepo.GetAllAsync(page, pageSize);
            total = await inspectionRepo.CountAllAsync();
        }
        else if (currentUser.IsInspector)
        {
            items = await inspectionRepo.GetByInspectorIdAsync(currentUser.UserId!, page, pageSize);
            total = await inspectionRepo.CountByInspectorIdAsync(currentUser.UserId!);
        }
        else
        {
            items = await inspectionRepo.GetAllAsync(page, pageSize);
            total = await inspectionRepo.CountAllAsync();
        }

        return await ToSummaryPageAsync(items, total, page, pageSize);
    }

    public async Task<PaginatedResult<InspectionSummaryDto>> GetByFacilityAsync(int facilityId, int page, int pageSize)
    {
        var items = await inspectionRepo.GetByFacilityIdAsync(facilityId, page, pageSize);
        var total = items.Count; // count is approximated — good enough for public profile view
        return await ToSummaryPageAsync(items, total, page, pageSize);
    }

    public async Task<InspectionDto?> GetInspectionAsync(int id)
    {
        var inspection = await inspectionRepo.GetByIdAsync(id, includeRelated: true);
        if (inspection is null) return null;

        var facility = await facilityRepo.GetByIdAsync(inspection.FacilityId);
        var permit = await permitRepo.GetByIdAsync(inspection.PermitApplicationId);
        return ToDetailDto(inspection, facility?.LegalName ?? "", permit?.ApplicationNumber ?? "");
    }

    public async Task<InspectionDto> CreateInspectionAsync(CreateInspectionRequest request)
    {
        var inspection = new Inspection
        {
            PermitApplicationId = request.PermitApplicationId,
            FacilityId = request.FacilityId,
            InspectorId = request.InspectorId,
            ScheduledDate = request.ScheduledDate,
            Status = InspectionStatus.Scheduled,
            InspectionType = request.InspectionType,
            CreatedAt = DateTime.UtcNow
        };
        var created = await inspectionRepo.AddAsync(inspection);

        var facility = await facilityRepo.GetByIdAsync(created.FacilityId);
        var permit = await permitRepo.GetByIdAsync(created.PermitApplicationId);
        return ToDetailDto(created, facility?.LegalName ?? "", permit?.ApplicationNumber ?? "");
    }

    public async Task<InspectionDto?> CompleteInspectionAsync(int id, CompleteInspectionRequest request)
    {
        var inspection = await inspectionRepo.GetByIdAsync(id);
        if (inspection is null) return null;
        if (inspection.InspectorId != currentUser.UserId && !currentUser.IsAdminOrStaff) return null;

        inspection.Status = InspectionStatus.Completed;
        inspection.FieldNotes = request.FieldNotes;
        inspection.OverallRating = request.OverallRating;
        inspection.CompletedDate = request.CompletedDate;

        await inspectionRepo.UpdateAsync(inspection);

        var facility = await facilityRepo.GetByIdAsync(inspection.FacilityId);
        var permit = await permitRepo.GetByIdAsync(inspection.PermitApplicationId);
        return ToDetailDto(inspection, facility?.LegalName ?? "", permit?.ApplicationNumber ?? "");
    }

    public async Task<InspectionDto?> UpdatePublicSummaryAsync(int id, string publicSummary)
    {
        var inspection = await inspectionRepo.GetByIdAsync(id);
        if (inspection is null) return null;
        if (!currentUser.IsAdminOrStaff) return null;

        inspection.PublicSummary = publicSummary;
        await inspectionRepo.UpdateAsync(inspection);

        var facility = await facilityRepo.GetByIdAsync(inspection.FacilityId);
        var permit = await permitRepo.GetByIdAsync(inspection.PermitApplicationId);
        return ToDetailDto(inspection, facility?.LegalName ?? "", permit?.ApplicationNumber ?? "");
    }

    public async Task<InspectionDto?> CancelAsync(int id)
    {
        var inspection = await inspectionRepo.GetByIdAsync(id);
        if (inspection is null) return null;
        if (!currentUser.IsAdminOrStaff) return null;
        if (inspection.Status == InspectionStatus.Completed) return null;

        inspection.Status = InspectionStatus.Cancelled;
        await inspectionRepo.UpdateAsync(inspection);

        var facility = await facilityRepo.GetByIdAsync(inspection.FacilityId);
        var permit = await permitRepo.GetByIdAsync(inspection.PermitApplicationId);
        return ToDetailDto(inspection, facility?.LegalName ?? "", permit?.ApplicationNumber ?? "");
    }

    private async Task<PaginatedResult<InspectionSummaryDto>> ToSummaryPageAsync(
        List<Inspection> items, int total, int page, int pageSize)
    {
        var facilityNames = new Dictionary<int, string>();
        var appNumbers = new Dictionary<int, string>();
        foreach (var i in items)
        {
            if (!facilityNames.ContainsKey(i.FacilityId))
            {
                var f = await facilityRepo.GetByIdAsync(i.FacilityId);
                facilityNames[i.FacilityId] = f?.LegalName ?? "";
            }
            if (!appNumbers.ContainsKey(i.PermitApplicationId))
            {
                var p = await permitRepo.GetByIdAsync(i.PermitApplicationId);
                appNumbers[i.PermitApplicationId] = p?.ApplicationNumber ?? "";
            }
        }

        var dtos = items.Select(i => ToSummaryDto(i, facilityNames[i.FacilityId], appNumbers[i.PermitApplicationId])).ToList();
        return PaginatedResult<InspectionSummaryDto>.Create(dtos, total, page, pageSize);
    }

    private static InspectionSummaryDto ToSummaryDto(Inspection i, string facilityName, string appNumber) => new(
        i.Id, i.InspectionNumber, i.PermitApplicationId, appNumber,
        i.FacilityId, facilityName, i.InspectorId,
        i.ScheduledDate, i.CompletedDate, i.Status.ToString(),
        i.InspectionType.ToString(), i.OverallRating?.ToString());

    private static InspectionDto ToDetailDto(Inspection i, string facilityName, string appNumber)
    {
        var violations = i.Violations?.Select(v => new ViolationSummaryDto(
            v.Id, v.ViolationNumber, v.Code, v.Severity.ToString(),
            v.Status.ToString(), v.DueDate)).ToList() ?? [];

        return new InspectionDto(
            i.Id, i.InspectionNumber, i.PermitApplicationId, appNumber,
            i.FacilityId, facilityName, i.InspectorId,
            i.ScheduledDate, i.CompletedDate, i.Status, i.InspectionType,
            i.FieldNotes, i.PublicSummary, i.OverallRating, i.CreatedAt, violations);
    }
}
