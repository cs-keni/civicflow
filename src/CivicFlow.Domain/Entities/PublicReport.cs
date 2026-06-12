using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public class PublicReport
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ReportType ReportType { get; set; }
    public int? FacilityId { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; }

    public Facility? Facility { get; set; }
}
