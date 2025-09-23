using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain;

namespace SkillSwap.Infrastructure;

public class SkillSwapDbContext : DbContext, IApplicationDbContext
{
    public SkillSwapDbContext(DbContextOptions<SkillSwapDbContext> options)
            : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);
}
