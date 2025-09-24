using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Users.Commands;
using SkillSwap.Domain;

namespace SkillSwap.Tests.Handlers;

public class RegisterUserCommandHandlerTests
{
    private static RegisterUserCommand GetCommand(string email = "newuser@example.com", string password = "password", string displayName = "New User")
        => new RegisterUserCommand(email, password, displayName);

    [Fact]
    public async Task Handle_InvalidValidation_ThrowsValidationException()
    {
        var db = Substitute.For<IApplicationDbContext>();
        var auth = Substitute.For<IAuthService>();
        var mapper = Substitute.For<IMapper>();
        var validator = Substitute.For<IValidator<RegisterUserCommand>>();
        var errors = new[] { new ValidationFailure("Email", "Required") };
        validator.ValidateAsync(Arg.Any<RegisterUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(errors));

        var handler = new RegisterUserCommandHandler(db, auth, mapper, validator);

        Func<Task> act = async () => await handler.Handle(GetCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Email*");
    }
}