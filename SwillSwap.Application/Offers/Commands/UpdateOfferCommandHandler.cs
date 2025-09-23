using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Commands;
public class UpdateOfferCommandHandler : IRequestHandler<UpdateOfferCommand, OfferDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UpdateOfferCommandHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<OfferDto?> Handle(UpdateOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _db.Offers.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (offer == null) return null;

        offer.Title = request.Title;
        offer.Description = request.Description;
        offer.Price = request.Price;
        offer.CreatedBy = request.CreatedBy;

        await _db.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OfferDto>(offer);
    }
}