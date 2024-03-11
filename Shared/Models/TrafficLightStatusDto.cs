namespace Shared.Models;

public class TrafficLightStatusDto
{
    public required string TrafficLightId { get; set; }
    public required string Color { get; set; }
    public required DateTime LastChanged { get; set; }
}