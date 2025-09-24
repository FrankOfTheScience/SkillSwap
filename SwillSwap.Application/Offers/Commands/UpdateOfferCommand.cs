using MediatR;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Commands;
public record UpdateOfferCommand(int Id, string Title, string Description, decimal Price, Guid UpdatedBy) : IRequest<OfferDto?>;
