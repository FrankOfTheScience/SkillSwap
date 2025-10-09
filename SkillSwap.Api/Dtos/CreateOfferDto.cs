using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Dtos;

[ExcludeFromCodeCoverage]
public class CreateOfferDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int DurationInMinutes { get; set; } = 60;
    public string? Location { get; set; }
    public bool IsOnline { get; set; } = true;
    public string? Requirements { get; set; }
    public string? Category { get; set; }
}
