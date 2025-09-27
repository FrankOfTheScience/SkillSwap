using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillSwap.Domain.Enums;
using SkillSwap.Application.Common.Interfaces;

namespace SkillSwap.Application.Bookings.Commands;

public class CompleteBookingCommandHandler : IRequestHandler<CompleteBookingCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CompleteBookingCommandHandler> _logger;

    public CompleteBookingCommandHandler(
        IApplicationDbContext context,
        ILogger<CompleteBookingCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(CompleteBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (booking == null)
            {
                _logger.LogWarning("Booking {BookingId} not found", request.BookingId);
                return false;
            }

            // Update booking with payment information
            booking.Status = BookingStatus.Completed;
            booking.StripeCheckoutSessionId = request.StripeCheckoutSessionId;
            booking.StripePaymentIntentId = request.StripePaymentIntentId;
            booking.CompletedAt = DateTime.UtcNow;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully completed booking {BookingId} with payment status {PaymentStatus}",
                request.BookingId, 
                request.PaymentStatus);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error completing booking {BookingId}", 
                request.BookingId);
            return false;
        }
    }
}