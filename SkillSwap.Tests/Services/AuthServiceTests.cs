using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SkillSwap.Application.Services;
using SkillSwap.Domain;

namespace SkillSwap.Tests.Services;

public class AuthServiceTests
{
    private static User GetUser(List<string>? roles = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            Roles = roles ?? new List<string> { "Admin", "User" }
        };
    }

    [Theory]
    [InlineData("Jwt:Key")]
    [InlineData("Jwt:Issuer")]
    [InlineData("Jwt:Audience")]
    public void GenerateJwt_MissingConfigKey_Throws(string missingKey)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "testkey",
            ["Jwt:Issuer"] = "testissuer",
            ["Jwt:Audience"] = "testaudience"
        };
        dict.Remove(missingKey);

        var config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        var user = GetUser();
        var service = new AuthService(config);

        Action act = () => service.GenerateJwt(user);

        act.Should().Throw<Exception>();
    }
}