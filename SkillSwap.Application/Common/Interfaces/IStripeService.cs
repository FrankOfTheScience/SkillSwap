namespace SkillSwap.Application.Common.Interfaces;

public interface IStripeService
{
    Task<string> CreateCheckoutSessionAsync(
        Guid bookingId,
        decimal amount,
        decimal commissionAmount,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);
        
    Task<bool> VerifyWebhookSignatureAsync(string payload, string signature);
}