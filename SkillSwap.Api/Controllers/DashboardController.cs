using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Api.Services;
using System.Security.Claims;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get user dashboard data for the authenticated user
    /// </summary>
    /// <returns>User dashboard statistics and recent activity</returns>
    [HttpGet("user")]
    public async Task<IActionResult> GetUserDashboard()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user ID in token");
            }

            // This will trigger a SignalR update as well
            await _dashboardService.SendUserStatsUpdate(userId);

            return Ok(new { message = "User dashboard data will be sent via SignalR" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user dashboard for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while retrieving dashboard data");
        }
    }

    /// <summary>
    /// Get admin dashboard data - requires admin role
    /// </summary>
    /// <returns>Platform-wide statistics and metrics</returns>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        try
        {
            // This will trigger a SignalR update to all admin dashboard clients
            await _dashboardService.SendAdminStatsUpdate();

            return Ok(new { message = "Admin dashboard data will be sent via SignalR" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin dashboard");
            return StatusCode(500, "An error occurred while retrieving admin dashboard data");
        }
    }

    /// <summary>
    /// Refresh user dashboard data manually
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("user/refresh")]
    public async Task<IActionResult> RefreshUserDashboard()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user ID in token");
            }

            await _dashboardService.SendUserStatsUpdate(userId);
            return Ok(new { message = "Dashboard refresh initiated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing user dashboard for user {UserId}", User.Identity?.Name);
            return StatusCode(500, "An error occurred while refreshing dashboard data");
        }
    }

    /// <summary>
    /// Refresh admin dashboard data manually - requires admin role
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("admin/refresh")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RefreshAdminDashboard()
    {
        try
        {
            await _dashboardService.SendAdminStatsUpdate();
            return Ok(new { message = "Admin dashboard refresh initiated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing admin dashboard");
            return StatusCode(500, "An error occurred while refreshing admin dashboard data");
        }
    }
}