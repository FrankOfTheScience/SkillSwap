using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain;

namespace SkillSwap.Application.Common.Interfaces;
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Offer> Offers { get; }
    DbSet<Booking> Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
