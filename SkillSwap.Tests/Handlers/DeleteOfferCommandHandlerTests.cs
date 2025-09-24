using FluentAssertions;
using NSubstitute;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Domain;
using SkillSwap.Tests.Common;

namespace SkillSwap.Tests.Handlers;
public class DeleteOfferCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_DeleteExistingOffer_And_ReturnTrue()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var offer = new Offer { Title = "todel", Description = "d", Price = 1, CreatedBy = Guid.NewGuid() };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        dbSub.SaveChangesAsync(Arg.Any<CancellationToken>())
             .Returns(ci => ctx.SaveChangesAsync(ci.Arg<CancellationToken>()));

        var handler = new DeleteOfferCommandHandler(dbSub);

        var result = await handler.Handle(new DeleteOfferCommand(offer.Id), CancellationToken.None);
        result.Should().BeTrue();

        var persisted = await ctx.Offers.FindAsync(offer.Id);
        persisted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnFalse_WhenNotFound()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);

        var handler = new DeleteOfferCommandHandler(dbSub);
        var result = await handler.Handle(new DeleteOfferCommand(999), CancellationToken.None);
        result.Should().BeFalse();
    }
}