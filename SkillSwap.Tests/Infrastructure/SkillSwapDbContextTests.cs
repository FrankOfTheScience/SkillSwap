using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain;
using SkillSwap.Infrastructure;

namespace SkillSwap.Tests.Infrastructure;

public class SkillSwapDbContextTests
{
    private DbContextOptions<SkillSwapDbContext> CreateInMemoryDbContextOptions(string? databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();
        return new DbContextOptionsBuilder<SkillSwapDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    [Fact]
    public void Constructor_WithValidOptions_ShouldCreateContext()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();

        // Act
        using var context = new SkillSwapDbContext(options);

        // Assert
        context.Should().NotBeNull();
        context.Should().BeAssignableTo<DbContext>();
        context.Should().BeAssignableTo<SkillSwap.Application.Common.Interfaces.IApplicationDbContext>();
    }

    [Fact]
    public void Users_Property_ShouldReturnDbSet()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        var usersDbSet = context.Users;

        // Assert
        usersDbSet.Should().NotBeNull();
        usersDbSet.Should().BeAssignableTo<DbSet<User>>();
    }

    [Fact]
    public void Offers_Property_ShouldReturnDbSet()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        var offersDbSet = context.Offers;

        // Assert
        offersDbSet.Should().NotBeNull();
        offersDbSet.Should().BeAssignableTo<DbSet<Offer>>();
    }

    [Fact]
    public void Bookings_Property_ShouldReturnDbSet()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        var bookingsDbSet = context.Bookings;

        // Assert
        bookingsDbSet.Should().NotBeNull();
        bookingsDbSet.Should().BeAssignableTo<DbSet<Booking>>();
    }

    [Fact]
    public async Task SaveChangesAsync_WithDefaultCancellationToken_ShouldSaveChanges()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash",
            Roles = new List<string> { "User" }
        };

        context.Users.Add(user);

        // Act
        var result = await context.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        savedUser.Should().NotBeNull();
        savedUser!.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ShouldSaveChanges()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);
        using var cts = new CancellationTokenSource();

        var offer = new Offer
        {
            Title = "Test Offer",
            Description = "Test Description",
            Price = 50.00m,
            CreatedBy = Guid.NewGuid()
        };

        context.Offers.Add(offer);

        // Act
        var result = await context.SaveChangesAsync(cts.Token);

        // Assert
        result.Should().Be(1);
        var savedOffer = await context.Offers.FirstOrDefaultAsync(o => o.Title == "Test Offer");
        savedOffer.Should().NotBeNull();
        savedOffer!.Price.Should().Be(50.00m);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCancelled_ShouldThrowOperationCancelledException()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash",
            Roles = new List<string> { "User" }
        };

        context.Users.Add(user);

        // Act & Assert
        await context.Invoking(c => c.SaveChangesAsync(cts.Token))
            .Should().ThrowAsync<System.OperationCanceledException>();
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureOfferEntity()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        using var context = new SkillSwapDbContext(options);

        // Act
        var offerEntityType = context.Model.FindEntityType(typeof(Offer));

        // Assert
        offerEntityType.Should().NotBeNull();

        // Check if Price property is configured (for non-InMemory databases this would have decimal precision)
        var priceProperty = offerEntityType!.FindProperty(nameof(Offer.Price));
        priceProperty.Should().NotBeNull();
    }

    [Fact]
    public async Task CanAddAndRetrieveUser()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "integration@test.com",
            DisplayName = "Integration Test User",
            PasswordHash = "testhash",
            Roles = new List<string> { "User", "TestRole" }
        };

        // Act & Assert
        using (var context = new SkillSwapDbContext(options))
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        using (var context = new SkillSwapDbContext(options))
        {
            var retrievedUser = await context.Users.FindAsync(userId);
            retrievedUser.Should().NotBeNull();
            retrievedUser!.Email.Should().Be("integration@test.com");
            retrievedUser.Roles.Should().BeEquivalentTo(new[] { "User", "TestRole" });
        }
    }

    [Fact]
    public async Task CanAddAndRetrieveOffer()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        var offerId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var offer = new Offer
        {
            Id = offerId,
            Title = "Integration Test Offer",
            Description = "This is a test offer for integration testing",
            Price = 75.50m,
            CreatedBy = createdBy
        };

        // Act & Assert
        using (var context = new SkillSwapDbContext(options))
        {
            context.Offers.Add(offer);
            await context.SaveChangesAsync();
        }

        using (var context = new SkillSwapDbContext(options))
        {
            var retrievedOffer = await context.Offers.FindAsync(offerId);
            retrievedOffer.Should().NotBeNull();
            retrievedOffer!.Title.Should().Be("Integration Test Offer");
            retrievedOffer.Price.Should().Be(75.50m);
            retrievedOffer.CreatedBy.Should().Be(createdBy);
        }
    }

    [Fact]
    public async Task CanAddAndRetrieveBooking()
    {
        // Arrange
        var options = CreateInMemoryDbContextOptions();
        var bookingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId,
            UserId = userId,
            OfferId = Guid.NewGuid(),
            Status = BookingStatus.Completed
        };

        // Act & Assert
        using (var context = new SkillSwapDbContext(options))
        {
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
        }

        using (var context = new SkillSwapDbContext(options))
        {
            var retrievedBooking = await context.Bookings.FindAsync(bookingId);
            retrievedBooking.Should().NotBeNull();
            retrievedBooking!.UserId.Should().Be(userId);
            retrievedBooking.Status.Should().Be(BookingStatus.Completed);
        }
    }
}