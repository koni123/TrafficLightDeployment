using Shared.Contracts;
using Shared.Models;

namespace ControlUnit.Services;

public interface ITrafficLightService
{
    public List<TrafficLightCommand> GetNormalOperationCommands(TrafficLightSet lightSet1, TrafficLightSet lightSet2);

    public (TrafficLightStatus, TrafficLightStatus) GetSetStatuses(
        TrafficLightSet trafficLightSet1,
        TrafficLightSet trafficLightSet2);
}