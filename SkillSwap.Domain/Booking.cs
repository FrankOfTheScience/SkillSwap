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
    public DateTime? CancelledAt { get; set; }
    
    // Scheduling information
    public DateTime ScheduledDateTime { get; set; }
    public int DurationInMinutes { get; set; }
    public string? Location { get; set; }
    public bool IsOnline { get; set; } = true;
    
    // Booking details
    public string? CustomerNotes { get; set; }
    public string? ProviderNotes { get; set; }
    
    // Meeting information
    public string? MeetingUrl { get; set; }
    public string? MeetingId { get; set; }
    public string? MeetingPassword { get; set; }
    
    // Feedback and rating
    public int? CustomerRating { get; set; } // 1-5 stars
    public string? CustomerFeedback { get; set; }
    public int? ProviderRating { get; set; } // 1-5 stars
    public string? ProviderFeedback { get; set; }
    public DateTime? FeedbackSubmittedAt { get; set; }
    
    // Navigation properties
    public Offer? Offer { get; set; }
    public User? User { get; set; }
}
