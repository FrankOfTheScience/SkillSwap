using MediatR;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Queries;
public record GetOfferByIdQuery(int Id) : IRequest<OfferDto?>;
