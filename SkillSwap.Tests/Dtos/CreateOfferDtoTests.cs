using FluentAssertions;
using SkillSwap.Api.Dtos;

namespace SkillSwap.Tests.Dtos;

public class CreateOfferDtoTests
{
    [Fact]
    public void CreateOfferDto_ShouldInitializeProperties_WhenCreated()
    {
        // Arrange
        var title = "Test Offer";
        var description = "Test Description";
        var price = 100.50m;

        // Act
        var dto = new CreateOfferDto
        {
            Title = title,
            Description = description,
            Price = price
        };

        // Assert
        dto.Title.Should().Be(title);
        dto.Description.Should().Be(description);
        dto.Price.Should().Be(price);
    }

    [Fact]
    public void CreateOfferDto_ShouldAllowPropertyChanges()
    {
        // Arrange
        var dto = new CreateOfferDto
        {
            Title = "Original",
            Description = "Original Description",
            Price = 50.0m
        };

        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newPrice = 75.25m;

        // Act
        dto.Title = newTitle;
        dto.Description = newDescription;
        dto.Price = newPrice;

        // Assert
        dto.Title.Should().Be(newTitle);
        dto.Description.Should().Be(newDescription);
        dto.Price.Should().Be(newPrice);
    }

    [Theory]
    [InlineData("", "Description", 100.0)]
    [InlineData("Title", "", 100.0)]
    [InlineData("Title", "Description", 0.0)]
    [InlineData("Title", "Description", -10.0)]
    public void CreateOfferDto_ShouldAllowEdgeCaseValues(string title, string description, decimal price)
    {
        // Act & Assert (should not throw)
        var dto = new CreateOfferDto
        {
            Title = title,
            Description = description,
            Price = price
        };

        dto.Title.Should().Be(title);
        dto.Description.Should().Be(description);
        dto.Price.Should().Be(price);
    }
}