using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using SkillSwap.Api.Configuration;
using SkillSwap.Application.Bookings.Commands;
using SkillSwap.Application.Common.Interfaces;
using MediatR;
using Event = Stripe.Event;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly StripeSettings _stripeSettings;
    private readonly IStripeService _stripeService;
    private readonly IStripeEventParser _eventParser;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IMediator mediator,
        IOptions<StripeSettings> stripeSettings,
        IStripeService stripeService,
        IStripeEventParser eventParser,
        ILogger<WebhookController> logger)
    {
        _mediator = mediator;
        _stripeSettings = stripeSettings.Value;
        _stripeService = stripeService;
        _eventParser = eventParser;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        
        try
        {
            // Verify webhook signature for security
            var stripeSignature = Request.Headers["Stripe-Signature"].ToString();
            
            var isValidSignature = await _stripeService.VerifyWebhookSignatureAsync(json, stripeSignature);
            if (!isValidSignature)
            {
                _logger.LogError("Invalid Stripe webhook signature");
                return BadRequest("Invalid signature");
            }

            var stripeEvent = _eventParser.ParseEvent(json);

            _logger.LogInformation("Received Stripe webhook: {EventType} - {EventId}", 
                stripeEvent.Type, stripeEvent.Id);

            // Handle different event types
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(stripeEvent);
                    break;

                case "checkout.session.expired":
                    await HandleCheckoutSessionExpired(stripeEvent);
                    break;

                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded(stripeEvent);
                    break;

                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed(stripeEvent);
                    break;

                case "payment_intent.canceled":
                    await HandlePaymentIntentCanceled(stripeEvent);
                    break;

                case "charge.dispute.created":
                    await HandleChargeDisputeCreated(stripeEvent);
                    break;

                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        if (session == null)
        {
            _logger.LogWarning("Checkout session data is null");
            return;
        }

        _logger.LogInformation("Processing checkout session completed: {SessionId}", session.Id);

        // Extract booking ID from metadata
        if (session.Metadata.TryGetValue("booking_id", out var bookingIdStr) &&
            Guid.TryParse(bookingIdStr, out var bookingId))
        {
            var command = new CompleteBookingCommand
            {
                BookingId = bookingId,
                StripeCheckoutSessionId = session.Id,
                StripePaymentIntentId = session.PaymentIntentId,
                PaymentStatus = session.PaymentStatus
            };

            await _mediator.Send(command);
            
            _logger.LogInformation("Successfully completed booking {BookingId}", bookingId);
        }
        else
        {
            _logger.LogWarning("Could not extract booking ID from session metadata");
        }
    }

    private Task HandlePaymentIntentSucceeded(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            _logger.LogWarning("Payment intent data is null");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Payment intent succeeded: {PaymentIntentId}", paymentIntent.Id);
        
        // Additional payment intent processing can go here if needed
        // For now, the main logic is handled in checkout.session.completed
        return Task.CompletedTask;
    }

    private Task HandlePaymentIntentFailed(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            _logger.LogWarning("Payment intent data is null");
            return Task.CompletedTask;
        }

        _logger.LogWarning("Payment intent failed: {PaymentIntentId}", paymentIntent.Id);
        
        // Handle payment failure - could update booking status to Failed
        // Implementation depends on business requirements
        return Task.CompletedTask;
    }

    private Task HandleCheckoutSessionExpired(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null)
        {
            _logger.LogWarning("Checkout session data is null");
            return Task.CompletedTask;
        }

        _logger.LogWarning("Checkout session expired: {SessionId}", session.Id);

        // Handle expired session - could update booking status to Expired
        if (session.Metadata != null && session.Metadata.TryGetValue("booking_id", out var bookingIdStr))
        {
            if (int.TryParse(bookingIdStr, out var bookingId))
            {
                _logger.LogInformation("Booking {BookingId} checkout session expired", bookingId);
                
                // Could implement: Update booking status to Expired/Canceled
                // var command = new CancelBookingCommand { BookingId = bookingId, Reason = "Session Expired" };
                // await _mediator.Send(command);
            }
        }
        
        return Task.CompletedTask;
    }

    private Task HandlePaymentIntentCanceled(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null)
        {
            _logger.LogWarning("Payment intent data is null");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Payment intent canceled: {PaymentIntentId}", paymentIntent.Id);
        
        // Handle payment cancellation - could update booking status to Canceled
        // Implementation depends on business requirements
        return Task.CompletedTask;
    }

    private Task HandleChargeDisputeCreated(Event stripeEvent)
    {
        var dispute = stripeEvent.Data.Object as Dispute;
        if (dispute == null)
        {
            _logger.LogWarning("Dispute data is null");
            return Task.CompletedTask;
        }

        _logger.LogWarning("Charge dispute created: {DisputeId} for charge {ChargeId}", 
            dispute.Id, dispute.ChargeId);
        
        // Handle dispute/chargeback - could notify admins, update booking status
        // This is a serious event that requires manual review
        _logger.LogError("URGENT: Chargeback received for dispute {DisputeId}. Manual review required!", 
            dispute.Id);
            
        return Task.CompletedTask;
    }
}