using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CivicFlow.API.Hubs;

// Phase 4 (TODO-7): real-time audit log feed for Admin role.
[Authorize(Roles = "Admin")]
public class AdminActivityHub : Hub { }
