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

    public CreateBookingCommandHandler(IApplicationDbContext context, IValidator<CreateBookingCommand> validator)
    {
        _context = context;
        _validator = validator;
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
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        return booking.Id;
    }
}