using DatabaseService;
using DatabaseService.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TrafficLightContext>();
builder.Services.AddScoped<IDbService, DbService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TrafficLightContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/traffic-light-status",
        async (TrafficLightStatusModel model, IDbService dbService) => { await dbService.AddModelToDatabaseAsync(model); })
    .WithName("SaveTrafficLightStatus")
    .WithOpenApi();

app.MapGet("/traffic-light-status",
        async (IDbService dbService) =>
        {
            var list = await dbService.GetCurrentStatusAsync();
            return list;
        })
    .WithName("GetTrafficLightStatus")
    .WithOpenApi();

app.Run();