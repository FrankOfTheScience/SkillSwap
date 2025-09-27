using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SkillSwap.Application.Bookings.Commands;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain;
using SkillSwap.Infrastructure;

namespace SkillSwap.Tests.Bookings.Commands;

public class CreateBookingCommandHandlerTests : IDisposable
{
    private readonly SkillSwapDbContext _context;
    
    public CreateBookingCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SkillSwapDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SkillSwapDbContext(options);
        
        // Seed test data
        var testUser = new User { Id = Guid.NewGuid(), Email = "test@example.com", DisplayName = "Test User", PasswordHash = "hash" };
        var testOffer = new Offer { Id = 1, Price = 100.00m, Title = "Test Offer", Description = "Test Description", CreatedBy = testUser.Id };
        
        _context.Users.Add(testUser);
        _context.Offers.Add(testOffer);
        _context.SaveChanges();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }

    private static IValidator<CreateBookingCommand> CreateMockValidator(bool isValid = true)
    {
        var validator = Substitute.For<IValidator<CreateBookingCommand>>();
        var validationResult = isValid 
            ? new ValidationResult() 
            : new ValidationResult(new[] { new ValidationFailure("OfferId", "Invalid offer") });
        
        validator.ValidateAsync(Arg.Any<CreateBookingCommand>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);
        
        return validator;
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateBooking()
    {
        // Arrange
        var validator = CreateMockValidator(true);
        var handler = new CreateBookingCommandHandler(_context, validator);
        
        var user = await _context.Users.FirstAsync();
        var command = new CreateBookingCommand(OfferId: 1, UserId: user.Id);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().BeGreaterThan(0);
        
        var createdBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == result);
        createdBooking.Should().NotBeNull();
        createdBooking!.OfferId.Should().Be(1);
        createdBooking.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_InvalidValidation_ShouldThrowValidationException()
    {
        // Arrange
        var validator = CreateMockValidator(false);
        var handler = new CreateBookingCommandHandler(_context, validator);
        
        var command = new CreateBookingCommand(OfferId: 1, UserId: Guid.NewGuid());
        
        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task Handle_NonExistentOffer_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var validator = CreateMockValidator(true);
        var handler = new CreateBookingCommandHandler(_context, validator);
        
        var user = await _context.Users.FirstAsync();
        var command = new CreateBookingCommand(OfferId: 999, UserId: user.Id); // Non-existent offer
        
        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Offer with ID 999 not found*");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var validator = CreateMockValidator(true);
        var handler = new CreateBookingCommandHandler(_context, validator);
        
        var nonExistentUserId = Guid.NewGuid();
        var command = new CreateBookingCommand(OfferId: 1, UserId: nonExistentUserId);
        
        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*User with ID {nonExistentUserId} not found*");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCalculateCommissionCorrectly()
    {
        // Arrange
        var validator = CreateMockValidator(true);
        var handler = new CreateBookingCommandHandler(_context, validator);
        
        var user = await _context.Users.FirstAsync();
        var command = new CreateBookingCommand(OfferId: 1, UserId: user.Id);
        
        // Act
        var bookingId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var createdBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        createdBooking.Should().NotBeNull();
        createdBooking!.Amount.Should().Be(100.00m);
        createdBooking.CommissionAmount.Should().Be(10.00m); // 10% commission
        createdBooking.Status.Should().Be(BookingStatus.Pending);
        createdBooking.OfferId.Should().Be(1);
        createdBooking.UserId.Should().Be(user.Id);
    }
}