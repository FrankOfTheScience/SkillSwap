using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Dtos;

namespace SkillSwap.Application.Offers.Queries;
public class GetOffersQueryHandler : IRequestHandler<GetOffersQuery, List<OfferDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetOffersQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<OfferDto>> Handle(GetOffersQuery request, CancellationToken cancellationToken)
    {
        var offers = await _db.Offers
            .AsNoTracking()
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        return _mapper.Map<List<OfferDto>>(offers);
    }
}