using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Application.Offers.Dtos;

[ExcludeFromCodeCoverage]
public class OfferDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CreatedBy { get; set; }
}
