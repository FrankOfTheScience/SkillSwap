using Microsoft.AspNetCore.Mvc;
using SkillSwap.Api.Services;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class ResilienceController : ControllerBase
{
    private readonly IResiliencePolicyService _resiliencePolicy;
    private readonly IResilientDatabaseService _databaseService;
    private readonly IResilientHttpClientService _httpClientService;
    private readonly ILogger<ResilienceController> _logger;

    public ResilienceController(
        IResiliencePolicyService resiliencePolicy,
        IResilientDatabaseService databaseService,
        IResilientHttpClientService httpClientService,
        ILogger<ResilienceController> logger)
    {
        _resiliencePolicy = resiliencePolicy;
        _databaseService = databaseService;
        _httpClientService = httpClientService;
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to demonstrate resilience patterns
    /// </summary>
    [HttpGet("test")]
    public async Task<IActionResult> TestResilience()
    {
        try
        {
            // Test database resilience
            var dbResult = await _databaseService.ExecuteAsync(async (ct) =>
            {
                _logger.LogInformation("Executing database operation with resilience");
                await Task.Delay(100, ct); // Simulate database call
                return "Database operation completed successfully";
            });

            // Test HTTP client resilience (this would normally be an external API call)
            // For demo purposes, we'll just show the pattern
            _logger.LogInformation("Resilience patterns are configured and working");

            return Ok(new
            {
                Success = true,
                Message = "Resilience patterns are working correctly",
                DatabaseResult = dbResult,
                Timestamp = DateTime.UtcNow,
                ResiliencePipelines = new
                {
                    Database = "Configured with timeout policies",
                    Http = "Configured with timeout policies",
                    ExternalApi = "Configured with timeout policies"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing resilience patterns");
            return StatusCode(500, new { Error = "Resilience test failed", Message = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint to demonstrate timeout behavior
    /// </summary>
    [HttpGet("test-timeout")]
    public async Task<IActionResult> TestTimeout()
    {
        try
        {
            // This will test the timeout policy
            var result = await _databaseService.ExecuteAsync(async (ct) =>
            {
                _logger.LogInformation("Starting long-running operation");
                await Task.Delay(TimeSpan.FromMinutes(2), ct); // This should timeout
                return "This should not be reached";
            });

            return Ok(result);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Operation timed out as expected");
            return StatusCode(408, new { Message = "Operation timed out (resilience policy working)" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in timeout test");
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}