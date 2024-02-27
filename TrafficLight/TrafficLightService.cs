using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using Shared.Contracts;

namespace TrafficLight;

public class TrafficLightService
{
    private static readonly List<Shared.Models.TrafficLight> TrafficLights = new();
    
    public readonly Func<TrafficLightCommandMessage, TrafficLightCommandMessage> CommandHandler = message =>
    {
        foreach (var trafficLightCommand in message.Commands)
        {
            HandleLights(trafficLightCommand);
        }

        foreach (var trafficLight in TrafficLights)
        {
            Console.WriteLine($"Light id: {trafficLight.TrafficLightId}. Color: {trafficLight.Color.ToString()}");
        }

        return message;
    };

    private static void HandleLights(TrafficLightCommand? message)
    {
        if (message is null) return;

        var trafficLight = TrafficLights.Find(tl => tl.TrafficLightId == message.TrafficLightId);
        if (trafficLight is null)
        {
            TrafficLights.Add(new Shared.Models.TrafficLight(message.TrafficLightId) { Color = message.Color });
        }
        else
        {
            trafficLight.Color = message.Color;
        }
    }
}