using MediatR;

namespace SkillSwap.Application.Users.Commands;

public record LoginUserCommand(string Email, string Password) : IRequest<string>;