using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Api.Dtos;
using SkillSwap.Api.Middleware;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Offers.Commands;
using SkillSwap.Application.Offers.Dtos;
using SkillSwap.Application.Offers.Mappings;
using SkillSwap.Application.Offers.Queries;
using SkillSwap.Application.Offers.Validators;
using SkillSwap.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddAutoMapper(typeof(OfferProfile));
builder.Services.AddValidatorsFromAssemblyContaining<CreateOfferCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateOfferCommandValidator>();

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


// GET /api/offers/{id}
app.MapGet("/offers/{id:int}", async (int id, IMediator mediator) =>
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
app.MapPost("/offers", async (CreateOfferDto dto, IMediator mediator, IValidator<CreateOfferCommand> validator) =>
{
    var cmd = new CreateOfferCommand(dto.Title, dto.Description, dto.Price, dto.CreatedBy);

    var validationResult = await validator.ValidateAsync(cmd);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var id = await mediator.Send(cmd);
    return Results.Created($"/offers/{id}", new { Id = id });
});

// PUT /api/offers/{id}
app.MapPut("/offers/{id:int}", async (int id, UpdateOfferDto dto, IMediator mediator, IValidator<UpdateOfferCommand> validator) =>
{
    var cmd = new UpdateOfferCommand(id, dto.Title, dto.Description, dto.Price, dto.CreatedBy);

    var validationResult = await validator.ValidateAsync(cmd);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var updated = await mediator.Send(cmd);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.WithName("UpdateOffer")
.Produces<OfferDto>(200)
.Produces(400)
.Produces(404);

// DELETE /api/offers/{id}
app.MapDelete("/offers/{id:int}", async (int id, IMediator mediator) =>
{
    var deleted = await mediator.Send(new DeleteOfferCommand(id));
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteOffer")
.Produces(204)
.Produces(404);


app.Run();