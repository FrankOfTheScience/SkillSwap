using System.ComponentModel.DataAnnotations;

namespace SkillSwap.Domain;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Roles { get; set; } = new List<string>(); // Stubbed until AWS Cognito integration

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
    public int ProfileCompletionPercentage { get; set; } = 0;
    public DateTime? LastProfileUpdate { get; set; }
    
    // Professional information
    public string? Profession { get; set; }
    public string? Company { get; set; }
    public int YearsOfExperience { get; set; } = 0;
    public List<string> Skills { get; set; } = new List<string>();
    
    // User preferences
    public string? PreferredLanguage { get; set; } = "en";
    public string? TimeZone { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
}