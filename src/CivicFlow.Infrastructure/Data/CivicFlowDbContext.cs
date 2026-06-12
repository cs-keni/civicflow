using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Data;

public class CivicFlowDbContext : IdentityDbContext<ApplicationUser>
{
    public CivicFlowDbContext(DbContextOptions<CivicFlowDbContext> options) : base(options) { }

    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<PermitApplication> PermitApplications => Set<PermitApplication>();
    public DbSet<PermitStatusHistory> PermitStatusHistories => Set<PermitStatusHistory>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<Violation> Violations => Set<Violation>();
    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
    public DbSet<PublicReport> PublicReports => Set<PublicReport>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureSequences(builder);
        ConfigureFacility(builder);
        ConfigurePermitApplication(builder);
        ConfigurePermitStatusHistory(builder);
        ConfigureInspection(builder);
        ConfigureViolation(builder);
        ConfigureReviewComment(builder);
        ConfigurePublicReport(builder);
        ConfigureAuditLog(builder);
    }

    private static void ConfigureSequences(ModelBuilder builder)
    {
        builder.HasSequence<int>("PermitNumberSequence", "seq").StartsAt(1).IncrementsBy(1);
        builder.HasSequence<int>("InspectionNumberSequence", "seq").StartsAt(1).IncrementsBy(1);
        builder.HasSequence<int>("ViolationNumberSequence", "seq").StartsAt(1).IncrementsBy(1);
    }

    private static void ConfigureFacility(ModelBuilder builder)
    {
        builder.Entity<Facility>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.LegalName).HasMaxLength(200).IsRequired();
            e.Property(f => f.DbaName).HasMaxLength(200);
            e.Property(f => f.FacilityType).HasConversion<string>().HasMaxLength(50);
            e.Property(f => f.Address).HasMaxLength(300).IsRequired();
            e.Property(f => f.City).HasMaxLength(100).IsRequired();
            e.Property(f => f.State).HasMaxLength(50).IsRequired();
            e.Property(f => f.ZipCode).HasMaxLength(20).IsRequired();
            e.Property(f => f.County).HasMaxLength(100).IsRequired();
            e.Property(f => f.OwnerId).HasMaxLength(450).IsRequired();

            e.HasIndex(f => f.OwnerId);
            e.HasIndex(f => f.IsActive);
        });
    }

    private static void ConfigurePermitApplication(ModelBuilder builder)
    {
        builder.Entity<PermitApplication>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.ApplicationNumber)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValueSql(
                    "'APP-' + CAST(YEAR(GETDATE()) AS NVARCHAR(4)) + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR seq.PermitNumberSequence AS NVARCHAR(4)), 4)");
            e.Property(p => p.ApplicantId).HasMaxLength(450).IsRequired();
            e.Property(p => p.AssignedStaffId).HasMaxLength(450);
            e.Property(p => p.PermitType).HasConversion<string>().HasMaxLength(50);
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(p => p.Description).HasMaxLength(2000).IsRequired();
            e.Property(p => p.ProjectDetails).HasMaxLength(4000).IsRequired();
            e.Property(p => p.EstimatedCost).HasColumnType("decimal(18,2)");

            e.HasIndex(p => p.ApplicationNumber).IsUnique();
            e.HasIndex(p => p.ApplicantId);
            e.HasIndex(p => p.Status);

            e.HasOne(p => p.Facility)
                .WithMany(f => f.PermitApplications)
                .HasForeignKey(p => p.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePermitStatusHistory(ModelBuilder builder)
    {
        builder.Entity<PermitStatusHistory>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(50);
            e.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(50);
            e.Property(h => h.ChangedById).HasMaxLength(450).IsRequired();
            e.Property(h => h.Notes).HasMaxLength(1000);

            e.HasIndex(h => h.PermitApplicationId);

            e.HasOne(h => h.PermitApplication)
                .WithMany(p => p.StatusHistory)
                .HasForeignKey(h => h.PermitApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureInspection(ModelBuilder builder)
    {
        builder.Entity<Inspection>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.InspectionNumber)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValueSql(
                    "'INS-' + CAST(YEAR(GETDATE()) AS NVARCHAR(4)) + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR seq.InspectionNumberSequence AS NVARCHAR(4)), 4)");
            e.Property(i => i.InspectorId).HasMaxLength(450).IsRequired();
            e.Property(i => i.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(i => i.InspectionType).HasConversion<string>().HasMaxLength(50);
            e.Property(i => i.OverallRating).HasConversion<string>().HasMaxLength(50);
            e.Property(i => i.FieldNotes).HasMaxLength(8000);
            e.Property(i => i.PublicSummary).HasMaxLength(4000);

            e.HasIndex(i => i.InspectionNumber).IsUnique();
            e.HasIndex(i => i.InspectorId);
            e.HasIndex(i => i.Status);

            e.HasOne(i => i.PermitApplication)
                .WithMany(p => p.Inspections)
                .HasForeignKey(i => i.PermitApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.Facility)
                .WithMany(f => f.Inspections)
                .HasForeignKey(i => i.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureViolation(ModelBuilder builder)
    {
        builder.Entity<Violation>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.ViolationNumber)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValueSql(
                    "'VIO-' + CAST(YEAR(GETDATE()) AS NVARCHAR(4)) + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR seq.ViolationNumberSequence AS NVARCHAR(4)), 4)");
            e.Property(v => v.Code).HasMaxLength(50).IsRequired();
            e.Property(v => v.Description).HasMaxLength(2000).IsRequired();
            e.Property(v => v.RegulatoryBasis).HasMaxLength(500);
            e.Property(v => v.Severity).HasConversion<string>().HasMaxLength(50);
            e.Property(v => v.Status).HasConversion<string>().HasMaxLength(50);
            e.Property(v => v.Notes).HasMaxLength(2000);

            e.HasIndex(v => v.ViolationNumber).IsUnique();
            e.HasIndex(v => v.Status);

            e.HasOne(v => v.Inspection)
                .WithMany(i => i.Violations)
                .HasForeignKey(v => v.InspectionId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.Facility)
                .WithMany(f => f.Violations)
                .HasForeignKey(v => v.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReviewComment(ModelBuilder builder)
    {
        builder.Entity<ReviewComment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.AuthorId).HasMaxLength(450).IsRequired();
            e.Property(c => c.Content).HasMaxLength(4000).IsRequired();

            // D7: soft delete — queries automatically exclude deleted rows
            e.HasQueryFilter(c => !c.IsDeleted);

            e.HasIndex(c => c.PermitApplicationId);

            e.HasOne(c => c.PermitApplication)
                .WithMany(p => p.ReviewComments)
                .HasForeignKey(c => c.PermitApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePublicReport(ModelBuilder builder)
    {
        builder.Entity<PublicReport>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Title).HasMaxLength(300).IsRequired();
            e.Property(r => r.ReportType).HasConversion<string>().HasMaxLength(50);
            e.Property(r => r.Content).HasColumnType("nvarchar(max)").IsRequired();

            e.HasIndex(r => r.IsPublished);

            e.HasOne(r => r.Facility)
                .WithMany(f => f.PublicReports)
                .HasForeignKey(r => r.FacilityId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }

    private static void ConfigureAuditLog(ModelBuilder builder)
    {
        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            e.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
            e.Property(a => a.Action).HasConversion<string>().HasMaxLength(50);
            e.Property(a => a.UserId).HasMaxLength(450);
            e.Property(a => a.IpAddress).HasMaxLength(45);
            e.Property(a => a.UserAgent).HasMaxLength(500);
            // OldValues / NewValues are JSON — no FK, no size limit
            e.Property(a => a.OldValues).HasColumnType("nvarchar(max)");
            e.Property(a => a.NewValues).HasColumnType("nvarchar(max)");

            e.HasIndex(a => a.OccurredAt);
            e.HasIndex(a => new { a.EntityType, a.EntityId });
            e.HasIndex(a => a.UserId);

            // No FK to ApplicationUser: log entries outlive users (append-only)
        });
    }
}
