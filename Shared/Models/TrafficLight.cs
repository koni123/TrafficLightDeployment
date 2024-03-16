using System.ComponentModel;

namespace Shared.Models;

public class TrafficLight
{
    public string TrafficLightId { get; }
    public TrafficLightColor Color { get; set; }
    public DateTime LastChanged { get; set; }

    public TrafficLight(string trafficLightId)
    {
        TrafficLightId = trafficLightId;
        LastChanged = DateTime.MinValue;
        Color = TrafficLightColor.Red;
    }

    public TrafficLightColor GetNextColor(DateTime dateTimeNow)
    {
        return Color switch
        {
            TrafficLightColor.Green => dateTimeNow < LastChanged.AddSeconds(5)
                ? TrafficLightColor.Green
                : TrafficLightColor.Yellow,
            TrafficLightColor.Yellow => dateTimeNow < LastChanged.AddSeconds(3)
                ? TrafficLightColor.Yellow
                : TrafficLightColor.Red,
            TrafficLightColor.Red => TrafficLightColor.Green,
            TrafficLightColor.Unknown => TrafficLightColor.Red,
            _ => throw new InvalidEnumArgumentException(nameof(Color))
        };
    }
}