using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillSwap.Application.Offers.Commands;
public class DeleteOfferCommandHandler : IRequestHandler<DeleteOfferCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeleteOfferCommandHandler(IApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> Handle(DeleteOfferCommand request, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        // Try multiple claim types for user ID
        var userIdClaim = user.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                         ?? user.FindFirstValue("sub") 
                         ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        var offer = await _db.Offers.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (offer is null)
            return false;

        if (!roles.Contains("Admin") && offer.CreatedBy != userId)
            throw new UnauthorizedAccessException("You can't delete an offer not owned by you");

        _db.Offers.Remove(offer);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}