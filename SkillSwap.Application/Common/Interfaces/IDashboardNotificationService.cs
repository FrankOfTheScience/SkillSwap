using SkillSwap.Domain;

namespace SkillSwap.Application.Common.Interfaces;

/// <summary>
/// Interface for sending real-time notifications to dashboard clients
/// </summary>
public interface IDashboardNotificationService
{
    /// <summary>
    /// Notify dashboard clients when a new booking is created
    /// </summary>
    Task NotifyBookingCreated(Booking booking);

    /// <summary>
    /// Notify dashboard clients when a booking is updated
    /// </summary>
    Task NotifyBookingUpdated(Booking booking);

    /// <summary>
    /// Notify dashboard clients when a new user is registered
    /// </summary>
    Task NotifyUserRegistered(User user);

    /// <summary>
    /// Send updated stats to a specific user's dashboard
    /// </summary>
    Task SendUserStatsUpdate(Guid userId);

    /// <summary>
    /// Send updated stats to admin dashboards
    /// </summary>
    Task SendAdminStatsUpdate();
}