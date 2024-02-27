using Shared.Models;

namespace Shared.Contracts;

public class TrafficLightCommand : ITrackedMessage, IEquatable<TrafficLightCommand>
{
    public string TrafficLightId { get; set; }
    public string CorrelationId { get; set; }
    public TrafficLightColor Color { get; set; }

    public TrafficLightCommand(TrafficLightColor color, string trafficLightId, string correlationId)
    {
        Color = color;
        TrafficLightId = trafficLightId;
        CorrelationId = correlationId;
    }

    public TrafficLightCommand(TrafficLightColor color, string trafficLightId)
    {
        Color = color;
        TrafficLightId = trafficLightId;
        CorrelationId = Guid.NewGuid().ToString();
    }

    public TrafficLightCommand()
    {
    }

    public bool Equals(TrafficLightCommand? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return TrafficLightId == other.TrafficLightId && CorrelationId == other.CorrelationId && Color == other.Color;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TrafficLightCommand)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TrafficLightId, CorrelationId, (int)Color);
    }
}