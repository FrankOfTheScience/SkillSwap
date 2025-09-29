using MediatR;

namespace SkillSwap.Application.Bookings.Commands;

public class CancelBookingCommand : IRequest<bool>
{
    public int BookingId { get; set; }
    public Guid UserId { get; set; }
}