namespace Shared.Models;

public class TrafficLightSet
{
    public TrafficLightStatus Status { get; set; }
    public List<TrafficLight> TrafficLights { get; }

    public TrafficLightSet(List<TrafficLight> trafficLights)
    {
        Status = TrafficLightStatus.Idle;
        TrafficLights = trafficLights;
    }
}

public enum TrafficLightStatus
{
    Idle,
    Transition,
    Finished
}