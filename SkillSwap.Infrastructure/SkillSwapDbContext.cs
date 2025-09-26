using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Infrastructure;

[ExcludeFromCodeCoverage]
public class SkillSwapDbContext : DbContext, IApplicationDbContext
{
    public SkillSwapDbContext(DbContextOptions<SkillSwapDbContext> options)
            : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Offer entity
        modelBuilder.Entity<Offer>(entity =>
        {
            entity.Property(e => e.Id)
                .UseIdentityByDefaultColumn()
                .HasAnnotation("Npgsql:ValueGenerationStrategy", 
                    Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
                    
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);
}
