// See https://aka.ms/new-console-template for more information

using ControlUnit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IControlUnit, ControlUnit.Services.ControlUnit>();
builder.Services.AddSingleton<ITrafficLightCommandService, TrafficLightCommandService>();
builder.Services.AddTransient<IMessagingService, MessagingService>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

using var host = builder.Build();

var controlUnit = host.Services.GetService<IControlUnit>();

while (true)
{
    try
    {
        await controlUnit!.RunNormalOperation();
        await Task.Delay(4000);
    }
    catch (ApplicationException e)
    {
        Console.WriteLine($"Failure in running traffic lights: {e.Message}");
    }
}


Console.WriteLine("Hello, World!");