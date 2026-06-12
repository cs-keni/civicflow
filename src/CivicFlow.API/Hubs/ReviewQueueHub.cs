using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CivicFlow.API.Hubs;

// Phase 4 (TODO-7): real-time review queue updates for Staff role.
[Authorize(Roles = "Staff,Admin")]
public class ReviewQueueHub : Hub { }
