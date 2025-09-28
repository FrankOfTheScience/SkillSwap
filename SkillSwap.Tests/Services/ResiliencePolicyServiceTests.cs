using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwap.Api.Configuration;
using SkillSwap.Api.Services;
using Xunit;
using FluentAssertions;
using Polly;

namespace SkillSwap.Tests.Services;

public class ResiliencePolicyServiceTests
{
    private readonly ILogger<ResiliencePolicyService> _mockLogger;
    private readonly ResilienceSettings _testSettings;
    private readonly ResiliencePolicyService _service;

    public ResiliencePolicyServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ResiliencePolicyService>>();
        _testSettings = CreateTestResilienceSettings();
        _service = new ResiliencePolicyService(_testSettings, _mockLogger);
    }

    private static ResilienceSettings CreateTestResilienceSettings()
    {
        return new ResilienceSettings
        {
            Retry = new RetrySettings
            {
                MaxAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                MaxDelay = TimeSpan.FromSeconds(5),
                UseJitter = true
            },
            CircuitBreaker = new CircuitBreakerSettings
            {
                HandledEventsAllowedBeforeBreaking = 5,
                DurationOfBreak = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                FailureThreshold = 0.5, // 50%
                SamplingDuration = TimeSpan.FromSeconds(60)
            },
            Timeout = new TimeoutSettings
            {
                DefaultTimeout = TimeSpan.FromSeconds(30),
                DatabaseTimeout = TimeSpan.FromSeconds(45),
                HttpTimeout = TimeSpan.FromSeconds(15),
                ExternalApiTimeout = TimeSpan.FromSeconds(90)
            },
            Database = new DatabaseSettings
            {
                EnableRetry = true,
                MaxRetryAttempts = 5,
                MaxRetryDelay = TimeSpan.FromSeconds(10),
                EnableCircuitBreaker = true
            },
            Http = new HttpSettings
            {
                EnableRetry = true,
                EnableCircuitBreaker = true,
                EnableTimeout = true,
                Clients = new Dictionary<string, HttpClientSettings>
                {
                    ["TestClient"] = new HttpClientSettings
                    {
                        MaxRetryAttempts = 2,
                        Timeout = TimeSpan.FromSeconds(20),
                        EnableCircuitBreaker = true,
                        CircuitBreakerFailureThreshold = 3,
                        CircuitBreakerDuration = TimeSpan.FromSeconds(25)
                    }
                }
            }
        };
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ResiliencePolicyService(null!, _mockLogger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ResiliencePolicyService(_testSettings, null!));
    }

    [Fact]
    public void GetDatabasePipeline_ShouldReturnValidPipeline()
    {
        // Act
        var pipeline = _service.GetDatabasePipeline();

        // Assert
        pipeline.Should().NotBeNull();
        pipeline.Should().BeOfType<ResiliencePipeline>();
    }

    [Fact]
    public void GetHttpPipeline_ShouldReturnValidPipeline()
    {
        // Act
        var pipeline = _service.GetHttpPipeline();

        // Assert
        pipeline.Should().NotBeNull();
        pipeline.Should().BeOfType<ResiliencePipeline>();
    }

    [Fact]
    public void GetExternalApiPipeline_ShouldReturnValidPipeline()
    {
        // Act
        var pipeline = _service.GetExternalApiPipeline();

        // Assert
        pipeline.Should().NotBeNull();
        pipeline.Should().BeOfType<ResiliencePipeline>();
    }

    [Fact]
    public void GetHttpPipeline_WithClientName_ShouldReturnValidPipeline()
    {
        // Act
        var pipeline = _service.GetHttpPipeline("TestClient");

        // Assert
        pipeline.Should().NotBeNull();
        pipeline.Should().BeOfType<ResiliencePipeline>();
    }

    [Fact]
    public void GetTypedPipeline_ShouldReturnValidTypedPipeline()
    {
        // Act
        var pipeline = _service.GetTypedPipeline<string>();

        // Assert
        pipeline.Should().NotBeNull();
        pipeline.Should().BeOfType<ResiliencePipeline<string>>();
    }

    [Fact]
    public async Task DatabasePipeline_ShouldExecuteOperation()
    {
        // Arrange
        var pipeline = _service.GetDatabasePipeline();
        var operationExecuted = false;

        // Act
        await pipeline.ExecuteAsync(async (ct) =>
        {
            operationExecuted = true;
            await Task.CompletedTask;
        });

        // Assert
        operationExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task HttpPipeline_ShouldExecuteOperation()
    {
        // Arrange
        var pipeline = _service.GetHttpPipeline();
        var operationExecuted = false;

        // Act
        await pipeline.ExecuteAsync(async (ct) =>
        {
            operationExecuted = true;
            await Task.CompletedTask;
        });

        // Assert
        operationExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task TypedPipeline_ShouldExecuteOperationAndReturnResult()
    {
        // Arrange
        var pipeline = _service.GetTypedPipeline<int>();
        const int expectedResult = 42;

        // Act
        var result = await pipeline.ExecuteAsync(async (ct) =>
        {
            await Task.CompletedTask;
            return expectedResult;
        });

        // Assert
        result.Should().Be(expectedResult);
    }
}