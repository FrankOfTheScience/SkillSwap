using MediatR;

namespace SkillSwap.Application.Bookings.Commands;

public class CancelBookingCommand : IRequest<bool>
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
}