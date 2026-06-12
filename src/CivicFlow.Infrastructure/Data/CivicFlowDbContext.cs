using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CivicFlow.Infrastructure.Data;

public class CivicFlowDbContext : IdentityDbContext<ApplicationUser>
{
    public CivicFlowDbContext(DbContextOptions<CivicFlowDbContext> options) : base(options) { }

    // Domain entity DbSets added in Phase 1
    // public DbSet<PermitApplication> PermitApplications { get; set; }
    // public DbSet<Inspection> Inspections { get; set; }
    // public DbSet<Violation> Violations { get; set; }
    // public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Fluent API configuration added in Phase 1
    }
}
