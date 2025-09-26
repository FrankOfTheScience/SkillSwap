using FluentAssertions;
using SkillSwap.Domain;

namespace SkillSwap.Tests.Domain;

public class UserTests
{
    [Fact]
    public void User_ShouldInitializeWithDefaults_WhenCreated()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().Be(Guid.Empty);
        user.Email.Should().BeNull();
        user.DisplayName.Should().BeNull();
        user.PasswordHash.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.Roles.Should().NotBeNull();
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void User_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "test@example.com";
        var displayName = "Test User";
        var passwordHash = "hashed_password";
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var roles = new List<string> { "User", "Admin" };

        // Act
        var user = new User
        {
            Id = id,
            Email = email,
            DisplayName = displayName,
            PasswordHash = passwordHash,
            CreatedAt = createdAt,
            Roles = roles
        };

        // Assert
        user.Id.Should().Be(id);
        user.Email.Should().Be(email);
        user.DisplayName.Should().Be(displayName);
        user.PasswordHash.Should().Be(passwordHash);
        user.CreatedAt.Should().Be(createdAt);
        user.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void User_ShouldAllowRoleManipulation()
    {
        // Arrange
        var user = new User();

        // Act
        user.Roles.Add("User");
        user.Roles.Add("Admin");

        // Assert
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().Contain("User");
        user.Roles.Should().Contain("Admin");
    }

    [Fact]
    public void User_ShouldAllowRoleRemoval()
    {
        // Arrange
        var user = new User
        {
            Roles = new List<string> { "User", "Admin", "Moderator" }
        };

        // Act
        user.Roles.Remove("Admin");

        // Assert
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().NotContain("Admin");
        user.Roles.Should().Contain("User");
        user.Roles.Should().Contain("Moderator");
    }

    [Theory]
    [InlineData("user@domain.com", "John Doe")]
    [InlineData("test.email+tag@subdomain.example.org", "Jane Smith-Wilson")]
    [InlineData("simple@test.co", "Bob")]
    public void User_ShouldHandleVariousEmailDisplayNameCombinations(string email, string displayName)
    {
        // Act
        var user = new User
        {
            Email = email,
            DisplayName = displayName
        };

        // Assert
        user.Email.Should().Be(email);
        user.DisplayName.Should().Be(displayName);
    }

    [Fact]
    public void User_CreatedAt_ShouldDefaultToCurrentUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var user = new User();
        var afterCreation = DateTime.UtcNow;

        // Assert
        user.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        user.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void User_ShouldAllowEmptyRolesList()
    {
        // Act
        var user = new User
        {
            Roles = new List<string>()
        };

        // Assert
        user.Roles.Should().BeEmpty();
    }

    [Fact]
    public void User_ShouldAllowDuplicateRoles()
    {
        // Arrange
        var user = new User();

        // Act
        user.Roles.Add("User");
        user.Roles.Add("User"); // Duplicate

        // Assert
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().AllBe("User");
    }
}