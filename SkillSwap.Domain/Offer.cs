namespace SkillSwap.Domain;

public class Offer
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public Guid CreatedBy { get; set; }
}
