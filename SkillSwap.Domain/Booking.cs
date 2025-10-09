using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Domain;

public class Booking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Offer))]
    public Guid OfferId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 10000.00)]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.00, 1000.00)]
    public decimal CommissionAmount { get; set; }

    [MaxLength(200)]
    public string? StripeCheckoutSessionId { get; set; }

    [MaxLength(200)]
    public string? StripePaymentIntentId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Scheduling information
    [Required]
    public DateTime ScheduledDateTime { get; set; }

    [Required]
    [Range(15, 480)] // 15 minutes to 8 hours
    public int DurationInMinutes { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public bool IsOnline { get; set; } = true;

    // Booking details
    [MaxLength(1000)]
    public string? CustomerNotes { get; set; }

    [MaxLength(1000)]
    public string? ProviderNotes { get; set; }

    // Meeting information
    [MaxLength(500)]
    [Url]
    public string? MeetingUrl { get; set; }

    [MaxLength(100)]
    public string? MeetingId { get; set; }

    [MaxLength(100)]
    public string? MeetingPassword { get; set; }

    // Feedback and rating
    [Range(1, 5)]
    public int? CustomerRating { get; set; } // 1-5 stars

    [MaxLength(1000)]
    public string? CustomerFeedback { get; set; }

    [Range(1, 5)]
    public int? ProviderRating { get; set; } // 1-5 stars

    [MaxLength(1000)]
    public string? ProviderFeedback { get; set; }

    public DateTime? FeedbackSubmittedAt { get; set; }

    // Navigation properties
    public Offer? Offer { get; set; }
    public User? User { get; set; }
}
