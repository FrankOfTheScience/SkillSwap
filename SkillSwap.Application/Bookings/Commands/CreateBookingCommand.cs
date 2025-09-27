using MediatR;

namespace SkillSwap.Application.Bookings.Commands;

public record CreateBookingCommand(
    int OfferId,
    Guid UserId
) : IRequest<int>;