using AutoMapper;
using FluentAssertions;
using SkillSwap.Application.Users.Commands;
using SkillSwap.Application.Users.Mappings;
using SkillSwap.Domain;
using System;
using System.Collections.Generic;
using Xunit;

namespace SkillSwap.Tests.Mappings;

public class UserProfileTests
{
    private readonly IMapper _mapper;

    public UserProfileTests()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Should_MapRegisterUserCommandToUser_WithCorrectProperties()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            DisplayName: "Test User",
            Password: "TestPassword123"
        );

        // Act
        var user = _mapper.Map<User>(command);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be("test@example.com");
        user.DisplayName.Should().Be("Test User");
        user.Id.Should().NotBe(Guid.Empty);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.Roles.Should().NotBeNull();
        user.Roles.Should().ContainSingle("User");
    }

    [Fact]
    public void Should_GenerateNewGuid_ForEachMapping()
    {
        // Arrange
        var command1 = new RegisterUserCommand(
            Email: "user1@example.com",
            DisplayName: "User One",
            Password: "Password123"
        );
        
        var command2 = new RegisterUserCommand(
            Email: "user2@example.com",
            DisplayName: "User Two",
            Password: "Password456"
        );

        // Act
        var user1 = _mapper.Map<User>(command1);
        var user2 = _mapper.Map<User>(command2);

        // Assert
        user1.Id.Should().NotBe(user2.Id);
        user1.Id.Should().NotBe(Guid.Empty);
        user2.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Should_IgnorePasswordHash_InMapping()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            DisplayName: "Test User",
            Password: "TestPassword123"
        );

        // Act
        var user = _mapper.Map<User>(command);

        // Assert
        user.PasswordHash.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Should_SetDefaultUserRole()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            DisplayName: "Test User",
            Password: "TestPassword123"
        );

        // Act
        var user = _mapper.Map<User>(command);

        // Assert
        user.Roles.Should().HaveCount(1);
        user.Roles.Should().Contain("User");
    }

    [Fact]
    public void Should_SetCreatedAtToCurrentTime()
    {
        // Arrange
        var beforeMapping = DateTime.UtcNow;
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            DisplayName: "Test User",
            Password: "TestPassword123"
        );

        // Act
        var user = _mapper.Map<User>(command);
        var afterMapping = DateTime.UtcNow;

        // Assert
        user.CreatedAt.Should().BeAfter(beforeMapping.AddSeconds(-1));
        user.CreatedAt.Should().BeBefore(afterMapping.AddSeconds(1));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("test@example.com", "")]
    [InlineData("", "Test User")]
    [InlineData("long.email.address@verylongdomainname.com", "Very Long Display Name That Exceeds Normal Length")]
    public void Should_MapDifferentEmailAndDisplayNameCombinations(string email, string displayName)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: email,
            DisplayName: displayName,
            Password: "TestPassword123"
        );

        // Act
        var user = _mapper.Map<User>(command);

        // Assert
        user.Email.Should().Be(email);
        user.DisplayName.Should().Be(displayName);
        user.Id.Should().NotBe(Guid.Empty);
        user.Roles.Should().ContainSingle("User");
    }

    [Fact]
    public void Configuration_Should_BeValid()
    {
        // Act & Assert
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
        configuration.AssertConfigurationIsValid();
    }
}