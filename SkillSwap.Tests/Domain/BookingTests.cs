using FluentAssertions;
using SkillSwap.Domain;

namespace SkillSwap.Tests.Domain;

public class BookingTests
{
    [Fact]
    public void Booking_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var booking = new Booking();
        
        // Assert
        booking.Id.Should().Be(0);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)); // Increased tolerance
        booking.CompletedAt.Should().BeNull();
        booking.StripeCheckoutSessionId.Should().BeNull();
        booking.StripePaymentIntentId.Should().BeNull();
    }
    
    [Fact]
    public void Booking_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var offerId = 123;
        var userId = Guid.NewGuid();
        var status = BookingStatus.Completed;
        var amount = 150.75m;
        var commissionAmount = 15.08m;
        var stripeSessionId = "cs_12345";
        var stripePaymentIntentId = "pi_12345";
        var completedAt = DateTime.UtcNow;
        
        // Act
        var booking = new Booking
        {
            OfferId = offerId,
            UserId = userId,
            Status = status,
            Amount = amount,
            CommissionAmount = commissionAmount,
            StripeCheckoutSessionId = stripeSessionId,
            StripePaymentIntentId = stripePaymentIntentId,
            CompletedAt = completedAt
        };
        
        // Assert
        booking.OfferId.Should().Be(offerId);
        booking.UserId.Should().Be(userId);
        booking.Status.Should().Be(status);
        booking.Amount.Should().Be(amount);
        booking.CommissionAmount.Should().Be(commissionAmount);
        booking.StripeCheckoutSessionId.Should().Be(stripeSessionId);
        booking.StripePaymentIntentId.Should().Be(stripePaymentIntentId);
        booking.CompletedAt.Should().Be(completedAt);
    }
    
    [Fact]
    public void Booking_NavigationProperties_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var booking = new Booking();
        
        // Assert
        booking.Offer.Should().BeNull();
        booking.User.Should().BeNull();
    }
    
    [Fact]
    public void Booking_NavigationProperties_ShouldBeSettable()
    {
        // Arrange
        var offer = new Offer
        {
            Id = 1,
            Title = "Test Offer",
            Description = "Test Description",
            Price = 100m,
            CreatedBy = Guid.NewGuid()
        };
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash123"
        };
        
        var booking = new Booking();
        
        // Act
        booking.Offer = offer;
        booking.User = user;
        
        // Assert
        booking.Offer.Should().Be(offer);
        booking.User.Should().Be(user);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(50.99)]
    [InlineData(999.99)]
    [InlineData(1234.567)]
    public void Booking_Amount_ShouldAcceptValidDecimalValues(decimal amount)
    {
        // Arrange & Act
        var booking = new Booking { Amount = amount };
        
        // Assert
        booking.Amount.Should().Be(amount);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(5.99)]
    [InlineData(99.99)]
    [InlineData(123.456)]
    public void Booking_CommissionAmount_ShouldAcceptValidDecimalValues(decimal commissionAmount)
    {
        // Arrange & Act
        var booking = new Booking { CommissionAmount = commissionAmount };
        
        // Assert
        booking.CommissionAmount.Should().Be(commissionAmount);
    }
    
    [Fact]
    public void Booking_CreatedAt_ShouldBeUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var booking = new Booking();
        var afterCreation = DateTime.UtcNow;
        
        // Assert
        booking.CreatedAt.Should().BeAfter(beforeCreation.AddMilliseconds(-1));
        booking.CreatedAt.Should().BeBefore(afterCreation.AddMilliseconds(1));
        booking.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }
    
    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Refunded)]
    public void Booking_Status_ShouldAcceptDifferentStatuses(BookingStatus status)
    {
        // Arrange & Act
        var booking = new Booking { Status = status };
        
        // Assert
        booking.Status.Should().Be(status);
    }
    
    [Fact]
    public void Booking_WithFullData_ShouldRetainAllProperties()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 42,
            OfferId = 100,
            UserId = Guid.NewGuid(),
            Status = BookingStatus.Completed,
            Amount = 250.00m,
            CommissionAmount = 25.00m,
            StripeCheckoutSessionId = "cs_test_session",
            StripePaymentIntentId = "pi_test_intent",
            CompletedAt = DateTime.UtcNow.AddDays(-1)
        };
        
        // Act & Assert - Verify all properties are maintained
        booking.Id.Should().Be(42);
        booking.OfferId.Should().Be(100);
        booking.UserId.Should().NotBe(Guid.Empty);
        booking.Status.Should().Be(BookingStatus.Completed);
        booking.Amount.Should().Be(250.00m);
        booking.CommissionAmount.Should().Be(25.00m);
        booking.StripeCheckoutSessionId.Should().Be("cs_test_session");
        booking.StripePaymentIntentId.Should().Be("pi_test_intent");
        booking.CompletedAt.Should().HaveValue();
        booking.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}