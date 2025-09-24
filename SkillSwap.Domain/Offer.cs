using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Domain;

[ExcludeFromCodeCoverage]
public class Offer
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int CreatedBy { get; set; }
}
