using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public class Facility
{
    public int Id { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string? DbaName { get; set; }
    public FacilityType FacilityType { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PermitApplication> PermitApplications { get; set; } = new List<PermitApplication>();
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    public ICollection<PublicReport> PublicReports { get; set; } = new List<PublicReport>();
}
