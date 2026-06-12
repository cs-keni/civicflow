using CivicFlow.API.Middleware;
using CivicFlow.Application.DTOs;
using CivicFlow.Application.Validators;
using CivicFlow.Domain.Entities;
using CivicFlow.Infrastructure;
using CivicFlow.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

var isSwaggerGen = builder.Environment.EnvironmentName == "SwaggerGen";

// ── Database ──────────────────────────────────────────────────────────────────
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
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    opts.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// ── Infrastructure DI ─────────────────────────────────────────────────────────
builder.Services.AddInfrastructure();

// ── FluentValidation ──────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

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

    opts.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = "civicflow_auth",
        Description = "HttpOnly session cookie (BFF pattern — set via POST /api/auth/login)"
    });
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "cookieAuth" }
            },
            []
        }
    });
});

// ── Health checks ─────────────────────────────────────────────────────────────
if (!isSwaggerGen)
{
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<CivicFlowDbContext>("database");
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
builder.Services.AddRazorPages();

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

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options", "DENY");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseStaticFiles();
app.UseBlazorFrameworkFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<AuditLogMiddleware>(); // D4: populate audit context from HTTP request
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

app.MapFallbackToFile("index.html");

// ── Seed database ──────────────────────────────────────────────────────────────
if (!isSwaggerGen)
{
    using var scope = app.Services.CreateScope();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

app.Run();

public partial class Program { }
