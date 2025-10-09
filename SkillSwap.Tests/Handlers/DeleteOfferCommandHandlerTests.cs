using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Domain;
using SkillSwap.Tests.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillSwap.Tests.Handlers;

public class DeleteOfferHandlerTests
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
    public async Task Handle_Should_DeleteOffer_WhenUserIsAdmin()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var offer = new Offer { Title = "to delete", Description = "desc", Price = 10, CreatedBy = Guid.NewGuid() };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(CreateHttpContext(Guid.NewGuid(), "Admin"));

        var handler = new DeleteOfferCommandHandler(ctx, accessor);

        var result = await handler.Handle(new DeleteOfferCommand(offer.Id), CancellationToken.None);

        result.Should().BeTrue();
        (await ctx.Offers.FindAsync(offer.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_DeleteOwnOffer_WhenUserIsOwner()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var ownerId = Guid.NewGuid();
        var offer = new Offer { Title = "mine", Description = "desc", Price = 5, CreatedBy = ownerId };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(CreateHttpContext(ownerId, "User"));

        var handler = new DeleteOfferCommandHandler(ctx, accessor);

        var result = await handler.Handle(new DeleteOfferCommand(offer.Id), CancellationToken.None);

        result.Should().BeTrue();
        (await ctx.Offers.FindAsync(offer.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ThrowUnauthorized_WhenUserNotOwnerAndNotAdmin()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var offer = new Offer { Title = "other", Description = "desc", Price = 3, CreatedBy = Guid.NewGuid() };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(CreateHttpContext(Guid.NewGuid(), "User"));

        var handler = new DeleteOfferCommandHandler(ctx, accessor);

        Func<Task> act = async () => await handler.Handle(new DeleteOfferCommand(offer.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnFalse_WhenOfferNotFound()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(CreateHttpContext(Guid.NewGuid(), "Admin"));

        var handler = new DeleteOfferCommandHandler(ctx, accessor);

        var result = await handler.Handle(new DeleteOfferCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
    }
}
