using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CivicFlow.API.Hubs;

[Authorize(Roles = "Admin")]
public class AdminActivityHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin-feed");
        await base.OnConnectedAsync();
    }
}
