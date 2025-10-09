using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Api.Dtos;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Application.Offers.Dtos;
using SkillSwap.Application.Offers.Queries;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class OffersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OffersController> _logger;

    public OffersController(IMediator mediator, ILogger<OffersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get an offer by ID
    /// </summary>
    /// <param name="id">Offer ID</param>
    /// <returns>Offer details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var offer = await _mediator.Send(new GetOfferByIdQuery(id));
            return offer is null ? NotFound() : Ok(offer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting offer {OfferId}", id);
            return StatusCode(500, new { Error = "Failed to get offer" });
        }
    }

    /// <summary>
    /// Get offers with filtering and pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="search">Search term</param>
    /// <param name="maxBudget">Maximum budget filter</param>
    /// <param name="showOnlyMyOffers">Show only current user's offers</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <returns>Paginated list of offers</returns>
    [HttpGet]
    public async Task<IActionResult> GetOffers(
        [FromQuery] int? page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] decimal? maxBudget = null,
        [FromQuery] bool? showOnlyMyOffers = null,
        [FromQuery] string? sortBy = "id",
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true 
                ? User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                  ?? User.FindFirstValue("sub") 
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            var query = new GetOffersQuery(
                page ?? 1, 
                Math.Min(pageSize, 50), // Max 50 items per page
                search,
                maxBudget,
                showOnlyMyOffers,
                sortBy ?? "id",
                sortDescending,
                userId
            );

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting offers");
            return StatusCode(500, new { Error = "Failed to get offers" });
        }
    }

    /// <summary>
    /// Create a new offer
    /// </summary>
    /// <param name="dto">Offer details</param>
    /// <param name="validator">Command validator</param>
    /// <returns>Created offer ID</returns>
    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateOfferDto dto, 
        [FromServices] IValidator<CreateOfferCommand> validator)
    {
        try
        {
            // Debug user claims
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            _logger.LogInformation("User claims: {@Claims}", userClaims);
            
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirstValue("sub") 
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No user ID found in claims for offer creation");
                return BadRequest(new { Error = "User ID not found in token" });
            }

            var cmd = new CreateOfferCommand(
                dto.Title, 
                dto.Description, 
                dto.Price, 
                Guid.Parse(userId),
                dto.DurationInMinutes,
                dto.Location,
                dto.IsOnline,
                dto.Requirements,
                dto.Category
            );

            var validationResult = await validator.ValidateAsync(cmd);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Errors = validationResult.Errors });
            }

            var id = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(GetById), new { id }, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating offer");
            return StatusCode(500, new { Error = "Failed to create offer" });
        }
    }

    /// <summary>
    /// Update an existing offer
    /// </summary>
    /// <param name="id">Offer ID</param>
    /// <param name="dto">Updated offer details</param>
    /// <param name="validator">Command validator</param>
    /// <returns>Updated offer</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Update(
        Guid id, 
        [FromBody] UpdateOfferDto dto, 
        [FromServices] IValidator<UpdateOfferCommand> validator)
    {
        try
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirstValue("sub") 
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { Error = "User ID not found in token" });
            }

            var cmd = new UpdateOfferCommand(
                id, 
                dto.Title, 
                dto.Description, 
                dto.Price, 
                Guid.Parse(userId),
                dto.DurationInMinutes,
                dto.Location,
                dto.IsOnline,
                dto.Requirements,
                dto.Category
            );

            var validationResult = await validator.ValidateAsync(cmd);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Errors = validationResult.Errors });
            }

            var updated = await _mediator.Send(cmd);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating offer {OfferId}", id);
            return StatusCode(500, new { Error = "Failed to update offer" });
        }
    }

    /// <summary>
    /// Delete an offer
    /// </summary>
    /// <param name="id">Offer ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _mediator.Send(new DeleteOfferCommand(id));
            return deleted ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting offer {OfferId}", id);
            return StatusCode(500, new { Error = "Failed to delete offer" });
        }
    }
}