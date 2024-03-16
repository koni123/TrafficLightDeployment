using Shared.Contracts;
using Shared.Models;
using Shared.Services;

namespace ControlUnit.Services;

public class TrafficLightService : ITrafficLightService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public TrafficLightService(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public List<TrafficLightCommand> GetNormalOperationCommands(TrafficLightSet lightSet1, TrafficLightSet lightSet2)
    {
        var commands = new List<TrafficLightCommand>();
        if (lightSet1.Status == TrafficLightStatus.Transition &&
            lightSet2.Status == TrafficLightStatus.Stop)
        {
            commands.AddRange(GetColorChangeCommandsForSet(lightSet1));
            commands.AddRange(
                lightSet2.TrafficLights.Select(tl => new TrafficLightCommand(tl.Color, tl.TrafficLightId)));
            return commands;
        }

        if (lightSet2.Status == TrafficLightStatus.Transition &&
            lightSet1.Status == TrafficLightStatus.Stop)
        {
            commands.AddRange(GetColorChangeCommandsForSet(lightSet2));
            commands.AddRange(
                lightSet1.TrafficLights.Select(tl => new TrafficLightCommand(tl.Color, tl.TrafficLightId)));
            return commands;
        }
        
        return commands;
    }

    public List<TrafficLightCommand> GetStopOperationCommands(TrafficLightSet lightSet1, TrafficLightSet lightSet2)
    {
        // first run normal cycle to the end and then hold red
        var commands = new List<TrafficLightCommand>();
        if (lightSet1.Status == TrafficLightStatus.Transition &&
            lightSet2.Status == TrafficLightStatus.Stop)
        {
            commands.AddRange(GetColorChangeCommandsForSet(lightSet1));
            commands.AddRange(
                lightSet2.TrafficLights.Select(tl => new TrafficLightCommand(tl.Color, tl.TrafficLightId)));
            return commands;
        }

        if (lightSet2.Status == TrafficLightStatus.Transition &&
            lightSet1.Status == TrafficLightStatus.Stop)
        {
            commands.AddRange(GetColorChangeCommandsForSet(lightSet2));
            commands.AddRange(
                lightSet1.TrafficLights.Select(tl => new TrafficLightCommand(tl.Color, tl.TrafficLightId)));
            return commands;
        }

        // stop and stop
        commands.AddRange(lightSet1.TrafficLights.Select(tl => new TrafficLightCommand(TrafficLightColor.Red, tl.TrafficLightId)));
        commands.AddRange(lightSet2.TrafficLights.Select(tl => new TrafficLightCommand(TrafficLightColor.Red, tl.TrafficLightId)));
        return commands;
    }

    public (TrafficLightStatus, TrafficLightStatus) GetSetNormalStatuses(TrafficLightSet trafficLightSet1,
        TrafficLightSet trafficLightSet2)
    {
        // status "idle" => "transition" => "idle" => "transition" =>
        if (trafficLightSet1.Status == TrafficLightStatus.Stop && trafficLightSet2.Status == TrafficLightStatus.Stop)
        {
            // longest last updated time or set 1 takes priority
            var latest1 = trafficLightSet1.TrafficLights
                .Select(tl => tl.LastChanged)
                .ToList()
                .Max();
            var latest2 = trafficLightSet2.TrafficLights
                .Select(tl => tl.LastChanged)
                .Max();

            // if there's a requirement to keep both lights red for a while this is the place to do it

            return latest1 <= latest2
                ? (TrafficLightStatus.Transition, TrafficLightStatus.Stop)
                : (TrafficLightStatus.Stop, TrafficLightStatus.Transition);
        }

        if (trafficLightSet1.Status == TrafficLightStatus.Transition &&
            trafficLightSet2.Status == TrafficLightStatus.Stop)
        {
            return trafficLightSet1.TrafficLights.Any(tl => tl.Color != TrafficLightColor.Red)
                ? (TrafficLightStatus.Transition, TrafficLightStatus.Stop)
                : (TrafficLightStatus.Stop, TrafficLightStatus.Transition);
        }

        if (trafficLightSet2.Status == TrafficLightStatus.Transition &&
            trafficLightSet1.Status == TrafficLightStatus.Stop)
        {
            return trafficLightSet2.TrafficLights.Any(tl => tl.Color != TrafficLightColor.Red)
                ? (TrafficLightStatus.Stop, TrafficLightStatus.Transition)
                : (TrafficLightStatus.Transition, TrafficLightStatus.Stop);
        }

        throw new ApplicationException(
            $"Unknown state combination: {trafficLightSet1.Status.ToString()} and {trafficLightSet2.Status.ToString()}!");
    }

    public (TrafficLightStatus, TrafficLightStatus) GetSetStopStatuses(TrafficLightSet trafficLightSet1,
        TrafficLightSet trafficLightSet2)
    {
        // status "transition" => "stop"
        if (trafficLightSet1.Status == TrafficLightStatus.Stop && trafficLightSet2.Status == TrafficLightStatus.Stop)
        {
            return (TrafficLightStatus.Stop, TrafficLightStatus.Stop);
        }

        if (trafficLightSet1.Status == TrafficLightStatus.Transition &&
            trafficLightSet2.Status == TrafficLightStatus.Stop)
        {
            return trafficLightSet1.TrafficLights.Any(tl => tl.Color != TrafficLightColor.Red)
                ? (TrafficLightStatus.Transition, TrafficLightStatus.Stop)
                : (TrafficLightStatus.Stop, TrafficLightStatus.Stop);
        }

        if (trafficLightSet2.Status == TrafficLightStatus.Transition &&
            trafficLightSet1.Status == TrafficLightStatus.Stop)
        {
            return trafficLightSet2.TrafficLights.Any(tl => tl.Color != TrafficLightColor.Red)
                ? (TrafficLightStatus.Stop, TrafficLightStatus.Transition)
                : (TrafficLightStatus.Stop, TrafficLightStatus.Stop);
        }

        throw new ApplicationException(
            $"Unknown state combination: {trafficLightSet1.Status.ToString()} and {trafficLightSet2.Status.ToString()}!");
    }

    private IEnumerable<TrafficLightCommand> GetColorChangeCommandsForSet(TrafficLightSet trafficLightSet)
    {
        var commands = new List<TrafficLightCommand>();
        foreach (var trafficLight in trafficLightSet.TrafficLights)
        {
            var nextColor = trafficLight.GetNextColor(_dateTimeProvider.NowUtc);
            commands.Add(new TrafficLightCommand(nextColor, trafficLight.TrafficLightId));
        }

        return commands;
    }
}