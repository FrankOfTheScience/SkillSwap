using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwap.Api.Middleware;
using SkillSwap.Api.Services;
using SkillSwap.Tests.Common;
using System.Text;
using Xunit;
using FluentAssertions;

namespace SkillSwap.Tests.Middleware;

public class ResilienceMiddlewareTests
{
    private readonly IResiliencePolicyService _mockPolicyService;
    private readonly ILogger<ResilienceMiddleware> _mockLogger;
    private readonly ResilienceMiddleware _middleware;
    private readonly RequestDelegate _nextDelegate;

    public ResilienceMiddlewareTests()
    {
        _mockPolicyService = Substitute.For<IResiliencePolicyService>();
        _mockLogger = Substitute.For<ILogger<ResilienceMiddleware>>();
        _nextDelegate = Substitute.For<RequestDelegate>();
        _middleware = new ResilienceMiddleware(_nextDelegate, _mockPolicyService, _mockLogger);

        // Setup default pipeline
        var mockPipeline = TestHelper.CreateMockPipeline();
        _mockPolicyService.GetHttpPipeline().Returns(mockPipeline);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _middleware.Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithSuccessfulRequest_ShouldCallNextDelegate()
    {
        // Arrange
        var context = CreateHttpContext();
        _nextDelegate.When(x => x(context)).Do(_ => { }); // Mock successful execution

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        await _nextDelegate.Received(1).Invoke(context);
        _mockPolicyService.Received(1).GetHttpPipeline();
    }

    [Fact]
    public async Task InvokeAsync_WithTimeoutException_ShouldReturn408()
    {
        // Arrange
        var context = CreateHttpContext();
        _nextDelegate.When(x => x(context)).Do(_ => throw new TimeoutException("Request timed out"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(408); // Request Timeout
        await _nextDelegate.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithCircuitBreakerException_ShouldReturn503()
    {
        // Arrange
        var context = CreateHttpContext();
        _nextDelegate.When(x => x(context)).Do(_ => throw new Exception("Circuit breaker is open"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(503); // Service Unavailable
        await _nextDelegate.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseResiliencePipeline()
    {
        // Arrange
        var context = CreateHttpContext();
        _nextDelegate.When(x => x(context)).Do(_ => { });

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockPolicyService.Received(1).GetHttpPipeline();
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleRequests_ShouldUseResiliencePipelineEachTime()
    {
        // Arrange
        var context1 = CreateHttpContext("/api/test1");
        var context2 = CreateHttpContext("/api/test2");
        _nextDelegate.When(x => x(Arg.Any<HttpContext>())).Do(_ => { });

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);

        // Assert
        _mockPolicyService.Received(2).GetHttpPipeline();
        await _nextDelegate.Received(2).Invoke(Arg.Any<HttpContext>());
    }

    private static HttpContext CreateHttpContext(string path = "/api/test")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = "GET";
        context.Response.Body = new MemoryStream();
        return context;
    }
}