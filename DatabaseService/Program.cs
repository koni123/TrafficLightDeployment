using DatabaseService;
using DatabaseService.Database;
using DatabaseService.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TrafficLightContext>((_, option) =>
    option.UseNpgsql("Host=database;Database=traffic_light;Username=postgres;Password=postgres"));
builder.Services.AddScoped<IDbService, DbService>();
var app = builder
    .Build();

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

app.MapPost("/operation-mode",
        async (OperationModeDto operationMode, IDbService dbService) =>
        {
            await dbService.AddOperationModeToDatabaseAsync(operationMode);
        })
    .WithName("SaveOperationMode")
    .WithOpenApi();

app.MapGet("/operation-mode",
        async (IDbService dbService) =>
        {
            var model = await dbService.GetCurrentModeAsync();
            return model is null
                ? null
                : new OperationModeDto(model.OperationMode, model.CreatedAt);
        })
    .WithName("GetCurrentOperationMode")
    .WithOpenApi();

app.MapPost("/traffic-light-status",
        async (TrafficLightStatusDto model, IDbService dbService) =>
        {
            await dbService.AddStatusModelToDatabaseAsync(model);
        })
    .WithName("SaveTrafficLightStatus")
    .WithOpenApi();

app.MapGet("/traffic-light-status",
        async (IDbService dbService) =>
        {
            var list = await dbService.GetCurrentStatusAsync();
            return list
                .Select(tl => tl.ToDto());
        })
    .WithName("GetTrafficLightStatus")
    .WithOpenApi();

app.MapGet("/traffic-light-status/all",
        async (IDbService dbService) =>
        {
            var list = await dbService.GetAllAsync();
            return list
                .Select(tl => tl.ToDto());
        })
    .WithName("GetTrafficLightStatusAll")
    .WithOpenApi();

app.Run();