using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.DTOs;

public record FacilityDto(
    int Id,
    string LegalName,
    string? DbaName,
    string FacilityType,
    string Address,
    string City,
    string State,
    string ZipCode,
    string County,
    string OwnerId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateFacilityRequest(
    string LegalName,
    string? DbaName,
    FacilityType FacilityType,
    string Address,
    string City,
    string State,
    string ZipCode,
    string County);

public record UpdateFacilityRequest(
    string? LegalName,
    string? DbaName,
    FacilityType? FacilityType,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? County,
    bool? IsActive);
