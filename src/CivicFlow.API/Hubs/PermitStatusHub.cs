using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CivicFlow.API.Hubs;

[Authorize]
public class PermitStatusHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is null)
        {
            await base.OnConnectedAsync();
            return;
        }

        // Every authenticated user subscribes to their personal feed
        await Groups.AddToGroupAsync(Context.ConnectionId, $"applicant-{userId}");

        // Role-based group membership
        var user = Context.User!;
        if (user.IsInRole("AgencyStaff") || user.IsInRole("Admin"))
            await Groups.AddToGroupAsync(Context.ConnectionId, "staff-reviewers");

        if (user.IsInRole("Inspector") || user.IsInRole("AgencyStaff") || user.IsInRole("Admin"))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"inspector-{userId}");

        if (user.IsInRole("Admin"))
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin-feed");

        await base.OnConnectedAsync();
    }
}
