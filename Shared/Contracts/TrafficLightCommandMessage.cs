namespace Shared.Contracts;

public class TrafficLightCommandMessage : ITrackedMessage
{
    public string? CorrelationId { get; set; }
    public List<TrafficLightCommand> Commands { get; set; } = new();

    public TrafficLightCommandMessage()
    {
    }
}