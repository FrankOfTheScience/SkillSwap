using Polly;
using System.Text.Json;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Services;

public interface IResilientHttpClientService
{
    Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP client service with resilience patterns
/// </summary>
[ExcludeFromCodeCoverage]
public class ResilientHttpClientService : IResilientHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly IResiliencePolicyService _policyService;
    private readonly ILogger<ResilientHttpClientService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ResilientHttpClientService(
        HttpClient httpClient,
        IResiliencePolicyService policyService,
        ILogger<ResilientHttpClientService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _policyService = policyService ?? throw new ArgumentNullException(nameof(policyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Performs a resilient HTTP GET request
    /// </summary>
    public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        var pipeline = _policyService.GetHttpPipeline();
        
        return await pipeline.ExecuteAsync(async (ct) =>
        {
            var response = await _httpClient.GetAsync(requestUri, ct);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }, cancellationToken);
    }

    /// <summary>
    /// Performs a resilient HTTP POST request
    /// </summary>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        var pipeline = _policyService.GetHttpPipeline();
        
        return await pipeline.ExecuteAsync(async (ct) =>
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(requestUri, content, ct);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
        }, cancellationToken);
    }

    /// <summary>
    /// Performs a resilient HTTP PUT request
    /// </summary>
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string requestUri, TRequest data, CancellationToken cancellationToken = default)
    {
        var pipeline = _policyService.GetHttpPipeline();
        
        return await pipeline.ExecuteAsync(async (ct) =>
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(requestUri, content, ct);
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
        }, cancellationToken);
    }

    /// <summary>
    /// Performs a resilient HTTP DELETE request
    /// </summary>
    public async Task<bool> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        var pipeline = _policyService.GetHttpPipeline();
        
        return await pipeline.ExecuteAsync(async (ct) =>
        {
            var response = await _httpClient.DeleteAsync(requestUri, ct);
            return response.IsSuccessStatusCode;
        }, cancellationToken);
    }
}