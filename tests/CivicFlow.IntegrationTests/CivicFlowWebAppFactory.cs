using System.Net.Http.Json;
using CivicFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFlow.IntegrationTests;

public class CivicFlowWebAppFactory : WebApplicationFactory<Program>
{
    // Each factory instance gets its own InMemory DB so test classes don't share state.
    private readonly string _dbName = "CivicFlowTest-" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Swap SQL Server for InMemory
            var desc = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<CivicFlowDbContext>));
            if (desc is not null) services.Remove(desc);

            services.AddDbContext<CivicFlowDbContext>(opts =>
                opts.UseInMemoryDatabase(_dbName));

            // Relax cookie policy so TestServer (plain HTTP) can set and send auth cookies
            services.PostConfigure<CookieAuthenticationOptions>(
                IdentityConstants.ApplicationScheme, opts =>
                {
                    opts.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    opts.Cookie.SameSite = SameSiteMode.Lax;
                });
        });
    }

    /// <summary>
    /// Logs in with the given credentials and returns the raw cookie string
    /// (e.g. "civicflow_auth=CfDJ8…") suitable for use as a Cookie header value.
    /// </summary>
    public async Task<string> LoginAsync(string email, string password)
    {
        using var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var resp = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        resp.EnsureSuccessStatusCode();

        // Set-Cookie: civicflow_auth=<token>; Path=/; HttpOnly; SameSite=Lax
        var setCookie = resp.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.StartsWith("civicflow_auth=", StringComparison.OrdinalIgnoreCase)) ?? "";
        return setCookie.Split(';')[0]; // strip Path/HttpOnly/SameSite attributes
    }

    /// <summary>Creates an HttpClient that carries the given raw cookie on every request.</summary>
    public HttpClient CreateClientWithCookie(string cookie)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Cookie", cookie);
        return client;
    }

    /// <summary>Logs in as the seeded admin user and returns an authenticated HttpClient.</summary>
    public async Task<HttpClient> CreateAdminClientAsync() =>
        CreateClientWithCookie(await LoginAsync("admin1@civicflow.dev", "CivicFlow@2026!"));

    /// <summary>Logs in as the seeded staff user and returns an authenticated HttpClient.</summary>
    public async Task<HttpClient> CreateStaffClientAsync() =>
        CreateClientWithCookie(await LoginAsync("staff1@civicflow.dev", "CivicFlow@2026!"));

    /// <summary>Logs in as the seeded applicant user and returns an authenticated HttpClient.</summary>
    public async Task<HttpClient> CreateApplicantClientAsync() =>
        CreateClientWithCookie(await LoginAsync("applicant1@civicflow.dev", "CivicFlow@2026!"));
}
