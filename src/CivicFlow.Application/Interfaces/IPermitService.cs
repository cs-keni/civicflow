using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;

namespace CivicFlow.Application.Interfaces;

public interface IPermitService
{
    Task<PaginatedResult<PermitApplicationSummaryDto>> GetPermitsAsync(int page, int pageSize);
    Task<PermitApplicationDto?> GetPermitAsync(int id);
    Task<PermitApplicationDto> CreatePermitAsync(CreatePermitApplicationRequest request);
    Task<PermitApplicationDto?> UpdatePermitAsync(int id, UpdatePermitApplicationRequest request);
    Task<PermitApplicationDto?> SubmitPermitAsync(int id);
    Task<PermitApplicationDto?> AssignStaffAsync(int id, string staffId);
    Task<PermitApplicationDto?> ApproveAsync(int id, string? notes);
    Task<PermitApplicationDto?> DenyAsync(int id, string? notes);
    Task<PermitApplicationDto?> RequestChangesAsync(int id, string? notes);
    Task<List<PermitStatusHistoryDto>> GetStatusHistoryAsync(int permitId);
    Task<List<ReviewCommentDto>> GetCommentsAsync(int permitId);
    Task<ReviewCommentDto> AddCommentAsync(int permitId, CreateReviewCommentRequest request);
    Task DeleteCommentAsync(int permitId, int commentId);
}
