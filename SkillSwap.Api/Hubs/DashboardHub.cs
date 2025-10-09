using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SkillSwap.Api.Hubs;

[Authorize]
public class DashboardHub : Hub
{
    public async Task JoinUserDashboard()
    {
        var userId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
    }

    public async Task JoinUserGroup(string userId)
    {
        // Verify the user can only join their own group
        var tokenUserId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (tokenUserId == userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
    }

    public async Task JoinAdminDashboard()
    {
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "Admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminDashboard");
        }
    }

    public async Task LeaveUserDashboard()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
    }

    public async Task LeaveAdminDashboard()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminDashboard");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up group memberships
        var userId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        if (userRole == "Admin")
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AdminDashboard");
        }

        await base.OnDisconnectedAsync(exception);
    }
}