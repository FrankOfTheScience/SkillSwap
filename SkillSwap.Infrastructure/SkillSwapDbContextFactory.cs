using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Infrastructure;

[ExcludeFromCodeCoverage]
public class SkillSwapDbContextFactory : IDesignTimeDbContextFactory<SkillSwapDbContext>
{
    public SkillSwapDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SKILLSWAP_DB_CONNECTION")
                             ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                             ?? throw new InvalidOperationException("Database connection string not found. Set SKILLSWAP_DB_CONNECTION environment variable.");

        return new SkillSwapDbContext(DbContextOptionsFactory.Create(connectionString));
    }
}
