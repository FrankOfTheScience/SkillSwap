using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Api.Dtos;
using SkillSwap.Application.Bookings.Commands;
using SkillSwap.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class CheckoutController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStripeService _stripeService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(
        IMediator mediator,
        IStripeService stripeService,
        IApplicationDbContext dbContext,
        IConfiguration configuration,
        ILogger<CheckoutController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _stripeService = stripeService ?? throw new ArgumentNullException(nameof(stripeService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string GetBaseUrl(HttpRequest request)
    {
        var scheme = request.Scheme;
        var host = request.Host.Value;
        
        // For development, determine the frontend port based on the API port
        if (request.Host.Host == "localhost")
        {
            var frontendPort = request.Host.Port switch
            {
                5000 or 5001 => 3000, // Default development frontend port
                7000 or 7001 => 3000, // Alternative API ports
                _ => 3000 // Default fallback
            };
            return $"{scheme}://localhost:{frontendPort}";
        }
        
        return $"{scheme}://{host}";
    }

    /// <summary>
    /// Create a Stripe checkout session
    /// </summary>
    /// <param name="request">Checkout session request</param>
    /// <returns>Checkout URL and booking ID</returns>
    [HttpPost("session")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
    {
        try
        {
            // Get user ID from token
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirstValue("sub") 
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { Error = "Invalid user ID in token" });
            }

            // Override request UserId with token UserId for security
            var createBookingCommand = new CreateBookingCommand(
                request.OfferId,
                userId
            );

            // Create the booking
            var bookingId = await _mediator.Send(createBookingCommand);

            // Get stripe configuration and build dynamic URLs
            var stripeConfig = _configuration.GetSection("Stripe");
            var baseUrl = GetBaseUrl(Request);
            var successUrl = stripeConfig["CheckoutUrls:SuccessUrl"] ?? $"{baseUrl}/?showModal=bookingSuccess&session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = stripeConfig["CheckoutUrls:CancelUrl"] ?? $"{baseUrl}/booking/cancel";
            
            // Use dynamic base URL if not configured in appsettings
            if (successUrl.StartsWith("http://localhost:"))
            {
                successUrl = $"{baseUrl}/?showModal=bookingSuccess&session_id={{CHECKOUT_SESSION_ID}}";
            }
            if (cancelUrl.StartsWith("http://localhost:"))
            {
                cancelUrl = $"{baseUrl}/booking/cancel";
            }

            // Get booking details to get the amount
            var booking = await _dbContext.Bookings
                .Include(b => b.Offer)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking?.Offer == null)
            {
                _logger.LogError("Booking or offer not found after creation. BookingId: {BookingId}", bookingId);
                return Problem("Booking or offer not found after creation");
            }

            // Create Stripe checkout session
            var checkoutUrl = await _stripeService.CreateCheckoutSessionAsync(
                bookingId,
                booking.Amount,
                booking.CommissionAmount,
                successUrl,
                cancelUrl
            );

            return Ok(new CreateCheckoutSessionResponse
            {
                CheckoutUrl = checkoutUrl,
                BookingId = bookingId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session for offer {OfferId}", request.OfferId);
            return Problem($"Error creating checkout session: {ex.Message}");
        }
    }
}