using Shared.Models;

namespace Shared.Services;

public interface IDatabaseService
{
    Task AddTrafficLightStatus(TrafficLightStatusDto statusDto);
}