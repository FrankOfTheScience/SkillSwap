using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwap.Api.Services;
using SkillSwap.Tests.Common;
using Xunit;
using FluentAssertions;
using Polly;

namespace SkillSwap.Tests.Services;

public class ResilientDatabaseServiceTests
{
    private readonly IResiliencePolicyService _mockPolicyService;
    private readonly ILogger<ResilientDatabaseService> _mockLogger;
    private readonly ResilientDatabaseService _service;

    public ResilientDatabaseServiceTests()
    {
        _mockPolicyService = Substitute.For<IResiliencePolicyService>();
        _mockLogger = Substitute.For<ILogger<ResilientDatabaseService>>();
        _service = new ResilientDatabaseService(_mockPolicyService, _mockLogger);

        // Setup default pipeline that just executes the operation
        var mockPipeline = TestHelper.CreateMockPipeline();
        _mockPolicyService.GetDatabasePipeline().Returns(mockPipeline);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullPolicyService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ResilientDatabaseService(null!, _mockLogger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ResilientDatabaseService(_mockPolicyService, null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithResult_ShouldExecuteOperationAndReturnResult()
    {
        // Arrange
        const int expectedResult = 42;
        var operationExecuted = false;

        // Act
        var result = await _service.ExecuteAsync(async (ct) =>
        {
            operationExecuted = true;
            await Task.CompletedTask;
            return expectedResult;
        });

        // Assert
        result.Should().Be(expectedResult);
        operationExecuted.Should().BeTrue();
        _mockPolicyService.Received(1).GetDatabasePipeline();
    }

    [Fact]
    public async Task ExecuteAsync_WithoutResult_ShouldExecuteOperation()
    {
        // Arrange
        var operationExecuted = false;

        // Act
        await _service.ExecuteAsync(async (ct) =>
        {
            operationExecuted = true;
            await Task.CompletedTask;
        });

        // Assert
        operationExecuted.Should().BeTrue();
        _mockPolicyService.Received(1).GetDatabasePipeline();
    }

    [Fact]
    public async Task ExecuteAsync_WithException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ExecuteAsync<string>(async (ct) =>
            {
                await Task.CompletedTask;
                throw expectedException;
            }));

        exception.Should().Be(expectedException);
        _mockPolicyService.Received(1).GetDatabasePipeline();
    }

    [Fact]
    public async Task ExecuteAsync_VoidOperation_WithException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ExecuteAsync(async (ct) =>
            {
                await Task.CompletedTask;
                throw expectedException;
            }));

        exception.Should().Be(expectedException);
        _mockPolicyService.Received(1).GetDatabasePipeline();
    }

    [Fact]
    public async Task ExecuteAsync_MultipleOperations_ShouldUseDatabasePipelineEachTime()
    {
        // Arrange & Act
        await _service.ExecuteAsync(async (ct) => { await Task.CompletedTask; return "First"; });
        await _service.ExecuteAsync(async (ct) => { await Task.CompletedTask; return "Second"; });
        await _service.ExecuteAsync(async (ct) => { await Task.CompletedTask; });

        // Assert
        _mockPolicyService.Received(3).GetDatabasePipeline();
    }

    [Fact]
    public async Task ExecuteAsync_WithLongRunningOperation_ShouldNotTimeout()
    {
        // Arrange
        const string expectedResult = "Long operation completed";

        // Act
        var result = await _service.ExecuteAsync(async (ct) =>
        {
            // Simulate a database operation that takes some time but is within timeout
            await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
            return expectedResult;
        });

        // Assert
        result.Should().Be(expectedResult);
        _mockPolicyService.Received(1).GetDatabasePipeline();
    }

    [Fact]
    public async Task ExecuteAsync_WithGenericType_ShouldWorkWithDifferentTypes()
    {
        // Arrange
        var testDto = new TestDto { Id = 1, Name = "Test" };

        // Act
        var result = await _service.ExecuteAsync(async (ct) =>
        {
            await Task.CompletedTask;
            return testDto;
        });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(testDto.Id);
        result.Name.Should().Be(testDto.Name);
        _mockPolicyService.Received(1).GetDatabasePipeline();
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}