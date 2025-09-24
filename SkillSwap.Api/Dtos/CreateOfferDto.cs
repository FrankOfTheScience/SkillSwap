using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Dtos;

[ExcludeFromCodeCoverage]
public class CreateOfferDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int CreatedBy { get; set; }
}
