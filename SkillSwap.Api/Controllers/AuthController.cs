using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Users.Commands;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="command">Registration details</param>
    /// <returns>JWT token</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        try
        {
            var token = await _mediator.Send(command);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return BadRequest(new { Error = "Registration failed" });
        }
    }

    /// <summary>
    /// Login user
    /// </summary>
    /// <param name="command">Login credentials</param>
    /// <returns>JWT token</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        try
        {
            var token = await _mediator.Send(command);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return Unauthorized(new { Error = "Invalid credentials" });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    [Authorize(Roles = "User,Admin")]
    public IActionResult GetProfile()
    {
        try
        {
            var user = User;
            return Ok(new
            {
                Id = user.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Email = user.FindFirstValue(JwtRegisteredClaimNames.Email),
                DisplayName = user.FindFirstValue("displayName"),
                Roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { Error = "Failed to get user profile" });
        }
    }
}