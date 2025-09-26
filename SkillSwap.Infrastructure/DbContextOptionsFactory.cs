using Microsoft.EntityFrameworkCore;

namespace SkillSwap.Infrastructure;

public static class DbContextOptionsFactory
{
    public static DbContextOptions<SkillSwapDbContext> Create(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<SkillSwapDbContext>();
        builder.UseNpgsql(connectionString);
        return builder.Options;
    }
}
