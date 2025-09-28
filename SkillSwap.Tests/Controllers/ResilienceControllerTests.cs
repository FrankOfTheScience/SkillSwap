using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwap.Api.Controllers;
using SkillSwap.Api.Services;
using SkillSwap.Tests.Common;
using Xunit;
using FluentAssertions;

namespace SkillSwap.Tests.Controllers;

public class ResilienceControllerTests
{
    private readonly IResiliencePolicyService _mockResiliencePolicy;
    private readonly IResilientDatabaseService _mockDatabaseService;
    private readonly IResilientHttpClientService _mockHttpClientService;
    private readonly ILogger<ResilienceController> _mockLogger;
    private readonly ResilienceController _controller;

    public ResilienceControllerTests()
    {
        _mockResiliencePolicy = Substitute.For<IResiliencePolicyService>();
        _mockDatabaseService = Substitute.For<IResilientDatabaseService>();
        _mockHttpClientService = Substitute.For<IResilientHttpClientService>();
        _mockLogger = Substitute.For<ILogger<ResilienceController>>();
        
        _controller = new ResilienceController(
            _mockResiliencePolicy,
            _mockDatabaseService,
            _mockHttpClientService,
            _mockLogger);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _controller.Should().NotBeNull();
    }

    [Fact]
    public async Task TestResilience_WithSuccessfulExecution_ShouldReturnOkResult()
    {
        // Arrange
        _mockDatabaseService.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<string>>>())
            .Returns(Task.FromResult("Database operation completed successfully"));

        // Act
        var result = await _controller.TestResilience();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
        
        // Verify the response structure
        var responseValue = okResult.Value!.ToString();
        responseValue.Should().Contain("Success = True");
        responseValue.Should().Contain("Resilience patterns are working correctly");
    }

    [Fact]
    public async Task TestResilience_ShouldCallDatabaseService()
    {
        // Arrange
        _mockDatabaseService.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<string>>>())
            .Returns(Task.FromResult("Test result"));

        // Act
        await _controller.TestResilience();

        // Assert
        await _mockDatabaseService.Received(1).ExecuteAsync(Arg.Any<Func<CancellationToken, Task<string>>>());
    }

    [Fact]
    public async Task TestResilience_WithDatabaseException_ShouldReturnInternalServerError()
    {
        // Arrange
        var exception = new InvalidOperationException("Database error");
        _mockDatabaseService.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<string>>>())
            .Returns<string>(_ => throw exception);

        // Act
        var result = await _controller.TestResilience();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value!.ToString().Should().Contain("Resilience test failed");
    }

    [Fact]
    public async Task TestTimeout_WithSuccessfulExecution_ShouldReturnOkResult()
    {
        // Arrange
        const string expectedResult = "Quick operation completed";
        _mockDatabaseService.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<string>>>())
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _controller.TestTimeout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TestTimeout_WithNonTimeoutException_ShouldReturn500()
    {
        // Arrange
        var exception = new InvalidOperationException("Unexpected error");
        _mockDatabaseService.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<string>>>())
            .Returns<string>(_ => throw exception);

        // Act
        var result = await _controller.TestTimeout();

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value!.ToString().Should().Contain("Unexpected error");
    }

    [Fact]
    public async Task TestTimeout_ShouldCallDatabaseService()
    {
        // Arrange
        _mockDatabaseService.ExecuteAsync(Arg.Any<Func<CancellationToken, Task<string>>>())
            .Returns(Task.FromResult("Test result"));

        // Act
        await _controller.TestTimeout();

        // Assert
        await _mockDatabaseService.Received(1).ExecuteAsync(Arg.Any<Func<CancellationToken, Task<string>>>());
    }
}