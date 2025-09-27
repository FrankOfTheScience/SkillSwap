using FluentAssertions;
using SkillSwap.Application.Bookings.Commands;
using System;
using Xunit;

namespace SkillSwap.Tests.Validators;

public class CreateBookingCommandValidatorTests
{
    private readonly CreateBookingCommandValidator _validator;

    public CreateBookingCommandValidatorTests()
    {
        _validator = new CreateBookingCommandValidator();
    }

    [Fact]
    public void Should_BeValid_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new CreateBookingCommand(
            OfferId: 1,
            UserId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_BeInvalid_WhenOfferIdIsZeroOrNegative(int offerId)
    {
        // Arrange
        var command = new CreateBookingCommand(
            OfferId: offerId,
            UserId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("OfferId must be greater than 0");
    }

    [Fact]
    public void Should_BeInvalid_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new CreateBookingCommand(
            OfferId: 1,
            UserId: Guid.Empty
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("UserId is required");
    }

    [Fact]
    public void Should_BeInvalid_WhenBothOfferIdAndUserIdAreInvalid()
    {
        // Arrange
        var command = new CreateBookingCommand(
            OfferId: 0,
            UserId: Guid.Empty
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.ErrorMessage == "OfferId must be greater than 0");
        result.Errors.Should().Contain(e => e.ErrorMessage == "UserId is required");
    }

    [Fact]
    public void Should_BeValid_WhenOfferIdIsLarge()
    {
        // Arrange
        var command = new CreateBookingCommand(
            OfferId: int.MaxValue,
            UserId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}