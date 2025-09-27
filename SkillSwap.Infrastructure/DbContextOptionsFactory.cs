using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DbContextOptionsFactory
{
    public static DbContextOptions<SkillSwapDbContext> Create(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<SkillSwapDbContext>();
        builder.UseNpgsql(connectionString);
        return builder.Options;
    }
}
