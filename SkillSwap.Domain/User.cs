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
}