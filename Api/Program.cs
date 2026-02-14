using Api.Data;
using Api.Models;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultPostgresConnectionString")));

builder.Services.AddScoped<ILeagueService, LeagueService>();
builder.Services.AddScoped<IEventService, EventService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/leagues", async (ILeagueService leagueService, CancellationToken cancellationToken) =>
{
    var leagues = await leagueService.GetAsync(cancellationToken);
    return Results.Ok(leagues);
})
.WithName("GetLeagues")
.WithOpenApi();

app.MapGet("/{leagueId}/events", async (IEventService eventService, Guid leagueId, CancellationToken cancellationToken) =>
{
    var @event = await eventService.GetAllByLeagueIdAsync(leagueId, cancellationToken);
    return Results.Ok(@event);
})
.WithName("GetAllByLeagueId")
.WithOpenApi();

app.MapPost("/leagues", async (ILeagueService leagueService, League league, CancellationToken cancellationToken) =>
{
    var createdLeague = await leagueService.CreateAsync(league, cancellationToken);
    return Results.Created($"/leagues/{createdLeague.Id}", createdLeague);
})
.WithName("CreateLeague")
.WithOpenApi();

app.MapPost("/events", async (IEventService eventService, ScheduledEvent @event, CancellationToken cancellationToken) =>
{
    var createdEvent = await eventService.CreateAsync(@event, cancellationToken);
    return Results.Created($"/events/{createdEvent.Id}", createdEvent);
})
.WithName("CreateEvent")
.WithOpenApi();

app.Run();