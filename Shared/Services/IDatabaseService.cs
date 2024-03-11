using Shared.Models;

namespace Shared.Services;

public interface IDatabaseService
{
    Task AddTrafficLightStatus(TrafficLightStatusModel statusModel);
}