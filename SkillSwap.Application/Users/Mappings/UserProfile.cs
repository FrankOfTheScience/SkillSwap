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
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(_ => new List<string> { "User" }))
            // Profile information - set to defaults for new users
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore())
            .ForMember(dest => dest.Bio, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
            .ForMember(dest => dest.City, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
            // Profile completion tracking
            .ForMember(dest => dest.ProfileCompletionPercentage, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.LastProfileUpdate, opt => opt.Ignore())
            // Professional information
            .ForMember(dest => dest.Profession, opt => opt.Ignore())
            .ForMember(dest => dest.Company, opt => opt.Ignore())
            .ForMember(dest => dest.YearsOfExperience, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.Skills, opt => opt.MapFrom(_ => new List<string>()))
            // User preferences
            .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(_ => "en"))
            .ForMember(dest => dest.TimeZone, opt => opt.Ignore())
            .ForMember(dest => dest.EmailNotifications, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.PushNotifications, opt => opt.MapFrom(_ => true));
    }
}