using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Infrastructure;

[ExcludeFromCodeCoverage]
public class SkillSwapDbContextFactory : IDesignTimeDbContextFactory<SkillSwapDbContext>
{
    public SkillSwapDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SKILLSWAP_DB_CONNECTION")
                             ?? "Host=localhost;Port=5432;Database=skillswap;Username=postgres;Password=i4M'3!6XexL[";

        return new SkillSwapDbContext(DbContextOptionsFactory.Create(connectionString));
    }
}
