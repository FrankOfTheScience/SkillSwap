using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Tests.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillSwap.Tests.Handlers;

public class CreateOfferHandlerTests
{
    private static HttpContext CreateHttpContext(Guid userId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, "test@user.com")
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        return new DefaultHttpContext { User = principal };
    }

    [Fact]
    public async Task Handle_Should_CreateOffer_ForAuthenticatedUser()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var accessor = new HttpContextAccessor { HttpContext = CreateHttpContext(userId, "User") };
        var mapper = TestHelper.CreateMapper();

        var handler = new CreateOfferCommandHandler(ctx, mapper, accessor);

        var cmd = new CreateOfferCommand("newTitle", "newDesc", 100m, Guid.NewGuid());
        var offerId = await handler.Handle(cmd, CancellationToken.None);

        var offer = await ctx.Offers.FindAsync(offerId);
        offer.Should().NotBeNull();
        offer!.Title.Should().Be("newTitle");
        offer.CreatedBy.Should().Be(userId);
    }
}
