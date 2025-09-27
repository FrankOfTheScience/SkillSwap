using Microsoft.Extensions.Options;
using SkillSwap.Api.Configuration;
using SkillSwap.Application.Common.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace SkillSwap.Api.Services;

public class StripeService : IStripeService
{
    private readonly StripeSettings _stripeConfig;
    private readonly SessionService _sessionService;

    public StripeService(IOptions<StripeSettings> stripeConfig)
    {
        _stripeConfig = stripeConfig.Value;
        StripeConfiguration.ApiKey = _stripeConfig.SecretKey;
        _sessionService = new SessionService();
    }

    public async Task<string> CreateCheckoutSessionAsync(
        int bookingId,
        decimal amount,
        decimal commissionAmount,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(amount * 100), // Convert to cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"SkillSwap Service Booking #{bookingId}",
                            Description = "Professional service booking through SkillSwap platform"
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "booking_id", bookingId.ToString() },
                { "commission_amount", commissionAmount.ToString("F2") }
            }
            // Note: ApplicationFeeAmount is only for Stripe Connect accounts
            // For direct payments, we handle commission internally
        };

        var session = await _sessionService.CreateAsync(options, null, cancellationToken);
        return session.Url;
    }

    public Task<bool> VerifyWebhookSignatureAsync(string payload, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload, 
                signature, 
                _stripeConfig.WebhookSecret);
            return Task.FromResult(true);
        }
        catch (StripeException)
        {
            return Task.FromResult(false);
        }
    }
}