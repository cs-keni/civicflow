using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CivicFlow.API.Hubs;

// Phase 4 (TODO-7): real-time inspection assignment + status updates for Inspector role.
[Authorize(Roles = "Inspector,Staff,Admin")]
public class InspectionHub : Hub { }
