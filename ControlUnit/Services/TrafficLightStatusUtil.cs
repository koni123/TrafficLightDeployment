using Shared.Models;

namespace ControlUnit.Services;

public static class TrafficLightStatusUtil
{
    public static (TrafficLightStatus, TrafficLightStatus) GetStatus(TrafficLightSet trafficLightSet1,
        TrafficLightSet trafficLightSet2)
    {
        // status "idle" => "transition" => "finished" => "idle"
        // set 1 is started first on boot
        if (trafficLightSet1.Status == TrafficLightStatus.Idle && trafficLightSet2.Status == TrafficLightStatus.Idle)
        {
            return (TrafficLightStatus.Transition, trafficLightSet2.Status);
        }

        if (trafficLightSet1.Status == TrafficLightStatus.Transition && trafficLightSet2.Status != TrafficLightStatus.Transition)
        {
            return trafficLightSet1.TrafficLights.Any(tl => tl.Color != TrafficLightColor.Red)
                ? (trafficLightSet1.Status, trafficLightSet2.Status)
                : (TrafficLightStatus.Finished, trafficLightSet2.Status);
        }

        if (trafficLightSet2.Status == TrafficLightStatus.Transition && trafficLightSet1.Status != TrafficLightStatus.Transition)
        {
            return trafficLightSet2.TrafficLights.Any(tl => tl.Color != TrafficLightColor.Red)
                ? (trafficLightSet1.Status, trafficLightSet2.Status)
                : (trafficLightSet1.Status, TrafficLightStatus.Finished);
        }

        // could do without these too with only two statuses idle and transition, but it is here to give some breathing room..
        if (trafficLightSet1.Status == TrafficLightStatus.Finished &&
            trafficLightSet2.Status == TrafficLightStatus.Idle)
        {
            return (TrafficLightStatus.Idle, TrafficLightStatus.Transition);
        }

        if (trafficLightSet2.Status == TrafficLightStatus.Finished &&
            trafficLightSet1.Status == TrafficLightStatus.Idle)
        {
            return (TrafficLightStatus.Transition, TrafficLightStatus.Idle);
        }

        throw new ApplicationException(
            $"Unknown state combination: {trafficLightSet1.Status.ToString()} and {trafficLightSet2.Status.ToString()}!");
    }
}