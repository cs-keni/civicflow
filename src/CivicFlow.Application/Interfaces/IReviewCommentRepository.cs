using CivicFlow.Domain.Entities;

namespace CivicFlow.Application.Interfaces;

public interface IReviewCommentRepository
{
    Task<ReviewComment?> GetByIdAsync(int id);
    Task<List<ReviewComment>> GetByPermitIdAsync(int permitId, bool includeInternal);
    Task<ReviewComment> AddAsync(ReviewComment comment);
    Task SoftDeleteAsync(int id);
}
