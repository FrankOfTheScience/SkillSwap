using MediatR;

namespace SkillSwap.Application.Offers.Commands;
public record DeleteOfferCommand(Guid Id) : IRequest<bool>;