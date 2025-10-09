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

        ctx.Offers.AddRange(new Offer { Id = Guid.NewGuid(), Title = "a", Description = "d", Price = 1, CreatedBy = userId1 },
                            new Offer { Id = Guid.NewGuid(), Title = "b", Description = "d2", Price = 2, CreatedBy = userId2 });
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

    [Fact]
    public async Task Handle_WithSearchFilter_FiltersOffersByTitleAndDescription()
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();

        ctx.Offers.AddRange(
            new Offer { Id = Guid.NewGuid(), Title = "Guitar Lessons", Description = "Learn to play guitar", Price = 50, CreatedBy = userId },
            new Offer { Id = Guid.NewGuid(), Title = "Piano Tutorials", Description = "Piano for beginners", Price = 40, CreatedBy = userId },
            new Offer { Id = Guid.NewGuid(), Title = "Math Tutoring", Description = "Advanced mathematics help", Price = 60, CreatedBy = userId }
        );
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act - Search for "guitar" (should match title)
        var query = new GetOffersQuery(Search: "guitar");
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(1);
        result.Offers[0].Title.Should().Be("Guitar Lessons");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithMaxBudgetFilter_FiltersOffersByPrice()
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();

        ctx.Offers.AddRange(
            new Offer { Id = Guid.NewGuid(), Title = "Cheap Service", Description = "Budget option", Price = 25, CreatedBy = userId },
            new Offer { Id = Guid.NewGuid(), Title = "Premium Service", Description = "High quality", Price = 100, CreatedBy = userId },
            new Offer { Id = Guid.NewGuid(), Title = "Mid-range Service", Description = "Good value", Price = 50, CreatedBy = userId }
        );
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act - Filter by max budget of 60
        var query = new GetOffersQuery(MaxBudget: 60);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(2);
        result.Offers.All(o => o.Price <= 60).Should().BeTrue();
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithShowOnlyMyOffers_True_ShowsOnlyUserOffers()
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        ctx.Offers.AddRange(
            new Offer { Id = Guid.NewGuid(), Title = "My Offer 1", Description = "User 1 offer", Price = 50, CreatedBy = userId1 },
            new Offer { Id = Guid.NewGuid(), Title = "My Offer 2", Description = "User 1 offer 2", Price = 40, CreatedBy = userId1 },
            new Offer { Id = Guid.NewGuid(), Title = "Other Offer", Description = "User 2 offer", Price = 60, CreatedBy = userId2 }
        );
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act
        var query = new GetOffersQuery(ShowOnlyMyOffers: true, UserId: userId1.ToString());
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(2);
        result.Offers.Should().AllSatisfy(o => o.Title.Should().StartWith("My Offer"));
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithShowOnlyMyOffers_False_ExcludesUserOffers()
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        ctx.Offers.AddRange(
            new Offer { Id = Guid.NewGuid(), Title = "My Offer", Description = "User 1 offer", Price = 50, CreatedBy = userId1 },
            new Offer { Id = Guid.NewGuid(), Title = "Other Offer 1", Description = "User 2 offer 1", Price = 40, CreatedBy = userId2 },
            new Offer { Id = Guid.NewGuid(), Title = "Other Offer 2", Description = "User 2 offer 2", Price = 60, CreatedBy = userId2 }
        );
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act
        var query = new GetOffersQuery(ShowOnlyMyOffers: false, UserId: userId1.ToString());
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(2);
        result.Offers.Should().AllSatisfy(o => o.Title.Should().StartWith("Other Offer"));
        result.TotalCount.Should().Be(2);
    }

    [Theory]
    [InlineData("title", false, "Guitar Lessons")]   // Title ascending: Guitar -> Math -> Piano  
    [InlineData("title", true, "Piano Tutorials")]   // Title descending: Piano -> Math -> Guitar
    [InlineData("price", false, "Math Tutoring")]    // Price ascending: 40 -> 60 -> 80
    [InlineData("price", true, "Piano Tutorials")]   // Price descending: 80 -> 60 -> 40
    [InlineData("createdat", false, "Guitar Lessons")]// Created ascending: insertion order
    [InlineData("created", false, "Guitar Lessons")] // Created ascending: insertion order
    [InlineData("", false, "Guitar Lessons")]        // Default (created ascending)
    [InlineData(null, true, "Math Tutoring")]        // Default descending: newest first (last inserted)
    public async Task Handle_WithDifferentSortOptions_SortsCorrectly(string? sortBy, bool descending, string expectedFirst)
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();

        // Add offers in specific order to test sorting
        var baseDate = DateTime.UtcNow.AddDays(-3); // Ensure chronological order
        var offers = new[]
        {
            new Offer { Id = Guid.NewGuid(), Title = "Guitar Lessons", Description = "Learn guitar", Price = 60, CreatedBy = userId, CreatedAt = baseDate.AddHours(0) }, // First created
            new Offer { Id = Guid.NewGuid(), Title = "Piano Tutorials", Description = "Learn piano", Price = 80, CreatedBy = userId, CreatedAt = baseDate.AddHours(1) }, // Second created  
            new Offer { Id = Guid.NewGuid(), Title = "Math Tutoring", Description = "Learn math", Price = 40, CreatedBy = userId, CreatedAt = baseDate.AddHours(2) }    // Third created
        };

        ctx.Offers.AddRange(offers);
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act
        var query = new GetOffersQuery(SortBy: sortBy, SortDescending: descending);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(3);
        result.Offers[0].Title.Should().Be(expectedFirst);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPageAndTotalPages()
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();

        // Create 15 offers for pagination testing
        var offers = Enumerable.Range(1, 15)
            .Select(i => new Offer { Id = Guid.NewGuid(), Title = $"Offer {i}", Description = $"Description {i}", Price = i * 10, CreatedBy = userId })
            .ToArray();

        ctx.Offers.AddRange(offers);
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act - Get page 2 with page size 5
        var query = new GetOffersQuery(Page: 2, PageSize: 5);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithCombinedFilters_AppliesAllFiltersCorrectly()
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        ctx.Offers.AddRange(
            new Offer { Id = Guid.NewGuid(), Title = "Guitar Lessons", Description = "Learn guitar basics", Price = 50, CreatedBy = userId1 },
            new Offer { Id = Guid.NewGuid(), Title = "Advanced Guitar", Description = "Advanced techniques", Price = 100, CreatedBy = userId1 },
            new Offer { Id = Guid.NewGuid(), Title = "Piano Lessons", Description = "Learn piano", Price = 40, CreatedBy = userId2 },
            new Offer { Id = Guid.NewGuid(), Title = "Guitar Repair", Description = "Fix your guitar", Price = 30, CreatedBy = userId2 }
        );
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act - Search for "guitar", max budget 60, exclude user1's offers, sort by price ascending
        var query = new GetOffersQuery(
            Search: "guitar",
            MaxBudget: 60,
            ShowOnlyMyOffers: false,
            UserId: userId1.ToString(),
            SortBy: "price",
            SortDescending: false
        );
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(1);
        result.Offers[0].Title.Should().Be("Guitar Repair");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithEmptySearch_DoesNotFilterBySearch()
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();

        ctx.Offers.AddRange(
            new Offer { Id = Guid.NewGuid(), Title = "Offer 1", Description = "Description 1", Price = 50, CreatedBy = userId },
            new Offer { Id = Guid.NewGuid(), Title = "Offer 2", Description = "Description 2", Price = 40, CreatedBy = userId }
        );
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act - Empty search should return all offers
        var query = new GetOffersQuery(Search: "");
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithNullUserId_IgnoresOwnershipFilter()
    {
        // Arrange
        using var ctx = TestHelper.CreateInMemoryDbContext();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        ctx.Offers.AddRange(
            new Offer { Id = Guid.NewGuid(), Title = "Offer 1", Description = "Description 1", Price = 50, CreatedBy = userId1 },
            new Offer { Id = Guid.NewGuid(), Title = "Offer 2", Description = "Description 2", Price = 40, CreatedBy = userId2 }
        );
        await ctx.SaveChangesAsync();

        var dbSub = Substitute.For<IApplicationDbContext>();
        dbSub.Offers.Returns(ctx.Offers);
        var mapper = TestHelper.CreateMapper();
        var handler = new GetOffersQueryHandler(dbSub, mapper);

        // Act - ShowOnlyMyOffers=true but UserId=null should return all offers
        var query = new GetOffersQuery(ShowOnlyMyOffers: true, UserId: null);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Offers.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
}