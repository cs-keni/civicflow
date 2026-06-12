using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public class Inspection
{
    public int Id { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public int PermitApplicationId { get; set; }
    public int FacilityId { get; set; }
    public string InspectorId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public InspectionStatus Status { get; set; } = InspectionStatus.Scheduled;
    public InspectionType InspectionType { get; set; }
    public string? FieldNotes { get; set; }
    public string? PublicSummary { get; set; }
    public InspectionResult? OverallRating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PermitApplication PermitApplication { get; set; } = null!;
    public Facility Facility { get; set; } = null!;
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
