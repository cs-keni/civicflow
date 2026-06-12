using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CivicFlow.API.Hubs;

[Authorize(Roles = "AgencyStaff,Admin")]
public class ReviewQueueHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "staff-reviewers");
        await base.OnConnectedAsync();
    }
}
