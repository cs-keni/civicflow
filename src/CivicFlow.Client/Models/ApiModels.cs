namespace CivicFlow.Client.Models;

// ── Shared ────────────────────────────────────────────────────────────────────

public record PaginatedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

public record ApiError(string Message, IEnumerable<string>? Errors = null);

// ── Auth ──────────────────────────────────────────────────────────────────────

public record LoginRequest(string Email, string Password);

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    IList<string> Roles,
    bool IsActive);

// ── Facility ──────────────────────────────────────────────────────────────────

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
    string FacilityType,
    string Address,
    string City,
    string State,
    string ZipCode,
    string County);

public record UpdateFacilityRequest(
    string? LegalName = null,
    string? DbaName = null,
    string? FacilityType = null,
    string? Address = null,
    string? City = null,
    string? State = null,
    string? ZipCode = null,
    string? County = null,
    bool? IsActive = null);

// ── Public Compliance ─────────────────────────────────────────────────────────

public record FacilityComplianceDto(
    FacilityDto? Facility,
    int ActivePermitsCount,
    int TotalInspectionsCount,
    int OpenViolationsCount,
    List<InspectionPublicSummaryDto>? RecentInspections);

public record InspectionPublicSummaryDto(
    DateTime ScheduledDate,
    string InspectionType,
    string? OverallRating,
    string? PublicSummary);

// ── Permits ───────────────────────────────────────────────────────────────────

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
    string? AssignedStaffId,
    string? AssignedStaffName = null);

public record PermitApplicationDto(
    int Id,
    string ApplicationNumber,
    int FacilityId,
    string FacilityName,
    string ApplicantId,
    string PermitType,
    string Status,
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
    string? Notes,
    string? ChangedByName = null);

public record ReviewCommentDto(
    int Id,
    int PermitApplicationId,
    string AuthorId,
    string Content,
    bool IsInternal,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? AuthorName = null);

public record CreatePermitRequest(
    int FacilityId,
    string PermitType,
    string Description,
    DateTime? RequestedValidFrom,
    DateTime? RequestedValidTo);

public record UpdatePermitRequest(
    string? Description = null,
    string? ProjectDetails = null,
    decimal? EstimatedCost = null);

public record AssignStaffRequest(string StaffId);
public record ReviewActionRequest(string? Notes);
public record CreateReviewCommentRequest(string Content, bool IsInternal = false);

// ── Inspections ───────────────────────────────────────────────────────────────

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
    string Status,
    string InspectionType,
    string? FieldNotes,
    string? PublicSummary,
    string? OverallRating,
    DateTime CreatedAt,
    List<ViolationSummaryDto> Violations);

public record CreateInspectionRequest(
    int PermitApplicationId,
    int FacilityId,
    string InspectorId,
    DateTime ScheduledDate,
    string InspectionType);

public record CompleteInspectionRequest(
    string FieldNotes,
    string OverallRating,
    DateTime CompletedDate);

public record UpdatePublicSummaryRequest(string PublicSummary);

// ── Violations ────────────────────────────────────────────────────────────────

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
    string Severity,
    string Status,
    DateTime? DueDate,
    DateTime? ResolvedDate,
    string? Notes);

public record CreateViolationRequest(
    int InspectionId,
    int FacilityId,
    string Code,
    string Description,
    string? RegulatoryBasis,
    string Severity,
    DateTime? DueDate);

public record UpdateViolationStatusRequest(
    string Status,
    string? Notes = null,
    DateTime? ResolvedDate = null);

// ── Audit ─────────────────────────────────────────────────────────────────────

public record AuditLogDto(
    long Id,
    string EntityType,
    string EntityId,
    string Action,
    string? UserId,
    DateTime OccurredAt,
    string? OldValues,
    string? NewValues,
    string? IpAddress);
