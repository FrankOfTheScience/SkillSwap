using Microsoft.EntityFrameworkCore.Design;

namespace SkillSwap.Infrastructure;
public class SkillSwapDbContextFactory : IDesignTimeDbContextFactory<SkillSwapDbContext>
{
    public SkillSwapDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SKILLSWAP_DB_CONNECTION")
                             ?? "Host=localhost;Port=5432;Database=skillswap;Username=postgres;Password=";

        return new SkillSwapDbContext(DbContextOptionsFactory.Create(connectionString));
    }
}
