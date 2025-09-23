using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;

namespace SkillSwap.Application.Offers.Commands;
public class DeleteOfferCommandHandler : IRequestHandler<DeleteOfferCommand, bool>
{
    private readonly IApplicationDbContext _db;

    public DeleteOfferCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(DeleteOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _db.Offers.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (offer == null) return false;

        _db.Offers.Remove(offer);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}