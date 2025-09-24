using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain;
using System.Security.Cryptography;
using System.Text;

namespace SkillSwap.Infrastructure;
public static class DbSeeder
{
    public static async Task SeedAsync(SkillSwapDbContext db)
    {
        if (await db.Users.AnyAsync()) return; 

        using var sha = SHA256.Create();

        // ADMIN
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@skillswap.com",
            DisplayName = "AdminUser",
            PasswordHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes("Admin123"))),
            Roles = new List<string> { "Admin" }
        };

        // NORMAL USER
        var normalUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@skillswap.com",
            DisplayName = "NormalUser",
            PasswordHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes("User123"))),
            Roles = new List<string> { "User" }
        };

        // GUEST USER - NO ROLES    
        var guest = new User
        {
            Id = Guid.NewGuid(),
            Email = "guest@skillswap.com",
            DisplayName = "GuestUser",
            PasswordHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes("Guest123"))),
            Roles = new List<string>() 
        };

        db.Users.AddRange(admin, normalUser, guest);
        await db.SaveChangesAsync();
    }
}