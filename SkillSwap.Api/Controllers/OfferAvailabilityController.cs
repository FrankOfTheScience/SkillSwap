using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Infrastructure;
using SkillSwap.Domain;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SkillSwap.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OfferAvailabilityController : ControllerBase
{
    private readonly SkillSwapDbContext _context;

    public OfferAvailabilityController(SkillSwapDbContext context)
    {
        _context = context;
    }

    [HttpGet("{offerId:guid}")]
    public async Task<ActionResult<List<OfferAvailabilityDto>>> GetOfferAvailability(Guid offerId)
    {
        var offer = await _context.Offers
            .Include(o => o.Availabilities)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer == null)
        {
            return NotFound();
        }

        var availabilities = offer.Availabilities.Select(a => new OfferAvailabilityDto
        {
            Id = a.Id,
            OfferId = a.OfferId,
            DayOfWeek = (int)a.DayOfWeek,
            StartTime = a.StartTime.ToString("HH:mm"),
            EndTime = a.EndTime.ToString("HH:mm"),
            IsAvailable = a.IsAvailable
        }).ToList();

        return Ok(availabilities);
    }

    [HttpPost("{offerId:guid}")]
    public async Task<ActionResult<OfferAvailabilityDto>> CreateAvailability(Guid offerId, CreateAvailabilityDto request)
    {
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var offer = await _context.Offers
            .FirstOrDefaultAsync(o => o.Id == offerId && o.CreatedBy == userId);

        if (offer == null)
        {
            return NotFound("Offer not found or you don't have permission to modify it");
        }

        if (!TimeOnly.TryParse(request.StartTime, out var startTime) ||
            !TimeOnly.TryParse(request.EndTime, out var endTime))
        {
            return BadRequest("Invalid time format. Use HH:mm format.");
        }

        if (startTime >= endTime)
        {
            return BadRequest("Start time must be before end time");
        }

        var availability = new OfferAvailability
        {
            OfferId = offerId,
            DayOfWeek = (DayOfWeek)request.DayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            IsAvailable = true
        };

        _context.OfferAvailabilities.Add(availability);
        await _context.SaveChangesAsync();

        return Ok(new OfferAvailabilityDto
        {
            Id = availability.Id,
            OfferId = availability.OfferId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsAvailable = availability.IsAvailable
        });
    }

    [HttpPut("{availabilityId:guid}")]
    public async Task<ActionResult<OfferAvailabilityDto>> UpdateAvailability(Guid availabilityId, UpdateAvailabilityDto request)
    {
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var availability = await _context.OfferAvailabilities
            .Include(a => a.Offer)
            .FirstOrDefaultAsync(a => a.Id == availabilityId);

        if (availability == null)
        {
            return NotFound();
        }

        if (availability.Offer?.CreatedBy != userId)
        {
            return Forbid("You don't have permission to modify this availability");
        }

        if (request.StartTime != null && request.EndTime != null)
        {
            if (!TimeOnly.TryParse(request.StartTime, out var startTime) ||
                !TimeOnly.TryParse(request.EndTime, out var endTime))
            {
                return BadRequest("Invalid time format. Use HH:mm format.");
            }

            if (startTime >= endTime)
            {
                return BadRequest("Start time must be before end time");
            }

            availability.StartTime = startTime;
            availability.EndTime = endTime;
        }

        if (request.DayOfWeek.HasValue)
        {
            availability.DayOfWeek = (DayOfWeek)request.DayOfWeek.Value;
        }

        if (request.IsAvailable.HasValue)
        {
            availability.IsAvailable = request.IsAvailable.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(new OfferAvailabilityDto
        {
            Id = availability.Id,
            OfferId = availability.OfferId,
            DayOfWeek = (int)availability.DayOfWeek,
            StartTime = availability.StartTime.ToString("HH:mm"),
            EndTime = availability.EndTime.ToString("HH:mm"),
            IsAvailable = availability.IsAvailable
        });
    }

    [HttpDelete("{availabilityId:guid}")]
    public async Task<ActionResult> DeleteAvailability(Guid availabilityId)
    {
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var availability = await _context.OfferAvailabilities
            .Include(a => a.Offer)
            .FirstOrDefaultAsync(a => a.Id == availabilityId);

        if (availability == null)
        {
            return NotFound();
        }

        if (availability.Offer?.CreatedBy != userId)
        {
            return Forbid("You don't have permission to delete this availability");
        }

        _context.OfferAvailabilities.Remove(availability);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{offerId:guid}/available-slots")]
    public async Task<ActionResult<List<AvailableSlotDto>>> GetAvailableSlots(Guid offerId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var offer = await _context.Offers
            .Include(o => o.Availabilities)
            .Include(o => o.Bookings)
            .FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer == null)
        {
            return NotFound();
        }

        var availableSlots = new List<AvailableSlotDto>();
        var current = startDate.Date;

        while (current <= endDate.Date)
        {
            var dayOfWeek = current.DayOfWeek;
            var dayAvailabilities = offer.Availabilities
                .Where(a => a.DayOfWeek == dayOfWeek && a.IsAvailable)
                .ToList();

            foreach (var availability in dayAvailabilities)
            {
                var slots = GenerateTimeSlots(current, availability, offer.DurationInMinutes);
                
                // Filter out booked slots
                var availableSlots_day = slots.Where(slot => !IsSlotBooked(slot, offer.Bookings)).ToList();
                availableSlots.AddRange(availableSlots_day);
            }

            current = current.AddDays(1);
        }

        return Ok(availableSlots.OrderBy(s => s.DateTime).ToList());
    }

    private List<AvailableSlotDto> GenerateTimeSlots(DateTime date, OfferAvailability availability, int durationInMinutes)
    {
        var slots = new List<AvailableSlotDto>();
        var startDateTime = date.Add(availability.StartTime.ToTimeSpan());
        var endDateTime = date.Add(availability.EndTime.ToTimeSpan());

        var current = startDateTime;
        while (current.AddMinutes(durationInMinutes) <= endDateTime)
        {
            // Only show future slots
            if (current > DateTime.UtcNow)
            {
                slots.Add(new AvailableSlotDto
                {
                    DateTime = current,
                    DurationInMinutes = durationInMinutes,
                    AvailabilityId = availability.Id
                });
            }
            current = current.AddMinutes(durationInMinutes);
        }

        return slots;
    }

    private bool IsSlotBooked(AvailableSlotDto slot, List<Booking> bookings)
    {
        return bookings.Any(b => 
            b.ScheduledDateTime <= slot.DateTime && 
            b.ScheduledDateTime.AddMinutes(b.DurationInMinutes) > slot.DateTime &&
            b.Status != BookingStatus.Cancelled);
    }
}

// DTOs
public class OfferAvailabilityDto
{
    public Guid Id { get; set; }
    public Guid OfferId { get; set; }
    public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, etc.
    public string StartTime { get; set; } = string.Empty; // HH:mm format
    public string EndTime { get; set; } = string.Empty; // HH:mm format
    public bool IsAvailable { get; set; }
}

public class CreateAvailabilityDto
{
    public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, etc.
    public string StartTime { get; set; } = string.Empty; // HH:mm format
    public string EndTime { get; set; } = string.Empty; // HH:mm format
}

public class UpdateAvailabilityDto
{
    public int? DayOfWeek { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public bool? IsAvailable { get; set; }
}

public class AvailableSlotDto
{
    public DateTime DateTime { get; set; }
    public int DurationInMinutes { get; set; }
    public Guid AvailabilityId { get; set; }
}