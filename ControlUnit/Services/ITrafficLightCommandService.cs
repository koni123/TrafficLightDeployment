using Shared.Contracts;
using Shared.Models;

namespace ControlUnit.Services;

public interface ITrafficLightCommandService
{
    public List<TrafficLightCommand> GetNormalOperationCommands(TrafficLightSet lightSet1, TrafficLightSet lightSet2);
}