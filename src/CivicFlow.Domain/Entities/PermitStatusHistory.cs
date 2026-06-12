using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public class PermitStatusHistory
{
    public int Id { get; set; }
    public int PermitApplicationId { get; set; }
    public PermitStatus FromStatus { get; set; }
    public PermitStatus ToStatus { get; set; }
    public string ChangedById { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public PermitApplication PermitApplication { get; set; } = null!;
}
