using Anthropic.SDK;
using CivicFlow.Application.Common;
using CivicFlow.Application.Interfaces;
using CivicFlow.Application.Services;
using CivicFlow.Infrastructure.Repositories;
using CivicFlow.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFlow.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Audit context — populated per-request by AuditLogMiddleware
        services.AddScoped<IAuditContext, AuditContext>();

        // HTTP context accessor for CurrentUserService
        services.AddHttpContextAccessor();

        // Current user
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Repositories
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IPermitRepository, PermitRepository>();
        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddScoped<IViolationRepository, ViolationRepository>();
        services.AddScoped<IReviewCommentRepository, ReviewCommentRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Application services
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IPermitService, PermitService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<IViolationService, ViolationService>();
        services.AddScoped<IAuditService, AuditService>();

        // AI services — switch between real Claude and deterministic Mock via AI_PROVIDER env var
        if (string.Equals(config["AI_PROVIDER"], "claude", StringComparison.OrdinalIgnoreCase))
        {
            var apiKey = config["ANTHROPIC_API_KEY"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                    "ANTHROPIC_API_KEY must be set when AI_PROVIDER=claude");
            services.AddSingleton(_ => new AnthropicClient(new APIAuthentication(apiKey)));
            services.AddScoped<IPermitAIService, ClaudePermitAIService>();
            services.AddScoped<IInspectionAIService, ClaudeInspectionAIService>();
        }
        else
        {
            services.AddScoped<IPermitAIService, MockPermitAIService>();
            services.AddScoped<IInspectionAIService, MockInspectionAIService>();
        }

        // Real-time notifier — null fallback; overridden in Program.cs with SignalRNotifier
        services.AddScoped<IRealtimeNotifier, NullRealtimeNotifier>();

        return services;
    }
}
