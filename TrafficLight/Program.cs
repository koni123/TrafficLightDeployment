// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Services;
using TrafficLight;

var builder = Host.CreateApplicationBuilder(args);

var cBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
builder.Configuration.AddConfiguration(cBuilder.Build());
builder.Services.AddSingleton<IMessagingService, MessagingService>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

using var host = builder.Build();

var messagingService = host.Services.GetService<IMessagingService>();
var service = new TrafficLightService();

while (true)
{
    await messagingService!.ReceiveAndReplyAsync(
        TrafficLightConfig.ControlUnitToTrafficLightQueueName,
        TrafficLightConfig.TrafficLightToControlUnitQueueName,
        service.CommandHandler);

    await Task.Delay(2000);
}


Console.WriteLine("Hello, World!");