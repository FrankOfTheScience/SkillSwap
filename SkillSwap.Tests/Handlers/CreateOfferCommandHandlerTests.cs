using FluentAssertions;
using NSubstitute;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Tests.Common;

namespace SkillSwap.Tests.Handlers;
public class CreateOfferCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_AddOffer_And_ReturnId_And_Call_SaveChanges()
    {
        using var realCtx = TestHelper.CreateInMemoryDbContext();
        var dbSub = Substitute.For<IApplicationDbContext>();

        dbSub.Offers.Returns(realCtx.Offers);

        dbSub.SaveChangesAsync(Arg.Any<System.Threading.CancellationToken>())
             .Returns(ci => realCtx.SaveChangesAsync(ci.Arg<System.Threading.CancellationToken>()));

        var handler = new CreateOfferCommandHandler(dbSub);

        var cmd = new CreateOfferCommand("titolo", "desc", 10m, 1);

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().BeGreaterThan(0);

        var added = await realCtx.Offers.FindAsync(id);
        added.Should().NotBeNull();
        added!.Title.Should().Be("titolo");

        await dbSub.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}