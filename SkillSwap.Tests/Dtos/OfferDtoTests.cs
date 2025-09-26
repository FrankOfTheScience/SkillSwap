using FluentAssertions;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Tests.Dtos;

public class OfferDtoTests
{
    [Fact]
    public void OfferDto_ShouldInitializeWithDefaults_WhenCreated()
    {
        // Act
        var dto = new OfferDto
        {
            Title = "Required Title"
        };

        // Assert
        dto.Id.Should().Be(0);
        dto.Title.Should().Be("Required Title");
        dto.Description.Should().BeNull();
        dto.Price.Should().Be(0m);
        dto.CreatedBy.Should().Be(Guid.Empty);
    }

    [Fact]
    public void OfferDto_ShouldAllowFullPropertyAssignment()
    {
        // Arrange
        var id = 123;
        var title = "Test Offer";
        var description = "Test Description";
        var price = 199.99m;
        var createdBy = Guid.NewGuid();

        // Act
        var dto = new OfferDto
        {
            Id = id,
            Title = title,
            Description = description,
            Price = price,
            CreatedBy = createdBy
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.Title.Should().Be(title);
        dto.Description.Should().Be(description);
        dto.Price.Should().Be(price);
        dto.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void OfferDto_ShouldAllowNullDescription()
    {
        // Act
        var dto = new OfferDto
        {
            Title = "Title Only",
            Description = null
        };

        // Assert
        dto.Title.Should().Be("Title Only");
        dto.Description.Should().BeNull();
    }

    [Fact]
    public void OfferDto_ShouldAllowPropertyModification()
    {
        // Arrange
        var dto = new OfferDto
        {
            Title = "Original Title",
            Description = "Original Description",
            Price = 100m
        };

        // Act
        dto.Title = "Modified Title";
        dto.Description = "Modified Description";
        dto.Price = 200m;

        // Assert
        dto.Title.Should().Be("Modified Title");
        dto.Description.Should().Be("Modified Description");
        dto.Price.Should().Be(200m);
    }

    [Theory]
    [InlineData(1, "Web Development", "Build responsive websites", 500.0)]
    [InlineData(999, "Music Lessons", "Learn guitar basics", 75.50)]
    [InlineData(42, "Math Tutoring", null, 30.0)]
    public void OfferDto_ShouldHandleVariousDataCombinations(int id, string title, string? description, decimal price)
    {
        // Arrange
        var createdBy = Guid.NewGuid();

        // Act
        var dto = new OfferDto
        {
            Id = id,
            Title = title,
            Description = description,
            Price = price,
            CreatedBy = createdBy
        };

        // Assert
        dto.Id.Should().Be(id);
        dto.Title.Should().Be(title);
        dto.Description.Should().Be(description);
        dto.Price.Should().Be(price);
        dto.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void OfferDto_ShouldHandleEmptyStringDescription()
    {
        // Act
        var dto = new OfferDto
        {
            Title = "Test Title",
            Description = ""
        };

        // Assert
        dto.Description.Should().Be("");
        dto.Description.Should().NotBeNull();
    }

    [Fact]
    public void OfferDto_ShouldHandleWhitespaceDescription()
    {
        // Act
        var dto = new OfferDto
        {
            Title = "Test Title",
            Description = "   "
        };

        // Assert
        dto.Description.Should().Be("   ");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.01)]
    [InlineData(999999.99)]
    [InlineData(-100.0)]
    public void OfferDto_ShouldAcceptVariousPriceValues(decimal price)
    {
        // Act
        var dto = new OfferDto
        {
            Title = "Price Test",
            Price = price
        };

        // Assert
        dto.Price.Should().Be(price);
    }

    [Fact]
    public void OfferDto_ShouldAllowCreatedByModification()
    {
        // Arrange
        var originalCreator = Guid.NewGuid();
        var newCreator = Guid.NewGuid();

        var dto = new OfferDto
        {
            Title = "Creator Test",
            CreatedBy = originalCreator
        };

        // Act
        dto.CreatedBy = newCreator;

        // Assert
        dto.CreatedBy.Should().Be(newCreator);
        dto.CreatedBy.Should().NotBe(originalCreator);
    }
}