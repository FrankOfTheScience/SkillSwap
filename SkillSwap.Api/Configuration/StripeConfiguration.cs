using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Configuration;

[ExcludeFromCodeCoverage]
public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public CommissionSettings Commission { get; set; } = new();
    public CheckoutUrlsSettings CheckoutUrls { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class CommissionSettings
{
    public decimal Rate { get; set; } = 0.10m;
    public int MinimumAmount { get; set; } = 50;
}

[ExcludeFromCodeCoverage]
public class CheckoutUrlsSettings
{
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}