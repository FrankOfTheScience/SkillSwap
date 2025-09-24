using FluentAssertions;
using SkillSwap.Application.Users.Commands;
using SkillSwap.Application.Users.Validators;

namespace SkillSwap.Tests.Users.Validators;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var command = new RegisterUserCommand
        (
            "user@example.com",
            "User",
            "secure123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyEmail_ReturnsEmailMandatoryError()
    {
        var command = new RegisterUserCommand
        (
            "",
            "User",
            "secure123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email is mandatory");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ReturnsEmailFormatError()
    {
        var command = new RegisterUserCommand
        (
            "not-an-email",
            "User",
            "secure123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email format is not valid");
    }

    [Fact]
    public void Validate_EmptyDisplayName_ReturnsDisplayNameMandatoryError()
    {
        var command = new RegisterUserCommand
        (
            "user@example.com",
            "",
            "secure123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName" && e.ErrorMessage == "DisplayName is mandatory");
    }

    [Fact]
    public void Validate_EmptyPassword_ReturnsPasswordMandatoryError()
    {
        var command = new RegisterUserCommand
        (
            "user@example.com",
            "User",
            ""
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password is mandatory");
    }

    [Fact]
    public void Validate_ShortPassword_ReturnsPasswordLengthError()
    {
        var command = new RegisterUserCommand
        (
            "user@example.com",
            "User",
            "123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password must have at least 6 characters");
    }

    [Fact]
    public void Validate_AllFieldsEmpty_ReturnsAllErrors()
    {
        var command = new RegisterUserCommand
        (
            "",
            "",
            ""
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email is mandatory");
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName" && e.ErrorMessage == "DisplayName is mandatory");
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password is mandatory");
    }
}