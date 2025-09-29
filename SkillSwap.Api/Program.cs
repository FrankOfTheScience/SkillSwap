using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SkillSwap.Api.Configuration;
using SkillSwap.Api.Dtos;
using SkillSwap.Api.Middleware;
using SkillSwap.Api.Services;
using SkillSwap.Application.Bookings.Commands;
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
        
        // Add controller support for webhook and booking endpoints
        builder.Services.AddControllers();
        
        // Add SignalR
        builder.Services.AddSignalR();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("SKILLSWAP_DB_CONNECTION");

        builder.Services.AddDbContext<SkillSwapDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SkillSwapDbContext>());
        builder.Services.AddScoped<IAuthService, AuthService>();
        
        // Add HTTP client factory with resilience
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient("Default");
        builder.Services.AddHttpClient("Stripe", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(45);
            client.DefaultRequestHeaders.Add("User-Agent", "SkillSwap/1.0");
        });
        builder.Services.AddHttpClient("ExternalApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOfferCommand).Assembly));

        // Configure Stripe
        builder.Services.Configure<SkillSwap.Api.Configuration.StripeSettings>(builder.Configuration.GetSection("Stripe"));
        
        // Configure Resilience
        builder.Services.Configure<ResilienceSettings>(builder.Configuration.GetSection(ResilienceSettings.SectionName));
        
        // Register ResilienceSettings as a singleton for direct injection
        builder.Services.AddSingleton<ResilienceSettings>(provider =>
        {
            var resilienceSettings = new ResilienceSettings();
            builder.Configuration.GetSection(ResilienceSettings.SectionName).Bind(resilienceSettings);
            return resilienceSettings;
        });
        
        // Register resilience services
        builder.Services.AddSingleton<IResiliencePolicyService, ResiliencePolicyService>();
        builder.Services.AddScoped<IResilientDatabaseService, ResilientDatabaseService>();
        builder.Services.AddScoped<IResilientHttpClientService, ResilientHttpClientService>();
        
        // Register Stripe services with resilience
        builder.Services.AddScoped<SkillSwap.Application.Common.Interfaces.IStripeService, SkillSwap.Api.Services.StripeService>();
        builder.Services.AddScoped<SkillSwap.Application.Common.Interfaces.IStripeEventParser, SkillSwap.Api.Services.StripeEventParser>();

        // Register dashboard service
        builder.Services.AddScoped<SkillSwap.Api.Services.IDashboardService, SkillSwap.Api.Services.DashboardService>();
        builder.Services.AddScoped<SkillSwap.Application.Common.Interfaces.IDashboardNotificationService, SkillSwap.Api.Services.DashboardService>();


        // Add session support for secure booking flow
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(20);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        // Register validators and mappers
        builder.Services.AddAutoMapper(typeof(OfferProfile));
        builder.Services.AddValidatorsFromAssemblyContaining<CreateOfferCommandValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<UpdateOfferCommandValidator>();
        builder.Services.AddAutoMapper(typeof(UserProfile));
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<SkillSwap.Application.Bookings.Commands.CreateBookingCommandValidator>();

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
        app.UseResilience(); // Add resilience middleware

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");
        app.UseSession(); // Add session middleware for secure booking flow
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

        // Map controllers for all API endpoints
        app.MapControllers();
        
        // Map SignalR hubs
        app.MapHub<SkillSwap.Api.Hubs.DashboardHub>("/hubs/dashboard");

        app.Run();
    }
}