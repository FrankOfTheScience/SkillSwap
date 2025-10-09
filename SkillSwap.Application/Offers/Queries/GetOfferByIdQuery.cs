using MediatR;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Queries;
public record GetOfferByIdQuery(Guid Id) : IRequest<OfferDto?>;
