using FluentAssertions;
using FluentValidation.TestHelper;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Application.Offers.Validators;

namespace SkillSwap.Tests.Validators;

public class UpdateOfferCommandValidatorTests
{
    private readonly UpdateOfferCommandValidator _validator = new UpdateOfferCommandValidator();

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        var cmd = new UpdateOfferCommand(1, "", "d", 0m, 0);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(c => c.Title);
        result.ShouldHaveValidationErrorFor(c => c.Price);
        result.ShouldHaveValidationErrorFor(c => c.CreatedBy);
    }

    [Fact]
    public void Should_Pass_When_Valid()
    {
        var cmd = new UpdateOfferCommand(1, "t", "d", 10m, 1);
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeTrue();
    }
}