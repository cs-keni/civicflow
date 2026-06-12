using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public class PermitApplication
{
    public int Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public int FacilityId { get; set; }
    public string ApplicantId { get; set; } = string.Empty;
    public PermitType PermitType { get; set; }
    public PermitStatus Status { get; set; } = PermitStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ProjectDetails { get; set; } = string.Empty;
    public decimal? EstimatedCost { get; set; }
    public string? AssignedStaffId { get; set; }

    public Facility Facility { get; set; } = null!;
    public ICollection<PermitStatusHistory> StatusHistory { get; set; } = new List<PermitStatusHistory>();
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
}
