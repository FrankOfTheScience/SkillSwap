using Microsoft.EntityFrameworkCore;
using SkillSwap.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("SKILLSWAP_DB_CONNECTION");

builder.Services.AddDbContext<SkillSwapDbContext>(options =>
    options.UseNpgsql(connectionString));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.Run();

// Minimal API
app.MapGet("/", () => "SkillSwap API running");