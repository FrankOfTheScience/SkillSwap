using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain;
using SkillSwap.Infrastructure;
using System.Security.Cryptography;

namespace SkillSwap.Tests.Infrastructure;

public class DbSeederTests
{
    private DbContextOptions<SkillSwapDbContext> CreateInMemoryDbContextOptions(string databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();
        return new DbContextOptionsBuilder<SkillSwapDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    [Fact]
    public async Task SeedAsync_WithEmptyDatabase_ShouldAddUsersAndOffers()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var users = await context.Users.ToListAsync();
        var offers = await context.Offers.ToListAsync();

        users.Should().HaveCount(3);
        offers.Should().HaveCount(5);
    }

    [Fact]
    public async Task SeedAsync_WithExistingUsers_ShouldNotAddDuplicates()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Add an existing user
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            DisplayName = "Existing User",
            PasswordHash = "hash",
            Roles = new List<string> { "User" }
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var users = await context.Users.ToListAsync();
        users.Should().HaveCount(1); // Only the existing user, no seed data added
        users.First().Email.Should().Be("existing@test.com");
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateAdminUser()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@skillswap.com");
        adminUser.Should().NotBeNull();
        adminUser!.DisplayName.Should().Be("AdminUser");
        adminUser.Roles.Should().Contain("Admin");
        
        // Verify password hash is generated
        adminUser.PasswordHash.Should().NotBeNullOrEmpty();
        
        // Verify the hash matches expected value for "Admin123"
        using var sha = SHA256.Create();
        var expectedHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes("Admin123")));
        adminUser.PasswordHash.Should().Be(expectedHash);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateNormalUser()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var normalUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "user@skillswap.com");
        normalUser.Should().NotBeNull();
        normalUser!.DisplayName.Should().Be("NormalUser");
        normalUser.Roles.Should().Contain("User");
        
        // Verify password hash is generated for "User123"
        using var sha = SHA256.Create();
        var expectedHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes("User123")));
        normalUser.PasswordHash.Should().Be(expectedHash);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateGuestUser()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var guestUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "guest@skillswap.com");
        guestUser.Should().NotBeNull();
        guestUser!.DisplayName.Should().Be("GuestUser");
        guestUser.Roles.Should().BeEmpty();
        
        // Verify password hash is generated for "Guest123"
        using var sha = SHA256.Create();
        var expectedHash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes("Guest123")));
        guestUser.PasswordHash.Should().Be(expectedHash);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateSampleOffers()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var offers = await context.Offers.ToListAsync();
        offers.Should().HaveCount(5);

        // Verify specific offers
        var webDevOffer = offers.FirstOrDefault(o => o.Title == "Web Development Tutoring");
        webDevOffer.Should().NotBeNull();
        webDevOffer!.Price.Should().Be(45.00m);
        webDevOffer.Description.Should().Contain("React, TypeScript, and Next.js");

        var guitarOffer = offers.FirstOrDefault(o => o.Title == "Guitar Lessons for Beginners");
        guitarOffer.Should().NotBeNull();
        guitarOffer!.Price.Should().Be(30.00m);

        var pythonOffer = offers.FirstOrDefault(o => o.Title == "Python Programming Course");
        pythonOffer.Should().NotBeNull();
        pythonOffer!.Price.Should().Be(55.00m);

        var photographyOffer = offers.FirstOrDefault(o => o.Title == "Photography Workshop");
        photographyOffer.Should().NotBeNull();
        photographyOffer!.Price.Should().Be(75.00m);

        var spanishOffer = offers.FirstOrDefault(o => o.Title == "Spanish Language Lessons");
        spanishOffer.Should().NotBeNull();
        spanishOffer!.Price.Should().Be(25.00m);
    }

    [Fact]
    public async Task SeedAsync_OffersAssignedToCorrectUsers()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var users = await context.Users.ToListAsync();
        var offers = await context.Offers.ToListAsync();

        var normalUser = users.First(u => u.Email == "user@skillswap.com");
        var adminUser = users.First(u => u.Email == "admin@skillswap.com");
        var guestUser = users.First(u => u.Email == "guest@skillswap.com");

        // Normal user should have 2 offers (Web Development and Python)
        var normalUserOffers = offers.Where(o => o.CreatedBy == normalUser.Id).ToList();
        normalUserOffers.Should().HaveCount(2);

        // Admin user should have 2 offers (Guitar and Spanish)
        var adminUserOffers = offers.Where(o => o.CreatedBy == adminUser.Id).ToList();
        adminUserOffers.Should().HaveCount(2);

        // Guest user should have 1 offer (Photography)
        var guestUserOffers = offers.Where(o => o.CreatedBy == guestUser.Id).ToList();
        guestUserOffers.Should().HaveCount(1);
    }

    [Fact]
    public async Task SeedAsync_ShouldGenerateUniqueUserIds()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var users = await context.Users.ToListAsync();
        var userIds = users.Select(u => u.Id).ToList();
        
        userIds.Should().OnlyHaveUniqueItems();
        userIds.Should().AllSatisfy(id => id.Should().NotBeEmpty());
    }

    [Fact]
    public async Task SeedAsync_WithInMemoryDatabase_ShouldSkipSequenceReset()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act & Assert - Should not throw an exception for sequence reset in InMemory database
        var seedAction = async () => await DbSeeder.SeedAsync(context);
        await seedAction.Should().NotThrowAsync();
        
        // Verify that data was seeded correctly
        var offers = await context.Offers.ToListAsync();
        offers.Should().HaveCount(5);
    }

    [Fact]
    public async Task SeedAsync_MultipleCallsOnSameDatabase_ShouldOnlySeedOnce()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var options = CreateInMemoryDbContextOptions(databaseName);

        // Act - First call
        using (var context = new SkillSwapDbContext(options))
        {
            await DbSeeder.SeedAsync(context);
        }

        // Act - Second call
        using (var context = new SkillSwapDbContext(options))
        {
            await DbSeeder.SeedAsync(context);
        }

        // Assert
        using (var context = new SkillSwapDbContext(options))
        {
            var users = await context.Users.ToListAsync();
            var offers = await context.Offers.ToListAsync();

            users.Should().HaveCount(3); // Still only 3 users
            offers.Should().HaveCount(5); // Still only 5 offers
        }
    }

    [Fact]
    public async Task SeedAsync_ShouldSetOfferIdsCorrectly()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        await DbSeeder.SeedAsync(context);

        // Assert
        var offers = await context.Offers.OrderBy(o => o.Id).ToListAsync();
        
        offers[0].Id.Should().Be(1);
        offers[1].Id.Should().Be(2);
        offers[2].Id.Should().Be(3);
        offers[3].Id.Should().Be(4);
        offers[4].Id.Should().Be(5);
    }
}
