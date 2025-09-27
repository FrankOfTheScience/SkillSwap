using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Domain.Constants;
[ExcludeFromCodeCoverage]
public static class StripeConstants
{
    public const string CheckoutSessionPrefix = "cs_";
    public const string PaymentIntentPrefix = "pi_";
}
[ExcludeFromCodeCoverage]
public static class CommissionConstants
{
    public const decimal DefaultCommissionRate = 0.10m; // 10%
}
[ExcludeFromCodeCoverage]
public static class ValidationConstants
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 1000;
    public const decimal MinPrice = 0.01m;
    public const decimal MaxPrice = 99999.99m;
}