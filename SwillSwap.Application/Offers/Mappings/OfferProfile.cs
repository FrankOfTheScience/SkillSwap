using AutoMapper;
using SkillSwap.Application.Offers.Dtos;
using SkillSwap.Domain;

namespace SkillSwap.Application.Offers.Mappings;
public class OfferProfile : Profile
{
    public OfferProfile()
    {
        CreateMap<Offer, OfferDto>();
    }
}
