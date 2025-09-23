using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Offers.Mappings;
using SkillSwap.Infrastructure;

namespace SkillSwap.Tests.Common;
public static class TestHelper
{
    public static SkillSwapDbContext CreateInMemoryDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<SkillSwapDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var ctx = new SkillSwapDbContext(options);
        return ctx;
    }

    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OfferProfile>(); 
        });
        return config.CreateMapper();
    }
}