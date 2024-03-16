namespace Shared.Contracts;

public class OperationModeCommand : ITrackedMessage
{
    public required string CorrelationId { get; set; }
    public required string TrafficLightOperationMode { get; set; }
}