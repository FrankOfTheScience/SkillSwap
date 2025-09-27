using FluentValidation;
using SkillSwap.Application.Offers.Commands;

namespace SkillSwap.Application.Offers.Validators;
public class CreateOfferCommandValidator : AbstractValidator<CreateOfferCommand>
{
    public CreateOfferCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is mandatory")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is mandatory");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}