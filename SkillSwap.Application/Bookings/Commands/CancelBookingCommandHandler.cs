using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Bookings.Commands;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(IApplicationDbContext context, ILogger<CancelBookingCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.UserId == request.UserId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking {BookingId} not found for user {UserId}", request.BookingId, request.UserId);
                return false;
            }

            // Only allow cancellation of pending bookings
            if (booking.Status != BookingStatus.Pending)
            {
                _logger.LogWarning("Cannot cancel booking {BookingId} with status {Status}", request.BookingId, booking.Status);
                return false;
            }

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Booking {BookingId} successfully cancelled", request.BookingId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", request.BookingId);
            return false;
        }
    }
}