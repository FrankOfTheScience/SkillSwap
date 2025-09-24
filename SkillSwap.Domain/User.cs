using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Domain;

[ExcludeFromCodeCoverage]
public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
