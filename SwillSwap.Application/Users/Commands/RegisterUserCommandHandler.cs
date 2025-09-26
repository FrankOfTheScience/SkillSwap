using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain;

namespace SkillSwap.Application.Users.Commands;
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{
    private readonly IApplicationDbContext _db;
    private readonly IAuthService _auth;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserCommandHandler(IApplicationDbContext db, IAuthService auth, IMapper mapper, IValidator<RegisterUserCommand> validator)
    {
        _db = db;
        _auth = auth;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<string> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new FluentValidation.ValidationException(validation.Errors);

        if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
            throw new Exception("Email already registered");

        var user = _mapper.Map<User>(request);

        using var sha = System.Security.Cryptography.SHA256.Create();
        user.PasswordHash = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password)));

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return _auth.GenerateJwt(user);
    }
}