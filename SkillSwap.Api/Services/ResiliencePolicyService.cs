using Polly;
using System.Diagnostics.CodeAnalysis;
using SkillSwap.Api.Configuration;

namespace SkillSwap.Api.Services;

public interface IResiliencePolicyService
{
    ResiliencePipeline GetDatabasePipeline();
    ResiliencePipeline GetHttpPipeline();
    ResiliencePipeline GetHttpPipeline(string clientName);
    ResiliencePipeline GetExternalApiPipeline();
    ResiliencePipeline<T> GetTypedPipeline<T>();
}

/// <summary>
/// Service for managing resilience policies using Polly
/// </summary>
[ExcludeFromCodeCoverage]
public class ResiliencePolicyService : IResiliencePolicyService
{
    private readonly ResilienceSettings _settings;
    private readonly ILogger<ResiliencePolicyService> _logger;
    private readonly Dictionary<string, ResiliencePipeline> _pipelines;

    public ResiliencePolicyService(ResilienceSettings settings, ILogger<ResiliencePolicyService> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pipelines = new Dictionary<string, ResiliencePipeline>();
        
        InitializePipelines();
    }

    /// <summary>
    /// Gets the database resilience pipeline
    /// </summary>
    /// <returns>Database resilience pipeline</returns>
    public ResiliencePipeline GetDatabasePipeline() => GetPipeline("Database");

    /// <summary>
    /// Gets the HTTP resilience pipeline
    /// </summary>
    /// <returns>HTTP resilience pipeline</returns>
    public ResiliencePipeline GetHttpPipeline() => GetPipeline("Http");

    /// <summary>
    /// Gets a named HTTP client resilience pipeline
    /// </summary>
    /// <param name="clientName">The client name</param>
    /// <returns>HTTP client resilience pipeline</returns>
    public ResiliencePipeline GetHttpPipeline(string clientName) => GetPipeline($"Http_{clientName}");

    /// <summary>
    /// Gets the external API resilience pipeline
    /// </summary>
    /// <returns>External API resilience pipeline</returns>
    public ResiliencePipeline GetExternalApiPipeline() => GetPipeline("ExternalApi");

    /// <summary>
    /// Gets a typed resilience pipeline for specific operations
    /// </summary>
    /// <typeparam name="T">The result type</typeparam>
    /// <returns>Typed resilience pipeline</returns>
    public ResiliencePipeline<T> GetTypedPipeline<T>()
    {
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(_settings.Timeout.DefaultTimeout)
            .Build();
    }

    private ResiliencePipeline GetPipeline(string pipelineName)
    {
        if (_pipelines.TryGetValue(pipelineName, out var pipeline))
        {
            return pipeline;
        }
        
        _logger.LogWarning("Pipeline {PipelineName} not found, returning default pipeline", pipelineName);
        return GetDefaultPipeline();
    }

    private void InitializePipelines()
    {
        // Database pipeline
        var databasePipeline = new ResiliencePipelineBuilder()
            .AddTimeout(_settings.Timeout.DatabaseTimeout)
            .Build();
        
        _pipelines["Database"] = databasePipeline;

        // HTTP pipeline
        var httpPipeline = new ResiliencePipelineBuilder()
            .AddTimeout(_settings.Timeout.HttpTimeout)
            .Build();
            
        _pipelines["Http"] = httpPipeline;

        // External API pipeline (for Stripe, etc.)
        var externalApiPipeline = new ResiliencePipelineBuilder()
            .AddTimeout(_settings.Timeout.ExternalApiTimeout)
            .Build();
            
        _pipelines["ExternalApi"] = externalApiPipeline;

        // Create specific client pipelines
        foreach (var clientConfig in _settings.Http.Clients)
        {
            var clientPipeline = CreateHttpClientPipeline(clientConfig.Value);
            _pipelines[$"Http_{clientConfig.Key}"] = clientPipeline;
        }
    }

    private ResiliencePipeline CreateHttpClientPipeline(HttpClientSettings settings)
    {
        return new ResiliencePipelineBuilder()
            .AddTimeout(settings.Timeout)
            .Build();
    }

    private ResiliencePipeline GetDefaultPipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddTimeout(_settings.Timeout.DefaultTimeout)
            .Build();
    }
}