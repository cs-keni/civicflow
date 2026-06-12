using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public class Violation
{
    public int Id { get; set; }
    public string ViolationNumber { get; set; } = string.Empty;
    public int InspectionId { get; set; }
    public int FacilityId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? RegulatoryBasis { get; set; }
    public ViolationSeverity Severity { get; set; }
    public ViolationStatus Status { get; set; } = ViolationStatus.Open;
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? Notes { get; set; }

    public Inspection Inspection { get; set; } = null!;
    public Facility Facility { get; set; } = null!;
}
