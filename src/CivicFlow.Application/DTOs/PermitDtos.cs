using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.DTOs;

public record PermitApplicationSummaryDto(
    int Id,
    string ApplicationNumber,
    int FacilityId,
    string FacilityName,
    string ApplicantId,
    string PermitType,
    string Status,
    DateTime? SubmittedAt,
    DateTime? ExpiresAt,
    string? AssignedStaffId);

public record PermitApplicationDto(
    int Id,
    string ApplicationNumber,
    int FacilityId,
    string FacilityName,
    string ApplicantId,
    PermitType PermitType,
    PermitStatus Status,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    DateTime? ApprovedAt,
    DateTime? ExpiresAt,
    string Description,
    string ProjectDetails,
    decimal? EstimatedCost,
    string? AssignedStaffId,
    List<PermitStatusHistoryDto> StatusHistory,
    List<ReviewCommentDto> Comments);

public record PermitStatusHistoryDto(
    int Id,
    string FromStatus,
    string ToStatus,
    string ChangedById,
    DateTime ChangedAt,
    string? Notes);

public record CreatePermitApplicationRequest(
    int FacilityId,
    PermitType PermitType,
    string Description,
    string ProjectDetails,
    decimal? EstimatedCost);

public record UpdatePermitApplicationRequest(
    string? Description,
    string? ProjectDetails,
    decimal? EstimatedCost);

public record AssignStaffRequest(string StaffId);

public record ReviewActionRequest(string? Notes);
