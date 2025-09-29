using MediatR;

namespace SkillSwap.Application.Bookings.Commands;

public record CreateBookingCommand(
    int OfferId,
    Guid UserId,
    DateTime? ScheduledDateTime = null,
    int? DurationInMinutes = null,
    string? CustomerNotes = null,
    string? Location = null,
    bool? IsOnline = null
) : IRequest<int>;