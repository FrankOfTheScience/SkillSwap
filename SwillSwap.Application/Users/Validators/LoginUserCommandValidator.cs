using FluentValidation;
using SkillSwap.Application.Users.Commands;

namespace SkillSwap.Application.Users.Validators;
public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is mandatory")
            .EmailAddress().WithMessage("Email format is not valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is mandatory");
    }
}