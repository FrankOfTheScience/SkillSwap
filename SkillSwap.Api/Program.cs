using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Api.Dtos;
using SkillSwap.Api.Middleware;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Application.Offers.Mappings;
using SkillSwap.Application.Offers.Queries;
using SkillSwap.Application.Offers.Validators;
using SkillSwap.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("SKILLSWAP_DB_CONNECTION");

builder.Services.AddDbContext<SkillSwapDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<SkillSwapDbContext>());
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateOfferCommand).Assembly));

// Register validators and mappers
builder.Services.AddValidatorsFromAssemblyContaining<CreateOfferCommandValidator>();
builder.Services.AddAutoMapper(typeof(OfferProfile));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

// Register Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// POST /api/offers
app.MapPost("/offers", async (CreateOfferDto dto, IMediator mediator, IValidator<CreateOfferCommand> validator) =>
{
    var cmd = new CreateOfferCommand(dto.Title, dto.Description, dto.Price, dto.CreatedBy);

    var validationResult = await validator.ValidateAsync(cmd);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var id = await mediator.Send(cmd);
    return Results.Created($"/offers/{id}", new { Id = id });
});

// GET /api/offers
app.MapGet("/offers", async (int page, int pageSize, IMediator mediator) =>
{
    var offers = await mediator.Send(new GetOffersQuery(page, pageSize));
    return Results.Ok(offers);
});

app.Run();