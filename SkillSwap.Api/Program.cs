using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SkillSwap.Api.Dtos;
using SkillSwap.Api.Middleware;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Application.Offers.Dtos;
using SkillSwap.Application.Offers.Mappings;
using SkillSwap.Application.Offers.Queries;
using SkillSwap.Application.Offers.Validators;
using SkillSwap.Application.Services;
using SkillSwap.Application.Users.Commands;
using SkillSwap.Application.Users.Mappings;
using SkillSwap.Application.Users.Validators;
using SkillSwap.Infrastructure;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

internal class Program
{
    [ExcludeFromCodeCoverage]
    private static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("SKILLSWAP_DB_CONNECTION");

        builder.Services.AddDbContext<SkillSwapDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SkillSwapDbContext>());
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOfferCommand).Assembly));


        // Register validators and mappers
        builder.Services.AddAutoMapper(typeof(OfferProfile));
        builder.Services.AddValidatorsFromAssemblyContaining<CreateOfferCommandValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<UpdateOfferCommandValidator>();
        builder.Services.AddAutoMapper(typeof(UserProfile));
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();

        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                           ?? Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',')
                           ?? new[] { "http://localhost:3000" };

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtKey = builder.Configuration["Jwt:Key"] 
                        ?? Environment.GetEnvironmentVariable("Jwt__Key")
                        ?? Environment.GetEnvironmentVariable("SKILLSWAP_JWT_KEY");
                        
            var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                           ?? Environment.GetEnvironmentVariable("Jwt__Issuer")
                           ?? "SkillSwap";
                           
            var jwtAudience = builder.Configuration["Jwt:Audience"]
                             ?? Environment.GetEnvironmentVariable("Jwt__Audience")
                             ?? "SkillSwapUsers";

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is required. Set it in configuration or environment variables.");
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
        builder.Services.AddAuthorization();

        // Build the app
        var app = builder.Build();

        // Register Middleware
        app.UseMiddleware<ErrorHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true && context.Request.Headers.TryGetValue("X-User-Roles", out var rolesHeader))
            {
                var claims = rolesHeader.ToString().Split(',')
                    .Select(r => new System.Security.Claims.Claim("role", r));
                var appIdentity = new System.Security.Claims.ClaimsIdentity(claims);
                context.User.AddIdentity(appIdentity);
            }

            await next();
        });

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SkillSwapDbContext>();
            await DbSeeder.SeedAsync(db);
        }

        // POST /auth/register
        app.MapPost("auth/register", async (RegisterUserCommand cmd, IMediator mediator) =>
            {
                var token = await mediator.Send(cmd);
                return Results.Ok(new { Token = token });
            });

        // POST /auth/login
        app.MapPost("auth/login", async (LoginUserCommand cmd, IMediator mediator) =>
            {
                var token = await mediator.Send(cmd);
                return Results.Ok(new { Token = token });
            });

        // GET /user/profile
        app.MapGet("/user/profile", 
            [Microsoft.AspNetCore.Authorization.Authorize(Roles = "User,Admin")] (ClaimsPrincipal user) =>
            {
                return Results.Ok(new
                {
                    Id = user.FindFirstValue(JwtRegisteredClaimNames.Sub),
                    Email = user.FindFirstValue(JwtRegisteredClaimNames.Email),
                    DisplayName = user.FindFirstValue("displayName"),
                    Roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
                });
            });

        // GET /offers/{id}
        app.MapGet("/offers/{id:int}", 
            async (int id, IMediator mediator) =>
            {
                var offer = await mediator.Send(new GetOfferByIdQuery(id));
                return offer is null ? Results.NotFound() : Results.Ok(offer);
            })
        .WithName("GetOfferById")
        .Produces<OfferDto>(200)
        .Produces(404);

        // GET /offers
        app.MapGet("/offers", async (IMediator mediator, [FromQuery] int? page = 1, [FromQuery] int pageSize = 10) =>
            {
                var offers = await mediator.Send(new GetOffersQuery(page ?? 1, pageSize));
                return Results.Ok(offers);
            });

        // POST /offers
        app.MapPost("/offers",
            [Authorize(Roles = "User,Admin")] async (CreateOfferDto dto, IMediator mediator, IValidator<CreateOfferCommand> validator, ClaimsPrincipal user) =>
            {
                // Debug: Log all claims
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }

                var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                    ?? user.FindFirstValue("sub") 
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("ERROR: No user ID found in claims!");
                    return Results.BadRequest("User ID not found in token");
                }

                var cmd = new CreateOfferCommand(dto.Title, dto.Description, dto.Price, Guid.Parse(userId));

                var validationResult = await validator.ValidateAsync(cmd);
                if (!validationResult.IsValid)
                    return Results.BadRequest(validationResult.Errors);

                var id = await mediator.Send(cmd);
                return Results.Created($"/offers/{id}", new { Id = id });
            })
        .WithName("CreateOffer")
        .Produces(201)
        .Produces(400)
        .RequireAuthorization();

        // PUT /offers/{id}
        app.MapPut("/offers/{id:int}",
            [Authorize(Roles = "User,Admin")] async (int id, UpdateOfferDto dto, IMediator mediator, IValidator<UpdateOfferCommand> validator, ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                    ?? user.FindFirstValue("sub") 
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.BadRequest("User ID not found in token");
                }

                // NB: CreatedBy arriva dal token, non dal DTO
                var cmd = new UpdateOfferCommand(id, dto.Title, dto.Description, dto.Price, Guid.Parse(userId));

                var validationResult = await validator.ValidateAsync(cmd);
                if (!validationResult.IsValid)
                    return Results.BadRequest(validationResult.Errors);

                var updated = await mediator.Send(cmd);
                return updated is null ? Results.NotFound() : Results.Ok(updated);
            })
        .WithName("UpdateOffer")
        .Produces<OfferDto>(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization();

        // DELETE /offers/{id}
        app.MapDelete("/offers/{id:int}",
            [Authorize(Roles = "User,Admin")] async (int id, IMediator mediator, ClaimsPrincipal user) =>
            {
                try
                {
                    var deleted = await mediator.Send(new DeleteOfferCommand(id));
                    return deleted ? Results.NoContent() : Results.NotFound();
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Forbid();
                }
            })
        .WithName("DeleteOffer")
        .Produces(204)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization();

        app.Run();
    }
}