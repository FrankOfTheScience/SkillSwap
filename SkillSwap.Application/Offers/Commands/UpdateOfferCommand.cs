using MediatR;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Commands;
public record UpdateOfferCommand(
    Guid Id, 
    string Title, 
    string Description, 
    decimal Price, 
    Guid UpdatedBy,
    int DurationInMinutes = 60,
    string? Location = null,
    bool IsOnline = true,
    string? Requirements = null,
    string? Category = null
) : IRequest<OfferDto?>;
