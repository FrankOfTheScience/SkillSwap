using MediatR;

namespace SkillSwap.Application.Offers.Commands;
public record CreateOfferCommand(string Title, string Description, decimal Price, int CreatedBy) : IRequest<int>;
