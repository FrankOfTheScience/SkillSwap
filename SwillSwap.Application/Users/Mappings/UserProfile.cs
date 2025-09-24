using AutoMapper;
using SkillSwap.Application.Users.Commands;
using SkillSwap.Domain;

namespace SkillSwap.Application.Users.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<RegisterUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}