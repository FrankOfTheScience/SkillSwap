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

    public DbSet<User> Users => Set<User>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<OfferAvailability> OfferAvailabilities => Set<OfferAvailability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Skills)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                    
            entity.Property(e => e.Roles)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });
        
        // Configure Offer entity
        modelBuilder.Entity<Offer>(entity =>
        {
            entity.Property(e => e.Id)
                .UseIdentityByDefaultColumn()
                .HasAnnotation("Npgsql:ValueGenerationStrategy", 
                    Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
                    
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
                
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                    
            // Configure relationship with User (Creator)
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configure relationship with OfferAvailability
            entity.HasMany(e => e.Availabilities)
                .WithOne(a => a.Offer)
                .HasForeignKey(a => a.OfferId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure relationship with Bookings
            entity.HasMany(e => e.Bookings)
                .WithOne(b => b.Offer)
                .HasForeignKey(b => b.OfferId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configure OfferAvailability entity
        modelBuilder.Entity<OfferAvailability>(entity =>
        {
            entity.Property(e => e.Id)
                .UseIdentityByDefaultColumn()
                .HasAnnotation("Npgsql:ValueGenerationStrategy", 
                    Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });
        
        // Configure Booking entity
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");
                
            entity.Property(e => e.CommissionAmount)
                .HasColumnType("decimal(18,2)");
                
            // Configure relationship with User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => base.SaveChangesAsync(cancellationToken);
}
