using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Application.Users.Commands;

[ExcludeFromCodeCoverage]
public record RegisterUserCommand(string Email, string DisplayName, string Password) : IRequest<string>;
