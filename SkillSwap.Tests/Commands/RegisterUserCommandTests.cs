using FluentAssertions;
using SkillSwap.Application.Users.Commands;

namespace SkillSwap.Tests.Commands;

public class RegisterUserCommandTests
{
    [Fact]
    public void RegisterUserCommand_ShouldInitializeProperties_WhenCreated()
    {
        // Arrange
        var email = "test@example.com";
        var displayName = "Test User";
        var password = "TestPassword123";

        // Act
        var command = new RegisterUserCommand(email, displayName, password);

        // Assert
        command.Email.Should().Be(email);
        command.DisplayName.Should().Be(displayName);
        command.Password.Should().Be(password);
    }

    [Fact]
    public void RegisterUserCommand_ShouldBeImmutable_WhenCreated()
    {
        // Arrange
        var email = "immutable@example.com";
        var displayName = "Immutable User";
        var password = "ImmutablePassword123";

        // Act
        var command = new RegisterUserCommand(email, displayName, password);

        // Assert
        // Record types are immutable by default - properties cannot be changed after creation
        command.Email.Should().Be(email);
        command.DisplayName.Should().Be(displayName);
        command.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("user@domain.com", "John Doe", "SecurePass123")]
    [InlineData("test.email+tag@example.org", "Jane Smith", "AnotherPass456")]
    [InlineData("simple@test.co", "Bob", "Pass")]
    public void RegisterUserCommand_ShouldHandleVariousInputs(string email, string displayName, string password)
    {
        // Act
        var command = new RegisterUserCommand(email, displayName, password);

        // Assert
        command.Email.Should().Be(email);
        command.DisplayName.Should().Be(displayName);
        command.Password.Should().Be(password);
    }

    [Fact]
    public void RegisterUserCommand_ShouldImplementEquality_ForSameValues()
    {
        // Arrange
        var email = "equality@test.com";
        var displayName = "Equality Test";
        var password = "EqualityPass123";

        // Act
        var command1 = new RegisterUserCommand(email, displayName, password);
        var command2 = new RegisterUserCommand(email, displayName, password);

        // Assert
        command1.Should().Be(command2);
        command1.GetHashCode().Should().Be(command2.GetHashCode());
    }

    [Fact]
    public void RegisterUserCommand_ShouldNotBeEqual_ForDifferentValues()
    {
        // Arrange
        var command1 = new RegisterUserCommand("user1@test.com", "User One", "Pass123");
        var command2 = new RegisterUserCommand("user2@test.com", "User Two", "Pass456");

        // Assert
        command1.Should().NotBe(command2);
    }
}