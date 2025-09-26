using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Infrastructure;

namespace SkillSwap.Tests.Infrastructure;

public class DbContextOptionsFactoryTests
{
    [Fact]
    public void Create_WithValidConnectionString_ShouldReturnDbContextOptions()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=testdb;User Id=test;Password=test;";

        // Act
        var options = DbContextOptionsFactory.Create(connectionString);

        // Assert
        options.Should().NotBeNull();
        options.Should().BeOfType<DbContextOptions<SkillSwapDbContext>>();
    }

    [Fact]
    public void Create_WithEmptyConnectionString_ShouldReturnDbContextOptions()
    {
        // Arrange
        var connectionString = "";

        // Act
        var options = DbContextOptionsFactory.Create(connectionString);

        // Assert
        options.Should().NotBeNull();
        options.Should().BeOfType<DbContextOptions<SkillSwapDbContext>>();
    }



    [Fact]
    public void Create_WithValidConnectionString_ShouldConfigureNpgsql()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=testdb;User Id=test;Password=test;";

        // Act
        var options = DbContextOptionsFactory.Create(connectionString);

        // Assert
        options.Should().NotBeNull();
        
        // Verify that Npgsql is configured by checking the extensions
        var extensions = options.Extensions;
        extensions.Should().NotBeEmpty();
        extensions.Should().Contain(ext => ext.GetType().Name.Contains("Npgsql"));
    }

    [Fact]
    public void Create_WithDifferentConnectionStrings_ShouldReturnDifferentOptions()
    {
        // Arrange
        var connectionString1 = "Server=localhost;Database=testdb1;User Id=test;Password=test;";
        var connectionString2 = "Server=localhost;Database=testdb2;User Id=test;Password=test;";

        // Act
        var options1 = DbContextOptionsFactory.Create(connectionString1);
        var options2 = DbContextOptionsFactory.Create(connectionString2);

        // Assert
        options1.Should().NotBeNull();
        options2.Should().NotBeNull();
        options1.Should().NotBeSameAs(options2);
    }

    [Fact]
    public void Create_CanBeUsedToCreateDbContext()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=testdb;User Id=test;Password=test;";

        // Act
        var options = DbContextOptionsFactory.Create(connectionString);
        
        // Assert - Should be able to create a DbContext with these options
        var contextCreation = () => new SkillSwapDbContext(options);
        contextCreation.Should().NotThrow();

        using var context = new SkillSwapDbContext(options);
        context.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithSpecialCharactersInConnectionString_ShouldWork()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=test_db-123;User Id=test@domain;Password=p@ssw0rd!;";

        // Act
        var options = DbContextOptionsFactory.Create(connectionString);

        // Assert
        options.Should().NotBeNull();
        options.Should().BeOfType<DbContextOptions<SkillSwapDbContext>>();
    }

    [Fact]
    public void Create_MultipleCalls_WithSameConnectionString_ShouldReturnEquivalentOptions()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=testdb;User Id=test;Password=test;";

        // Act
        var options1 = DbContextOptionsFactory.Create(connectionString);
        var options2 = DbContextOptionsFactory.Create(connectionString);

        // Assert
        options1.Should().NotBeNull();
        options2.Should().NotBeNull();
        // They should have the same configuration even if they are different instances
        options1.GetType().Should().Be(options2.GetType());
    }
}