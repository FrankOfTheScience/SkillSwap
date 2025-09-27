namespace SkillSwap.Api.Dtos;

public class CreateCheckoutSessionRequest
{
    public int OfferId { get; set; }
    public Guid UserId { get; set; }
}

public class CreateCheckoutSessionResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public int BookingId { get; set; }
}