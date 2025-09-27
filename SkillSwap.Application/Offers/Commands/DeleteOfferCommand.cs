using MediatR;

namespace SkillSwap.Application.Offers.Commands;
public record DeleteOfferCommand(int Id) : IRequest<bool>;