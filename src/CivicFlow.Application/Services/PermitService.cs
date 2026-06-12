using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Services;

public class PermitService(
    IPermitRepository permitRepo,
    IFacilityRepository facilityRepo,
    IReviewCommentRepository commentRepo,
    ICurrentUserService currentUser,
    IRealtimeNotifier notifier) : IPermitService
{
    public async Task<PaginatedResult<PermitApplicationSummaryDto>> GetPermitsAsync(int page, int pageSize)
    {
        List<PermitApplication> items;
        int total;

        if (currentUser.IsAdminOrStaff || currentUser.IsInspector)
        {
            items = await permitRepo.GetAllAsync(page, pageSize);
            total = await permitRepo.CountAllAsync();
        }
        else
        {
            var userId = currentUser.UserId!;
            items = await permitRepo.GetByApplicantIdAsync(userId, page, pageSize);
            total = await permitRepo.CountByApplicantIdAsync(userId);
        }

        var facilityCacheIds = items.Select(p => p.FacilityId).Distinct().ToList();
        var facilityNames = new Dictionary<int, string>();
        foreach (var fid in facilityCacheIds)
        {
            var f = await facilityRepo.GetByIdAsync(fid);
            if (f is not null) facilityNames[fid] = f.LegalName;
        }

        var dtos = items.Select(p => ToSummaryDto(p, facilityNames.GetValueOrDefault(p.FacilityId, ""))).ToList();
        return PaginatedResult<PermitApplicationSummaryDto>.Create(dtos, total, page, pageSize);
    }

    public async Task<PermitApplicationDto?> GetPermitAsync(int id)
    {
        var permit = await permitRepo.GetByIdAsync(id, includeRelated: true);
        if (permit is null) return null;
        if (!CanAccessPermit(permit)) return null;

        var facility = await facilityRepo.GetByIdAsync(permit.FacilityId);
        return await ToDetailDto(permit, facility?.LegalName ?? "");
    }

    public async Task<PermitApplicationDto> CreatePermitAsync(CreatePermitApplicationRequest request)
    {
        var permit = new PermitApplication
        {
            FacilityId = request.FacilityId,
            ApplicantId = currentUser.UserId!,
            PermitType = request.PermitType,
            Status = PermitStatus.Draft,
            Description = request.Description,
            ProjectDetails = request.ProjectDetails,
            EstimatedCost = request.EstimatedCost
        };
        var created = await permitRepo.AddAsync(permit);
        var facility = await facilityRepo.GetByIdAsync(created.FacilityId);
        return await ToDetailDto(created, facility?.LegalName ?? "");
    }

    public async Task<PermitApplicationDto?> UpdatePermitAsync(int id, UpdatePermitApplicationRequest request)
    {
        var permit = await permitRepo.GetByIdAsync(id);
        if (permit is null) return null;
        if (!CanAccessPermit(permit)) return null;
        if (permit.Status != PermitStatus.Draft && permit.Status != PermitStatus.ChangesRequested) return null;

        if (request.Description is not null) permit.Description = request.Description;
        if (request.ProjectDetails is not null) permit.ProjectDetails = request.ProjectDetails;
        if (request.EstimatedCost.HasValue) permit.EstimatedCost = request.EstimatedCost;

        await permitRepo.UpdateAsync(permit);
        var facility = await facilityRepo.GetByIdAsync(permit.FacilityId);
        return await ToDetailDto(permit, facility?.LegalName ?? "");
    }

    public async Task<PermitApplicationDto?> SubmitPermitAsync(int id)
    {
        var permit = await permitRepo.GetByIdAsync(id);
        if (permit is null || permit.ApplicantId != currentUser.UserId) return null;
        if (permit.Status != PermitStatus.Draft && permit.Status != PermitStatus.ChangesRequested) return null;

        var facility = await facilityRepo.GetByIdAsync(permit.FacilityId);
        var dto = await TransitionStatusAsync(permit, PermitStatus.Submitted, currentUser.UserId!, null);
        if (dto is not null)
            notifier.NotifyPermitSubmitted(permit.Id, permit.ApplicationNumber, facility?.LegalName ?? "", permit.ApplicantId);
        return dto;
    }

    public async Task<PermitApplicationDto?> AssignStaffAsync(int id, string staffId)
    {
        if (!currentUser.IsAdminOrStaff) return null;
        var permit = await permitRepo.GetByIdAsync(id);
        if (permit is null) return null;

        permit.AssignedStaffId = staffId;
        if (permit.Status == PermitStatus.Submitted)
        {
            return await TransitionStatusAsync(permit, PermitStatus.UnderReview, currentUser.UserId!, null);
        }
        await permitRepo.UpdateAsync(permit);
        var facility = await facilityRepo.GetByIdAsync(permit.FacilityId);
        return await ToDetailDto(permit, facility?.LegalName ?? "");
    }

    public async Task<PermitApplicationDto?> ApproveAsync(int id, string? notes)
    {
        if (!currentUser.IsAdminOrStaff) return null;
        var permit = await permitRepo.GetByIdAsync(id);
        if (permit is null || permit.Status != PermitStatus.UnderReview) return null;

        permit.ApprovedAt = DateTime.UtcNow;
        permit.ReviewedAt = DateTime.UtcNow;
        permit.ExpiresAt = DateTime.UtcNow.AddYears(2);
        var dto = await TransitionStatusAsync(permit, PermitStatus.Approved, currentUser.UserId!, notes);
        if (dto is not null)
            notifier.NotifyPermitStatusChanged(permit.Id, permit.ApplicationNumber, "Approved", permit.ApplicantId);
        return dto;
    }

    public async Task<PermitApplicationDto?> DenyAsync(int id, string? notes)
    {
        if (!currentUser.IsAdminOrStaff) return null;
        var permit = await permitRepo.GetByIdAsync(id);
        if (permit is null || permit.Status != PermitStatus.UnderReview) return null;

        permit.ReviewedAt = DateTime.UtcNow;
        var dto = await TransitionStatusAsync(permit, PermitStatus.Denied, currentUser.UserId!, notes);
        if (dto is not null)
            notifier.NotifyPermitStatusChanged(permit.Id, permit.ApplicationNumber, "Denied", permit.ApplicantId);
        return dto;
    }

    public async Task<PermitApplicationDto?> RequestChangesAsync(int id, string? notes)
    {
        if (!currentUser.IsAdminOrStaff) return null;
        var permit = await permitRepo.GetByIdAsync(id);
        if (permit is null || permit.Status != PermitStatus.UnderReview) return null;

        permit.ReviewedAt = DateTime.UtcNow;
        var dto = await TransitionStatusAsync(permit, PermitStatus.ChangesRequested, currentUser.UserId!, notes);
        if (dto is not null)
            notifier.NotifyPermitStatusChanged(permit.Id, permit.ApplicationNumber, "Changes Requested", permit.ApplicantId);
        return dto;
    }

    public async Task<List<ReviewCommentDto>> GetCommentsAsync(int permitId)
    {
        bool includeInternal = currentUser.IsAdminOrStaff || currentUser.IsInspector;
        var comments = await commentRepo.GetByPermitIdAsync(permitId, includeInternal);
        return comments.Select(ToCommentDto).ToList();
    }

    public async Task<ReviewCommentDto> AddCommentAsync(int permitId, CreateReviewCommentRequest request)
    {
        var comment = new ReviewComment
        {
            PermitApplicationId = permitId,
            AuthorId = currentUser.UserId!,
            Content = request.Content,
            IsInternal = request.IsInternal && (currentUser.IsAdminOrStaff || currentUser.IsInspector),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var created = await commentRepo.AddAsync(comment);
        return ToCommentDto(created);
    }

    public async Task<List<PermitStatusHistoryDto>> GetStatusHistoryAsync(int permitId)
    {
        var permit = await permitRepo.GetByIdAsync(permitId, includeRelated: true);
        if (permit is null) return [];
        return permit.StatusHistory?.Select(h => new PermitStatusHistoryDto(
            h.Id, h.FromStatus.ToString(), h.ToStatus.ToString(),
            h.ChangedById, h.ChangedAt, h.Notes)).ToList() ?? [];
    }

    public async Task DeleteCommentAsync(int permitId, int commentId)
    {
        if (!currentUser.IsAdminOrStaff) return;
        await commentRepo.SoftDeleteAsync(commentId);
    }

    private async Task<PermitApplicationDto> TransitionStatusAsync(
        PermitApplication permit, PermitStatus toStatus, string changedById, string? notes)
    {
        var history = new PermitStatusHistory
        {
            PermitApplicationId = permit.Id,
            FromStatus = permit.Status,
            ToStatus = toStatus,
            ChangedById = changedById,
            ChangedAt = DateTime.UtcNow,
            Notes = notes
        };

        if (permit.Status == PermitStatus.Draft && toStatus == PermitStatus.Submitted)
            permit.SubmittedAt = DateTime.UtcNow;

        permit.Status = toStatus;
        await permitRepo.UpdateAsync(permit);
        await permitRepo.AddStatusHistoryAsync(history);

        var facility = await facilityRepo.GetByIdAsync(permit.FacilityId);
        return await ToDetailDto(permit, facility?.LegalName ?? "");
    }

    private bool CanAccessPermit(PermitApplication permit)
    {
        if (currentUser.IsAdminOrStaff || currentUser.IsInspector) return true;
        return permit.ApplicantId == currentUser.UserId;
    }

    private async Task<PermitApplicationDto> ToDetailDto(PermitApplication p, string facilityName)
    {
        var comments = await commentRepo.GetByPermitIdAsync(p.Id, currentUser.IsAdminOrStaff);
        var history = p.StatusHistory?.Select(h => new PermitStatusHistoryDto(
            h.Id, h.FromStatus.ToString(), h.ToStatus.ToString(),
            h.ChangedById, h.ChangedAt, h.Notes)).ToList() ?? [];
        return new PermitApplicationDto(
            p.Id, p.ApplicationNumber, p.FacilityId, facilityName,
            p.ApplicantId, p.PermitType, p.Status,
            p.SubmittedAt, p.ReviewedAt, p.ApprovedAt, p.ExpiresAt,
            p.Description, p.ProjectDetails, p.EstimatedCost, p.AssignedStaffId,
            history, comments.Select(ToCommentDto).ToList());
    }

    private static PermitApplicationSummaryDto ToSummaryDto(PermitApplication p, string facilityName) => new(
        p.Id, p.ApplicationNumber, p.FacilityId, facilityName,
        p.ApplicantId, p.PermitType.ToString(), p.Status.ToString(),
        p.SubmittedAt, p.ExpiresAt, p.AssignedStaffId);

    private static ReviewCommentDto ToCommentDto(ReviewComment c) => new(
        c.Id, c.PermitApplicationId, c.AuthorId, c.Content,
        c.IsInternal, c.CreatedAt, c.UpdatedAt);
}
