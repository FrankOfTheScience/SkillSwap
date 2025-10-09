using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Domain;
using SkillSwap.Tests.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillSwap.Tests.Handlers;

public class UpdateOfferHandlerTests
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
    public async Task Handle_Should_UpdateOffer_WhenUserIsAdmin()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var offer = new Offer { Title = "old", Description = "d", Price = 1, CreatedBy = Guid.NewGuid() };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var accessor = new HttpContextAccessor { HttpContext = CreateHttpContext(Guid.NewGuid(), "Admin") };
        var mapper = TestHelper.CreateMapper();

        var handler = new UpdateOfferCommandHandler(ctx, mapper, accessor);

        var cmd = new UpdateOfferCommand(offer.Id, "new", "newdesc", 9m, Guid.NewGuid());
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("new");

        var persisted = await ctx.Offers.FindAsync(offer.Id);
        persisted!.Title.Should().Be("new");
    }

    [Fact]
    public async Task Handle_Should_UpdateOwnOffer_WhenUserIsOwner()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var offer = new Offer { Title = "old", Description = "d", Price = 1, CreatedBy = ownerId };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var accessor = new HttpContextAccessor { HttpContext = CreateHttpContext(ownerId, "User") };
        var mapper = TestHelper.CreateMapper();

        var handler = new UpdateOfferCommandHandler(ctx, mapper, accessor);

        var cmd = new UpdateOfferCommand(offer.Id, "newTitle", "newDesc", 5m, ownerId);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("newTitle");

        var persisted = await ctx.Offers.FindAsync(offer.Id);
        persisted!.Title.Should().Be("newTitle");
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorized_WhenUserNotOwnerAndNotAdmin()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var offer = new Offer { Title = "old", Description = "d", Price = 1, CreatedBy = ownerId };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var accessor = new HttpContextAccessor { HttpContext = CreateHttpContext(Guid.NewGuid(), "User") };
        var mapper = TestHelper.CreateMapper();

        var handler = new UpdateOfferCommandHandler(ctx, mapper, accessor);

        var cmd = new UpdateOfferCommand(offer.Id, "hack", "hack", 99m, Guid.NewGuid());

        Func<Task> act = async () => await handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_WhenOfferNotFound()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var accessor = new HttpContextAccessor { HttpContext = CreateHttpContext(Guid.NewGuid(), "Admin") };
        var mapper = TestHelper.CreateMapper();

        var handler = new UpdateOfferCommandHandler(ctx, mapper, accessor);

        var cmd = new UpdateOfferCommand(Guid.NewGuid(), "x", "y", 1m, Guid.NewGuid());
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().BeNull();
    }
}
