using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwap.Api.Controllers;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain;
using SkillSwap.Domain.Enums;
using SkillSwap.Infrastructure;
using SkillSwap.Tests.Common;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SkillSwap.Tests.Controllers;

public class BookingControllerTests : IDisposable
{
    private readonly IMediator _mockMediator;
    private readonly ILogger<BookingController> _mockLogger;
    private readonly BookingController _controller;
    private readonly SkillSwapDbContext _dbContext;
    private readonly IApplicationDbContext _mockContext;

    public BookingControllerTests()
    {
        _mockMediator = Substitute.For<IMediator>();
        _mockLogger = Substitute.For<ILogger<BookingController>>();
        _dbContext = TestHelper.CreateInMemoryDbContext();
        _mockContext = Substitute.For<IApplicationDbContext>();

        // Use the real context for tests that need database access
        _mockContext.Bookings.Returns(_dbContext.Bookings);
        _controller = new BookingController(_dbContext, _mockMediator, _mockLogger); // Use real context
    }

    private void SetupControllerUser(Guid userId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };

        // Setup basic session mock for BookingSuccess tests
        var session = Substitute.For<ISession>();
        httpContext.Session = session;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    [Fact]
    public async Task BookingSuccess_WithValidSessionId_ReturnsRedirectToSuccessPage()
    {
        // Arrange
        var sessionId = "cs_test_12345";
        var user = new User
        {
            DisplayName = "John Doe",
            Email = "john@example.com",
            PasswordHash = "hashedPassword123"
        };
        var offer = new Offer
        {
            Title = "Guitar Lessons",
            Description = "Learn guitar basics",
            Price = 50.0m,
            CreatedBy = Guid.NewGuid()
        };

        _dbContext.Users.Add(user);
        _dbContext.Offers.Add(offer);
        await _dbContext.SaveChangesAsync();

        var booking = new Booking
        {
            Status = BookingStatus.Completed,
            Amount = 50.0m,
            CommissionAmount = 5.0m,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            CompletedAt = DateTime.UtcNow,
            StripeCheckoutSessionId = sessionId,
            OfferId = offer.Id,
            UserId = user.Id
        };

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();

        // Setup HttpContext with session
        var httpContext = new DefaultHttpContext();
        var session = Substitute.For<ISession>();
        httpContext.Session = session;
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _controller.BookingSuccess(sessionId);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult?.Url.Should().Be($"/booking/success/{booking.Id}");
    }

    [Fact]
    public async Task BookingSuccess_WithEmptySessionId_ReturnsRedirectToGenericSuccess()
    {
        // Setup HttpContext
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _controller.BookingSuccess("");

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult?.Url.Should().Be("/booking/success");
    }

    [Fact]
    public async Task BookingSuccess_WithNullSessionId_ReturnsRedirectToGenericSuccess()
    {
        // Setup HttpContext
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _controller.BookingSuccess(null!);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult?.Url.Should().Be("/booking/success");
    }

    [Fact]
    public async Task BookingSuccess_WithNonExistentSessionId_ReturnsRedirectToGenericSuccess()
    {
        // Arrange
        var sessionId = "cs_test_nonexistent";

        // Setup HttpContext
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await _controller.BookingSuccess(sessionId);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult?.Url.Should().Be("/booking/success");
    }

    [Fact]
    public async Task BookingCancel_WithValidSessionId_ReturnsOkWithBookingDetails()
    {
        // Arrange
        var sessionId = "cs_test_12345";
        var user = new User
        {
            DisplayName = "John Doe",
            Email = "john@example.com",
            PasswordHash = "hashedPassword123"
        };
        var offer = new Offer
        {
            Title = "Guitar Lessons",
            Description = "Learn guitar basics",
            Price = 50.0m,
            CreatedBy = Guid.NewGuid()
        };

        _dbContext.Users.Add(user);
        _dbContext.Offers.Add(offer);
        await _dbContext.SaveChangesAsync();

        var booking = new Booking
        {
            Status = BookingStatus.Cancelled,
            Amount = 50.0m,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            StripeCheckoutSessionId = sessionId,
            OfferId = offer.Id,
            UserId = user.Id
        };

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.BookingCancel(sessionId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value;

        response.Should().NotBeNull();
        response.GetType().GetProperty("bookingId")?.GetValue(response).Should().Be(booking.Id);
        response.GetType().GetProperty("status")?.GetValue(response).Should().Be("Cancelled");
        response.GetType().GetProperty("message")?.GetValue(response).Should().Be("Payment was cancelled. You can try again anytime.");
    }

    [Fact]
    public async Task BookingCancel_WithEmptySessionId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.BookingCancel("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        var response = badRequestResult!.Value;

        response!.GetType().GetProperty("error")?.GetValue(response).Should().Be("Missing session_id parameter");
    }

    [Fact]
    public async Task BookingCancel_WithNonExistentSessionId_ReturnsNotFound()
    {
        // Arrange
        var sessionId = "cs_test_nonexistent";

        // Act
        var result = await _controller.BookingCancel(sessionId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        var response = notFoundResult!.Value;

        response!.GetType().GetProperty("error")?.GetValue(response).Should().Be("Booking not found");
    }

    [Fact]
    public async Task GetBookingStatus_WithValidId_ReturnsOkWithBookingStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerUser(userId, "User");

        var offer = new Offer
        {
            Title = "Guitar Lessons",
            Description = "Learn guitar basics",
            Price = 50.0m,
            CreatedBy = Guid.NewGuid()
        };

        _dbContext.Offers.Add(offer);
        await _dbContext.SaveChangesAsync();

        var booking = new Booking
        {
            Status = BookingStatus.Pending,
            Amount = 50.0m,
            CommissionAmount = 5.0m,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            StripeCheckoutSessionId = "cs_test_12345",
            StripePaymentIntentId = "pi_test_12345",
            OfferId = offer.Id,
            UserId = userId // Make sure booking belongs to the authenticated user
        };

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetBookingStatus(booking.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult?.Value;

        response.Should().NotBeNull();
        response.GetType().GetProperty("bookingId")?.GetValue(response).Should().Be(booking.Id);
        response.GetType().GetProperty("status")?.GetValue(response).Should().Be("Pending");
        response.GetType().GetProperty("stripeCheckoutSessionId")?.GetValue(response).Should().Be("cs_test_12345");
        response.GetType().GetProperty("stripePaymentIntentId")?.GetValue(response).Should().Be("pi_test_12345");
    }

    [Fact]
    public async Task GetBookingStatus_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupControllerUser(userId, "User");

        var bookingId = Guid.NewGuid();

        // Act
        var result = await _controller.GetBookingStatus(bookingId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        var response = notFoundResult!.Value;

        response!.GetType().GetProperty("error")?.GetValue(response).Should().Be("Booking not found");
    }
}