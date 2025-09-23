using FluentAssertions;
using NSubstitute;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Queries;
using SkillSwap.Domain;
using SkillSwap.Tests.Common;

namespace SkillSwap.Tests.Handlers;
public class GetOffersQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_AllOffersMapped()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        ctx.Offers.AddRange(new Offer { Title = "a", Description = "d", Price = 1, CreatedBy = 1 },
                            new Offer { Title = "b", Description = "d2", Price = 2, CreatedBy = 2 });
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);

        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        var result = await handler.Handle(new GetOffersQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(x => x.Title).Should().Contain(new[] { "a", "b" });
    }
}