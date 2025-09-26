using FluentAssertions;
using NSubstitute;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Queries;
using SkillSwap.Domain;
using SkillSwap.Tests.Common;

namespace SkillSwap.Tests.Handlers;
public class GetOfferQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_PagedOffersResult()
    {
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
        ctx.Offers.AddRange(new Offer { Title = "a", Description = "d", Price = 1, CreatedBy = userId1 },
                            new Offer { Title = "b", Description = "d2", Price = 2, CreatedBy = userId2 });
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);

        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        var query = new GetOffersQuery(
            Page: 1,
            PageSize: 10,
            UserId: userId1.ToString()
        );

        var result = await handler.Handle(query, CancellationToken.None);

        // Test the PagedOffersResult structure
        result.Should().NotBeNull();
        result.Offers.Should().HaveCount(2);
        result.Offers.Select(x => x.Title).Should().Contain(new[] { "a", "b" });
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }
}