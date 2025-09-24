using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Dtos;
using System.Security.Claims;

namespace SkillSwap.Application.Offers.Commands;
public class UpdateOfferCommandHandler : IRequestHandler<UpdateOfferCommand, OfferDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateOfferCommandHandler(IApplicationDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OfferDto?> Handle(UpdateOfferCommand request, CancellationToken ct)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var offer = await _db.Offers.FirstOrDefaultAsync(o => o.Id == request.Id, ct);
        if (offer is null)
            return null;

        if (!roles.Contains("Admin") && offer.CreatedBy != request.UpdatedBy)
            throw new UnauthorizedAccessException("You can't update an offer not owned by you");

        offer.Title = request.Title;
        offer.Description = request.Description;
        offer.Price = request.Price;

        await _db.SaveChangesAsync(ct);

        return _mapper.Map<OfferDto>(offer);
    }
}