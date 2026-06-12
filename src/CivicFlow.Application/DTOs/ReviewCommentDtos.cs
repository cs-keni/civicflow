namespace CivicFlow.Application.DTOs;

public record ReviewCommentDto(
    int Id,
    int PermitApplicationId,
    string AuthorId,
    string Content,
    bool IsInternal,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateReviewCommentRequest(
    string Content,
    bool IsInternal);
