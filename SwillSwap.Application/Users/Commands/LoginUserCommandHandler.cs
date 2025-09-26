using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;

namespace SkillSwap.Application.Users.Commands;
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly IApplicationDbContext _db;
    private readonly IAuthService _auth;
    private readonly IValidator<LoginUserCommand> _validator;

    public LoginUserCommandHandler(IApplicationDbContext db, IAuthService auth, IValidator<LoginUserCommand> validator)
    {
        _db = db;
        _auth = auth;
        _validator = validator;
    }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new FluentValidation.ValidationException(validation.Errors);

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user == null) throw new Exception("User not found");

        using var sha = System.Security.Cryptography.SHA256.Create();
        var passwordHash = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password)));

        if (user.PasswordHash != passwordHash)
            throw new Exception("Wrong password");

        return _auth.GenerateJwt(user);
    }
}