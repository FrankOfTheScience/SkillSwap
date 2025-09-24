using FluentAssertions;
using NSubstitute;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Domain;
using SkillSwap.Tests.Common;

namespace SkillSwap.Tests.Handlers;
public class UpdateOfferCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_ExistingOffer_And_ReturnDto()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var offer = new Offer { Title = "old", Description = "d", Price = 1, CreatedBy = Guid.NewGuid() };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        dbSub.SaveChangesAsync(Arg.Any<CancellationToken>())
             .Returns(ci => ctx.SaveChangesAsync(ci.Arg<CancellationToken>()));

        var mapper = TestHelper.CreateMapper();
        var handler = new UpdateOfferCommandHandler(dbSub, mapper);

        var cmd = new UpdateOfferCommand(offer.Id, "new", "newdesc", 9m, Guid.NewGuid());

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("new");

        var persisted = await ctx.Offers.FindAsync(offer.Id);
        persisted!.Title.Should().Be("new");
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_WhenNotFound()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);

        var mapper = TestHelper.CreateMapper();
        var handler = new UpdateOfferCommandHandler(dbSub, mapper);

        var result = await handler.Handle(new UpdateOfferCommand(999, "x", "y", 1, Guid.NewGuid()), CancellationToken.None);
        result.Should().BeNull();
    }
}