using MediatR;

namespace SkillSwap.Application.Bookings.Commands;

public class CompleteBookingCommand : IRequest<bool>
{
    public int BookingId { get; set; }
    public string StripeCheckoutSessionId { get; set; } = string.Empty;
    public string? StripePaymentIntentId { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}