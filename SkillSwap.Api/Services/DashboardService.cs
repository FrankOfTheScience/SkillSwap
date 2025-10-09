using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Api.Hubs;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Infrastructure;
using SkillSwap.Domain;

namespace SkillSwap.Api.Services;

public interface IDashboardService
{
    Task SendUserStatsUpdate(Guid userId);
    Task SendAdminStatsUpdate();
    Task NotifyBookingCreated(Booking booking);
    Task NotifyBookingUpdated(Booking booking);
    Task NotifyUserRegistered(User user);
}

public class DashboardService : IDashboardService, IDashboardNotificationService
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly SkillSwapDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IHubContext<DashboardHub> hubContext,
        SkillSwapDbContext context,
        ILogger<DashboardService> logger)
    {
        _hubContext = hubContext;
        _context = context;
        _logger = logger;
    }

    public async Task SendUserStatsUpdate(Guid userId)
    {
        try
        {
            var userStats = await GetUserStats(userId);
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("UserStatsUpdate", userStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending user stats update for user {UserId}", userId);
        }
    }

    public async Task SendAdminStatsUpdate()
    {
        try
        {
            var adminStats = await GetAdminStats();
            await _hubContext.Clients.Group("AdminDashboard")
                .SendAsync("AdminStatsUpdate", adminStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin stats update");
        }
    }

    public async Task NotifyBookingCreated(Booking booking)
    {
        try
        {
            // Notify the user who made the booking
            await SendUserStatsUpdate(booking.UserId);

            // Notify the offer creator
            var offer = await _context.Offers.FindAsync(booking.OfferId);
            if (offer != null)
            {
                await SendUserStatsUpdate(offer.CreatedBy);
            }

            // Notify admins
            await SendAdminStatsUpdate();

            // Send real-time booking notification
            await _hubContext.Clients.Group("AdminDashboard")
                .SendAsync("NewBookingNotification", new
                {
                    booking.Id,
                    booking.Amount,
                    booking.CreatedAt,
                    OfferTitle = offer?.Title,
                    UserId = booking.UserId
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying booking created for booking {BookingId}", booking.Id);
        }
    }

    public async Task NotifyBookingUpdated(Booking booking)
    {
        try
        {
            // Notify the user who made the booking
            await SendUserStatsUpdate(booking.UserId);

            // Notify the offer creator
            var offer = await _context.Offers.FindAsync(booking.OfferId);
            if (offer != null)
            {
                await SendUserStatsUpdate(offer.CreatedBy);
            }

            // Notify admins
            await SendAdminStatsUpdate();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying booking updated for booking {BookingId}", booking.Id);
        }
    }

    public async Task NotifyUserRegistered(User user)
    {
        try
        {
            // Notify admins of new user registration
            await SendAdminStatsUpdate();

            await _hubContext.Clients.Group("AdminDashboard")
                .SendAsync("NewUserNotification", new
                {
                    user.Id,
                    user.DisplayName,
                    user.Email,
                    user.CreatedAt
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user registered for user {UserId}", user.Id);
        }
    }

    private async Task<object> GetUserStats(Guid userId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);

        // Get user's bookings as customer
        var customerBookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.Offer)
            .ToListAsync();

        // Get user's bookings as provider (through their offers)
        var providerBookings = await _context.Bookings
            .Where(b => b.Offer != null && b.Offer.CreatedBy == userId)
            .Include(b => b.Offer)
            .ToListAsync();

        // Get user's offers
        var offers = await _context.Offers
            .Where(o => o.CreatedBy == userId)
            .ToListAsync();

        // Get profile completion
        var user = await _context.Users.FindAsync(userId);
        var profileCompletion = user?.ProfileCompletionPercentage ?? 0;

        // Calculate earnings (as a provider)
        var totalEarnings = providerBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.Amount - b.CommissionAmount);

        var monthlyEarnings = providerBookings
            .Where(b => b.Status == BookingStatus.Completed && b.CreatedAt >= startOfMonth)
            .Sum(b => b.Amount - b.CommissionAmount);

        // Calculate spending (as a customer)
        var totalSpending = customerBookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Sum(b => b.Amount);

        var monthlySpending = customerBookings
            .Where(b => b.Status == BookingStatus.Completed && b.CreatedAt >= startOfMonth)
            .Sum(b => b.Amount);

        // Recent activity (last 10 bookings)
        var recentActivity = customerBookings
            .Concat(providerBookings)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(b => new
            {
                b.Id,
                b.Status,
                b.Amount,
                b.CreatedAt,
                b.ScheduledDateTime,
                OfferTitle = b.Offer?.Title,
                Type = customerBookings.Contains(b) ? "Customer" : "Provider"
            })
            .ToList();

        // Upcoming appointments
        var upcomingAppointments = customerBookings
            .Concat(providerBookings)
            .Where(b => b.ScheduledDateTime > now && b.Status == BookingStatus.Pending)
            .OrderBy(b => b.ScheduledDateTime)
            .Take(5)
            .Select(b => new
            {
                b.Id,
                b.ScheduledDateTime,
                b.DurationInMinutes,
                b.Location,
                b.IsOnline,
                OfferTitle = b.Offer?.Title,
                Type = customerBookings.Contains(b) ? "Customer" : "Provider"
            })
            .ToList();

        return new
        {
            ProfileCompletion = profileCompletion,
            TotalOffers = offers.Count,
            ActiveOffers = offers.Count(o => o.IsActive),
            
            // Earnings (as provider)
            TotalEarnings = totalEarnings,
            MonthlyEarnings = monthlyEarnings,
            
            // Spending (as customer)  
            TotalSpending = totalSpending,
            MonthlySpending = monthlySpending,
            
            // Booking stats
            TotalBookingsAsCustomer = customerBookings.Count,
            TotalBookingsAsProvider = providerBookings.Count,
            CompletedBookingsAsCustomer = customerBookings.Count(b => b.Status == BookingStatus.Completed),
            CompletedBookingsAsProvider = providerBookings.Count(b => b.Status == BookingStatus.Completed),
            
            // Recent activity
            RecentActivity = recentActivity,
            UpcomingAppointments = upcomingAppointments,
            
            LastUpdated = now
        };
    }

    private async Task<object> GetAdminStats()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
        var startOfDay = now.Date;

        // User statistics
        var totalUsers = await _context.Users.CountAsync();
        var newUsersThisMonth = await _context.Users
            .CountAsync(u => u.CreatedAt >= startOfMonth);
        var newUsersThisWeek = await _context.Users
            .CountAsync(u => u.CreatedAt >= startOfWeek);
        var newUsersToday = await _context.Users
            .CountAsync(u => u.CreatedAt >= startOfDay);

        // Offer statistics
        var totalOffers = await _context.Offers.CountAsync();
        var activeOffers = await _context.Offers.CountAsync(o => o.IsActive);
        var newOffersThisMonth = await _context.Offers
            .CountAsync(o => o.CreatedAt >= startOfMonth);

        // Booking statistics
        var totalBookings = await _context.Bookings.CountAsync();
        var completedBookings = await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Completed);
        var pendingBookings = await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Pending);
        var bookingsThisMonth = await _context.Bookings
            .CountAsync(b => b.CreatedAt >= startOfMonth);
        var bookingsThisWeek = await _context.Bookings
            .CountAsync(b => b.CreatedAt >= startOfWeek);
        var bookingsToday = await _context.Bookings
            .CountAsync(b => b.CreatedAt >= startOfDay);

        // Revenue statistics
        var totalRevenue = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .SumAsync(b => b.Amount);
        var totalCommissions = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .SumAsync(b => b.CommissionAmount);
        var revenueThisMonth = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Completed && b.CreatedAt >= startOfMonth)
            .SumAsync(b => b.Amount);
        var commissionsThisMonth = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Completed && b.CreatedAt >= startOfMonth)
            .SumAsync(b => b.CommissionAmount);

        // Recent registrations
        var recentUsers = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(10)
            .Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.Email,
                u.CreatedAt,
                u.ProfileCompletionPercentage
            })
            .ToListAsync();

        // Recent bookings
        var recentBookings = await _context.Bookings
            .Include(b => b.Offer)
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(b => new
            {
                b.Id,
                b.Status,
                b.Amount,
                b.CreatedAt,
                b.ScheduledDateTime,
                OfferTitle = b.Offer!.Title,
                CustomerName = b.User!.DisplayName
            })
            .ToListAsync();

        // Daily booking chart data (last 30 days)
        var dailyBookings = await _context.Bookings
            .Where(b => b.CreatedAt >= now.AddDays(-30))
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                Revenue = g.Sum(b => b.Amount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return new
        {
            // User metrics
            TotalUsers = totalUsers,
            NewUsersThisMonth = newUsersThisMonth,
            NewUsersThisWeek = newUsersThisWeek,
            NewUsersToday = newUsersToday,
            
            // Offer metrics
            TotalOffers = totalOffers,
            ActiveOffers = activeOffers,
            NewOffersThisMonth = newOffersThisMonth,
            
            // Booking metrics
            TotalBookings = totalBookings,
            CompletedBookings = completedBookings,
            PendingBookings = pendingBookings,
            BookingsThisMonth = bookingsThisMonth,
            BookingsThisWeek = bookingsThisWeek,
            BookingsToday = bookingsToday,
            
            // Revenue metrics
            TotalRevenue = totalRevenue,
            TotalCommissions = totalCommissions,
            RevenueThisMonth = revenueThisMonth,
            CommissionsThisMonth = commissionsThisMonth,
            
            // Recent data
            RecentUsers = recentUsers,
            RecentBookings = recentBookings,
            DailyBookings = dailyBookings,
            
            LastUpdated = now
        };
    }
}