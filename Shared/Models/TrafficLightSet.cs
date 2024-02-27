namespace Shared.Models;

public class TrafficLightSet
{
    public TrafficLightStatus Status { get; set; }
    public List<TrafficLight> TrafficLights { get; }

    public TrafficLightSet(List<TrafficLight> trafficLights)
    {
        Status = TrafficLightStatus.Stop;
        TrafficLights = trafficLights;
    }

    public void SetStatus()
    {
        Status = TrafficLights.All(tl => tl.Color == TrafficLightColor.Red)
            ? TrafficLightStatus.Stop
            : TrafficLightStatus.Transition;
    }
}

public enum TrafficLightStatus
{
    Stop,
    Transition
}