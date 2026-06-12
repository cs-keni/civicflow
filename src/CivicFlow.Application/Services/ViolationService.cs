using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;

namespace CivicFlow.Application.Services;

public class ViolationService(
    IViolationRepository violationRepo,
    IFacilityRepository facilityRepo,
    IInspectionRepository inspectionRepo,
    ICurrentUserService currentUser) : IViolationService
{
    public async Task<PaginatedResult<ViolationDto>> GetViolationsAsync(int page, int pageSize)
    {
        var items = await violationRepo.GetAllAsync(page, pageSize);
        var total = await violationRepo.CountAllAsync();
        return await ToPageAsync(items, total, page, pageSize);
    }

    public async Task<PaginatedResult<ViolationDto>> GetByFacilityAsync(int facilityId, int page, int pageSize)
    {
        var items = await violationRepo.GetByFacilityIdAsync(facilityId, page, pageSize);
        return await ToPageAsync(items, items.Count, page, pageSize);
    }

    public async Task<ViolationDto?> GetViolationAsync(int id)
    {
        var violation = await violationRepo.GetByIdAsync(id);
        if (violation is null) return null;
        var facility = await facilityRepo.GetByIdAsync(violation.FacilityId);
        var inspection = await inspectionRepo.GetByIdAsync(violation.InspectionId);
        return ToDto(violation, facility?.LegalName ?? "", inspection?.InspectionNumber ?? "");
    }

    public async Task<ViolationDto> CreateViolationAsync(CreateViolationRequest request)
    {
        var violation = new Violation
        {
            InspectionId = request.InspectionId,
            FacilityId = request.FacilityId,
            Code = request.Code,
            Description = request.Description,
            RegulatoryBasis = request.RegulatoryBasis,
            Severity = request.Severity,
            Status = Domain.Enums.ViolationStatus.Open,
            DueDate = request.DueDate
        };
        var created = await violationRepo.AddAsync(violation);
        var facility = await facilityRepo.GetByIdAsync(created.FacilityId);
        var inspection = await inspectionRepo.GetByIdAsync(created.InspectionId);
        return ToDto(created, facility?.LegalName ?? "", inspection?.InspectionNumber ?? "");
    }

    public async Task<ViolationDto?> UpdateStatusAsync(int id, UpdateViolationStatusRequest request)
    {
        var violation = await violationRepo.GetByIdAsync(id);
        if (violation is null) return null;

        violation.Status = request.Status;
        if (request.Notes is not null) violation.Notes = request.Notes;
        if (request.ResolvedDate.HasValue) violation.ResolvedDate = request.ResolvedDate;

        await violationRepo.UpdateAsync(violation);
        var facility = await facilityRepo.GetByIdAsync(violation.FacilityId);
        var inspection = await inspectionRepo.GetByIdAsync(violation.InspectionId);
        return ToDto(violation, facility?.LegalName ?? "", inspection?.InspectionNumber ?? "");
    }

    private async Task<PaginatedResult<ViolationDto>> ToPageAsync(
        List<Violation> items, int total, int page, int pageSize)
    {
        var facilityNames = new Dictionary<int, string>();
        var inspectionNumbers = new Dictionary<int, string>();
        foreach (var v in items)
        {
            if (!facilityNames.ContainsKey(v.FacilityId))
            {
                var f = await facilityRepo.GetByIdAsync(v.FacilityId);
                facilityNames[v.FacilityId] = f?.LegalName ?? "";
            }
            if (!inspectionNumbers.ContainsKey(v.InspectionId))
            {
                var i = await inspectionRepo.GetByIdAsync(v.InspectionId);
                inspectionNumbers[v.InspectionId] = i?.InspectionNumber ?? "";
            }
        }

        var dtos = items.Select(v => ToDto(v, facilityNames[v.FacilityId], inspectionNumbers[v.InspectionId])).ToList();
        return PaginatedResult<ViolationDto>.Create(dtos, total, page, pageSize);
    }

    private static ViolationDto ToDto(Violation v, string facilityName, string inspectionNumber) => new(
        v.Id, v.ViolationNumber, v.InspectionId, inspectionNumber,
        v.FacilityId, facilityName, v.Code, v.Description,
        v.RegulatoryBasis, v.Severity, v.Status, v.DueDate, v.ResolvedDate, v.Notes);
}
