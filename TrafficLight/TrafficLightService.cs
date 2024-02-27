using Shared.Contracts;

namespace TrafficLight;

public class TrafficLightService
{
    private static readonly List<Shared.Models.TrafficLight> TrafficLights = [];
    
    public readonly Func<TrafficLightCommandMessage, TrafficLightCommandMessage> CommandHandler = message =>
    {
        foreach (var trafficLightCommand in message.Commands)
        {
            HandleLights(trafficLightCommand);
        }

        var lightStatus = TrafficLights.Select(tl => tl.Color.ToString());
        Console.WriteLine($"Light statuses in traffic lights service: {string.Join(",", lightStatus)}");

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