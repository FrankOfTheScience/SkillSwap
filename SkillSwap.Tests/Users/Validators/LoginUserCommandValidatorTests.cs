using FluentAssertions;
using SkillSwap.Application.Users.Commands;
using SkillSwap.Application.Users.Validators;

namespace SkillSwap.Tests.Users.Validators;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        var command = new LoginUserCommand
        (
            "user@example.com",
            "password"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyEmail_ReturnsEmailMandatoryError()
    {
        var command = new LoginUserCommand
        (
            "",
            "password"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email is mandatory");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ReturnsEmailFormatError()
    {
        var command = new LoginUserCommand
        (
            "not-an-email",
            "password"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email format is not valid");
    }

    [Fact]
    public void Validate_EmptyPassword_ReturnsPasswordMandatoryError()
    {
        var command = new LoginUserCommand
        (
             "user@example.com",
             ""
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password is mandatory");
    }

    [Fact]
    public void Validate_EmptyEmailAndPassword_ReturnsBothErrors()
    {
        var command = new LoginUserCommand
        (
             "",
             ""
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email is mandatory");
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Password is mandatory");
    }
}