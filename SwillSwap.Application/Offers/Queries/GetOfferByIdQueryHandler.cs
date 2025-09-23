using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Queries;
public class GetOfferByIdQueryHandler : IRequestHandler<GetOfferByIdQuery, OfferDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetOfferByIdQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<OfferDto?> Handle(GetOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var offer = await _db.Offers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        return offer is null ? null : _mapper.Map<OfferDto>(offer);
    }
}