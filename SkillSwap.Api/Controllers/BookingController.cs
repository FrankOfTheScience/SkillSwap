using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using MediatR;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("api/booking")]
public class BookingController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        IApplicationDbContext context,
        IMediator mediator,
        ILogger<BookingController> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("success")]
    public async Task<IActionResult> BookingSuccess([FromQuery] string session_id)
    {
        try
        {
            if (string.IsNullOrEmpty(session_id))
            {
                _logger.LogWarning("Success endpoint called without session_id");
                return BadRequest(new { error = "Missing session_id parameter" });
            }

            // Find the booking by Stripe checkout session ID
            var booking = await _context.Bookings
                .Include(b => b.Offer)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.StripeCheckoutSessionId == session_id);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found for session_id: {SessionId}", session_id);
                return NotFound(new { error = "Booking not found" });
            }

            // Return booking details for success page
            var result = new
            {
                bookingId = booking.Id,
                status = booking.Status.ToString(),
                amount = booking.Amount,
                commissionAmount = booking.CommissionAmount,
                createdAt = booking.CreatedAt,
                completedAt = booking.CompletedAt,
                offer = new
                {
                    id = booking.Offer?.Id,
                    title = booking.Offer?.Title,
                    description = booking.Offer?.Description,
                    price = booking.Offer?.Price
                },
                user = new
                {
                    id = booking.User?.Id,
                    displayName = booking.User?.DisplayName,
                    email = booking.User?.Email
                },
                sessionId = session_id
            };

            _logger.LogInformation("Booking success page accessed for booking {BookingId}", booking.Id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing booking success for session {SessionId}", session_id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("cancel")]
    public async Task<IActionResult> BookingCancel([FromQuery] string session_id)
    {
        try
        {
            if (string.IsNullOrEmpty(session_id))
            {
                _logger.LogWarning("Cancel endpoint called without session_id");
                return BadRequest(new { error = "Missing session_id parameter" });
            }

            // Find the booking by Stripe checkout session ID
            var booking = await _context.Bookings
                .Include(b => b.Offer)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.StripeCheckoutSessionId == session_id);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found for cancelled session_id: {SessionId}", session_id);
                return NotFound(new { error = "Booking not found" });
            }

            // Return booking details for cancel page
            var result = new
            {
                bookingId = booking.Id,
                status = booking.Status.ToString(),
                amount = booking.Amount,
                createdAt = booking.CreatedAt,
                offer = new
                {
                    id = booking.Offer?.Id,
                    title = booking.Offer?.Title,
                    description = booking.Offer?.Description,
                    price = booking.Offer?.Price
                },
                sessionId = session_id,
                message = "Payment was cancelled. You can try again anytime."
            };

            _logger.LogInformation("Booking cancel page accessed for booking {BookingId}", booking.Id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing booking cancel for session {SessionId}", session_id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetBookingStatus(int id)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Offer)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound(new { error = "Booking not found" });
            }

            var result = new
            {
                bookingId = booking.Id,
                status = booking.Status.ToString(),
                amount = booking.Amount,
                commissionAmount = booking.CommissionAmount,
                createdAt = booking.CreatedAt,
                completedAt = booking.CompletedAt,
                stripeCheckoutSessionId = booking.StripeCheckoutSessionId,
                stripePaymentIntentId = booking.StripePaymentIntentId,
                offer = new
                {
                    id = booking.Offer?.Id,
                    title = booking.Offer?.Title,
                    price = booking.Offer?.Price
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking status for booking {BookingId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}