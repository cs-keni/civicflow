using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CivicFlow.IntegrationTests.Api;

public class AuthEndpointTests(CivicFlowWebAppFactory factory) : IClassFixture<CivicFlowWebAppFactory>
{
    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndSetsCookie()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin1@civicflow.dev", password = "CivicFlow@2026!" });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Headers.Should().ContainKey("Set-Cookie");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin1@civicflow.dev", password = "WrongPassword1!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithoutAuth_Returns401()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var resp = await client.GetAsync("/api/auth/me");

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_AfterLogin_ReturnsCorrectEmail()
    {
        using var client = await factory.CreateAdminClientAsync();

        var resp = await client.GetAsync("/api/auth/me");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("admin1@civicflow.dev");
    }

    [Fact]
    public async Task Logout_ClearsSession()
    {
        // Use the default client whose cookie jar honors Set-Cookie responses
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Login — cookie jar auto-stores the auth cookie
        var loginResp = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin1@civicflow.dev", password = "CivicFlow@2026!" });
        loginResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Logout — server sends Set-Cookie with Max-Age=0, cookie jar clears it
        await client.PostAsync("/api/auth/logout", null);

        // /me should return 401 because the cookie was cleared from the jar
        var resp = await client.GetAsync("/api/auth/me");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
