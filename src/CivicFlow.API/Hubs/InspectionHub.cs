using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CivicFlow.API.Hubs;

[Authorize(Roles = "Inspector,AgencyStaff,Admin")]
public class InspectionHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"inspector-{userId}");

        await base.OnConnectedAsync();
    }
}
