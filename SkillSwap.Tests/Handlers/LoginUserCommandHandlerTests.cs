using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Users.Commands;

namespace SkillSwap.Tests.Handlers;

public class LoginUserCommandHandlerTests
{
    private static LoginUserCommand GetCommand(string email = "user@example.com", string password = "password")
        => new LoginUserCommand(email, password);

    [Fact]
    public async Task Handle_InvalidValidation_ThrowsValidationException()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var auth = Substitute.For<IAuthService>();
        var validator = Substitute.For<IValidator<LoginUserCommand>>();
        var errors = new[] { new ValidationFailure("Email", "Required") };
        validator.ValidateAsync(Arg.Any<LoginUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(errors));

        var handler = new LoginUserCommandHandler(db, auth, validator);

        Func<Task> act = async () => await handler.Handle(GetCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Email*");
    }
}