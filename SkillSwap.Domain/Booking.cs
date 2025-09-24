using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Domain;

[ExcludeFromCodeCoverage]
public class Booking
{
    public int Id { get; set; }
    public int OfferId { get; set; }
    public int UserId { get; set; }
    public string? Status { get; set; }
}
