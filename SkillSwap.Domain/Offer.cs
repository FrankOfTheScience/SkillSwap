using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSwap.Domain;

public class Offer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 10000.00)]
    public decimal Price { get; set; }

    [Required]
    [ForeignKey(nameof(Creator))]
    public Guid CreatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Availability information
    [Range(15, 480)] // 15 minutes to 8 hours
    public int DurationInMinutes { get; set; } = 60; // Default 1 hour

    [MaxLength(200)]
    public string? Location { get; set; } // Physical location or "Online"

    public bool IsOnline { get; set; } = true;

    [MaxLength(500)]
    public string? Requirements { get; set; } // Prerequisites or equipment needed

    // Availability schedule
    public List<OfferAvailability> Availabilities { get; set; } = new List<OfferAvailability>();

    // Offer status
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }

    // Categories and tags
    [MaxLength(100)]
    public string? Category { get; set; }

    public List<string> Tags { get; set; } = new List<string>();

    // Navigation properties
    public User? Creator { get; set; }
    public List<Booking> Bookings { get; set; } = new List<Booking>();
}

public class OfferAvailability
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Offer))]
    public Guid OfferId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [Required]
    public TimeOnly EndTime { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Navigation property
    public Offer? Offer { get; set; }
}
