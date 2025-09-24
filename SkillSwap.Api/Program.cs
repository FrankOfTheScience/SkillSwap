using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
                               ?? Environment.GetEnvironmentVariable("SKILLSWAP_DB_CONNECTION");

        builder.Services.AddDbContext<SkillSwapDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SkillSwapDbContext>());
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOfferCommand).Assembly));


        // Register validators and mappers
        builder.Services.AddAutoMapper(typeof(OfferProfile));
        builder.Services.AddValidatorsFromAssemblyContaining<CreateOfferCommandValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<UpdateOfferCommandValidator>();
        builder.Services.AddAutoMapper(typeof(UserProfile));
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

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

        app.UseAuthentication();

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

        // POST /api/register
        app.MapPost("/register", async (RegisterUserCommand cmd, IMediator mediator) =>
            {
                var token = await mediator.Send(cmd);
                return Results.Ok(new { Token = token });
            });

        // POST /api/login
        app.MapPost("/login", async (LoginUserCommand cmd, IMediator mediator) =>
            {
                var token = await mediator.Send(cmd);
                return Results.Ok(new { Token = token });
            });

        // GET /api/user/profile
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

        // GET /api/offers/{id}
        app.MapGet("/offers/{id:int}", 
            async (int id, IMediator mediator) =>
            {
                var offer = await mediator.Send(new GetOfferByIdQuery(id));
                return offer is null ? Results.NotFound() : Results.Ok(offer);
            })
        .WithName("GetOfferById")
        .Produces<OfferDto>(200)
        .Produces(404);

        // GET /api/offers
        app.MapGet("/offers", async (int page, int pageSize, IMediator mediator) =>
            {
                var offers = await mediator.Send(new GetOffersQuery(page, pageSize));
                return Results.Ok(offers);
            });

        // POST /api/offers
        app.MapPost("/offers",
            [Authorize(Roles = "User,Admin")] async (CreateOfferDto dto, IMediator mediator, IValidator<CreateOfferCommand> validator, ClaimsPrincipal user) =>
            {
                // Recupera l'utente autenticato
                var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

                // Forziamo che il CreatedBy venga dal token, non dal DTO
                var cmd = new CreateOfferCommand(dto.Title, dto.Description, dto.Price, Guid.Parse(userId!));

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

        // PUT /api/offers/{id}
        app.MapPut("/offers/{id:int}",
            [Authorize(Roles = "User,Admin")] async (int id, UpdateOfferDto dto, IMediator mediator, IValidator<UpdateOfferCommand> validator, ClaimsPrincipal user) =>
            {
                var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

                // NB: CreatedBy arriva dal token, non dal DTO
                var cmd = new UpdateOfferCommand(id, dto.Title, dto.Description, dto.Price, Guid.Parse(userId!));

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

        // DELETE /api/offers/{id}
        app.MapDelete("/offers/{id:int}",
            [Authorize(Roles = "Admin")] async (int id, IMediator mediator) =>
            {
                // Solo Admin può cancellare
                var deleted = await mediator.Send(new DeleteOfferCommand(id));
                return deleted ? Results.NoContent() : Results.NotFound();
            })
        .WithName("DeleteOffer")
        .Produces(204)
        .Produces(404)
        .RequireAuthorization();

        app.Run();
    }
}