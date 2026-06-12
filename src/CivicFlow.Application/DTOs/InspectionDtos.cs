using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.DTOs;

public record InspectionSummaryDto(
    int Id,
    string InspectionNumber,
    int PermitApplicationId,
    string ApplicationNumber,
    int FacilityId,
    string FacilityName,
    string InspectorId,
    DateTime ScheduledDate,
    DateTime? CompletedDate,
    string Status,
    string InspectionType,
    string? OverallRating);

public record InspectionDto(
    int Id,
    string InspectionNumber,
    int PermitApplicationId,
    string ApplicationNumber,
    int FacilityId,
    string FacilityName,
    string InspectorId,
    DateTime ScheduledDate,
    DateTime? CompletedDate,
    InspectionStatus Status,
    InspectionType InspectionType,
    string? FieldNotes,
    string? PublicSummary,
    InspectionResult? OverallRating,
    DateTime CreatedAt,
    List<ViolationSummaryDto> Violations);

public record CreateInspectionRequest(
    int PermitApplicationId,
    int FacilityId,
    string InspectorId,
    DateTime ScheduledDate,
    InspectionType InspectionType);

public record CompleteInspectionRequest(
    string FieldNotes,
    InspectionResult OverallRating,
    DateTime CompletedDate);

public record UpdatePublicSummaryRequest(string PublicSummary);
