using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Api.Dtos;
using SkillSwap.Infrastructure;
using System.Security.Claims;

namespace SkillSwap.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly SkillSwapDbContext _context;

    public UserProfileController(SkillSwapDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<GetUserProfileResponse>> GetProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var response = new GetUserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            CreatedAt = user.CreatedAt,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            City = user.City,
            Country = user.Country,
            ProfileImageUrl = user.ProfileImageUrl,
            Profession = user.Profession,
            Company = user.Company,
            YearsOfExperience = user.YearsOfExperience,
            Skills = user.Skills?.ToList() ?? new List<string>(),
            PreferredLanguage = user.PreferredLanguage ?? "en",
            TimeZone = user.TimeZone,
            EmailNotifications = user.EmailNotifications,
            PushNotifications = user.PushNotifications,
            ProfileCompletionPercentage = CalculateProfileCompletion(user),
            LastProfileUpdate = user.LastProfileUpdate
        };

        return Ok(response);
    }

    [HttpPut]
    public async Task<ActionResult<GetUserProfileResponse>> UpdateProfile(UpdateUserProfileRequest request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Update user properties
        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.Bio != null) user.Bio = request.Bio;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
        if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth;
        if (request.City != null) user.City = request.City;
        if (request.Country != null) user.Country = request.Country;
        if (request.Profession != null) user.Profession = request.Profession;
        if (request.Company != null) user.Company = request.Company;
        if (request.YearsOfExperience.HasValue) user.YearsOfExperience = request.YearsOfExperience.Value;
        if (request.Skills != null) user.Skills = request.Skills;
        if (request.PreferredLanguage != null) user.PreferredLanguage = request.PreferredLanguage;
        if (request.TimeZone != null) user.TimeZone = request.TimeZone;
        if (request.EmailNotifications.HasValue) user.EmailNotifications = request.EmailNotifications.Value;
        if (request.PushNotifications.HasValue) user.PushNotifications = request.PushNotifications.Value;

        user.LastProfileUpdate = DateTime.UtcNow;
        user.ProfileCompletionPercentage = CalculateProfileCompletion(user);

        await _context.SaveChangesAsync();

        // Return updated profile
        return await GetProfile();
    }

    [HttpGet("completion")]
    public async Task<ActionResult<ProfileCompletionResponse>> GetProfileCompletion()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var missingFields = new List<string>();
        var suggestions = new List<string>();

        if (string.IsNullOrEmpty(user.FirstName)) { missingFields.Add("firstName"); suggestions.Add("Add your first name"); }
        if (string.IsNullOrEmpty(user.LastName)) { missingFields.Add("lastName"); suggestions.Add("Add your last name"); }
        if (string.IsNullOrEmpty(user.Bio)) { missingFields.Add("bio"); suggestions.Add("Write a bio about yourself"); }
        if (string.IsNullOrEmpty(user.PhoneNumber)) { missingFields.Add("phoneNumber"); suggestions.Add("Add your phone number"); }
        if (!user.DateOfBirth.HasValue) { missingFields.Add("dateOfBirth"); suggestions.Add("Add your date of birth"); }
        if (string.IsNullOrEmpty(user.City)) { missingFields.Add("city"); suggestions.Add("Add your city"); }
        if (string.IsNullOrEmpty(user.Country)) { missingFields.Add("country"); suggestions.Add("Add your country"); }
        if (string.IsNullOrEmpty(user.Profession)) { missingFields.Add("profession"); suggestions.Add("Add your profession"); }
        if (string.IsNullOrEmpty(user.Company)) { missingFields.Add("company"); suggestions.Add("Add your company"); }
        if (user.YearsOfExperience == 0) { missingFields.Add("yearsOfExperience"); suggestions.Add("Add your years of experience"); }
        if (user.Skills == null || !user.Skills.Any()) { missingFields.Add("skills"); suggestions.Add("Add your skills"); }

        var percentage = CalculateProfileCompletion(user);

        return Ok(new ProfileCompletionResponse
        {
            Percentage = percentage,
            MissingFields = missingFields,
            Suggestions = suggestions
        });
    }

    private int CalculateProfileCompletion(Domain.User user)
    {
        var fields = 0;
        var completed = 0;

        fields++; if (!string.IsNullOrEmpty(user.FirstName)) completed++;
        fields++; if (!string.IsNullOrEmpty(user.LastName)) completed++;
        fields++; if (!string.IsNullOrEmpty(user.Bio)) completed++;
        fields++; if (!string.IsNullOrEmpty(user.PhoneNumber)) completed++;
        fields++; if (user.DateOfBirth.HasValue) completed++;
        fields++; if (!string.IsNullOrEmpty(user.City)) completed++;
        fields++; if (!string.IsNullOrEmpty(user.Country)) completed++;
        fields++; if (!string.IsNullOrEmpty(user.Profession)) completed++;
        fields++; if (!string.IsNullOrEmpty(user.Company)) completed++;
        fields++; if (user.YearsOfExperience > 0) completed++;
        fields++; if (user.Skills != null && user.Skills.Any()) completed++;

        return fields > 0 ? (int)Math.Round((double)completed / fields * 100) : 0;
    }
}
