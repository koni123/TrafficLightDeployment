// See https://aka.ms/new-console-template for more information

using ControlUnit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IControlUnit, ControlUnit.Services.ControlUnit>();
builder.Services.AddSingleton<ITrafficLightService, TrafficLightService>();
builder.Services.AddMessagingService();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Services.AddHttpClient<IDatabaseService, DatabaseService>();

using var host = builder.Build();

var controlUnit = host.Services.GetRequiredService<IControlUnit>();
var messagingService = host.Services.GetRequiredService<IMessagingService>();

while (true)
{
    var opModeMessage = await messagingService.ReceiveAsync<OperationModeCommand>(
        TrafficLightConfig.UiToControlUnitQueueName,
        TrafficLightConfig.UiToControlUnitRoutingKey);

    await controlUnit.RunOperation(opModeMessage?.TrafficLightOperationMode);
    
    await Task.Delay(2500);
}