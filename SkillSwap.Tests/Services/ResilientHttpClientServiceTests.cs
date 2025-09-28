using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwap.Api.Services;
using SkillSwap.Tests.Common;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using FluentAssertions;

namespace SkillSwap.Tests.Services;

public class ResilientHttpClientServiceTests
{
    private readonly IResiliencePolicyService _mockPolicyService;
    private readonly ILogger<ResilientHttpClientService> _mockLogger;
    private readonly TestHttpMessageHandler _testHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ResilientHttpClientService _service;

    public ResilientHttpClientServiceTests()
    {
        _mockPolicyService = Substitute.For<IResiliencePolicyService>();
        _mockLogger = Substitute.For<ILogger<ResilientHttpClientService>>();
        _testHttpMessageHandler = new TestHttpMessageHandler();
        
        _httpClient = new HttpClient(_testHttpMessageHandler)
        {
            BaseAddress = new Uri("https://api.test.com")
        };
        
        _service = new ResilientHttpClientService(_httpClient, _mockPolicyService, _mockLogger);
        
        // Setup default pipeline that just executes the operation
        var mockPipeline = TestHelper.CreateMockPipeline();
        _mockPolicyService.GetHttpPipeline().Returns(mockPipeline);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ResilientHttpClientService(null!, _mockPolicyService, _mockLogger));
    }

    [Fact]
    public void Constructor_WithNullPolicyService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ResilientHttpClientService(_httpClient, null!, _mockLogger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ResilientHttpClientService(_httpClient, _mockPolicyService, null!));
    }

    [Fact]
    public async Task GetAsync_WithSuccessfulResponse_ShouldReturnDeserializedData()
    {
        // Arrange
        var expectedData = new TestDto { Id = 1, Name = "Test" };
        var jsonResponse = JsonSerializer.Serialize(expectedData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        _testHttpMessageHandler.SetResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _service.GetAsync<TestDto>("/test");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedData.Id);
        result.Name.Should().Be(expectedData.Name);
    }

    [Fact]
    public async Task GetAsync_WithHttpRequestException_ShouldThrowException()
    {
        // Arrange
        _testHttpMessageHandler.SetException(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetAsync<TestDto>("/test"));
    }

    [Fact]
    public async Task PostAsync_WithSuccessfulResponse_ShouldReturnDeserializedData()
    {
        // Arrange
        var requestData = new TestDto { Id = 1, Name = "Test" };
        var responseData = new TestDto { Id = 2, Name = "Response" };
        var jsonResponse = JsonSerializer.Serialize(responseData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        _testHttpMessageHandler.SetResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _service.PostAsync<TestDto, TestDto>("/test", requestData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(responseData.Id);
        result.Name.Should().Be(responseData.Name);
        
        _testHttpMessageHandler.LastRequest.Should().NotBeNull();
        _testHttpMessageHandler.LastRequest!.Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task PutAsync_WithSuccessfulResponse_ShouldReturnDeserializedData()
    {
        // Arrange
        var requestData = new TestDto { Id = 1, Name = "Test" };
        var responseData = new TestDto { Id = 1, Name = "Updated" };
        var jsonResponse = JsonSerializer.Serialize(responseData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        _testHttpMessageHandler.SetResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        var result = await _service.PutAsync<TestDto, TestDto>("/test", requestData);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(responseData.Id);
        result.Name.Should().Be(responseData.Name);
        
        _testHttpMessageHandler.LastRequest.Should().NotBeNull();
        _testHttpMessageHandler.LastRequest!.Method.Should().Be(HttpMethod.Put);
    }

    [Fact]
    public async Task DeleteAsync_WithSuccessfulResponse_ShouldReturnTrue()
    {
        // Arrange
        _testHttpMessageHandler.SetResponse(HttpStatusCode.NoContent, "");

        // Act
        var result = await _service.DeleteAsync("/test");

        // Assert
        result.Should().BeTrue();
        _testHttpMessageHandler.LastRequest.Should().NotBeNull();
        _testHttpMessageHandler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteAsync_WithUnsuccessfulResponse_ShouldReturnFalse()
    {
        // Arrange
        _testHttpMessageHandler.SetResponse(HttpStatusCode.NotFound, "");

        // Act
        var result = await _service.DeleteAsync("/test");

        // Assert
        result.Should().BeFalse();
        _testHttpMessageHandler.LastRequest.Should().NotBeNull();
        _testHttpMessageHandler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
    }

    [Fact]
    public async Task GetAsync_ShouldUseResiliencePipeline()
    {
        // Arrange
        var jsonResponse = JsonSerializer.Serialize(new TestDto { Id = 1, Name = "Test" });
        _testHttpMessageHandler.SetResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        await _service.GetAsync<TestDto>("/test");

        // Assert
        _mockPolicyService.Received(1).GetHttpPipeline();
    }

    [Fact]
    public async Task PostAsync_WithJsonContent_ShouldSetCorrectContentType()
    {
        // Arrange
        var requestData = new TestDto { Id = 1, Name = "Test" };
        var responseData = new TestDto { Id = 2, Name = "Response" };
        var jsonResponse = JsonSerializer.Serialize(responseData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        
        _testHttpMessageHandler.SetResponse(HttpStatusCode.OK, jsonResponse);

        // Act
        await _service.PostAsync<TestDto, TestDto>("/test", requestData);

        // Assert
        var lastRequest = _testHttpMessageHandler.LastRequest;
        lastRequest.Should().NotBeNull();
        lastRequest!.Content.Should().NotBeNull();
        lastRequest.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
        lastRequest.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

public class TestHttpMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public HttpResponseMessage? ResponseToReturn { get; private set; }
    public Exception? ExceptionToThrow { get; private set; }

    public void SetResponse(HttpStatusCode statusCode, string content)
    {
        ResponseToReturn = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
        ExceptionToThrow = null;
    }

    public void SetException(Exception exception)
    {
        ExceptionToThrow = exception;
        ResponseToReturn = null;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;

        if (ExceptionToThrow != null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(ResponseToReturn ?? new HttpResponseMessage(HttpStatusCode.OK));
    }
}