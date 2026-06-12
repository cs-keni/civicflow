using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using CivicFlow.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

var isSwaggerGen = builder.Environment.EnvironmentName == "SwaggerGen";

// ── Database ──────────────────────────────────────────────────────────────────
// SwaggerGen guard: use in-memory DB when generating Swagger docs in CI so the
// step does not require a live SQL Server connection.
if (isSwaggerGen)
{
    builder.Services.AddDbContext<CivicFlowDbContext>(opts =>
        opts.UseInMemoryDatabase("SwaggerGen"));
}
else
{
    builder.Services.AddDbContext<CivicFlowDbContext>(opts =>
        opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 8;
    opts.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<CivicFlowDbContext>()
.AddDefaultTokenProviders();

// ── Cookie auth (BFF pattern, D1) ────────────────────────────────────────────
// HttpOnly SameSite=Strict: no JWT in JS, OWASP A07 compliant.
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Cookie.HttpOnly = true;
    opts.Cookie.SameSite = SameSiteMode.Strict;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opts.Cookie.Name = "civicflow_auth";
    opts.ExpireTimeSpan = TimeSpan.FromHours(8);
    opts.SlidingExpiration = true;
    opts.Events.OnRedirectToLogin = ctx =>
    {
        // Return 401 instead of redirect — Blazor WASM handles navigation
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    opts.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new()
    {
        Title = "CivicFlow API",
        Version = "v1",
        Description = "Permit and compliance management platform — portfolio project targeting Windsor Solutions"
    });

    // Cookie auth security definition added in Phase 2 when auth endpoints exist
});

// ── Health checks (skip when generating Swagger — no DB available) ─────────────
// EF DbContext health check (AspNetCore.HealthChecks.EntityFrameworkCore) added in Phase 2
if (!isSwaggerGen)
{
    builder.Services.AddHealthChecks();
}

// ── Rate limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(opts =>
{
    opts.AddFixedWindowLimiter("login", policy =>
    {
        policy.PermitLimit = 5;
        policy.Window = TimeSpan.FromMinutes(1);
        policy.QueueLimit = 0;
    });
});

// ── Blazor WASM hosting (same-origin, D2/D14) ─────────────────────────────────
// CivicFlow.Client wwwroot is served from the API — same origin as the API,
// which is required for SameSite=Strict cookies to work (D1).
builder.Services.AddRazorPages(); // required for _Host fallback

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "CivicFlow v1");
        opts.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// CSP header — tightened in Phase 2 after all endpoints are known
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options", "DENY");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseStaticFiles();           // Blazor WASM static files
app.UseBlazorFrameworkFiles(); // serves _framework/ for WASM

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CivicFlow.API.Hubs.PermitStatusHub>("/hubs/permit-status");
app.MapHub<CivicFlow.API.Hubs.ReviewQueueHub>("/hubs/review-queue");
app.MapHub<CivicFlow.API.Hubs.InspectionHub>("/hubs/inspection");
app.MapHub<CivicFlow.API.Hubs.AdminActivityHub>("/hubs/admin-activity");

if (!isSwaggerGen)
{
    app.MapHealthChecks("/health");
}

// Fallback to Blazor WASM index.html for all non-API routes (SPA routing)
app.MapFallbackToFile("index.html");

// ── Seed database ──────────────────────────────────────────────────────────────
if (!isSwaggerGen)
{
    using var scope = app.Services.CreateScope();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();

// Exposed for WebApplicationFactory in integration tests
public partial class Program { }
