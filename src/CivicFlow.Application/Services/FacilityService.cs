using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;

namespace CivicFlow.Application.Services;

public class FacilityService(
    IFacilityRepository facilityRepo,
    ICurrentUserService currentUser) : IFacilityService
{
    public async Task<PaginatedResult<FacilityDto>> GetFacilitiesAsync(int page, int pageSize)
    {
        List<Facility> items;
        int total;

        if (currentUser.IsAdminOrStaff || currentUser.IsInspector)
        {
            items = await facilityRepo.GetAllAsync(page, pageSize);
            total = await facilityRepo.CountAsync();
        }
        else
        {
            var userId = currentUser.UserId!;
            items = await facilityRepo.GetByOwnerIdAsync(userId, page, pageSize);
            total = await facilityRepo.CountByOwnerIdAsync(userId);
        }

        return PaginatedResult<FacilityDto>.Create(items.Select(ToDto).ToList(), total, page, pageSize);
    }

    public async Task<FacilityDto?> GetFacilityAsync(int id)
    {
        var facility = await facilityRepo.GetByIdAsync(id);
        if (facility is null) return null;
        if (!await CanAccessFacilityAsync(facility)) return null;
        return ToDto(facility);
    }

    public async Task<FacilityDto> CreateFacilityAsync(CreateFacilityRequest request)
    {
        var facility = new Facility
        {
            LegalName = request.LegalName,
            DbaName = request.DbaName,
            FacilityType = request.FacilityType,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            County = request.County,
            OwnerId = currentUser.UserId!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await facilityRepo.AddAsync(facility);
        return ToDto(created);
    }

    public async Task<FacilityDto?> UpdateFacilityAsync(int id, UpdateFacilityRequest request)
    {
        var facility = await facilityRepo.GetByIdAsync(id);
        if (facility is null) return null;
        if (!await CanAccessFacilityAsync(facility)) return null;

        if (request.LegalName is not null) facility.LegalName = request.LegalName;
        if (request.DbaName is not null) facility.DbaName = request.DbaName;
        if (request.FacilityType.HasValue) facility.FacilityType = request.FacilityType.Value;
        if (request.Address is not null) facility.Address = request.Address;
        if (request.City is not null) facility.City = request.City;
        if (request.State is not null) facility.State = request.State;
        if (request.ZipCode is not null) facility.ZipCode = request.ZipCode;
        if (request.County is not null) facility.County = request.County;
        if (request.IsActive.HasValue) facility.IsActive = request.IsActive.Value;
        facility.UpdatedAt = DateTime.UtcNow;

        await facilityRepo.UpdateAsync(facility);
        return ToDto(facility);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var facility = await facilityRepo.GetByIdAsync(id);
        if (facility is null) return false;
        facility.IsActive = false;
        facility.UpdatedAt = DateTime.UtcNow;
        await facilityRepo.UpdateAsync(facility);
        return true;
    }

    public Task<bool> CanAccessFacilityAsync(int facilityId) =>
        facilityRepo.GetByIdAsync(facilityId)
            .ContinueWith(t => t.Result is { } f && CanAccessFacilityAsync(f).Result);

    private Task<bool> CanAccessFacilityAsync(Facility facility)
    {
        if (currentUser.IsAdminOrStaff || currentUser.IsInspector) return Task.FromResult(true);
        return Task.FromResult(facility.OwnerId == currentUser.UserId);
    }

    private static FacilityDto ToDto(Facility f) => new(
        f.Id, f.LegalName, f.DbaName, f.FacilityType.ToString(),
        f.Address, f.City, f.State, f.ZipCode, f.County,
        f.OwnerId, f.IsActive, f.CreatedAt, f.UpdatedAt);
}
