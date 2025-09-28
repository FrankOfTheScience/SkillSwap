using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Configuration;

[ExcludeFromCodeCoverage]
public class ResilienceSettings
{
    public const string SectionName = "Resilience";

    public RetrySettings Retry { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
    public TimeoutSettings Timeout { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
    public HttpSettings Http { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class RetrySettings
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(1000);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);
    public bool UseJitter { get; set; } = true;
}

[ExcludeFromCodeCoverage]
public class CircuitBreakerSettings
{
    public int HandledEventsAllowedBeforeBreaking { get; set; } = 5;
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);
    public int MinimumThroughput { get; set; } = 10;
    public double FailureThreshold { get; set; } = 0.5; // 50%
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(60);
}

[ExcludeFromCodeCoverage]
public class TimeoutSettings
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan DatabaseTimeout { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ExternalApiTimeout { get; set; } = TimeSpan.FromSeconds(45);
}

[ExcludeFromCodeCoverage]
public class DatabaseSettings
{
    public bool EnableRetry { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableCircuitBreaker { get; set; } = true;
}

[ExcludeFromCodeCoverage]
public class HttpSettings
{
    public bool EnableRetry { get; set; } = true;
    public bool EnableCircuitBreaker { get; set; } = true;
    public bool EnableTimeout { get; set; } = true;
    public Dictionary<string, HttpClientSettings> Clients { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class HttpClientSettings
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableCircuitBreaker { get; set; } = true;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
}