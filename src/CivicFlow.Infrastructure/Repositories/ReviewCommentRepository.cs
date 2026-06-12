using CivicFlow.Application.Interfaces;
using CivicFlow.Domain.Entities;
using CivicFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Repositories;

public class ReviewCommentRepository(CivicFlowDbContext db) : IReviewCommentRepository
{
    public Task<ReviewComment?> GetByIdAsync(int id) =>
        db.ReviewComments.FirstOrDefaultAsync(c => c.Id == id);

    public Task<List<ReviewComment>> GetByPermitIdAsync(int permitId, bool includeInternal)
    {
        var q = db.ReviewComments.Where(c => c.PermitApplicationId == permitId);
        if (!includeInternal) q = q.Where(c => !c.IsInternal);
        return q.OrderBy(c => c.CreatedAt).ToListAsync();
    }

    public async Task<ReviewComment> AddAsync(ReviewComment comment)
    {
        db.ReviewComments.Add(comment);
        await db.SaveChangesAsync();
        return comment;
    }

    public async Task SoftDeleteAsync(int id)
    {
        var comment = await db.ReviewComments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null) return;
        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
