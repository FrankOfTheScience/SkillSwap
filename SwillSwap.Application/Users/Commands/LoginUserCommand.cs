using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Application.Users.Commands;

[ExcludeFromCodeCoverage]
public record LoginUserCommand(string Email, string Password) : IRequest<string>;