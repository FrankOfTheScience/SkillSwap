using MediatR;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Queries;
public record GetOffersQuery(int Page = 1, int PageSize = 10) : IRequest<List<OfferDto>>;