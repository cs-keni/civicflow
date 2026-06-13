using System.Net;
using System.Net.Http.Json;
using CivicFlow.Application.Common;
using CivicFlow.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CivicFlow.IntegrationTests.Api;

public class PermitsEndpointTests(CivicFlowWebAppFactory factory) : IClassFixture<CivicFlowWebAppFactory>
{
    [Fact]
    public async Task GetPermits_WithoutAuth_Returns401()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.GetAsync("/api/permits");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostPermit_WithoutAuth_Returns401()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.PostAsJsonAsync("/api/permits", new { });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPermits_AsAdmin_ReturnsPaginatedResult()
    {
        using var client = await factory.CreateAdminClientAsync();

        var resp = await client.GetAsync("/api/permits?page=1&pageSize=10");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await resp.Content.ReadFromJsonAsync<PaginatedResult<PermitApplicationSummaryDto>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPermits_PageSizeRespected()
    {
        using var client = await factory.CreateAdminClientAsync();

        var resp = await client.GetAsync("/api/permits?page=1&pageSize=5");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await resp.Content.ReadFromJsonAsync<PaginatedResult<PermitApplicationSummaryDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetFacilities_WithoutAuth_Returns401()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.GetAsync("/api/facilities");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFacilities_AsAdmin_ReturnsSeededFacilities()
    {
        using var client = await factory.CreateAdminClientAsync();

        var resp = await client.GetAsync("/api/facilities?page=1&pageSize=20");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await resp.Content.ReadFromJsonAsync<PaginatedResult<FacilityDto>>();
        result.Should().NotBeNull();
        result!.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetInspections_WithoutAuth_Returns401()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.GetAsync("/api/inspections");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
