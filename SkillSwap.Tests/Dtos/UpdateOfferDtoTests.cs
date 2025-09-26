using FluentAssertions;
using SkillSwap.Api.Dtos;

namespace SkillSwap.Tests.Dtos;

public class UpdateOfferDtoTests
{
    [Fact]
    public void UpdateOfferDto_ShouldInitializeProperties_WhenCreated()
    {
        // Arrange
        var title = "Updated Offer";
        var description = "Updated Description";
        var price = 200.75m;

        // Act
        var dto = new UpdateOfferDto
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
    public void UpdateOfferDto_ShouldAllowPropertyModification()
    {
        // Arrange
        var dto = new UpdateOfferDto
        {
            Title = "Original Title",
            Description = "Original Description",
            Price = 150.0m
        };

        // Act
        dto.Title = "Modified Title";
        dto.Description = "Modified Description";
        dto.Price = 250.50m;

        // Assert
        dto.Title.Should().Be("Modified Title");
        dto.Description.Should().Be("Modified Description");
        dto.Price.Should().Be(250.50m);
    }

    [Theory]
    [InlineData("Short", "Short desc", 1.0)]
    [InlineData("Very Long Title That Could Be Used For Testing Maximum Length Scenarios", "Very long description that tests the boundaries", 999999.99)]
    public void UpdateOfferDto_ShouldHandleVariousLengths(string title, string description, decimal price)
    {
        // Act
        var dto = new UpdateOfferDto
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
}