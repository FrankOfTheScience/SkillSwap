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

        // ADD SAMPLE OFFERS
        var offers = new List<Offer>
        {
            new Offer
            {
                Id = 1,
                Title = "Web Development Tutoring",
                Description = "Learn modern web development with React, TypeScript, and Next.js. Perfect for beginners and intermediate developers looking to improve their skills.",
                Price = 45.00m,
                CreatedBy = normalUser.Id
            },
            new Offer
            {
                Id = 2,
                Title = "Guitar Lessons for Beginners",
                Description = "Learn to play guitar from scratch! I'll teach you chords, strumming patterns, and your first songs. All levels welcome.",
                Price = 30.00m,
                CreatedBy = admin.Id
            },
            new Offer
            {
                Id = 3,
                Title = "Python Programming Course",
                Description = "Master Python programming from basics to advanced topics. Includes data structures, algorithms, and practical projects.",
                Price = 55.00m,
                CreatedBy = normalUser.Id
            },
            new Offer
            {
                Id = 4,
                Title = "Photography Workshop",
                Description = "Learn professional photography techniques, composition, and photo editing. Bring your camera or use mine!",
                Price = 75.00m,
                CreatedBy = guest.Id
            },
            new Offer
            {
                Id = 5,
                Title = "Spanish Language Lessons",
                Description = "Native Spanish speaker offering conversational classes for all levels. Learn grammar, vocabulary, and cultural insights.",
                Price = 25.00m,
                CreatedBy = admin.Id
            }
        };

        db.Offers.AddRange(offers);
        await db.SaveChangesAsync();

        // Reset the auto-increment sequences to avoid conflicts
        await db.Database.ExecuteSqlRawAsync("SELECT setval('\"Offers_Id_seq\"', COALESCE((SELECT MAX(\"Id\") FROM \"Offers\"), 1), true);");
    }
}