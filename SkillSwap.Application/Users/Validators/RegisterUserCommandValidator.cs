using FluentValidation;
using SkillSwap.Application.Users.Commands;

namespace SkillSwap.Application.Users.Validators;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is mandatory")
            .EmailAddress().WithMessage("Email format is not valid");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("DisplayName is mandatory");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is mandatory")
            .MinimumLength(6).WithMessage("Password must have at least 6 characters");
    }
}