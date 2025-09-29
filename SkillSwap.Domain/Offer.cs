namespace SkillSwap.Domain;

public class Offer
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Availability information
    public int DurationInMinutes { get; set; } = 60; // Default 1 hour
    public string? Location { get; set; } // Physical location or "Online"
    public bool IsOnline { get; set; } = true;
    public string? Requirements { get; set; } // Prerequisites or equipment needed
    
    // Availability schedule
    public List<OfferAvailability> Availabilities { get; set; } = new List<OfferAvailability>();
    
    // Offer status
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }
    
    // Categories and tags
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    
    // Navigation properties
    public User? Creator { get; set; }
    public List<Booking> Bookings { get; set; } = new List<Booking>();
}

public class OfferAvailability
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    
    // Navigation property
    public Offer? Offer { get; set; }
}
