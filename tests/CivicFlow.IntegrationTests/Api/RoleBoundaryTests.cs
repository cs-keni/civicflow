using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CivicFlow.IntegrationTests.Api;

/// <summary>
/// Verifies role-specific 403 Forbidden responses — ensuring that authenticated users
/// cannot escalate to endpoints reserved for higher roles. These test the [Authorize(Roles=...)]
/// guards at the controller level, not business logic, so no seeded entity IDs are required.
/// </summary>
public class RoleBoundaryTests(CivicFlowWebAppFactory factory) : IClassFixture<CivicFlowWebAppFactory>
{
    // ── Permit staff-only actions (Applicant → 403) ───────────────────────────

    [Fact]
    public async Task ApprovePermit_AsApplicant_Returns403()
    {
        using var client = await factory.CreateApplicantClientAsync();

        var resp = await client.PostAsJsonAsync("/api/permits/1/approve", new { notes = "test" });

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DenyPermit_AsApplicant_Returns403()
    {
        using var client = await factory.CreateApplicantClientAsync();

        var resp = await client.PostAsJsonAsync("/api/permits/1/deny", new { notes = "test" });

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignPermit_AsApplicant_Returns403()
    {
        using var client = await factory.CreateApplicantClientAsync();

        var resp = await client.PostAsJsonAsync("/api/permits/1/assign", new { staffUserId = "x" });

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteComment_AsApplicant_Returns403()
    {
        using var client = await factory.CreateApplicantClientAsync();

        var resp = await client.DeleteAsync("/api/permits/1/comments/1");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Admin-only endpoints (Staff → 403) ────────────────────────────────────

    [Fact]
    public async Task GetAuditLogs_AsStaff_Returns403()
    {
        using var client = await factory.CreateStaffClientAsync();

        var resp = await client.GetAsync("/api/admin/audit-logs");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuditLogs_AsApplicant_Returns403()
    {
        using var client = await factory.CreateApplicantClientAsync();

        var resp = await client.GetAsync("/api/admin/audit-logs");

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
