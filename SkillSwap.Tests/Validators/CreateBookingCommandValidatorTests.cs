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
            OfferId: Guid.NewGuid(),
            UserId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Should_BeInvalid_WhenOfferIdIsEmpty()
    {
        // Arrange
        var command = new CreateBookingCommand(
            OfferId: Guid.Empty,
            UserId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("OfferId is required");
    }

    [Fact]
    public void Should_BeInvalid_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new CreateBookingCommand(
            OfferId: Guid.NewGuid(),
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
            OfferId: Guid.Empty,
            UserId: Guid.Empty
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.ErrorMessage == "OfferId is required");
        result.Errors.Should().Contain(e => e.ErrorMessage == "UserId is required");
    }

    [Fact]
    public void Should_BeValid_WhenOfferIdIsValidGuid()
    {
        // Arrange
        var command = new CreateBookingCommand(
            OfferId: Guid.NewGuid(),
            UserId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}