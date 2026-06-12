namespace CivicFlow.Domain.Entities;

public class ReviewComment
{
    public int Id { get; set; }
    public int PermitApplicationId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public PermitApplication PermitApplication { get; set; } = null!;
}
