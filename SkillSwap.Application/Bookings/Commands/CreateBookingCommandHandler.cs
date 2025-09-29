using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain;

namespace SkillSwap.Application.Bookings.Commands;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IValidator<CreateBookingCommand> _validator;
    private readonly IDashboardNotificationService _notificationService;

    public CreateBookingCommandHandler(
        IApplicationDbContext context, 
        IValidator<CreateBookingCommand> validator,
        IDashboardNotificationService notificationService)
    {
        _context = context;
        _validator = validator;
        _notificationService = notificationService;
    }

    public async Task<int> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new FluentValidation.ValidationException(validation.Errors);
        }

        // Get the offer to calculate amounts
        var offer = await _context.Offers
            .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken);

        if (offer == null)
        {
            throw new InvalidOperationException($"Offer with ID {request.OfferId} not found.");
        }

        // Verify user exists
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found.");
        }

        // Calculate commission using constant
        var commissionAmount = offer.Price * CommissionConstants.DefaultCommissionRate;

        var booking = new Booking
        {
            OfferId = request.OfferId,
            UserId = request.UserId,
            Amount = offer.Price,
            CommissionAmount = commissionAmount,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            // Scheduling information
            ScheduledDateTime = request.ScheduledDateTime ?? DateTime.UtcNow.AddHours(24), // Default to tomorrow if not specified
            DurationInMinutes = request.DurationInMinutes ?? offer.DurationInMinutes,
            Location = request.Location ?? offer.Location,
            IsOnline = request.IsOnline ?? offer.IsOnline,
            CustomerNotes = request.CustomerNotes
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        // Notify dashboard clients about the new booking
        try
        {
            await _notificationService.NotifyBookingCreated(booking);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the booking creation
            // In a real application, you might want to use a logger here
            Console.WriteLine($"Failed to send dashboard notification: {ex.Message}");
        }

        return booking.Id;
    }
}