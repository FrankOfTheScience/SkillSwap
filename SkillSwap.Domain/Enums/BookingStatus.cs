using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Domain.Enums;

public enum BookingStatus
{
    Pending,
    Completed,
    Cancelled,
    Refunded
}