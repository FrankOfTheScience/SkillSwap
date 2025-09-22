using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain;

namespace SkillSwap.Infrastructure;

public class SkillSwapDbContext : DbContext
{
    public SkillSwapDbContext(DbContextOptions<SkillSwapDbContext> options)
            : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
}
