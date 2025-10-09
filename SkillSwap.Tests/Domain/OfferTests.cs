using FluentAssertions;
using SkillSwap.Domain;

namespace SkillSwap.Tests.Domain;

public class OfferTests
{
    [Fact]
    public void Offer_ShouldInitializeWithDefaults_WhenCreated()
    {
        // Act
        var offer = new Offer
        {
            Title = "Default Title",
            Description = "Default Description"
        };

        // Assert
        offer.Id.Should().Be(Guid.Empty); // Default int value
        offer.Title.Should().Be("Default Title");
        offer.Description.Should().Be("Default Description");
        offer.Price.Should().Be(0m); // Default decimal value
        offer.CreatedBy.Should().Be(Guid.Empty); // Default Guid value
    }

    [Fact]
    public void Offer_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Offer";
        var description = "Test Description";
        var price = 199.99m;
        var createdBy = Guid.NewGuid();

        // Act
        var offer = new Offer
        {
            Id = id,
            Title = title,
            Description = description,
            Price = price,
            CreatedBy = createdBy
        };

        // Assert
        offer.Id.Should().Be(id);
        offer.Title.Should().Be(title);
        offer.Description.Should().Be(description);
        offer.Price.Should().Be(price);
        offer.CreatedBy.Should().Be(createdBy);
    }

    [Theory]
    [InlineData("Short Title", "Short description", 10.0)]
    [InlineData("Very Long Title That Could Be Used For Comprehensive Testing Of Maximum Length Scenarios", "Very detailed and comprehensive description that provides extensive information about the offer", 999999.99)]
    [InlineData("Programming Tutoring", "Learn programming fundamentals", 50.0)]
    public void Offer_ShouldHandleVariousContentSizes(string title, string description, decimal price)
    {
        // Act
        var offer = new Offer
        {
            Title = title,
            Description = description,
            Price = price
        };

        // Assert
        offer.Title.Should().Be(title);
        offer.Description.Should().Be(description);
        offer.Price.Should().Be(price);
    }

    [Fact]
    public void Offer_ShouldAllowPriceModification()
    {
        // Arrange
        var offer = new Offer
        {
            Title = "Price Test",
            Description = "Testing price changes",
            Price = 100.0m
        };

        // Act
        offer.Price = 150.50m;

        // Assert
        offer.Price.Should().Be(150.50m);
    }

    [Fact]
    public void Offer_ShouldAllowTitleModification()
    {
        // Arrange
        var offer = new Offer
        {
            Title = "Original Title",
            Description = "Description"
        };

        // Act
        offer.Title = "Modified Title";

        // Assert
        offer.Title.Should().Be("Modified Title");
    }

    [Fact]
    public void Offer_ShouldAllowDescriptionModification()
    {
        // Arrange
        var offer = new Offer
        {
            Title = "Title",
            Description = "Original Description"
        };

        // Act
        offer.Description = "Modified Description";

        // Assert
        offer.Description.Should().Be("Modified Description");
    }

    [Fact]
    public void Offer_ShouldAllowCreatedByModification()
    {
        // Arrange
        var originalCreator = Guid.NewGuid();
        var newCreator = Guid.NewGuid();

        var offer = new Offer
        {
            Title = "Test",
            Description = "Test",
            CreatedBy = originalCreator
        };

        // Act
        offer.CreatedBy = newCreator;

        // Assert
        offer.CreatedBy.Should().Be(newCreator);
        offer.CreatedBy.Should().NotBe(originalCreator);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.01)]
    [InlineData(1000000.00)]
    [InlineData(-10.50)] // Negative prices might be valid in some business scenarios
    public void Offer_ShouldAcceptVariousPriceValues(decimal price)
    {
        // Act
        var offer = new Offer
        {
            Title = "Price Variation Test",
            Description = "Testing various price values",
            Price = price
        };

        // Assert
        offer.Price.Should().Be(price);
    }

    [Fact]
    public void Offer_ShouldMaintainIdImmutabilityAfterSet()
    {
        // Arrange
        var offer = new Offer
        {
            Title = "ID Test",
            Description = "Testing ID persistence",
            Id = Guid.NewGuid()
        };

        var originalId = offer.Id;

        // Act
        offer.Title = "Modified Title"; // Modify other properties
        offer.Price = 100m;

        // Assert
        offer.Id.Should().Be(originalId); // ID should remain unchanged
    }
}