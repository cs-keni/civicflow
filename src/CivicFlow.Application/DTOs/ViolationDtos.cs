using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.DTOs;

public record ViolationSummaryDto(
    int Id,
    string ViolationNumber,
    string Code,
    string Severity,
    string Status,
    DateTime? DueDate);

public record ViolationDto(
    int Id,
    string ViolationNumber,
    int InspectionId,
    string InspectionNumber,
    int FacilityId,
    string FacilityName,
    string Code,
    string Description,
    string? RegulatoryBasis,
    ViolationSeverity Severity,
    ViolationStatus Status,
    DateTime? DueDate,
    DateTime? ResolvedDate,
    string? Notes);

public record CreateViolationRequest(
    int InspectionId,
    int FacilityId,
    string Code,
    string Description,
    string? RegulatoryBasis,
    ViolationSeverity Severity,
    DateTime? DueDate);

public record UpdateViolationStatusRequest(
    ViolationStatus Status,
    string? Notes,
    DateTime? ResolvedDate);
