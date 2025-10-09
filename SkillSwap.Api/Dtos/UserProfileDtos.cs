using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Api.Dtos;

public class GetUserProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    
    // Profile information
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageUrl { get; set; }
    
    // Profile completion tracking
    public int ProfileCompletionPercentage { get; set; }
    public DateTime? LastProfileUpdate { get; set; }
    
    // Professional information
    public string? Profession { get; set; }
    public string? Company { get; set; }
    public int YearsOfExperience { get; set; }
    public List<string> Skills { get; set; } = new List<string>();
    
    // User preferences
    public string PreferredLanguage { get; set; } = "en";
    public string? TimeZone { get; set; }
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    
    // Statistics
    public int TotalOffers { get; set; }
    public int TotalBookings { get; set; }
    public decimal? AverageRating { get; set; }
}

public class UpdateUserProfileRequest
{
    [StringLength(50)]
    public string? FirstName { get; set; }
    
    [StringLength(50)]
    public string? LastName { get; set; }
    
    [StringLength(1000)]
    public string? Bio { get; set; }
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [StringLength(200)]
    public string? Profession { get; set; }
    
    [StringLength(200)]
    public string? Company { get; set; }
    
    [Range(0, 100)]
    public int? YearsOfExperience { get; set; }
    
    public List<string>? Skills { get; set; }
    
    [StringLength(10)]
    public string? PreferredLanguage { get; set; }
    
    [StringLength(100)]
    public string? TimeZone { get; set; }
    
    public bool? EmailNotifications { get; set; }
    public bool? PushNotifications { get; set; }
}

public class ProfileCompletionResponse
{
    public int Percentage { get; set; }
    public List<string> MissingFields { get; set; } = new List<string>();
    public List<string> Suggestions { get; set; } = new List<string>();
}

public class UploadProfileImageRequest
{
    [Required]
    public IFormFile Image { get; set; } = null!;
}

public class UploadProfileImageResponse
{
    public string ProfileImageUrl { get; set; } = null!;
    public string Message { get; set; } = null!;
}