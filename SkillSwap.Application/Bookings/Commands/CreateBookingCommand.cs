using MediatR;

namespace SkillSwap.Application.Bookings.Commands;

public record CreateBookingCommand(
    Guid OfferId,
    Guid UserId,
    DateTime? ScheduledDateTime = null,
    int? DurationInMinutes = null,
    string? CustomerNotes = null,
    string? Location = null,
    bool? IsOnline = null
) : IRequest<Guid>;