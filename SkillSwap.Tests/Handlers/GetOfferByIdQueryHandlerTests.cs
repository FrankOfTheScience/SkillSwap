using FluentAssertions;
using NSubstitute;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Queries;
using SkillSwap.Domain;
using SkillSwap.Tests.Common;

namespace SkillSwap.Tests.Handlers;

public class GetOfferByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnOfferDto_WhenFound()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var offer = new Offer { Title = "uno", Description = "x", Price = 5, CreatedBy = Guid.NewGuid() };
        ctx.Offers.Add(offer);
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);

        var mapper = TestHelper.CreateMapper();
        var handler = new GetOfferByIdQueryHandler(dbSub, mapper);

        var result = await handler.Handle(new GetOfferByIdQuery(offer.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(offer.Id);
        result.Title.Should().Be("uno");
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_WhenNotFound()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);

        var mapper = TestHelper.CreateMapper();
        var handler = new GetOfferByIdQueryHandler(dbSub, mapper);

        var result = await handler.Handle(new GetOfferByIdQuery(Guid.NewGuid()), CancellationToken.None);
        result.Should().BeNull();
    }
}