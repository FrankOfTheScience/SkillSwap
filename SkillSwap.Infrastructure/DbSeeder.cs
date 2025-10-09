using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(SkillSwapDbContext db)
    {
        // Clear existing data
        db.Bookings.RemoveRange(db.Bookings);
        db.OfferAvailabilities.RemoveRange(db.OfferAvailabilities);
        db.Offers.RemoveRange(db.Offers);
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();

        // Create Admin User
        using var sha = System.Security.Cryptography.SHA256.Create();
        var adminPasswordHash = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Admin123")));

        var adminId = Guid.NewGuid();
        var admin = new User
        {
            Id = adminId,
            Email = "admin@skillswap.com",
            DisplayName = "Admin User",
            PasswordHash = adminPasswordHash, // Properly hashed password
            FirstName = "Admin",
            LastName = "User",
            Roles = new List<string> { "Admin" },
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            Bio = "System administrator for SkillSwap platform",
            City = "San Francisco",
            Country = "USA",
            TimeZone = "America/Los_Angeles",
            EmailNotifications = true,
            PushNotifications = true,
            ProfileCompletionPercentage = 100
        };

        // Create Regular Users
        // Hash the password "John123" properly for the test user
        var testUserPasswordHash = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("John123")));

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "john.developer@skillswap.com", // Match test credentials
            DisplayName = "John Developer",
            PasswordHash = testUserPasswordHash, // Properly hashed password
            FirstName = "John",
            LastName = "Developer",
            Roles = new List<string> { "User" },
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            Bio = "Full-stack developer with 8+ years experience in React, Node.js, and cloud technologies.",
            City = "Seattle",
            Country = "USA",
            TimeZone = "America/Los_Angeles",
            EmailNotifications = true,
            PushNotifications = true,
            Profession = "Software Developer",
            Company = "Tech Solutions Inc",
            YearsOfExperience = 8,
            Skills = new List<string> { "React", "Node.js", "TypeScript", "AWS" },
            ProfileCompletionPercentage = 90
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "maria.designer@email.com",
            DisplayName = "Maria Designer",
            PasswordHash = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Maria123"))), // Properly hashed password
            FirstName = "Maria",
            LastName = "Designer",
            Roles = new List<string> { "User" },
            CreatedAt = DateTime.UtcNow.AddDays(-12),
            Bio = "UX/UI designer with expertise in user research and design systems.",
            City = "New York",
            Country = "USA",
            TimeZone = "America/New_York",
            EmailNotifications = true,
            PushNotifications = false,
            Profession = "UX Designer",
            Company = "Design Studio",
            YearsOfExperience = 6,
            Skills = new List<string> { "Figma", "User Research", "Prototyping", "Design Systems" },
            ProfileCompletionPercentage = 85
        };

        var user3 = new User
        {
            Id = Guid.NewGuid(),
            Email = "alex.consultant@email.com",
            DisplayName = "Alex Consultant",
            PasswordHash = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Alex123"))), // Properly hashed password
            FirstName = "Alex",
            LastName = "Consultant",
            Roles = new List<string> { "User" },
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Bio = "Business strategy consultant specializing in digital transformation.",
            City = "Austin",
            Country = "USA",
            TimeZone = "America/Chicago",
            EmailNotifications = true,
            PushNotifications = true,
            Profession = "Business Consultant",
            Company = "Strategy Partners",
            YearsOfExperience = 12,
            Skills = new List<string> { "Strategy", "Digital Transformation", "Agile", "Change Management" },
            ProfileCompletionPercentage = 95
        };

        // Add users to database
        db.Users.Add(admin);
        db.Users.Add(user1);
        db.Users.Add(user2);
        db.Users.Add(user3);
        await db.SaveChangesAsync();

        // Create Offers
        var offer1 = new Offer
        {
            Id = Guid.NewGuid(),
            Title = "React & TypeScript Masterclass",
            Description = "Comprehensive course covering modern React development with TypeScript. We'll build a complete project from scratch, covering hooks, context, state management, and testing. Perfect for developers who want to level up their frontend skills and learn industry best practices.",
            Price = 85.00m,
            CreatedBy = user1.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            DurationInMinutes = 120,
            IsOnline = true,
            Location = "Online via Zoom",
            Category = "Programming",
            Tags = new List<string> { "React", "TypeScript", "Frontend", "JavaScript", "Web Development" },
            IsActive = true
        };

        var offer2 = new Offer
        {
            Id = Guid.NewGuid(),
            Title = "UX Research & User Testing",
            Description = "Master the art of understanding your users through research methods, user interviews, usability testing, and data analysis. Perfect for designers and product managers.",
            Price = 95.00m,
            CreatedBy = user2.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-4),
            DurationInMinutes = 120,
            IsOnline = true,
            Location = "Online via Zoom",
            Category = "Design",
            Tags = new List<string> { "UX", "User Research", "Testing", "Product Design" },
            IsActive = true
        };

        var offer3 = new Offer
        {
            Id = Guid.NewGuid(),
            Title = "Digital Transformation Strategy",
            Description = "Strategic consultation on digital transformation initiatives. We'll analyze your current state, identify opportunities, and create a roadmap for digital success.",
            Price = 150.00m,
            CreatedBy = user3.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-6),
            DurationInMinutes = 120,
            IsOnline = true,
            Location = "Online consultation",
            Category = "Business",
            Tags = new List<string> { "Strategy", "Digital Transformation", "Consulting" },
            IsActive = true
        };

        // Add offers to database
        db.Offers.Add(offer1);
        db.Offers.Add(offer2);
        db.Offers.Add(offer3);
        await db.SaveChangesAsync();

        // Create Offer Availabilities
        var availability1 = new OfferAvailability
        {
            Id = Guid.NewGuid(),
            OfferId = offer1.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsAvailable = true
        };

        var availability2 = new OfferAvailability
        {
            Id = Guid.NewGuid(),
            OfferId = offer1.Id,
            DayOfWeek = DayOfWeek.Wednesday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsAvailable = true
        };

        var availability3 = new OfferAvailability
        {
            Id = Guid.NewGuid(),
            OfferId = offer2.Id,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(16, 0),
            IsAvailable = true
        };

        // Add availabilities to database
        db.OfferAvailabilities.Add(availability1);
        db.OfferAvailabilities.Add(availability2);
        db.OfferAvailabilities.Add(availability3);
        await db.SaveChangesAsync();

        // Create Sample Bookings
        var booking1 = new Booking
        {
            Id = Guid.NewGuid(),
            OfferId = offer1.Id,
            UserId = user2.Id,
            Amount = 85.00m,
            CommissionAmount = 8.50m,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ScheduledDateTime = DateTime.UtcNow.AddDays(3).AddHours(14),
            DurationInMinutes = 120,
            IsOnline = true,
            Location = "Online via Zoom",
            CustomerNotes = "Looking forward to learning advanced React patterns!",
            StripePaymentIntentId = "pi_test_123456789"
        };

        var booking2 = new Booking
        {
            Id = Guid.NewGuid(),
            OfferId = offer2.Id,
            UserId = user1.Id,
            Amount = 95.00m,
            CommissionAmount = 9.50m,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ScheduledDateTime = DateTime.UtcNow.AddDays(5).AddHours(10),
            DurationInMinutes = 120,
            IsOnline = true,
            Location = "Online via Zoom",
            CustomerNotes = "Want to improve my product's user experience",
            StripePaymentIntentId = "pi_test_987654321"
        };

        // Add bookings to database
        db.Bookings.Add(booking1);
        db.Bookings.Add(booking2);
        await db.SaveChangesAsync();

        Console.WriteLine("Database seeded successfully!");
        Console.WriteLine($"Created 1 Admin and 3 regular users");
        Console.WriteLine($"Created 3 offers across multiple categories");
        Console.WriteLine($"Created 3 availability slots");
        Console.WriteLine($"Created 2 sample bookings");
    }
}