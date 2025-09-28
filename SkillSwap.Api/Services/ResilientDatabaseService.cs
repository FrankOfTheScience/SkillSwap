using Microsoft.EntityFrameworkCore;
using Polly;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Services;

public interface IResilientDatabaseService
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation);
    Task ExecuteAsync(Func<CancellationToken, Task> operation);
}

/// <summary>
/// Service that provides resilient database operations using Polly
/// </summary>
[ExcludeFromCodeCoverage]
public class ResilientDatabaseService : IResilientDatabaseService
{
    private readonly IResiliencePolicyService _policyService;
    private readonly ILogger<ResilientDatabaseService> _logger;

    public ResilientDatabaseService(
        IResiliencePolicyService policyService,
        ILogger<ResilientDatabaseService> logger)
    {
        _policyService = policyService ?? throw new ArgumentNullException(nameof(policyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a database operation with resilience policies
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">The database operation to execute</param>
    /// <returns>The result of the operation</returns>
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation)
    {
        var pipeline = _policyService.GetDatabasePipeline();
        
        return await pipeline.ExecuteAsync(async (cancellationToken) =>
        {
            return await operation(cancellationToken);
        });
    }

    /// <summary>
    /// Executes a database operation with resilience policies
    /// </summary>
    /// <param name="operation">The database operation to execute</param>
    public async Task ExecuteAsync(Func<CancellationToken, Task> operation)
    {
        var pipeline = _policyService.GetDatabasePipeline();
        
        await pipeline.ExecuteAsync(async (cancellationToken) =>
        {
            await operation(cancellationToken);
        });
    }
}

/// <summary>
/// Extension methods for resilient database operations
/// </summary>
[ExcludeFromCodeCoverage]
public static class ResilientDbContextExtensions
{
    /// <summary>
    /// Saves changes with resilience
    /// </summary>
    public static async Task<int> SaveChangesWithResilienceAsync(
        this DbContext context, 
        IResilientDatabaseService resilientService,
        CancellationToken cancellationToken = default)
    {
        return await resilientService.ExecuteAsync(async (ct) =>
            await context.SaveChangesAsync(ct));
    }

    /// <summary>
    /// Executes a query with resilience
    /// </summary>
    public static async Task<List<T>> ExecuteQueryWithResilienceAsync<T>(
        this IQueryable<T> query, 
        IResilientDatabaseService resilientService,
        CancellationToken cancellationToken = default)
    {
        return await resilientService.ExecuteAsync(async (ct) =>
            await query.ToListAsync(ct));
    }

    /// <summary>
    /// Finds an entity with resilience
    /// </summary>
    public static async Task<T?> FindWithResilienceAsync<T>(
        this DbSet<T> dbSet, 
        IResilientDatabaseService resilientService,
        object?[] keyValues) where T : class
    {
        return await resilientService.ExecuteAsync(async (ct) =>
            await dbSet.FindAsync(keyValues, ct));
    }
}