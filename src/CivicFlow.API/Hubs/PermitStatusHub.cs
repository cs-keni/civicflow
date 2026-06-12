using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CivicFlow.API.Hubs;

// Phase 4 (TODO-7): real-time permit status updates for Applicant + Staff roles.
// Fire-and-forget sends from controllers/services (D12).
// Cookie auth via withCredentials on the WASM client (D13).
[Authorize]
public class PermitStatusHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }
}
