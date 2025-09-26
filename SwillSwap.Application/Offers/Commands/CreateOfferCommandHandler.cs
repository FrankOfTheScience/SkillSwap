using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillSwap.Application.Offers.Commands;
public class CreateOfferCommandHandler : IRequestHandler<CreateOfferCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateOfferCommandHandler(IApplicationDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<int> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = _mapper.Map<Offer>(request);
        // The CreatedBy is already set from the command parameter via AutoMapper

        _db.Offers.Add(offer);
        await _db.SaveChangesAsync(cancellationToken);

        return offer.Id;
    }
}