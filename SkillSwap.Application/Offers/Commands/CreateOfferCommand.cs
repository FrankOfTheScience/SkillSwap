using MediatR;

namespace SkillSwap.Application.Offers.Commands;
public record CreateOfferCommand(
    string Title, 
    string Description, 
    decimal Price, 
    Guid CreatedBy,
    int DurationInMinutes = 60,
    string? Location = null,
    bool IsOnline = true,
    string? Requirements = null,
    string? Category = null
) : IRequest<Guid>;
