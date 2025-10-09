using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Bookings.Queries;
using SkillSwap.Application.Bookings.Commands;
using SkillSwap.Application.Common.Models;
using SkillSwap.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
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

    /// <summary>
    /// Gets the current user's bookings with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirst("sub") 
                ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var query = new GetUserBookingsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            var response = new
            {
                data = result.Data.Select(b => new
                {
                    id = b.Id,
                    status = b.Status.ToString(),
                    amount = b.Amount,
                    commissionAmount = b.CommissionAmount,
                    createdAt = b.CreatedAt,
                    completedAt = b.CompletedAt,
                    cancelledAt = b.CancelledAt,
                    offer = b.Offer != null ? new
                    {
                        id = b.Offer.Id,
                        title = b.Offer.Title,
                        description = b.Offer.Description,
                        price = b.Offer.Price
                    } : null
                }).ToList(),
                total = result.Total,
                page = result.Page,
                pageSize = result.PageSize,
                totalPages = result.TotalPages
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user bookings for user");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets a specific booking by ID (only if user owns it)
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirst("sub") 
                ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var booking = await _context.Bookings
                .Include(b => b.Offer)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null)
            {
                return NotFound(new { error = "Booking not found" });
            }

            var result = new
            {
                id = booking.Id,
                status = booking.Status.ToString(),
                amount = booking.Amount,
                commissionAmount = booking.CommissionAmount,
                createdAt = booking.CreatedAt,
                completedAt = booking.CompletedAt,
                cancelledAt = booking.CancelledAt,
                stripeCheckoutSessionId = booking.StripeCheckoutSessionId,
                stripePaymentIntentId = booking.StripePaymentIntentId,
                offer = booking.Offer != null ? new
                {
                    id = booking.Offer.Id,
                    title = booking.Offer.Title,
                    description = booking.Offer.Description,
                    price = booking.Offer.Price
                } : null,
                user = booking.User != null ? new
                {
                    id = booking.User.Id,
                    displayName = booking.User.DisplayName,
                    email = booking.User.Email
                } : null
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking {BookingId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancels a booking (only if user owns it and it's pending)
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirst("sub") 
                ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var command = new CancelBookingCommand
            {
                BookingId = id,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { error = "Unable to cancel booking. It may not exist or cannot be cancelled." });
            }

            return Ok(new { message = "Booking cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("success")]
    [AllowAnonymous]
    public async Task<IActionResult> BookingSuccess([FromQuery] string session_id)
    {
        try
        {
            if (string.IsNullOrEmpty(session_id))
            {
                _logger.LogWarning("Success endpoint called without session_id");
                // Redirect to a generic success page without exposing error details
                return Redirect("/booking/success");
            }

            // Find the booking by Stripe checkout session ID
            var booking = await _context.Bookings
                .Include(b => b.Offer)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.StripeCheckoutSessionId == session_id);

            if (booking == null)
            {
                _logger.LogWarning("Booking not found for session_id: {SessionId}", session_id);
                // Redirect to a generic success page without exposing error details
                return Redirect("/booking/success");
            }

            // Store booking info in session/cache temporarily for the success page
            HttpContext.Session.SetString($"booking_success_{booking.Id}", "true");
            
            _logger.LogInformation("Booking success processed for booking {BookingId}", booking.Id);
            
            // Redirect to clean URL without sensitive parameters
            return Redirect($"/booking/success/{booking.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing booking success for session {SessionId}", session_id);
            return Redirect("/booking/success");
        }
    }

    [HttpGet("cancel")]
    [AllowAnonymous]
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

    [HttpGet("{id:guid}/status")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetBookingStatus(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirst("sub") 
                ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            var booking = await _context.Bookings
                .Include(b => b.Offer)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

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