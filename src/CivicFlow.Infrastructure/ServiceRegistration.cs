using CivicFlow.Application.Common;
using CivicFlow.Application.Interfaces;
using CivicFlow.Application.Services;
using CivicFlow.Infrastructure.Repositories;
using CivicFlow.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CivicFlow.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
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

        // AI services (Phase 5 — stubs registered in Phase 2 so controllers compile)
        // Actual Claude / Mock implementations added in Phase 5
        services.AddScoped<IPermitAIService, StubPermitAIService>();
        services.AddScoped<IInspectionAIService, StubInspectionAIService>();

        return services;
    }
}
