using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSwap.Domain;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<string> Roles { get; set; } = new List<string>(); // Stubbed until AWS Cognito integration

    // Profile information
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [MaxLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(500)]
    [Url]
    public string? ProfileImageUrl { get; set; }

    // Profile completion tracking
    [Range(0, 100)]
    public int ProfileCompletionPercentage { get; set; } = 0;

    public DateTime? LastProfileUpdate { get; set; }

    // Professional information
    [MaxLength(100)]
    public string? Profession { get; set; }

    [MaxLength(100)]
    public string? Company { get; set; }

    [Range(0, 100)]
    public int YearsOfExperience { get; set; } = 0;

    public List<string> Skills { get; set; } = new List<string>();

    // User preferences
    [MaxLength(10)]
    public string? PreferredLanguage { get; set; } = "en";

    [MaxLength(50)]
    public string? TimeZone { get; set; }

    public bool EmailNotifications { get; set; } = true;

    public bool PushNotifications { get; set; } = true;
}