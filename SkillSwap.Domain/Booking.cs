using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Domain;

public class Booking
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public Guid UserId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string? StripeCheckoutSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public Offer? Offer { get; set; }
    public User? User { get; set; }
}
