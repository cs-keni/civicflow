using System.Security.Claims;
using CivicFlow.Application.Common;

namespace CivicFlow.API.Middleware;

public class AuditLogMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAuditContext auditContext)
    {
        auditContext.UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        auditContext.IpAddress = GetIpAddress(context);
        auditContext.UserAgent = context.Request.Headers.UserAgent.ToString();
        await next(context);
    }

    private static string? GetIpAddress(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
