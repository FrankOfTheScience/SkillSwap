using MediatR;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain;

namespace SkillSwap.Application.Offers.Commands;
public class CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, int>
{
    private readonly IApplicationDbContext _db;

    public CreateOfferCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = new Offer
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            CreatedBy = request.CreatedBy
        };

        _db.Offers.Add(offer);
        await _db.SaveChangesAsync(cancellationToken);
        return offer.Id;
    }
}