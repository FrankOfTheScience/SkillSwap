using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Dtos;

[ExcludeFromCodeCoverage]
public class CreateCheckoutSessionRequest
{
    public int OfferId { get; set; }
    public Guid UserId { get; set; }
    public DateTime? ScheduledDateTime { get; set; }
    public int? DurationInMinutes { get; set; }
    public string? CustomerNotes { get; set; }
    public string? Location { get; set; }
    public bool? IsOnline { get; set; }
}

[ExcludeFromCodeCoverage]
public class CreateCheckoutSessionResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public int BookingId { get; set; }
}