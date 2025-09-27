using SkillSwap.Domain;

namespace SkillSwap.Application.Common.Interfaces;

public interface IAuthService
{
    string GenerateJwt(User user);
}