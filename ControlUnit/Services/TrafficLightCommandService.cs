using Shared.Contracts;
using Shared.Models;
using Shared.Services;

namespace ControlUnit.Services;

public class TrafficLightCommandService : ITrafficLightCommandService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public TrafficLightCommandService(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public List<TrafficLightCommand> GetNormalOperationCommands(TrafficLightSet lightSet1, TrafficLightSet lightSet2)
    {
        var commands = new List<TrafficLightCommand>();
        if (lightSet1.Status == TrafficLightStatus.Transition &&
            lightSet2.Status == TrafficLightStatus.Idle ||
            lightSet2.Status == TrafficLightStatus.Finished)
        {
            commands.AddRange(GetColorChangeCommandsForSet(lightSet1));
            commands.AddRange(
                lightSet2.TrafficLights.Select(tl => new TrafficLightCommand(tl.Color, tl.TrafficLightId)));
            return commands;
        }

        if (lightSet2.Status == TrafficLightStatus.Transition &&
            lightSet1.Status == TrafficLightStatus.Idle ||
            lightSet1.Status == TrafficLightStatus.Finished)
        {
            commands.AddRange(GetColorChangeCommandsForSet(lightSet2));
            commands.AddRange(
                lightSet1.TrafficLights.Select(tl => new TrafficLightCommand(tl.Color, tl.TrafficLightId)));
            return commands;
        }

        if (lightSet1.Status == TrafficLightStatus.Finished &&
            lightSet2.Status == TrafficLightStatus.Idle)
        {
            commands.AddRange(GetColorChangeCommandsForSet(lightSet2));
            commands.AddRange(
                lightSet1.TrafficLights.Select(tl => new TrafficLightCommand(tl.Color, tl.TrafficLightId)));
            return commands;
        }

        if (lightSet2.Status == TrafficLightStatus.Finished &&
            lightSet1.Status == TrafficLightStatus.Idle)
        {
            commands.AddRange(GetColorChangeCommandsForSet(lightSet1));
            commands.AddRange(
                lightSet2.TrafficLights.Select(tl => new TrafficLightCommand(tl.Color, tl.TrafficLightId)));
        }

        return commands;
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