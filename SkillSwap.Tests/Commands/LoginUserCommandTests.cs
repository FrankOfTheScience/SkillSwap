using FluentAssertions;
using SkillSwap.Application.Users.Commands;

namespace SkillSwap.Tests.Commands;

public class LoginUserCommandTests
{
    [Fact]
    public void LoginUserCommand_ShouldInitializeProperties_WhenCreated()
    {
        // Arrange
        var email = "login@example.com";
        var password = "LoginPassword123";

        // Act
        var command = new LoginUserCommand(email, password);

        // Assert
        command.Email.Should().Be(email);
        command.Password.Should().Be(password);
    }

    [Fact]
    public void LoginUserCommand_ShouldBeImmutable_WhenCreated()
    {
        // Arrange
        var email = "immutable@login.com";
        var password = "ImmutableLogin123";

        // Act
        var command = new LoginUserCommand(email, password);

        // Assert
        // Record types are immutable - properties cannot be changed after creation
        command.Email.Should().Be(email);
        command.Password.Should().Be(password);
    }

    [Theory]
    [InlineData("test@domain.com", "SimplePass")]
    [InlineData("complex.email+tag@subdomain.example.org", "ComplexPassword123!@#")]
    [InlineData("short@a.co", "P")]
    public void LoginUserCommand_ShouldHandleVariousEmailPasswordCombinations(string email, string password)
    {
        // Act
        var command = new LoginUserCommand(email, password);

        // Assert
        command.Email.Should().Be(email);
        command.Password.Should().Be(password);
    }

    [Fact]
    public void LoginUserCommand_ShouldImplementEquality_ForSameCredentials()
    {
        // Arrange
        var email = "same@credentials.com";
        var password = "SamePassword123";

        // Act
        var command1 = new LoginUserCommand(email, password);
        var command2 = new LoginUserCommand(email, password);

        // Assert
        command1.Should().Be(command2);
        command1.GetHashCode().Should().Be(command2.GetHashCode());
    }

    [Fact]
    public void LoginUserCommand_ShouldNotBeEqual_ForDifferentCredentials()
    {
        // Arrange
        var command1 = new LoginUserCommand("user1@test.com", "Password123");
        var command2 = new LoginUserCommand("user2@test.com", "Password456");

        // Assert
        command1.Should().NotBe(command2);
    }

    [Fact]
    public void LoginUserCommand_ShouldNotBeEqual_ForDifferentEmails()
    {
        // Arrange
        var command1 = new LoginUserCommand("email1@test.com", "SamePassword");
        var command2 = new LoginUserCommand("email2@test.com", "SamePassword");

        // Assert
        command1.Should().NotBe(command2);
    }

    [Fact]
    public void LoginUserCommand_ShouldNotBeEqual_ForDifferentPasswords()
    {
        // Arrange
        var command1 = new LoginUserCommand("same@email.com", "Password1");
        var command2 = new LoginUserCommand("same@email.com", "Password2");

        // Assert
        command1.Should().NotBe(command2);
    }
}