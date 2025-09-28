using Polly;
using SkillSwap.Api.Services;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Middleware;

[ExcludeFromCodeCoverage]
public class ResilienceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IResiliencePolicyService _resiliencePolicyService;
    private readonly ILogger<ResilienceMiddleware> _logger;

    public ResilienceMiddleware(
        RequestDelegate next,
        IResiliencePolicyService resiliencePolicyService,
        ILogger<ResilienceMiddleware> logger)
    {
        _next = next;
        _resiliencePolicyService = resiliencePolicyService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path.Value ?? "unknown";
        
        try
        {
            // Apply resilience policy to HTTP request processing
            var pipeline = _resiliencePolicyService.GetHttpPipeline();
            
            await pipeline.ExecuteAsync(async (cancellationToken) =>
            {
                await _next(context);
            }, context.RequestAborted);

            stopwatch.Stop();
            _logger.LogInformation("Request {RequestPath} completed in {ElapsedMilliseconds}ms", 
                requestPath, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {RequestPath} failed after {ElapsedMilliseconds}ms", 
                requestPath, stopwatch.ElapsedMilliseconds);

            // Handle different types of resilience failures
            if (ex is TimeoutException)
            {
                context.Response.StatusCode = 408; // Request Timeout
                await context.Response.WriteAsync("Request timeout. Please try again.");
            }
            else if (ex.Message.Contains("circuit") || ex.Message.Contains("breaker"))
            {
                context.Response.StatusCode = 503; // Service Unavailable
                await context.Response.WriteAsync("Service temporarily unavailable. Please try again later.");
            }
            else
            {
                throw; // Let other middleware handle it
            }
        }
    }
}

[ExcludeFromCodeCoverage]
public static class ResilienceMiddlewareExtensions
{
    public static IApplicationBuilder UseResilience(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResilienceMiddleware>();
    }
}