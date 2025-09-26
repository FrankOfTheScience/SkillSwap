using MediatR;

namespace SkillSwap.Application.Users.Commands;

public record RegisterUserCommand(string Email, string DisplayName, string Password) : IRequest<string>;
